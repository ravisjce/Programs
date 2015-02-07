///////////////////////////////////////////////////////////////////////
// RulesAndActions.cs - Parser rules specific to an application      //
// ver 2.1                                                           //
// Language:    C#, 2008, .Net Framework 4.0                         //
// Platform:    Dell Precision T7400, Win7, SP1                      //
// Application: Demonstration for CSE681, Project #2, Fall 2011      //
// Author:      Jim Fawcett, CST 4-187, Syracuse University          //
//              (315) 443-3948, jfawcett@twcny.rr.com                //
// Modified By: Ravi Nagendra                                        //
///////////////////////////////////////////////////////////////////////
/*
 * Package Operations:
 * -------------------
 * RulesAndActions package contains all of the Application specific
 * code required for most analysis tools.
 *
 * It defines the following Four rules which each have a
 * grammar construct detector and also a collection of IActions:
 *   - DetectNameSpace rule
 *   - DetectClass rule
 *   - DetectFunction rule
 *   - DetectScopeChange
 *   
 *   Three actions - some are specific to a parent rule:
 *   - Print
 *   - PrintFunction
 *   - PrintScope
 * 
 * The package also defines a Repository class for passing data between
 * actions and uses the services of a ScopeStack, defined in a package
 * of that name.
 *
 * Note:
 * This package does not have a test stub since it cannot execute
 * without requests from Parser.
 *  
 */
/* Required Files:
 *   IRuleAndAction.cs, RulesAndActions.cs, Parser.cs, ScopeStack.cs,
 *   Semi.cs, Toker.cs
 *   
 * Build command:
 *   csc /D:TEST_PARSER Parser.cs IRuleAndAction.cs RulesAndActions.cs \
 *                      ScopeStack.cs Semi.cs Toker.cs
 *   
 * Maintenance History:
 * --------------------
 * ver 2.3: 13 october 2014
 * - added 4 new rule to handle different types of relationship 
 * DetectInheritance
 * DetectAggregation
 * DetectComposition
 * DetectUsing
 * - added 4 new actions for different relationship types
 * doActionForInheritance
 * doActionForAggregation
 * doActionForComposition
 * doActionForUsing
 * - added 4 new element types for different relationship types
 * InheritanceElement
 * AggregationElement
 * CompositionElement
 * UsingElement
 * - added a method DetectParentClass which is used to find the parent class
 * whenever we find aggregation,composition relationship
 * ver 2.2 : 24 Sep 2011
 * - modified Semi package to extract compile directives (statements with #)
 *   as semiExpressions
 * - strengthened and simplified DetectFunction
 * - the previous changes fixed a bug, reported by Yu-Chi Jen, resulting in
 * - failure to properly handle a couple of special cases in DetectFunction
 * - fixed bug in PopStack, reported by Weimin Huang, that resulted in
 *   overloaded functions all being reported as ending on the same line
 * - fixed bug in isSpecialToken, in the DetectFunction class, found and
 *   solved by Zuowei Yuan, by adding "using" to the special tokens list.
 * - There is a remaining bug in Toker caused by using the @ just before
 *   quotes to allow using \ as characters so they are not interpreted as
 *   escape sequences.  You will have to avoid using this construct, e.g.,
 *   use "\\xyz" instead of @"\xyz".  Too many changes and subsequent testing
 *   are required to fix this immediately.
 * ver 2.1 : 13 Sep 2011
 * - made BuildCodeAnalyzer a public class
 * ver 2.0 : 05 Sep 2011
 * - removed old stack and added scope stack
 * - added Repository class that allows actions to save and 
 *   retrieve application specific data
 * - added rules and actions specific to Project #2, Fall 2010
 * ver 1.1 : 05 Sep 11
 * - added Repository and references to ScopeStack
 * - revised actions
 * - thought about added folding rules
 * ver 1.0 : 28 Aug 2011
 * - first release
 *
 * Planned Modifications (not needed for Project #2):
 * --------------------------------------------------
 * - add folding rules:
 *   - CSemiExp returns for(int i=0; i<len; ++i) { as three semi-expressions, e.g.:
 *       for(int i=0;
 *       i<len;
 *       ++i) {
 *     The first folding rule folds these three semi-expression into one,
 *     passed to parser. 
 *   - CToker returns operator[]( as four distinct tokens, e.g.: operator, [, ], (.
 *     The second folding rule coalesces the first three into one token so we get:
 *     operator[], ( 
 */
using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;

namespace CodeAnalysis
{
    public class Elem  // holds scope information
    {
        public string type { get; set; }
        public string name { get; set; }
        public int begin { get; set; }
        public int end { get; set; }
        public int scopecount { get; set; }

        public string filepath { get; set; }

