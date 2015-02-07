////////////////////////////////////////////////////////////////////////////////////////
// Display.cs - Used for displaying console output                                    //
// Language:    C#, 2014, .Net Framework 4.5                                          //
// Platform:    Win8                                                                  //
// Application: Code Analyzer Tool to find type relationship and function complexity  //
// Author:      Ravi Nagendra , Syracuse University                                   //
////////////////////////////////////////////////////////////////////////////////////////
/*
 * Package Operations
 * ==================
 * This package is responsible for displaying the console output for different analysis.
 * Depending on the input options, three types of results are displayed.
 * The first display contains list of various type details in each file.
 * The second display contains function size and complexities for each function in all files
 * The third display contains relationship analysis for inheritance,aggregation,composition and using type.
 * 
 */

using CodeAnalysis;
using System;
using System.Collections.Generic;


namespace CodeAnalyzer
{
    public class Display
    {
        // This method is called when there are no input parameters in the argument list          
        public void displayNoInput()
        {
            Console.Write("\n  NO INPUT PARAMETERS GIVEN. EXITING THE PROGRAM \n");                
        }
        // This method is called to display the details of each type in the each of the input files 
        public void displayTypesDetails()
        {
            try
            {
                int currentCount = 0;                
                Repository rep = Repository.getInstance();
                List<Elem> table = rep.locations;

                if (table.Count == 0)
                {
                    Console.WriteLine("The input set doesn't have any types");
                    return;
                }
                Console.Write("\n ==============================================================================\n");
                Console.Write("\n  CODE ANALYZER TOOL DEMONSTRATING FUNCTION COMPLEXITIES AND TYPE RELATIONSHIPS \n");
                Console.Write("\n ==============================================================================\n");
               
                Console.Write("\n ==============================\n");
                Console.Write("\n  TYPES DETAILS FOR EACH FILE\n");
                Console.Write("\n ==============================\n");
                foreach (Elem e in table)
                {
                    if (e.begin == 0 && e.end == 0 && e.filepath != "")
                    {
                        Console.WriteLine("\n\n\nThe summary for the file {0,15} is",e.filepath);
                        Console.WriteLine("\n\nType             Name           Begin       End    ");
                        Console.Write("==============================================================");
                    }

                    else if (e.type.Equals("namespace") || e.type.Equals("class") 
                        || e.type.Equals("interface") || e.type.Equals("function"))
                    {
                        Console.Write("\n{0,10} {1,15} {2,8} {3,11}", e.type, e.name, e.begin, e.end);
                    }
                    
                    currentCount++;
                }
                Console.WriteLine();
                Console.WriteLine();
            }
            catch (Exception)
            {
                Console.WriteLine("Error occurred while displaying complexity");
            }
        }

        // This method is called to display the function size and complexity details of each file       
        public void displayComplexityDetails()
        {
            try
            {
                int currentCount = 0;
                int complexityCount = 0;
                int averagecomplexityCount = 0;
                Console.Write("\n ====================================================\n");
                Console.Write("\n  FUNCTION SIZE AND COMPLEXITY DETAILS FOR EACH FILE \n");
                Console.Write("\n ===================================================\n");
                Repository rep = Repository.getInstance();
                List<Elem> table = rep.locations;

                foreach (Elem e in table)
                {
                    if (e.begin == 0 && e.end == 0 && e.filepath != "")
                    {
                        Console.WriteLine("\n\n\nThe function size and complexity details for the file {0,20} is", e.filepath);
                        Console.WriteLine("\n       Name               Size      Complexity");
                        Console.Write("=======================================================================================");
                        currentCount++;
                    }                    
                    else if (e.type.Equals("function"))
                    {
                        Console.Write("\n{0,15} {1,10} {2,10}", e.name, (e.end - e.begin + 1), e.scopecount);
                        complexityCount = complexityCount + e.scopecount;
                    }                    
                }
                averagecomplexityCount = complexityCount / currentCount;
                Console.WriteLine("\n\n\n====================================================\n");
                Console.WriteLine("FUNCTION SUMMARY FOR ALL THEFILES IN THE GIVEN SET");
                Console.Write("\n ===================================================\n");
                Console.Write("\n The total number of functions in the given input set is {0} and average scope count is {1} \n", currentCount, averagecomplexityCount);

                if (averagecomplexityCount > 50 && averagecomplexityCount < 200)
                {
                    Console.WriteLine("Since the average scope count in the given input set is in the range 50-200, user may consider optimising the code");
                }
                else if (averagecomplexityCount > 200)
                {
                    Console.WriteLine("Since the average scope count in the given input set is greater than 2000, user may should optimise the given code");
                }

                Console.WriteLine();
                Console.WriteLine();
            }
            catch (Exception)
            {
                Console.WriteLine("Error occurred while displaying complexity");
            }
        }

