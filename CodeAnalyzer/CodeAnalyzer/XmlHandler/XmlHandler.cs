////////////////////////////////////////////////////////////////////////////////////////
// Xmlhandler.cs - Used for creating an xml file which contains the output            //
// Language:    C#, 2014, .Net Framework 4.5                                          //
// Platform:    Win8                                                                  //
// Application: Code Analyzer Tool to find type relationship and function complexity  //
// Author:      Ravi Nagendra , Syracuse University                                   //
////////////////////////////////////////////////////////////////////////////////////////
/*
 * Package Operations
 * ==================
 * This package is responsible for generating the xml which contains output for different analysis.
 * Depending on the input options, three types of results are generated.
 * The first result generates list of various type details in each file.
 * The second result generates function size and complexities for each function in all files
 * The third result generates relationship analysis for inheritance,aggregation,composition and using type.
 * 
 */
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Linq;
using System.IO;
using System.Xml.Serialization;
using CodeAnalysis;

namespace CodeAnalyzer
{
    public class XmlDisplay
    {
        XDocument xmlDocument;
        XComment xmlDocumentComment;
        XElement xmRootElement;

        //constructor in which all the member variables are initialised     
        public XmlDisplay()
        {
            try
            {
                xmlDocument = new XDocument();
                xmlDocument.Declaration = new XDeclaration("1.0", "utf-8", "yes");
                xmlDocumentComment = new XComment("CODE ANALYSER TOOL FOR FINDING TYPE RELATIONSHIPS AND FUNCTION COMPLEXITIES");
                xmRootElement = new XElement("CODEANALYSISSUMMARY");
            }
            catch
            {
                Console.WriteLine("Error occurred while generating the xml file");
            }
        }

        //method used to save the xml file
        public void saveXml()
        {
            xmlDocument.Save("CodeAnalyzer.xml");
            Console.WriteLine("The output xml file is generated in the path {0}", System.IO.Directory.GetCurrentDirectory() + "\\CodeAnalyzer.xml");
        }

        // This method is called to generate the details of each type in the each of the input files        
        public void generateXmlTypesDetails()
        {
            try
            {
                Repository rep = Repository.getInstance();
                List<Elem> repoTable = rep.locations;
                xmlDocument.Add(xmlDocumentComment);
                xmlDocument.Add(xmRootElement);

                if (repoTable.Count == 0)
                {
                    Console.WriteLine("The input set doesn't have any types");
                    return;
                }

                XElement firstLevelChild = new XElement("TYPESDETAILSFOREACHFILE");
                XElement secondLevelChild = null;
                xmRootElement.Add(firstLevelChild);

                foreach (Elem elem in repoTable)
                {

                    if (elem.begin == 0 && elem.end == 0 && elem.filepath != "")
                    {
                        secondLevelChild = new XElement("FilenameName", elem.filepath);
                        xmRootElement.Add(secondLevelChild);
                    }

                    else if (elem.type.Equals("namespace") || elem.type.Equals("class")
                        || elem.type.Equals("interface") || elem.type.Equals("function"))
                    {
                        if (secondLevelChild != null)
                        {
                            XElement typeDetailsChild = new XElement("TYPEDETAILS");
                            secondLevelChild.Add(typeDetailsChild);
                            XElement typeChild = new XElement("Type", elem.type);
                            secondLevelChild.Add(typeChild);
                            XElement nameChild = new XElement("Name", elem.name);
                            secondLevelChild.Add(nameChild);
                            XElement beginChild = new XElement("Begin", elem.begin);
                            secondLevelChild.Add(beginChild);
                            XElement endChild = new XElement("End", elem.end);
                            secondLevelChild.Add(endChild);
                        }
                    }
                }
            }
            catch (Exception)
            {
                Console.WriteLine("Error occurred while generating the xml file");
            }
        }