        //constructor in which all the member variables are initialised      
        public Elem()
        {
            scopecount = 1;
        }

        //Used to represent all the class members in string format    

        public override string ToString()
        {
            StringBuilder temp = new StringBuilder();
            temp.Append("{");
            temp.Append(String.Format("{0,-10}", type)).Append(" : ");
            temp.Append(String.Format("{0,-10}", name)).Append(" : ");
            temp.Append(String.Format("{0,-5}", begin.ToString()));  // line of scope start
            temp.Append(String.Format("{0,-5}", end.ToString()));    // line of scope end
            temp.Append(string.Format("{0,-5}", scopecount.ToString()));
            temp.Append(String.Format("{0,-10}", filepath)).Append(" : ");
            temp.Append("}");
            return temp.ToString();
        }
    }

    /*This class is used to represent the inheritance data relationships
     * It contains the parent and list of all its children which inherits 
     * the parent class
     * */
    public class InheritanceElement
    {
        public string parent { get; set; }
        public ArrayList children { get; set; }
        public int childcount { get; set; }

        //constructor in which all the member variables are initialised     
        public InheritanceElement()
        {
            childcount = 0;
            children = new ArrayList();
        }

        //Used to represent all the class members in string format
        public override string ToString()
        {
            StringBuilder temp = new StringBuilder();
            temp.Append("{");
            temp.Append(String.Format("{0,10}", parent)).Append(" : ");
            temp.Append(String.Format("{0,10}", children)).Append(" : ");
            temp.Append(String.Format("{0,10}", childcount));
            temp.Append("}");
            return temp.ToString();
        }
    }//end of class


    /*This class is used to represent the Aggregation data relationships
     * It contains the aggregator and list of all the elements which is 
     * being aggregated by the aggregator
     */
    public class AggregationElement
    {
        public string aggregator { get; set; }
        public ArrayList aggregated { get; set; }
        public string type { get; set; }
       
        //constructor in which all the member variables are initialised             
        public AggregationElement()
        {
            aggregated = new ArrayList();
        }
    }//end of class


    /*This class is used to represent the Composition data relationships
     * It contains the compositor and list of all the elements which is 
     * being composed by the compositor
     */
    public class CompositionElement
    {
        public string compositor { get; set; }
        public ArrayList composedelement { get; set; }
        public string type { get; set; }

        //constructor in which all the member variables are initialised
        public CompositionElement()
        {
            composedelement = new ArrayList();
        }
    }//end of class

    /*This class is used to store the details of the types 
     * which is being used in some other class.
     * We use this structure in case of using relationship
     */ 
    public class TypeDetails
    {
        public string usedtypename { get; set; }
        public string type { get; set; }
    }//end of class


    /*This class is used to represent the Using data relationships
     * It contains the parent and list of all the elements which is 
     * being used by the parent
     */
    public class UsingElement
    {
        public string parent { get; set; }
        public List<TypeDetails> typeslist = new List<TypeDetails>();
        public string parentfunction { get; set; }

        //constructor in which all the member variables are initialised
        public UsingElement()
        {
            //empty constructor
        }
    }//end of class

    /*This is the class which contains result of all the analysis
     * This acts like a centalised repository for all the modules
     * The result of the complete analysis can be fethed from this class
    */
    public class Repository
    {
        ScopeStack<Elem> stack_ = new ScopeStack<Elem>();
        List<Elem> locations_ = new List<Elem>();
        List<InheritanceElement> inheritancedataList_ = new List<InheritanceElement>();
        List<AggregationElement> aggregationdataList_ = new List<AggregationElement>();
        List<CompositionElement> compositiondataList_ = new List<CompositionElement>();
        List<UsingElement> usingdataList_ = new List<UsingElement>();
        static Repository instance;

        //constructor in which all the member variables are initialised
        public Repository()
        {
            instance = this;
        }
        //Since the repository is static, we return an instance of the allocated memory
        public static Repository getInstance()
        {
            return instance;
        }

        // provides all actions access to current semiExp
        public CSsemi.CSemiExp semi
        {
            get;
            set;
        }

        // semi gets line count from toker who counts lines
        // while reading from its source
        public int lineCount  // saved by newline rule's action
        {
            get { return semi.lineCount; }
        }
        public int prevLineCount  // not used in this demo
        {
            get;
            set;
        }
        // enables recursively tracking entry and exit from scopes

        public ScopeStack<Elem> stack  // pushed and popped by scope rule's action
        {
            get { return stack_; }
        }

        //The list which contains the data of all the types used in the input set  
        public List<Elem> locations
        {
            get { return locations_; }
        }

        //The list which contains the data of all the inheritance types used in the input set
        public List<InheritanceElement> inheritancedataList
        {
            get { return inheritancedataList_; }
        }

