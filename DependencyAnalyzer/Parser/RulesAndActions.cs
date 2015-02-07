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
        //The type of the element (class,struct,interface)
        public string type { get; set; }
        
        //Name of the type
        public string name { get; set; }

        //Begin line number of this type in the package
        public int begin { get; set; }

        //Ending line number of this type in the package
        public int end { get; set; }
        public int scopecount { get; set; }

        //Name of package which contains this type
        public string packageName { get; set; }

        //Name of the server which contains this type
        public string serverName { get; set; }

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
            temp.Append("}");
            return temp.ToString();
        }
    }

    /// <summary>
    /// This holds the child details whenever a type dependency 
    /// is found. It gives the name of the type and also the 
    /// package which contains this type
    /// </summary>
    public class ChildTypeDetails
    {
        //Name of the type
        string name;

        //Package name which contains this type
        string filename;

        //Get and Set methods for Name
        public string Name
        {
            get { return name; }
            set { name = value; }
        }

        //Get and Set methods for filename
        public string Filename
        {
            get { return filename; }
            set { filename = value; }
        }

    }
    /*This class is used to represent the inheritance data relationships
     * It contains the parent and list of all its children which inherits 
     * the parent class
     * */
    public class InheritanceElement
    {
        //Name of parent class which is inherited by other classes
        public string parent { get; set; }

        //List of class which inherits the parent class
        public ArrayList children { get; set; }

        //Number of childern for this parent
        public int childcount { get; set; }

        //Name of the package which contains this type
        private string parentPackageName;

        //Get and set methods for parent packagename
        public string ParentPackageName
        {
            get { return parentPackageName; }
            set { parentPackageName = value; }
        }

        //List of children and its details
        private List<ChildTypeDetails> childDetails;

        //Get and Set methods for childdetails
        public List<ChildTypeDetails> ChildDetails
        {
            get { return childDetails; }
            set { childDetails = value; }
        }

        /*constructor where all the member variables are initialised        
         *
         */
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
        //Name of the parent which aggregates other classes
        public string aggregator { get; set; }

        //List of classes which is aggregate by the aggregtor type
        public ArrayList aggregated { get; set; }

        //This indicates whether parent is a class,struct,interface
        public string type { get; set; }

        //Package file which contains this element
        private string parentFileName;

        //Set and Get metods for parentfilename
        public string ParentFileName
        {
            get { return parentFileName; }
            set { parentFileName = value; }
        }

        //Indicates details of the children which is aggregated by thie element
        private List<ChildTypeDetails> childDetails;

        //Set and Get method for childdetails
        public List<ChildTypeDetails> ChildDetails
        {
            get { return childDetails; }
            set { childDetails = value; }
        }

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
        public ArrayList composedelement
        { get; set; }
        public string type { get; set; }

        //Package name which contains this element
        private string parentFileName;

        //Get and set method for parentfilename
        public string ParentFileName
        {
            get { return parentFileName; }
            set { parentFileName = value; }
        }

        //Details of the children present for this element
        private List<ChildTypeDetails> childDetails;

        //Get and set method for childdetails
        public List<ChildTypeDetails> ChildDetails
        {
            get { return childDetails; }
            set { childDetails = value; }
        }

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
        //indicates the name of the type which is used by using element        
        public string usedtypename { get; set; }
        public string type { get; set; }
    }//end of class


    /*This class is used to represent the Using data relationships
     * It contains the parent and list of all the elements which is 
     * being used by the parent
     */
    public class UsingElement
    {
        //Name of the parent in which using relation is present
        public string parent { get; set; }

        //List of types present in the using relation
        private List<TypeDetails> typeslist;

        //Set and Get method for typeslist
        public List<TypeDetails> Typeslist
        {
            get { return typeslist; }
            set { typeslist = value; }
        }

        //Details of the childrent type of this element
        private List<ChildTypeDetails> childDetails;

        //Package name which contains this element
        private string parentFileName;

        //Set and Get method for parentfilename
        public string ParentFileName
        {
            get { return parentFileName; }
            set { parentFileName = value; }
        }

        //Set and get method for childdetails
        public List<ChildTypeDetails> ChildDetails
        {
            get { return childDetails; }
            set { childDetails = value; }
        }

        //Parent function which contains the used element
        public string parentfunction { get; set; }

        //constructor in which all the member variables are initialised
        public UsingElement()
        {
            //empty constructor
        }
    }//end of class

    /// <summary>
    /// This class is used to represent the package dependency relationships
    /// It contains the parent and list of all the child package which has a 
    /// dependency on the parent
    /// </summary>
    public class PackageDependencyElement
    {
        //Set and get method for parentpackage
        public string parentPackage { get; set; }

        //Set and Get method for parent servername
        public string parentServerName { get; set; }

        //Set and Get method for childpackagename
        public string childPackageName { get; set; }

        //set and get method for child servername
        public string childServerName { get; set; }

        //Set and Get method for childcount
        public int childcount { get; set; }

        //Set and get method for typeName1
        public string typeName1 { get; set; }

        ////Set and get method for typeName2
        public string typeName2 { get; set; }

        //Set and get method for relationship
        public string relationship { get; set; }

        /// <summary>
        /// Constuctor where the member variables are initialised
        /// </summary>
        public PackageDependencyElement()
        {
            childcount = 0;
        }

        /// <summary>
        /// used to check whether two package elements are same
        /// </summary>
        /// <param name="temp"></param>
        /// <returns></returns>
        public bool isEqual(PackageDependencyElement temp)
        {
            if (this.parentServerName == temp.parentServerName
                && this.parentPackage == temp.parentPackage
                && this.childPackageName == temp.childPackageName
                && this.childServerName == temp.childServerName)
                return true;
            return false;
        }
    }

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
        List<PackageDependencyElement> packagedataList_ = new List<PackageDependencyElement>();
        static Repository instance;
        string currentFileName;

        public string CurrentFileName
        {
            get { return currentFileName; }
            set { currentFileName = value; }
        }

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

        /// <summary>
        /// Whenever there is request from new client to start the analysis,
        /// the server data has to be cleared. This is required, because the 
        /// server may have stored the analysis data for a previous request.
        /// So every time when there is a request, the contents of various
        /// list has to be cleared.
        /// </summary>
        public void setInstance()
        {
            locations_ = new List<Elem>();
            inheritancedataList_ = new List<InheritanceElement>();
            aggregationdataList_ = new List<AggregationElement>();
            compositiondataList_ = new List<CompositionElement>();
            usingdataList_ = new List<UsingElement>();
            packagedataList_ = new List<PackageDependencyElement>();
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
            set { locations_ = value; }
            get { return locations_; }
        }

        //The list which contains the data of all the inheritance types used in the input set
        public List<InheritanceElement> inheritancedataList
        {
            set { inheritancedataList_ = value; }
            get { return inheritancedataList_; }
        }

        //The list which contains the data of all the aggregation types used in the input set
        public List<AggregationElement> aggregationdataList
        {
            set { aggregationdataList_ = value; }
            get { return aggregationdataList_; }
        }

        //The list which contains the data of all the composition types used in the input set
        public List<CompositionElement> compositiondataList
        {
            set { compositiondataList_ = value; }
            get { return compositiondataList_; }
        }

        //The list which contains the data of all the using types used in the input set
        public List<UsingElement> usingdataList
        {
            set { usingdataList_ = value; }
            get { return usingdataList_; }
        }

        public List<PackageDependencyElement> packagedata
        {
            set { packagedataList_ = value; }
            get { return packagedataList_; }
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

        /// <summary>
        /// Used to find the index of the childtype in the
        /// global repo.
        /// </summary>
        /// <param name="childName">Name of the child for which index has to be found</param>
        /// <returns>Index of the repo list in which the child is present</returns>
        private int findChildTypeIndex(string childName)
        {
            for (int i = 0; i < repo_.locations.Count; i++)
            {
                Elem currentElement = repo_.locations[i];

                if (currentElement.name == childName)
                {
                    Console.WriteLine("Found the child index");
                    return i;
                }
            }
            return -1;
        }
        
        /// <summary>
        /// Used to find the index of the childtype in the
        /// inheritance list.
        /// </summary>
        /// <param name="childName">Name of the child for which index has to be found</param>
        /// <param name="beginLineCount">Beginning line number of the function where child is present</param>
        /// <returns>Index of the repo list in which the child is present</returns>
        private int findChildTypeIndexInheritance(string childName, int beginLineCount)
        {
            for (int i = 0; i < repo_.locations.Count; i++)
            {
                Elem currentElement = repo_.locations[i];

                if (currentElement.name == childName && currentElement.begin == beginLineCount)
                {
                    Console.WriteLine("Found the child index");
                    return i;
                }
            }
            return -1;
        }


        /// <summary>
        /// This method is used to create the package dependencies for all the input file set.
        /// The working algorithm for this method is
        /// 1. Initially the relationship list for inheritance,aggreation,using and composition
        /// types are generated.
        /// 2. Once these lists are created, then this method finds the parent and child class name
        /// 3. There is a field to identify the package name in which these parent and child class 
        /// are present
        /// 4. If the child and parent are in different packages then a new entry is added the package
        /// dependency list
        /// </summary>
        /// <param name="relationName">Relation type: inheritance,aggregation,composition,using</param>
        /// <param name="semi">Semi expression which contains child and the parent</param>
        /// <param name="parentServerIndex">The index which is used to find the parent server
        /// name</param>
        private void createPackageAnalysisList(string relationName, CSsemi.CSemiExp semi, int parentServerIndex)
        {
            try
            {
                PackageDependencyElement packageelem = new PackageDependencyElement();
                int parentServerNameIndex = 0;
                int childServerNameIndex = 0;
                int childTypeBeginLine = repo_.semi.lineCount - 1;
                int childIndexInRepo = 0;
                if (relationName.Equals("Inheritance"))
                    childIndexInRepo = findChildTypeIndexInheritance(semi[0], childTypeBeginLine);
                else
                    childIndexInRepo = findChildTypeIndex(semi[0]);
                parentServerNameIndex = findServerIndex(parentServerIndex);
                childServerNameIndex = findServerIndex(childIndexInRepo);
                string parentPackage = repo_.locations[parentServerNameIndex].packageName;
                string childPackage = repo_.locations[childServerNameIndex].packageName;
                packageelem.parentPackage = parentPackage;
                packageelem.parentServerName = repo_.locations[parentServerNameIndex].serverName;

                if (!parentPackage.Equals(childPackage))
                {
                    packageelem.typeName1 = semi[1];
                    packageelem.typeName2 = semi[0];
                    packageelem.relationship = relationName;
                    packageelem.childPackageName = childPackage;
                    packageelem.childServerName = repo_.locations[childServerNameIndex].serverName;

                    if (repo_.packagedata.Count > 0)
                        for (int i = 0; i < repo_.packagedata.Count; )
                        {
                            PackageDependencyElement curr = repo_.packagedata[i];
                            if (packageelem.isEqual(curr))
                                break;
                            else
                            {
                                repo_.packagedata.Add(packageelem);
                                break;
                            }
                        }
                    else
                        repo_.packagedata.Add(packageelem);
                }
            }
            catch (Exception ex) { Console.WriteLine(ex.Message); }
        }

        /// <summary>
        /// Used to find the location of child package name in the repo list.
        /// This is required to find the parent package name which is then
        /// checked with the child package name. If both are in same packages
        /// then package dependeny does not exist.
        /// </summary>
        /// <param name="childName">Name of the child type</param>
        /// <param name="beginLineCount">Begin line of the child type</param>
        /// <returns>The index in the list where the child is present</returns>
        public int findChildTypeIndex(string childName, int beginLineCount)
        {
            try
            {
                for (int i = 0; i < repo_.locations.Count; i++)
                {
                    Elem currentElement = repo_.locations[i];

                    if (currentElement.name == childName && currentElement.begin == beginLineCount)
                    {
                        Console.WriteLine("Found the child index");
                        return i;
                    }
                }
                return -1;
            }
            catch (Exception)
            {
                Console.WriteLine("Cannot find the child index");
                return -1;
            }
        }


        /// <summary>
        /// Used to find the server in which the current type is present.
        /// This is required to check whether the packages are found is same 
        /// server or different server.
        /// </summary>
        /// <param name="currentIndex">Index of the type in the repo list 
        /// for which the server name has to be found</param>
        /// <returns></returns>
        public int findServerIndex(int currentIndex)
        {
            try
            {
                for (int i = currentIndex; i >= 0; i--)
                {
                    if (repo_.locations[i].begin == 0 && repo_.locations[i].end == 0 && repo_.locations[i].type.CompareTo("dummy") == 0)
                    {
                        return i;
                    }
                }
                return -1;
            }
            catch (Exception)
            {
                Console.Write("\n\n cannot find the server index");
                return -1;
            }
        }

        /// <summary>
        /// The action performed for each of the input file
        /// to determine various dependency type
        /// </summary>
        /// <param name="semi"></param>
        public override void doAction(CSsemi.CSemiExp semi)
        {
            Elem elem = new Elem();
            elem.type = semi[0];  // expects type
            elem.name = semi[1];  // expects name
            elem.begin = repo_.semi.lineCount - 1;
            elem.end = 0;
            elem.packageName = repo_.CurrentFileName;
            repo_.stack.push(elem);
            repo_.locations.Add(elem);

            if (AAction.displaySemi)
            {
                Console.Write("\n  line# {0,-5}", repo_.semi.lineCount - 1);
                Console.Write("entering ");
                string indent = new string(' ', 2 * repo_.stack.count);
                Console.Write("{0}", indent);
                this.display(semi); // defined in abstract action
            }
            if (AAction.displayStack)
                repo_.stack.display();
        }

        /// <summary>
        /// Method used to find the parent for any relation type
        /// in the repository
        /// </summary>
        /// <param name="parentName">Name of the parent for which index has to be found</param>
        /// <returns>Returns the index in the repo where the parent is present</returns>
        private int findParentIndexinRepo(String parentName)
        {
            int index = -1;

            Elem elem = null;
            for (int i = 0; i < repo_.locations.Count; i++)
            {
                elem = repo_.locations[i];
                if (elem.type == "class" && parentName == elem.name)
                {
                    return i;
                }
            }
            return index;
        }

        /// <summary>
        /// Method used to find the child for any relation type
        /// in the repository
        /// </summary>
        /// <param name="parentName">Name of the child for which index has to be found</param>
        /// <returns>Returns the index in the repo where the child is present</returns>
        private int findChileTypeIndexinRepo(String childName)
        {
            int index = -1;
            Elem elem = null;
            for (int i = 0; i < repo_.locations.Count; i++)
            {
                elem = repo_.locations[i];
                if (elem.type == "class" || elem.type == "struct" && childName == elem.name)
                {
                    return i;
                }
            }
            return index;
        }

        /// <summary>
        /// Used to create the child details element for inheritance relation type
        /// </summary>
        /// <param name="semi">Contains the semi expression of the inheritance type</param>
        /// <returns>elment which contains the child details</returns>
        private ChildTypeDetails getChildDetailsElementForInheritance(CSsemi.CSemiExp semi)
        {
            ChildTypeDetails currentchildelement = new ChildTypeDetails();
            string childPackage = "";
            int childTypeBeginLine = repo_.semi.lineCount - 1;
            int childIndexInRepo = 0;
            int childServerNameIndex = 0;

            childIndexInRepo = findChildTypeIndex(semi[0], childTypeBeginLine);
            childServerNameIndex = findServerIndex(childIndexInRepo);
            childPackage = repo_.locations[childServerNameIndex].packageName;
            currentchildelement.Name = semi[0];
            currentchildelement.Filename = childPackage;
            return currentchildelement;
        }

        /// <summary>
        /// Used to create the child details element for using relation type
        /// </summary>
        /// <param name="semi">Contains the semi expression of the using type</param>
        /// <returns>elment which contains the child details</returns>
        private ChildTypeDetails getUsingChildDetailsElement(CSsemi.CSemiExp semi)
        {
            ChildTypeDetails currentchildelement = new ChildTypeDetails();
            int childIndex = 0;
            currentchildelement.Name = semi[2];
            childIndex = findChileTypeIndexinRepo(currentchildelement.Name);
            currentchildelement.Filename = repo_.locations[childIndex].packageName;

            return currentchildelement;
        }

        /// <summary>
        /// Used to create the child details element for using different type
        /// </summary>
        /// <param name="semi">Contains the semi expression of the different type</param>
        /// <returns>elment which contains the child details</returns>
        private ChildTypeDetails getChildDetailsElement(CSsemi.CSemiExp semi)
        {
            ChildTypeDetails currentchildelement = new ChildTypeDetails();
            int childIndex = 0;
            currentchildelement.Name = semi[0];
            childIndex = findChileTypeIndexinRepo(currentchildelement.Name);
            currentchildelement.Filename = repo_.locations[childIndex].packageName;

            return currentchildelement;
        }

        //The action performed whenever a inheritance type is detected in the current semi expression
        public override void doActionForInheritance(CSsemi.CSemiExp semi)
        {
            InheritanceElement inheritanceelem = new InheritanceElement();
            PackageDependencyElement packageelem = new PackageDependencyElement();
            Elem elem = new Elem();
            bool existingParent = false;
            ChildTypeDetails childelement = null;

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
                            if (inheritanceelem.ChildDetails == null)
                                inheritanceelem.ChildDetails = new List<ChildTypeDetails>();
                            childelement = getChildDetailsElementForInheritance(semi);
                            inheritanceelem.ChildDetails.Add(childelement);
                            repo_.inheritancedataList.Remove(inheritanceelem);
                            repo_.inheritancedataList.Insert(index, inheritanceelem);
                            createPackageAnalysisList("Inheritance", semi, i);
                        }
                    }
                    if (!existingParent)
                    {
                        inheritanceelem = new InheritanceElement();
                        inheritanceelem.children.Add(semi[0]);
                        inheritanceelem.parent = semi[1];
                        int parentIndex = findParentIndexinRepo(semi[1]);
                        inheritanceelem.ParentPackageName = repo_.locations[parentIndex].packageName;
                        inheritanceelem.childcount++;
                        if (inheritanceelem.ChildDetails == null)
                            inheritanceelem.ChildDetails = new List<ChildTypeDetails>();
                        childelement = getChildDetailsElementForInheritance(semi);
                        inheritanceelem.ChildDetails.Add(childelement);
                        repo_.inheritancedataList.Add(inheritanceelem);
                        createPackageAnalysisList("Inheritance", semi, i);
                    }
                }
            }
        }

        //The action performed whenever a aggregation type is detected in the current semi expression
        public override void doActionForAggregation(CSsemi.CSemiExp semi)
        {
            AggregationElement aggregationelem = new AggregationElement();
            Elem elem = new Elem();
            int parentIndex = 0;
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
                            if (aggregationelem.ChildDetails == null)
                                aggregationelem.ChildDetails = new List<ChildTypeDetails>();
                            aggregationelem.ChildDetails.Add(getChildDetailsElement(semi));
                            repo_.aggregationdataList.Remove(aggregationelem);
                            repo_.aggregationdataList.Insert(index, aggregationelem);
                            createPackageAnalysisList("Aggregation", semi, findParentIndexinRepo(semi[2]));
                        }
                    }

                    if (!existingAggregator)
                    {
                        aggregationelem = new AggregationElement();
                        aggregationelem.aggregated.Add(semi[1]);
                        aggregationelem.aggregator = semi[2];
                        aggregationelem.type = semi[0];
                        if (aggregationelem.ChildDetails == null)
                            aggregationelem.ChildDetails = new List<ChildTypeDetails>();
                        aggregationelem.ChildDetails.Add(getChildDetailsElement(semi));
                        parentIndex = findParentIndexinRepo(semi[2]);
                        aggregationelem.ParentFileName = repo_.locations[parentIndex].packageName;
                        repo_.aggregationdataList.Add(aggregationelem);
                        createPackageAnalysisList("Aggregation", semi, findParentIndexinRepo(semi[2]));
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
            int parentIndex = 0;

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
                            if (compositionelem.ChildDetails == null)
                                compositionelem.ChildDetails = new List<ChildTypeDetails>();
                            compositionelem.ChildDetails.Add(getChildDetailsElement(semi));
                            repo_.compositiondataList.Remove(compositionelem);
                            repo_.compositiondataList.Insert(index, compositionelem);
                            createPackageAnalysisList("Composition", semi, findParentIndexinRepo(semi[2]));
                        }
                    }
                    if (!existingCompositor)
                    {
                        compositionelem = new CompositionElement();
                        compositionelem.composedelement.Add(semi[1]);
                        compositionelem.compositor = semi[2];
                        compositionelem.type = semi[0];
                        if (compositionelem.ChildDetails == null)
                            compositionelem.ChildDetails = new List<ChildTypeDetails>();
                        compositionelem.ChildDetails.Add(getChildDetailsElement(semi));
                        parentIndex = findParentIndexinRepo(semi[2]);
                        compositionelem.ParentFileName = repo_.locations[parentIndex].packageName;
                        repo_.compositiondataList.Add(compositionelem);
                        createPackageAnalysisList("Composition", semi, findParentIndexinRepo(semi[2]));
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
            for (int i = 0; i < repo_.locations.Count; i++){
                elem = repo_.locations[i];
                if ((elem.type == "struct" || elem.type == "enum" || elem.type == "class" || elem.type == "interface") && type == elem.name)
                {
                    for (int j = 0; j < repo_.usingdataList.Count; j++){
                        usingelem = repo_.usingdataList[j];
                        if (functionname == usingelem.parentfunction){
                            existingfunction = true;
                            TypeDetails typeDetails = new TypeDetails();
                            usingelem.parent = classname;
                            usingelem.parentfunction = functionname;
                            typeDetails.type = type;
                            typeDetails.usedtypename = typename;
                            usingelem.Typeslist.Add(typeDetails);
                            int index = repo_.usingdataList.IndexOf(usingelem);
                            if (usingelem.ChildDetails == null) { usingelem.ChildDetails = new List<ChildTypeDetails>(); }
                            usingelem.ChildDetails.Add(getUsingChildDetailsElement(semi));
                            repo_.usingdataList.Remove(usingelem);
                            repo_.usingdataList.Insert(index, usingelem);
                            createPackageAnalysisList("Using", semi, i);
                        }
                    }
                    if (!existingfunction){
                        TypeDetails typeDetails = new TypeDetails();
                        usingelem = new UsingElement();
                        usingelem.parent = classname;
                        usingelem.parentfunction = functionname;
                        typeDetails.type = type;
                        typeDetails.usedtypename = typename;
                        if (usingelem.Typeslist == null) { usingelem.Typeslist = new List<TypeDetails>(); }
                        usingelem.Typeslist.Add(typeDetails);
                        if (usingelem.ChildDetails == null) { usingelem.ChildDetails = new List<ChildTypeDetails>(); }
                        usingelem.ChildDetails.Add(getUsingChildDetailsElement(semi));
                        usingelem.ParentFileName = repo_.locations[findParentIndexinRepo(semi[2])].packageName;
                        repo_.usingdataList.Add(usingelem);
                        createPackageAnalysisList("Using", semi, i);
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
            Elem elem;
            try
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
            catch
            {
                Console.Write("popped empty stack on semiExp: ");
                semi.display();
                return;
            }
            CSsemi.CSemiExp local = new CSsemi.CSemiExp();
            local.Add(elem.type).Add(elem.name);

            if (AAction.displaySemi)
            {
                Console.Write("\n  line# {0,-5}", repo_.semi.lineCount);
                Console.Write("leaving  ");
                string indent = new string(' ', 2 * (repo_.stack.count + 1));
                Console.Write("{0}", indent);
                this.display(local); // defined in abstract action
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

    /// <summary>
    /// This method defines the rule which is used to find the inheritance type
    /// </summary>
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
                doActionForInheritance(local);

                int multiInheritCount = semi.FindAll(",");
                while (multiInheritCount > 0)
                {
                    index2 = index2 + 2;
                    local = new CSsemi.CSemiExp();
                    local.displayNewLines = false;
                    local.Add(semi[index + 1]).Add(semi[index2 + 1]);
                    doActionForInheritance(local);
                    multiInheritCount--;
                }

                return true;
            }
            return false;
        }
    }//end of class

    /// <summary>
    /// This method defines the rule which is used to find the aggregation type
    /// </summary>
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

    /// <summary>
    /// This method defines the rule which is used to find the composition type
    /// </summary>
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
                //if (currclassName != "")
                {
                    CSsemi.CSemiExp local = new CSsemi.CSemiExp();
                    local.displayNewLines = false;
                    local.Add(variablecountlist[0]).Add(variablecountlist[1]).Add(currclassName);
                    doActionsForComposition(local);
                    return true;
                }
            }

            return false;
        }
    }//end of class

    /// <summary>
    /// This method defines the rule which is used to find the using type
    /// </summary>
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
                if (elem.type == "class" && (elem.begin != 0 && elem.end != 0))
                {
                    if (elem.begin >= begin && elem.begin < lineno && elem.end > lineno)
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