        // This method is called to generate the function size and complexity details of each file              
        public void generateXmlComplexityDetails()
        {
            try
            {
                Repository rep = Repository.getInstance();
                List<Elem> repoTable = rep.locations;

                if (repoTable.Count == 0)
                {
                    Console.WriteLine("The input set doesn't have any functions");
                    return;
                }

                XElement firstLevelChild = new XElement("FUNCTIONSIZEANDCOMPLEXITYDETAILSFOREACHFILE");
                XElement secondLevelChild = null;
                xmRootElement.Add(firstLevelChild);

                foreach (Elem elem in repoTable)
                {
                    if (elem.begin == 0 && elem.end == 0 && elem.filepath != "")
                    {
                        secondLevelChild = new XElement("FilenameName", elem.filepath);
                        xmRootElement.Add(secondLevelChild);
                    }

                    else if (elem.type.Equals("function"))
                    {
                        if (secondLevelChild != null)
                        {
                            XElement typeDetailsChild = new XElement("FUNCTIONDETAILS");
                            secondLevelChild.Add(typeDetailsChild);
                            XElement nameChild = new XElement("Name", elem.name);
                            secondLevelChild.Add(nameChild);
                            XElement sizeChild = new XElement("Size", (elem.end - elem.begin + 1));
                            secondLevelChild.Add(sizeChild);
                            XElement scopeCountChild = new XElement("ScopeCount", elem.scopecount);
                            secondLevelChild.Add(scopeCountChild);
                        }
                    }
                }                                
            }
            catch (Exception)
            {
                Console.WriteLine("Error occurred while generating xml");
            }
        }

        // This method is called to generate the type relationships(inheritance,aggregation,composition,using) in the input set       
        public void generateRelationshipsDetails()
        {
            try
            {
                Repository rep = Repository.getInstance();               
                generateInheritanceTable(rep);
                generateAggergationTable(rep);
                generateCompositionTable(rep);
                generateUsingTable(rep);
            }
            catch (Exception)
            {
                Console.WriteLine("Error occurred while displaying relationships");
            }
        }

        // This method generates all the inheritance type relationships in the input data set
        public void generateInheritanceTable(Repository rep)
        {
            List<InheritanceElement> inheritancetable = rep.inheritancedataList;
            
            if (inheritancetable.Count == 0)
            {
                XElement firstLevelChildForNoData = new XElement("NOINHERITANCERELATION");
                xmRootElement.Add(firstLevelChildForNoData);
                return;
            }

            XElement firstLevelChild = new XElement("INHERITANCEDETAILS");
            XElement secondLevelChild = null;
            
            foreach (InheritanceElement ie in inheritancetable)
            {
                foreach (string child in ie.children)
                {
                    secondLevelChild = new XElement("Parent",ie.parent);
                    firstLevelChild.Add(secondLevelChild);
                    secondLevelChild = new XElement("inherits","INHERITS");
                    firstLevelChild.Add(secondLevelChild);
                    secondLevelChild = new XElement("Child",child);
                    firstLevelChild.Add(secondLevelChild);
                }
            }
            xmRootElement.Add(firstLevelChild);
            Console.WriteLine();
        }

        // This method displays all the aggregation type relationships in the input data set
        public void generateAggergationTable(Repository rep)
        {
            List<AggregationElement> aggregatedtable = rep.aggregationdataList;     
            
            if (aggregatedtable.Count == 0)
            {
                XElement firstLevelChildForNoData = new XElement("NOAGGREGATIONRELATION");
                xmRootElement.Add(firstLevelChildForNoData);
                return;
            }

            XElement firstLevelChild = new XElement("AGGREGATIONDETAILS");
            XElement secondLevelChild = null;                      

            foreach (AggregationElement ae in aggregatedtable)
            {
                foreach (string agg in ae.aggregated)
                {
                    secondLevelChild = new XElement("Aggregator", ae.aggregator);
                    firstLevelChild.Add(secondLevelChild);
                    secondLevelChild = new XElement("aggregates","AGGREGATES");
                    firstLevelChild.Add(secondLevelChild);
                    secondLevelChild = new XElement("Child", agg);
                    firstLevelChild.Add(secondLevelChild);
                    secondLevelChild = new XElement("ofType", "WHICHISOFTYPE");
                    firstLevelChild.Add(secondLevelChild);
                    secondLevelChild = new XElement("Type", ae.type);
                    firstLevelChild.Add(secondLevelChild);
                }
            }
            xmRootElement.Add(firstLevelChild); 
        }