        //The list which contains the data of all the aggregation types used in the input set
        public List<AggregationElement> aggregationdataList
        {
            get { return aggregationdataList_; }
        }

        //The list which contains the data of all the composition types used in the input set
        public List<CompositionElement> compositiondataList
        {
            get { return compositiondataList_; }
        }

        //The list which contains the data of all the using types used in the input set
        public List<UsingElement> usingdataList
        {
            get { return usingdataList_; }
        }
    }//end of class
    /////////////////////////////////////////////////////////
    // pushes scope info on stack when entering new scope

    public class PushStack : AAction
    {
        Repository repo_;

        //constructor in which all the member variables are initialised
        public PushStack(Repository repo)
        {
            repo_ = repo;
        }

        //The action performed whenever a type(class,struct,function,namespace etc) is detected
        public override void doAction(CSsemi.CSemiExp semi)
        {
            Elem elem = new Elem();
            elem.type = semi[0];  // expects type
            elem.name = semi[1];  // expects name
            elem.begin = repo_.semi.lineCount - 1;
            elem.end = 0;
            repo_.stack.push(elem);
            repo_.locations.Add(elem);

            if (AAction.displayStack)
                repo_.stack.display();
        }

        //The action performed whenever a inheritance type is detected in the current semi expression
        public override void doActionForInheritance(CSsemi.CSemiExp semi)
        {
            InheritanceElement inheritanceelem = new InheritanceElement();
            Elem elem = new Elem();
            bool existingParent = false;

            for (int i = 0; i < repo_.locations.Count; i++)
            {
                elem = repo_.locations[i];
                if (elem.type == "class" && semi[1] == elem.name)
                {
                    for (int j = 0; j < repo_.inheritancedataList.Count; j++)
                    {
                        inheritanceelem = repo_.inheritancedataList[j];
                        if (semi[1] == inheritanceelem.parent)
                        {
                            existingParent = true;
                            int index = repo_.inheritancedataList.IndexOf(inheritanceelem);
                            inheritanceelem.children.Add(semi[0]);
                            inheritanceelem.childcount++;
                            repo_.inheritancedataList.Remove(inheritanceelem);
                            repo_.inheritancedataList.Insert(index, inheritanceelem);
                        }
                    }

                    if (!existingParent)
                    {
                        inheritanceelem = new InheritanceElement();
                        inheritanceelem.children.Add(semi[0]);
                        inheritanceelem.parent = semi[1];
                        inheritanceelem.childcount++;
                        repo_.inheritancedataList.Add(inheritanceelem);
                    }
                }
            }
        }

        //The action performed whenever a aggregation type is detected in the current semi expression
        public override void doActionForAggregation(CSsemi.CSemiExp semi)
        {
            AggregationElement aggregationelem = new AggregationElement();
            Elem elem = new Elem();
            bool existingAggregator = false;

            for (int i = 0; i < repo_.locations.Count; i++)
            {
                elem = repo_.locations[i];
                if (elem.type == "class" && semi[0] == elem.name)
                {
                    for (int j = 0; j < repo_.aggregationdataList.Count; j++)
                    {
                        aggregationelem = repo_.aggregationdataList[j];
                        if (semi[2] == aggregationelem.aggregator)
                        {
                            existingAggregator = true;
                            int index = repo_.aggregationdataList.IndexOf(aggregationelem);
                            aggregationelem.aggregated.Add(semi[1]);
                            aggregationelem.type = semi[0];
                            repo_.aggregationdataList.Remove(aggregationelem);
                            repo_.aggregationdataList.Insert(index, aggregationelem);
                        }
                    }

                    if (!existingAggregator)
                    {
                        aggregationelem = new AggregationElement();
                        aggregationelem.aggregated.Add(semi[1]);
                        aggregationelem.aggregator = semi[2];
                        aggregationelem.type = semi[0];
                        repo_.aggregationdataList.Add(aggregationelem);
                    }
                }
            }
        }

        //The action performed whenever a composition type is detected in the current semi expression
        public override void doActionForComposition(CSsemi.CSemiExp semi)
        {
            CompositionElement compositionelem = new CompositionElement();
            Elem elem = new Elem();
            bool existingCompositor = false;

            for (int i = 0; i < repo_.locations.Count; i++)
            {
                elem = repo_.locations[i];
                if ((elem.type == "struct" && semi[0] == elem.name) || (elem.type == "enum" && semi[0] == elem.name))
                {
                    for (int j = 0; j < repo_.compositiondataList.Count; j++)
                    {
                        compositionelem = repo_.compositiondataList[j];
                        if (semi[2] == compositionelem.compositor)
                        {
                            existingCompositor = true;
                            int index = repo_.compositiondataList.IndexOf(compositionelem);
                            compositionelem.composedelement.Add(semi[1]);
                            compositionelem.type = semi[0];
                            repo_.compositiondataList.Remove(compositionelem);
                            repo_.compositiondataList.Insert(index, compositionelem);
                        }
                    }

                    if (!existingCompositor)
                    {
                        compositionelem = new CompositionElement();
                        compositionelem.composedelement.Add(semi[1]);
                        compositionelem.compositor = semi[2];
                        compositionelem.type = semi[0];
                        repo_.compositiondataList.Add(compositionelem);
                    }
                }
            }
        }