        // This method is called to display the type relationships(inheritance,aggregation,composition,using) in the input set     
        public void displayRelationshipsDetails()
        {
            try
            {
                Repository rep = Repository.getInstance();
                Console.WriteLine("\n\n==================================\n");
                Console.WriteLine("RELATIONSHIP SUMMARY FOR THE FILES");
                Console.WriteLine("==================================\n \n");
                displayInheritanceTable(rep);
                displayAggergationTable(rep);
                displayCompositionTable(rep);
                displayUsingTable(rep);
                Console.WriteLine("\n\n\n====END OF ANALYSIS====\n \n");
            }
            catch (Exception)
            {
                Console.WriteLine("Error occurred while displaying relationships");
            }
        }

        // This method displays all the inheritance type relationships in the input data set
        public void displayInheritanceTable(Repository rep)
        {
            List<InheritanceElement> inheritancetable = rep.inheritancedataList;            
            Console.WriteLine("\n\nInheritance Analysis");
            Console.WriteLine("=====================");
            foreach (InheritanceElement ie in inheritancetable)
            {
                foreach (string child in ie.children)
                {
                    Console.WriteLine("{0,10}    inherits {1,20}", child, ie.parent);
                }
                Console.Write("\n");
            }
            Console.WriteLine();
        }

        // This method displays all the aggregation type relationships in the input data set
        public void displayAggergationTable(Repository rep)
        {
            List<AggregationElement> aggregatedtable = rep.aggregationdataList;
            Console.WriteLine("\n\n\n\nAggregation Analysis");
            Console.WriteLine("=======================");
            foreach (AggregationElement ae in aggregatedtable)
            {
                foreach (string agg in ae.aggregated)
                {
                    Console.WriteLine("{0} aggregates {1} which is of the type {2}", ae.aggregator, agg, ae.type);
                }
                Console.Write("\n");
            }
            Console.WriteLine();
        }

        // This method displays all the composition type relationships in the input data set
        public void displayCompositionTable(Repository rep)
        {
            List<CompositionElement> compositiontable = rep.compositiondataList;
            Console.WriteLine("\n\n\n\nComposition Analysis");
            Console.WriteLine("=====================");
            foreach (CompositionElement ce in compositiontable)
            {
                foreach (string comp in ce.composedelement)
                {
                    Console.WriteLine("class {0} composes of {1} which is of the type {2}", ce.compositor, comp.ToString(), ce.type);
                }
                Console.Write("\n");
            }
            Console.WriteLine();
        }

        // This method displays all the using type relationships in the input data set
        public void displayUsingTable(Repository rep)
        {
            List<UsingElement> usingtable = rep.usingdataList;
            Console.WriteLine("\n\n\n\nUsingRelationship Analysis");
            //Console.WriteLine("\nClass Name     in Function         Using(type)");
            Console.Write("========================================\n");

            foreach (UsingElement ue in usingtable)
            {
                foreach (TypeDetails elt in ue.typeslist)
                {
                    Console.WriteLine("\nclass {0} uses {1} which is of the type {2} declared as a parameter in function",
                        ue.parent, elt.usedtypename, elt.type, ue.parentfunction);
                }
                Console.Write("\n");
            }
            Console.WriteLine();
        }
    }//end of class

#if(TEST_DISPLAY)
    public class TestDisplay
    {
        //Test Stub
        public static void Main(String[] args)
        {
            Display displayForTestStub = new Display();
            Repository rep = Repository.getInstance();
            /* If the package is run as stand alone application
              * then add the default values for the member variables
             */ 
            try
            {
                if (rep.locations.Count == 0)
                {
                    Console.WriteLine("The repository is empty. Nothing to display");
                }

                displayForTestStub.displayNoInput();
                displayForTestStub.displayRelationshipsDetails();
                displayForTestStub.displayComplexityDetails();
                displayForTestStub.displayTypesDetails();
            }
            catch
            {
                Console.WriteLine("Error occured during display. Check the input parameters");
            }
        }
    }//end of class
#endif
} // end of namespace