        // This method displays all the composition type relationships in the input data set       
        public void generateCompositionTable(Repository rep)
        {
            List<CompositionElement> compositiontable = rep.compositiondataList;

            if (compositiontable.Count == 0)
            {
                XElement firstLevelChildForNoData = new XElement("NOCOMPOSITION");
                xmRootElement.Add(firstLevelChildForNoData);
                return;
            }

            XElement firstLevelChild = new XElement("COMPOSITIONDETAILS");
            XElement secondLevelChild = null;            

            foreach (CompositionElement ce in compositiontable)
            {
                foreach (string comp in ce.composedelement)
                {
                    secondLevelChild = new XElement("Class", ce.compositor);
                    firstLevelChild.Add(secondLevelChild);
                    secondLevelChild = new XElement("composes", "COMPOSES");
                    firstLevelChild.Add(secondLevelChild);
                    secondLevelChild = new XElement("element", comp.ToString());
                    firstLevelChild.Add(secondLevelChild);
                    secondLevelChild = new XElement("ofType", "WHICHISOFTYPE");
                    firstLevelChild.Add(secondLevelChild);
                    secondLevelChild = new XElement("Type", ce.type);
                    firstLevelChild.Add(secondLevelChild);
                }
            }
            xmRootElement.Add(firstLevelChild);
        }

         // This method displays all the using type relationships in the input data set       
        public void generateUsingTable(Repository rep)
        {
            List<UsingElement> usingtable = rep.usingdataList;
            XElement firstLevelChild = new XElement("USINGDETAILS");
            XElement secondLevelChild = null;

            if (usingtable.Count == 0)
            {
                XElement firstLevelChildForNoData = new XElement("NOUSINGRELATION");
                xmRootElement.Add(firstLevelChildForNoData);
                return;
            } 

            foreach (UsingElement ue in usingtable)
            {
                foreach (TypeDetails elt in ue.typeslist)
                {
                    secondLevelChild = new XElement("Class", ue.parent);
                    firstLevelChild.Add(secondLevelChild);
                    secondLevelChild = new XElement("uses", "uses");
                    firstLevelChild.Add(secondLevelChild);
                    secondLevelChild = new XElement("element", elt.usedtypename);
                    firstLevelChild.Add(secondLevelChild);
                    secondLevelChild = new XElement("ofType", "WHICHISOFTYPE");
                    firstLevelChild.Add(secondLevelChild);
                    secondLevelChild = new XElement("Type", elt.type);
                    firstLevelChild.Add(secondLevelChild);
                    secondLevelChild = new XElement("function", "GIVENASAPARAMETERINFUNCTION");
                    firstLevelChild.Add(secondLevelChild);
                    secondLevelChild = new XElement("function", ue.parentfunction);
                    firstLevelChild.Add(secondLevelChild);
                }
            }
            xmRootElement.Add(firstLevelChild);
        }
        
#if(TEST_XML)
        public class TestXml
        {
            //Test Stub
            public static void Main(String[] args)
            {
                XmlDisplay xmlDisplayForTestStub = new XmlDisplay();
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

                    xmlDisplayForTestStub.generateXmlTypesDetails();
                    xmlDisplayForTestStub.generateXmlComplexityDetails();
                    xmlDisplayForTestStub.generateRelationshipsDetails();
                }
                catch
                {
                    Console.WriteLine("Error occured during xml generation. Check the input parameters");
                }
            }
        }
#endif
    }//end of class
}// end of namespace