        //The action performed whenever a using type is detected in the current semi expression
        public override void doActionForUsing(CSsemi.CSemiExp semi)
        {
            UsingElement usingelem = new UsingElement();
            Elem elem = new Elem();
            bool existingfunction = false;
            string classname = semi[0];
            string functionname = semi[1];
            string type = semi[2];
            string typename = semi[3];

            for (int i = 0; i < repo_.locations.Count; i++)
            {
                elem = repo_.locations[i];
                if ((elem.type == "struct" || elem.type == "enum" || elem.type == "class" || elem.type == "interface")
                    && type == elem.name)
                {
                    for (int j = 0; j < repo_.usingdataList.Count; j++)
                    {
                        usingelem = repo_.usingdataList[j];
                        if (functionname == usingelem.parentfunction)
                        {
                            existingfunction = true;
                            TypeDetails typeDetails = new TypeDetails();
                            usingelem.parent = classname;
                            usingelem.parentfunction = functionname;
                            typeDetails.type = type;
                            typeDetails.usedtypename = typename;
                            usingelem.typeslist.Add(typeDetails);
                            int index = repo_.usingdataList.IndexOf(usingelem);
                            repo_.usingdataList.Remove(usingelem);
                            repo_.usingdataList.Insert(index, usingelem);
                        }
                    }

                    if (!existingfunction)
                    {
                        TypeDetails typeDetails = new TypeDetails();
                        usingelem = new UsingElement();
                        usingelem.parent = classname;
                        usingelem.parentfunction = functionname;
                        typeDetails.type = type;
                        typeDetails.usedtypename = typename;
                        usingelem.typeslist.Add(typeDetails);
                        repo_.usingdataList.Add(usingelem);
                    }
                }
            }
        }

    }//end of class
    /////////////////////////////////////////////////////////
    // pops scope info from stack when leaving scope

    public class PopStack : AAction
    {
        Repository repo_;

        //constructor in which all the member variables are initialised
        public PopStack(Repository repo)
        {
            repo_ = repo;
        }
        /*This method will be called when we encounter the end
         * of scope for any type(class,function,struct etc).
         * At the end we append the ending line number for that type
         * */
        public override void doAction(CSsemi.CSemiExp semi)
        {
            Elem elem = null;
            try
            {
                if (repo_.stack.count >= 1)
                {
                    elem = repo_.stack.pop();
                    for (int i = 0; i < repo_.locations.Count; ++i)
                    {
                        Elem temp = repo_.locations[i];
                        if (elem.type == temp.type)
                        {
                            if (elem.name == temp.name)
                            {
                                if ((repo_.locations[i]).end == 0)
                                {
                                    (repo_.locations[i]).end = repo_.semi.lineCount;
                                    break;
                                }
                            }
                        }
                        if (temp.end == 0 && temp.type == "function")
                            temp.scopecount++;
                    }
                }
            }
            catch
            {
                Console.Write("popped empty stack on semiExp: ");
                semi.display();
                return;
            }

            if (elem != null)
            {
                CSsemi.CSemiExp local = new CSsemi.CSemiExp();
                local.Add(elem.type).Add(elem.name);
            }
        }

        //No implementation since this function will not be used to do any actions
        public override void doActionForInheritance(CSsemi.CSemiExp semi)
        {
            throw new NotImplementedException();
        }

        //No implementation since this function will not be used to do any actions
        public override void doActionForAggregation(CSsemi.CSemiExp semi)
        {
            throw new NotImplementedException();
        }

        //No implementation since this function will not be used to do any actions
        public override void doActionForComposition(CSsemi.CSemiExp semi)
        {
            throw new NotImplementedException();
        }

        //No implementation since this function will not be used to do any actions
        public override void doActionForUsing(CSsemi.CSemiExp semi)
        {
            throw new NotImplementedException();
        }
    }//end of class

    ///////////////////////////////////////////////////////////
    // action to print function signatures - not used in demo

    public class PrintFunction : AAction
    {
        Repository repo_;

        //constructor in which all the member variables are initialised
        public PrintFunction(Repository repo)
        {
            repo_ = repo;
        }

