//////////////////////////////////////////////////////////////////////////////////////////
// XmlGenerator.cs - Used for creating an xml file which contains the output            //
// Language:    C#, 2014, .Net Framework 4.5                                            //
// Platform:    Win8                                                                    //
// Application: Dependency Analyzer Tool to find type dependency and package dependency //
// Author:      Ravi Nagendra , Syracuse University                                     //
//////////////////////////////////////////////////////////////////////////////////////////

/*
 * Module Operations
 * ==================
 * This package is responsible for generating the xml which contains output for different analysis.
 * Depending on the input options, two types of results are generated.
 * The first result generates dependency analysis for the types defined in the files.
 * The second result generates dependency analysis for the packages present in the input file set.
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
using HandCraftedService;
using CodeAnalyzer;
using CodeAnalysis;

namespace CodeAnalyzer
{
    /// <summary>
    /// Used for generating the xml file which contains the result of the 
    /// type and package dependencies
    /// </summary>
    public class XmlDisplay
    {
        //Defines the element for creating the xml file
        XDocument xmlDocument;

        //Comment for the xml file header 
        XComment xmlDocumentComment;

        //Root element to which all other elements are added
        XElement xmRootElement;

        //Repository which contains the result of the analysis
        Repository repo;

        //constructor where all the member variables are initialised     
        public XmlDisplay()
        {
            try
            {
                xmlDocument = new XDocument();
                xmlDocument.Declaration = new XDeclaration("1.0", "utf-8", "yes");
                xmlDocumentComment = new XComment("DEPENDENCY ANALYZER TOOL FOR FINDING TYPE DEPENDENCIES AND PACKAGE DEPENDENCIES");
                xmRootElement = new XElement("DEPENDENCYANALYSISSUMMARY");
                xmlDocument.Add(xmlDocumentComment);
                xmlDocument.Add(xmRootElement);
            }
            catch
            {
                Console.WriteLine("Error occurred while generating the xml file");
            }
        }

        /// <summary>
        /// Used to initialise the repository from which the data is fetched for createing xml file
        /// </summary>
        /// <param name="mergedRepo">The repo which contains the final analysis result</param>
        public void SetMergedRepo(Repository mergedRepo)
        {
            repo = mergedRepo;
        }

        /// <summary>
        /// method used to save the xml file
        /// </summary>
        public String saveXml()
        {
            if (xmlDocument != null)
            {
                xmlDocument.Save("DependencyAnalyzer.xml");
                Console.WriteLine("The output xml file is generated in the path {0}", System.IO.Directory.GetCurrentDirectory() + "\\CodeAnalyzer.xml");
            }
            return xmlDocument.ToString();
        }

        /// <summary>
        ///This method is called to generate the package dependencies across various servers
        /// </summary>
        public void generatePackageDependencyDetails()
        {
            List<PackageDependencyElement> packageDependencyTable = repo.packagedata;
            int itemsCount = 0;
            try
            {
                if (packageDependencyTable.Count == 0)
                {
                    XElement firstLevelChildForNoData = new XElement("NOPACKAGEDEPENDENCIES");
                    xmRootElement.Add(firstLevelChildForNoData);
                    return;
                }
                foreach (PackageDependencyElement ie in packageDependencyTable)
                {
                    // foreach (string child in ie.childrenPackage)
                    if (!ie.childPackageName.Equals(""))
                    {
                        itemsCount++;
                        break;
                    }
                }
                if (itemsCount == 0)
                {
                    XElement firstLevelChildForNoData = new XElement("NOPACKAGEDEPENDENCIES");
                    xmRootElement.Add(firstLevelChildForNoData);
                    return;
                }
                XElement firstLevelChild = new XElement("Package");
                XElement secondLevelChild = null;

                foreach (PackageDependencyElement ie in packageDependencyTable)
                {
                    secondLevelChild = new XElement("ParentPackage", ie.parentPackage);
                    firstLevelChild.Add(secondLevelChild);
                    secondLevelChild = new XElement("ChildPackageName", ie.childPackageName);
                    firstLevelChild.Add(secondLevelChild);
                    xmRootElement.Add(firstLevelChild);
                    firstLevelChild = new XElement("Package");
                }

                Console.WriteLine();
            }
            catch (Exception)
            {
                Console.WriteLine("could not create the package analysis. Please check the input");
            }
        }

        /// <summary>
        /// This method is called to generate the type relationships(inheritance,aggregation,composition,using) in the input set       
        /// </summary>
        public void generateRelationshipsDetails()
        {
            try
            {
                generateInheritanceTable();
                generateAggergationTable();
                generateCompositionTable();
                generateUsingTable();
            }
            catch (Exception)
            {
                Console.WriteLine("Error occurred while displaying relationships");
            }
        }


        /// <summary>
        /// This method generates all the inheritance type relationships in the input data set 
        /// </summary>
        public void generateInheritanceTable()
        {
            List<InheritanceElement> inheritancetable = repo.inheritancedataList;

            try
            {
                if (inheritancetable.Count == 0)
                {
                    XElement firstLevelChildForNoData = new XElement("NOINHERITANCERELATION");
                    xmRootElement.Add(firstLevelChildForNoData);
                    return;
                }

                XElement firstLevelChild = new XElement("type");
                XElement secondLevelChild = null;

                foreach (InheritanceElement ie in inheritancetable)
                {
                    foreach (ChildTypeDetails child in ie.ChildDetails)
                    {
                        secondLevelChild = new XElement("ParentTypeName", ie.parent);
                        firstLevelChild.Add(secondLevelChild);
                        secondLevelChild = new XElement("ParentFileName", ie.ParentPackageName);
                        firstLevelChild.Add(secondLevelChild);
                        secondLevelChild = new XElement("RelationName", "Inheritance");
                        firstLevelChild.Add(secondLevelChild);
                        secondLevelChild = new XElement("ChildTypeName", child.Name);
                        firstLevelChild.Add(secondLevelChild);
                        secondLevelChild = new XElement("ChildFileName", child.Filename);
                        firstLevelChild.Add(secondLevelChild);
                        xmRootElement.Add(firstLevelChild);
                        firstLevelChild = new XElement("type");
                    }
                }

                Console.WriteLine();
            }
            catch (Exception)
            {
                Console.WriteLine("could not create the inheritance result. Please check the input");
            }
        }

        /// <summary>
        /// This method displays all the aggregation type relationships in the input data set
        /// </summary>
        public void generateAggergationTable()
        {
            List<AggregationElement> aggregatedtable = repo.aggregationdataList;

            try
            {
                if (aggregatedtable.Count == 0)
                {
                    XElement firstLevelChildForNoData = new XElement("NOAGGREGATIONRELATION");
                    xmRootElement.Add(firstLevelChildForNoData);
                    return;
                }

                XElement firstLevelChild = new XElement("type");
                XElement secondLevelChild = null;

                foreach (AggregationElement ae in aggregatedtable)
                {
                    foreach (ChildTypeDetails child in ae.ChildDetails)
                    {

                        secondLevelChild = new XElement("ParentTypeName", ae.aggregator);
                        firstLevelChild.Add(secondLevelChild);
                        secondLevelChild = new XElement("ParentFileName", ae.ParentFileName);
                        firstLevelChild.Add(secondLevelChild);
                        secondLevelChild = new XElement("RelationName", "Aggregation");
                        firstLevelChild.Add(secondLevelChild);
                        secondLevelChild = new XElement("ChildTypeName", child.Name);
                        firstLevelChild.Add(secondLevelChild);
                        secondLevelChild = new XElement("ChildFileName", child.Filename);
                        firstLevelChild.Add(secondLevelChild);
                        xmRootElement.Add(firstLevelChild);
                        firstLevelChild = new XElement("type");
                    }
                }
            }
            catch (Exception)
            {
                Console.WriteLine("could not create the aggregation result. Please check the input");
            }
        }

        /// <summary>
        /// This method displays all the composition type relationships in the input data set       
        /// </summary>
        public void generateCompositionTable()
        {
            List<CompositionElement> compositiontable = repo.compositiondataList;

            try
            {
                if (compositiontable.Count == 0)
                {
                    XElement firstLevelChildForNoData = new XElement("NOCOMPOSITION");
                    xmRootElement.Add(firstLevelChildForNoData);
                    return;
                }

                XElement firstLevelChild = new XElement("type");
                XElement secondLevelChild = null;

                foreach (CompositionElement ce in compositiontable)
                {
                    foreach (ChildTypeDetails child in ce.ChildDetails)
                    {

                        secondLevelChild = new XElement("ParentTypeName", ce.compositor);
                        firstLevelChild.Add(secondLevelChild);
                        secondLevelChild = new XElement("ParentFileName", ce.ParentFileName);
                        firstLevelChild.Add(secondLevelChild);
                        secondLevelChild = new XElement("RelationName", "Composition");
                        firstLevelChild.Add(secondLevelChild);
                        secondLevelChild = new XElement("ChildTypeName", child.Name);
                        firstLevelChild.Add(secondLevelChild);
                        secondLevelChild = new XElement("ChildFileName", child.Filename);
                        firstLevelChild.Add(secondLevelChild);
                        xmRootElement.Add(firstLevelChild);
                        firstLevelChild = new XElement("type");
                    }
                }
            }
            catch (Exception)
            {
                Console.WriteLine("could not create the composition result. Please check the input");
            }
        }


        /// <summary>
        /// This method displays all the using type relationships in the input data set  
        /// </summary>
        public void generateUsingTable()
        {
            List<UsingElement> usingtable = repo.usingdataList;
            XElement firstLevelChild = new XElement("type");
            XElement secondLevelChild = null;

            try
            {
                if (usingtable.Count == 0)
                {
                    XElement firstLevelChildForNoData = new XElement("NOUSINGRELATION");
                    xmRootElement.Add(firstLevelChildForNoData);
                    return;
                }

                foreach (UsingElement ue in usingtable)
                {
                    foreach (ChildTypeDetails child in ue.ChildDetails)
                    {

                        secondLevelChild = new XElement("ParentTypeName", ue.parent);
                        firstLevelChild.Add(secondLevelChild);
                        secondLevelChild = new XElement("ParentFileName", ue.ParentFileName);
                        firstLevelChild.Add(secondLevelChild);
                        secondLevelChild = new XElement("RelationName", "Composition");
                        firstLevelChild.Add(secondLevelChild);
                        secondLevelChild = new XElement("ChildTypeName", child.Name);
                        firstLevelChild.Add(secondLevelChild);
                        secondLevelChild = new XElement("ChildFileName", child.Filename);
                        firstLevelChild.Add(secondLevelChild);
                        xmRootElement.Add(firstLevelChild);
                        firstLevelChild = new XElement("type");
                    }
                }
            }
            catch (Exception)
            {
                Console.WriteLine("could not create the using type result. Please check the input");
            }
        }

        //Test stub class
        public class TestXml
        {
            //Test Stub
            /// <summary>
            ///   If the package is run as stand alone application
            /// then add the default values for the member variables                 
            /// </summary>
            /// <param name="args"></param>
            public static void Main(String[] args)
            {
                XmlDisplay xmlDisplayForTestStub = new XmlDisplay();
                Repository rep = Repository.getInstance();

                try
                {
                    if (rep.locations.Count == 0)
                    {
                        Console.WriteLine("The repository is empty. Nothing to display");
                    }

                    xmlDisplayForTestStub.generateRelationshipsDetails();
                }
                catch
                {
                    Console.WriteLine("Error occured during xml generation. Check the input parameters");
                }
            }
        }

    }//end of class
}//end of namespace