        //Used to display the current semi expression
        public override void display(CSsemi.CSemiExp semi)
        {
            Console.Write("\n    line# {0}", repo_.semi.lineCount - 1);
            Console.Write("\n    ");
            for (int i = 0; i < semi.count; ++i)
                if (semi[i] != "\n" && !semi.isComment(semi[i]))
                    Console.Write("{0} ", semi[i]);
        }

        //Displays the semi expression when a type is detected
        public override void doAction(CSsemi.CSemiExp semi)
        {
            this.display(semi);
        }

        //Displays the semi expression when inheritance relation is detected
        public override void doActionForInheritance(CSsemi.CSemiExp semi)
        {
            this.display(semi);
        }

        //Displays the semi expression when aggregation relation is detected
        public override void doActionForAggregation(CSsemi.CSemiExp semi)
        {
            this.display(semi);
        }

        //Displays the semi expression when composition relation is detected
        public override void doActionForComposition(CSsemi.CSemiExp semi)
        {
            this.display(semi);
        }

        //Displays the semi expression when using relation is detected
        public override void doActionForUsing(CSsemi.CSemiExp semi)
        {
            this.display(semi);
        }
    }//end of class
    /////////////////////////////////////////////////////////
    // concrete printing action, useful for debugging

    public class Print : AAction
    {
        Repository repo_;

        //constructor in which all the member variables are initialised
        public Print(Repository repo)
        {
            repo_ = repo;
        }

        //Displays the line number when a type is detected       
        public override void doAction(CSsemi.CSemiExp semi)
        {
            Console.Write("\n  line# {0}", repo_.semi.lineCount - 1);
            this.display(semi);
        }

        //Displays the line number when inheritance relation is detected        
        public override void doActionForInheritance(CSsemi.CSemiExp semi)
        {
            Console.Write("\n  line# {0}", repo_.semi.lineCount - 1);
            this.display(semi);
        }

        //Displays the line number when aggregation relation is detected        
        public override void doActionForAggregation(CSsemi.CSemiExp semi)
        {
            Console.Write("\n  line# {0}", repo_.semi.lineCount - 1);
            this.display(semi);
        }

        //Displays the line number when composition relation is detected        
        public override void doActionForComposition(CSsemi.CSemiExp semi)
        {
            Console.Write("\n  line# {0}", repo_.semi.lineCount - 1);
            this.display(semi);
        }

        //Displays the line number when using relation is detected        
        public override void doActionForUsing(CSsemi.CSemiExp semi)
        {
            Console.Write("\n  line# {0}", repo_.semi.lineCount - 1);
            this.display(semi);
        }
    }//end of class
    /////////////////////////////////////////////////////////
    // rule to detect namespace declarations

    public class DetectNamespace : ARule
    {
        /*Checks whether the input semi expression contains namespace keyword.
         * If it is present then the action method for this type is called
         */
        public override bool test(CSsemi.CSemiExp semi)
        {
            int index = semi.Contains("namespace");
            if (index != -1)
            {
                CSsemi.CSemiExp local = new CSsemi.CSemiExp();
                // create local semiExp with tokens for type and name
                local.displayNewLines = false;
                local.Add(semi[index]).Add(semi[index + 1]);
                doActions(local);
                return true;
            }
            return false;
        }
    }//end of class
    /////////////////////////////////////////////////////////
    // rule to dectect class definitions

    public class DetectClass : ARule
    {
        /*Checks whether the input semi expression contains 
         * class/struct/interface/enum keyword.
        * If it is present then the action method for this type is called
        */
        public override bool test(CSsemi.CSemiExp semi)
        {
            int indexCL = semi.Contains("class");
            int indexIF = semi.Contains("interface");
            int indexST = semi.Contains("struct");
            int indexEN = semi.Contains("enum");

            int index = Math.Max(indexCL, indexIF);
            index = Math.Max(index, indexST);
            index = Math.Max(index, indexEN);
            if (index != -1)
            {
                CSsemi.CSemiExp local = new CSsemi.CSemiExp();
                // local semiExp with tokens for type and name
                local.displayNewLines = false;
                local.Add(semi[index]).Add(semi[index + 1]);
                doActions(local);

                return true;
            }
            return false;
        }
    }//end of class
    /////////////////////////////////////////////////////////
    // rule to dectect class inheritance

    public class DetectInheritance : ARule
    {
        /*Checks whether the input semi expression contains inheritance relationship
        * If it is present then the action method for inheritance is called
        */
        public override bool test(CSsemi.CSemiExp semi)
        {
            int indexCL = semi.Contains("class");
            int indexIF = semi.Contains("interface");

            int index = Math.Max(indexCL, indexIF);
            int index2 = semi.FindFirst(":");
            if (index != -1 && index2 != -1)
            {
                CSsemi.CSemiExp local = new CSsemi.CSemiExp();
                // local semiExp with tokens for type and name
                local.displayNewLines = false;
                local.Add(semi[index + 1]).Add(semi[index + 3]);
                doActionsForInheritance(local);

                int multiInheritCount = semi.FindAll(",");
                while (multiInheritCount > 0)
                {
                    index2 = index2 + 2;
                    local = new CSsemi.CSemiExp();
                    local.displayNewLines = false;
                    local.Add(semi[index + 1]).Add(semi[index2 + 1]);
                    doActionsForInheritance(local);
                    multiInheritCount--;
                }

                return true;
            }
            return false;
        }
    }//end of class
    /////////////////////////////////////////////////////////
    // rule to dectect class aggregation

    public class DetectAggregation : ARule
    {
        /*Checks whether the input semi expression contains aggregation relationship
        * If it is present then the action method for aggregation is called
        */
        public override bool test(CSsemi.CSemiExp semi)
        {
            Repository repo_ = Repository.getInstance();
            int index = semi.FindFirst("new");
            if (index != -1)
            {
                string currclassName = FindParentClass.getClassName(repo_.semi.lineCount);

                //if (currclassName.CompareTo("") == 0)
                //{
                //    return true;
                //}
                CSsemi.CSemiExp local = new CSsemi.CSemiExp();
                // local semiExp with tokens for type and name
                local.displayNewLines = false;
                local.Add(semi[index + 1]).Add(semi[index - 2]).Add(currclassName);
                doActionsForAggregation(local);
                return true;
            }

            return false;
        }
    }//end of class

    public class DetectComposition : ARule
    {
        /*Checks whether the input semi expression contains composition relationship
         * If it is present then the action method for composition is called
         */
        public override bool test(CSsemi.CSemiExp semi)
        {
            List<string> variablecountlist = null;
            Repository repo_ = Repository.getInstance();

            if (semi.count >= 2)
            {
                variablecountlist = semi.DetectVariables();
            }

            if (variablecountlist != null && variablecountlist.Count == 2)
            {
                string currclassName = FindParentClass.getClassName(repo_.semi.lineCount);

                CSsemi.CSemiExp local = new CSsemi.CSemiExp();
                local.displayNewLines = false;
                local.Add(variablecountlist[0]).Add(variablecountlist[1]).Add(currclassName);
                doActionsForComposition(local);
                return true;
            }

            return false;
        }
    }//end of class

    public class DetectUsing : ARule
    {
        /*Checks whether the input semi expression contains using relationship
         * If it is present then the action method for using is called
         */
        public static bool isSpecialToken(string token)
        {
            string[] SpecialToken = { "if", "else", "for", "foreach", "while", "catch", "try", "finally", "using", "switch", "case", "do" };
            foreach (string stoken in SpecialToken)
                if (stoken == token)
                    return true;
            return false;
        }

        public override bool test(CSsemi.CSemiExp semi)
        {
            Repository repo_ = Repository.getInstance();
            int index = semi.Contains("(");
            int endindex = semi.Contains(")");
            int loopindex = semi.FindAll(",");
            int currindex = 0;
            bool flag = false;

            if (index != -1 && !isSpecialToken(semi[index - 1]))
            {
                string currclassName = FindParentClass.getClassName(repo_.semi.lineCount);
                currindex = index;
                string functionname = semi[currindex - 1];
                for (int i = 0; i <= loopindex; i++)
                {
                    CSsemi.CSemiExp local = new CSsemi.CSemiExp();
                    local.displayNewLines = false;
                    local.Add(currclassName).Add(functionname).Add(semi[currindex + 1]).Add(semi[currindex + 2]);
                    doActionsForUsing(local);
                    flag = true;
                    currindex = index + 3;
                }
            }
            if (flag)
                return true;

            return false;
        }
    }//end of class
    /////////////////////////////////////////////////////////
    // rule to dectect function definitions

    public class DetectFunction : ARule
    {
        /*Checks whether the input semi expression contains a function
        * If it is present then the action method for this type is called
        */
        public static bool isSpecialToken(string token)
        {
            string[] SpecialToken = { "if", "else", "for", "foreach", "while", "catch", "try", "finally", "using", "switch", "case", "do" };
            foreach (string stoken in SpecialToken)
                if (stoken == token)
                    return true;
            return false;
        }
        public override bool test(CSsemi.CSemiExp semi)
        {
            if (semi[semi.count - 1] != "{")
                return false;

            int index = semi.FindFirst("(");
            if (index > 0 && !isSpecialToken(semi[index - 1]))
            {
                CSsemi.CSemiExp local = new CSsemi.CSemiExp();
                local.Add("function").Add(semi[index - 1]);
                doActions(local);
                return true;
            }
            return false;
        }
    }//end of class
    ////////////////////////////////////
    // rule to dectect scope with braces

    public class DetectScope : ARule
    {
        /*Checks whether the input semi expression contains a scope beginning
        * If it is present then the action method for this type is called
        */
        public override bool test(CSsemi.CSemiExp semi)
        {
            string[] SpecialToken = { "if", "else", "for", "foreach", "while", "catch", "try", "finally", "switch", "do" };
            int indexOfSpecialToken = -1;
            foreach (string stoken in SpecialToken)
            {
                int tempindex = semi.Contains(stoken);
                if (tempindex != -1)
                    indexOfSpecialToken = Math.Max(indexOfSpecialToken, tempindex);

            }
            int indexOfOpenBraces = semi.FindFirst("{");

            if (indexOfSpecialToken != -1 && indexOfOpenBraces != -1)
            {
                CSsemi.CSemiExp local = new CSsemi.CSemiExp();
                // local semiExp with tokens for type and name
                local.displayNewLines = false;
                local.Add("Scope").Add(semi[indexOfSpecialToken]);
                doActions(local);
                return true;
            }

            return false;
        }
    }//end of class
    ////////////////////////////////////
    // rule to dectect scope without braces

    public class DetectScopeWithoutBraces : ARule
    {
        /*Checks whether the input semi expression contains a scope beginning without braces
        * If it is present then the action method for this type is called
        */
        public override bool test(CSsemi.CSemiExp semi)
        {
            string[] SpecialToken = { "if", "else", "for", "foreach", "while", "switch", "do", "break" };
            int indexOfSpecialToken = -1;
            int indexOfOpenBraces = 0;
            CSsemi.CSemiExp local = new CSsemi.CSemiExp();
            bool foundOpenBrace = false;
            foreach (string stoken in SpecialToken)
            {
                indexOfSpecialToken = semi.Contains(stoken);
                indexOfOpenBraces = semi.FindFirst("{");
                if (indexOfSpecialToken != -1 && indexOfOpenBraces == -1)
                {
                    local = new CSsemi.CSemiExp();
                    // local semiExp with tokens for type and name
                    local.displayNewLines = false;
                    local.Add("Scope").Add(semi[indexOfSpecialToken]);
                    doActions(local);
                    foundOpenBrace = false;
                }
            }
            if (foundOpenBrace)
                return true;

            return false;
        }
    }//end of class

    /////////////////////////////////////////////////////////
    // detect entering anonymous scope
    // - expects namespace, class, and function scopes
    //   already handled, so put this rule after those
    public class DetectAnonymousScope : ARule
    {
        /*Checks whether the input semi expression contains anonymous scope
        * If it is present then the action method for this type is called
        */
        public override bool test(CSsemi.CSemiExp semi)
        {
            int index = semi.Contains("{");
            if (index != -1)
            {
                CSsemi.CSemiExp local = new CSsemi.CSemiExp();
                // create local semiExp with tokens for type and name
                local.displayNewLines = false;
                local.Add("control").Add("anonymous");
                doActions(local);
                return true;
            }
            return false;
        }
    }//end of class

    /////////////////////////////////////////////////////////
    // detect leaving scope

    public class DetectLeavingScopeWithoutBraces : ARule
    {
        /*Checks whether the input semi expression contains end of scope without any braces
        * If it is present then the action method for this type is called
        */
        public override bool test(CSsemi.CSemiExp semi)
        {
            string[] SpecialToken = { "if", "else", "for", "foreach", "while", "switch", "case", "do", "break" };
            int index = -1;
            int index2 = 0;
            CSsemi.CSemiExp local = new CSsemi.CSemiExp();
            bool flag = false;
            foreach (string stoken in SpecialToken)
            {
                index = semi.Contains(stoken);
                index2 = semi.FindFirst("{");
                if (index != -1 && index2 == -1)
                {
                    local = new CSsemi.CSemiExp();
                    // local semiExp with tokens for type and name
                    local.displayNewLines = false;
                    local.Add("Scope").Add(semi[index]);
                    doActions(local);
                    flag = false;
                }
            }
            if (flag)
                return true;

            return false;

        }
    }//end of class

    /////////////////////////////////////////////////////////
    // detect leaving scope

    public class DetectLeavingScope : ARule
    {
        /*Checks whether the input semi expression contains the end of scope
        * If it is present then the action method for this type is called
        */
        public override bool test(CSsemi.CSemiExp semi)
        {
            int index = semi.Contains("}");
            if (index != -1)
            {
                doActions(semi);
                return true;
            }
            return false;
        }
    }//end of class

    /*This class represents the main object which constructs the parser object 
     * The parser object contains all the rules and actions which is applied
     * for each of the semi expression.
     * This class is used to find different types and the complexity count for functions
    */
    public class BuildCodeAnalyzer
    {
        static Repository repo = new Repository();

        //constructor in which all the member variables are initialised
        public BuildCodeAnalyzer(CSsemi.CSemiExp semi)
        {
            repo.semi = semi;
        }

        /*creates the parser object and adds all the rules used to find the types like
         * class/struct/namespace/function etc
        */
        public virtual Parser build()
        {
            Parser parser = new Parser();

            // decide what to show
            AAction.displaySemi = true;
            AAction.displayStack = false;  // this is default so redundant

            // action used for namespaces, classes, and functions
            PushStack push = new PushStack(repo);

            // capture namespace info
            DetectNamespace detectNS = new DetectNamespace();
            detectNS.add(push);
            parser.add(detectNS);

            // capture class info
            DetectClass detectCl = new DetectClass();
            detectCl.add(push);
            parser.add(detectCl);

            // capture function info
            DetectFunction detectFN = new DetectFunction();
            detectFN.add(push);
            parser.add(detectFN);

            // used to find the scopes -  "if", "for", "foreach", "while", "catch", "try", "switch"  
            DetectScope detectScop = new DetectScope();
            detectScop.add(push);
            parser.add(detectScop);

            // used to find the scopes without braces-  "if", "for", "foreach", "while", "catch", "try", "switch"
            DetectScopeWithoutBraces detectScopWB = new DetectScopeWithoutBraces();
            detectScopWB.add(push);
            parser.add(detectScopWB);

            // used to find entering anonymous scopes, e.g., if, while, etc.
            DetectAnonymousScope anon = new DetectAnonymousScope();
            anon.add(push);
            parser.add(anon);

            // used to find leaving scopes without braces
            DetectLeavingScopeWithoutBraces leaveWB = new DetectLeavingScopeWithoutBraces();
            PopStack popWB = new PopStack(repo);
            leaveWB.add(popWB);
            parser.add(leaveWB);

            // used to find leaving scopes
            DetectLeavingScope leave = new DetectLeavingScope();
            PopStack pop = new PopStack(repo);
            leave.add(pop);
            parser.add(leave);

            // parser configured
            return parser;
        }
    }//end of class

    /*This class represents the main object which constructs the parser object 
     * The parser object contains all the rules and actions which is applied
     * for each of the semi expression.
     * This class is used to find the relationships such as inheritance/aggregation
     * /composition/using
    */
    public class BuildCodeAnalyzerForRelationshipTypes
    {
        Repository repo = Repository.getInstance();

        //constructor in which all the member variables are initialised
        public BuildCodeAnalyzerForRelationshipTypes(CSsemi.CSemiExp semi)
        {
            repo.semi = semi;
        }

        /*creates the parser object and adds all the rules used to find the relationships like
         * inheritance/aggregation/composition/using
        */
        public virtual Parser build()
        {
            Parser parser = new Parser();

            // decide what to show
            AAction.displaySemi = true;
            AAction.displayStack = false;  // this is default so redundant

            // action used for namespaces, classes, and functions
            PushStack push = new PushStack(repo);

            // Find inheritance info
            DetectInheritance detectIn = new DetectInheritance();
            detectIn.add(push);
            parser.add(detectIn);

            // Find aggregated info
            DetectAggregation detectAg = new DetectAggregation();
            detectAg.add(push);
            parser.add(detectAg);

            // Find composition info
            DetectComposition detectCs = new DetectComposition();
            detectCs.add(push);
            parser.add(detectCs);

            // Find using info
            DetectUsing detectUs = new DetectUsing();
            detectUs.add(push);
            parser.add(detectUs);

            // parser configured
            return parser;
        }
    }//end of class


    /* This method is used the class which contains aggregation/composition/using relationship.
     * In the rule when we detect any of the above mentioned relationship then we need to find
     * the parent class which contains these relationship types.
     * Eg: class A 
     * {
     *   B obj = new B();
     * }
     * In this case the method returns A as the parent
     */ 
    public static class FindParentClass
    {
        //Returns the parent class which contains aggregation/composition/using relationship
        static public string getClassName(int lineno)
        {
            string classname = "";
            Repository repo_ = Repository.getInstance();
            Elem elem = new Elem();
            int begin = 0;

            for (int i = 0; i < repo_.locations.Count; i++)
            {
                elem = repo_.locations[i];
                if (elem.type == "class" && (elem.begin!=0 && elem.end!=0))
                {
                    if (elem.begin > begin && elem.begin < lineno && elem.end > lineno)
                    {
                        begin = elem.begin;
                        classname = elem.name;                       
                    }
                }
            }
            
            return classname;
        }
    }//end of class
} // end of namespace

