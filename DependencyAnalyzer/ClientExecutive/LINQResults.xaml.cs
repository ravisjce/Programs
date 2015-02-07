//////////////////////////////////////////////////////////////////////////////////////////
// LINQResults.xaml.cs - Used for displaying the LINQ query results in the GUI          //
// Language:    C#, 2014, .Net Framework 4.5                                            //
// Platform:    Win8                                                                    //
// Application: Dependency Analyzer Tool to find type dependency and package dependency //
// Author:      Ravi Nagendra , Syracuse University                                     //
//////////////////////////////////////////////////////////////////////////////////////////

/*
 * Module Operations:
 * ------------------
 * This module is responsible for displaying the results of the LINQ query.
 * The result of the complete analysis is stored in the XML file. This module 
 * executes the LINQ query to fetch unique packages, unique typenames, unique
 * parents types, unique child types. This will be useful for the user to find
 * those packages which are dependent on some other packages.
 */

/*There is no test stub for this application since it is not a console based app
  * Its a WPF app and by default the UI will be launched on running this module
 * as a stand alone application
 */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using System.Xml.Serialization;
using System.Xml.Linq;

namespace ServiceClientExec
{
    /// <summary>
    /// Class used to create the result window which
    /// contains the result of the LINQ query executed
    /// for the generated XML file.
    /// </summary>
    public partial class LINQResults : Window
    {
        /// <summary>
        /// This contains the complete XML file represented in the form of 
        /// a string. Once the client generates the XML which contains the 
        /// result of complete analysis, this document will be represented
        /// as a string and this will be used to execute the LINQ query.
        /// </summary>
        String xmlDocument;

        /// <summary>
        /// This represents the XML document for which the 
        /// LINQ query is executed. 
        /// </summary>
        XDocument doc = null;

        /// <summary>
        /// Constructor where all the data members
        /// are initialised.
        /// </summary>
        /// <param name="xmlData">The string which represents the result XML document
        /// in string format</param>
        public LINQResults(String xmlData)
        {
            xmlDocument = xmlData;
            InitializeComponent();
            executeLinqQueries();
        }


        /// <summary>
        /// This method is responsible for executing all the LINQ queries.
        /// There are various linq queries which are executed for the 
        /// result XML file. All the queries are called from this method.
        /// </summary>
        private void executeLinqQueries()
        {
            doc = XDocument.Parse(xmlDocument);
            queryParentTypeNames();
            queryChildTypeNames();
            queryParentFileNames();
            queryChildFileNames();
            queryRelationshipNames();
            queryParentPackageNames();
            queryChildPackageName();
        }

        /// <summary>
        /// This method executes the linq query to find 
        /// unique parent type names. The parent type represents
        /// name of a class on which other child class is dependent
        /// through one of the relation, inheritance, aggregation,
        /// composition, using.
        /// </summary>
        private void queryParentTypeNames()
        {
            String displayString = "The linq query for distinct parent type names ";
            String elementList = "";
            int count = 0;

            try
            {
                var queryForParentTypeNames = (from x in
                                                   doc.Descendants()
                                               where (x.Name == "ParentTypeName")
                                               select x.Value).Distinct();

                foreach (var elem in queryForParentTypeNames)
                {
                    elementList = elem.ToString();
                    Linqlistbox.Items.Insert(0, elementList);
                    count++;
                }

                if (count == 0)
                {
                    displayString = "No parent types in the xml file";
                }

                Linqlistbox.Items.Insert(0, displayString);
            }
            catch (Exception)
            {
                Console.WriteLine("Error in the xml file. Please check the input");
            }
        }

        /// <summary>
        /// This method executes the linq query to find 
        /// unique child type names. The child type represents
        /// name of a class which is dependent on some other class
        /// through one of the relation, inheritance, aggregation,
        /// composition, using.
        /// </summary>
        private void queryChildTypeNames()
        {

            String displayString = "The linq query for distinct child type names ";
            String elementList = "";
            int count = 0;

            try
            {
                var childTypeNames = (from x in
                                          doc.Descendants()
                                      where (x.Name == "ChildTypeName")
                                      select x.Value).Distinct();

                foreach (var elem in childTypeNames)
                {
                    elementList = elem.ToString();
                    Linqlistbox.Items.Insert(0, elementList);
                    count++;
                }

                if (count == 0)
                {
                    displayString = "No parent types in the xml file";
                }

                Linqlistbox.Items.Insert(0, displayString);
            }
            catch (Exception)
            {
                Console.WriteLine("Error in the xml file. Please check the input");
            }
        }

        /// <summary>
        /// This method executes the linq query to find 
        /// unique parent filenames. The parent file contains one 
        /// ore more parent type on which a dependency is present.
        /// </summary>
        private void queryParentFileNames()
        {

            String displayString = "The linq query for distinct parent filenames ";
            String elementList = "";
            int count = 0;

            try
            {
                var parentFilenames = (from x in
                                           doc.Descendants()
                                       where (x.Name == "ParentFileName")
                                       select x.Value).Distinct();

                foreach (var elem in parentFilenames)
                {
                    elementList = elem.ToString();
                    Linqlistbox.Items.Insert(0, elementList);
                    count++;
                }

                if (count == 0)
                {
                    displayString = "No parent types in the xml file";
                }

                Linqlistbox.Items.Insert(0, displayString);
            }
            catch (Exception)
            {
                Console.WriteLine("Error in the xml file. Please check the input");
            }
        }

        /// <summary>
        /// This method is used to find the unique child filenames
        /// The child files consists of one of more child type which
        /// is dependent on other types.
        /// </summary>
        private void queryChildFileNames()
        {

            String displayString = "The linq query for distinct child filename";
            String elementList = "";
            int count = 0;

            try
            {
                var childFileNames = (from x in
                                          doc.Descendants()
                                      where (x.Name == "ChildFileName")
                                      select x.Value).Distinct();

                foreach (var elem in childFileNames)
                {
                    elementList = elem.ToString();
                    Linqlistbox.Items.Insert(0, elementList);
                    count++;
                }

                if (count == 0)
                {
                    displayString = "No parent types in the xml file";
                }

                Linqlistbox.Items.Insert(0, displayString);
            }
            catch (Exception)
            {
                Console.WriteLine("Error in the xml file. Please check the input");
            }
        }


        /// <summary>
        /// This method is used to find the unique relations
        /// which are present in the overall result. The relation
        /// types is either inheritance,aggregation,composition or using types
        /// </summary>
        private void queryRelationshipNames()
        {

            String relationshipNames = "The linq query for distinct relationship names ";
            String elementList = "";
            int count = 0;

            try
            {
                var queryForParentTypeNames = (from x in
                                                   doc.Descendants()
                                               where (x.Name == "RelationName")
                                               select x.Value).Distinct();

                foreach (var elem in queryForParentTypeNames)
                {
                    elementList = elem.ToString();
                    Linqlistbox.Items.Insert(0, elementList);
                    count++;
                }

                if (count == 0)
                {
                    relationshipNames = "No parent types in the xml file";
                }

                Linqlistbox.Items.Insert(0, relationshipNames);
            }
            catch (Exception)
            {
                Console.WriteLine("Error in the xml file. Please check the input");
            }
        }

        /// <summary>
        /// This method is used to execute linq query for unique parentpackages. 
        /// Parentpackage contains one or more types on which some other types
        /// are dependent.
        /// </summary>
        private void queryParentPackageNames()
        {
            String relationshipNames = "The linq query for distinct parent Package names ";
            String elementList = "";
            int count = 0;

            try
            {

                var queryParentPackageName = (from x in
                                                  doc.Descendants()
                                              where (x.Name == "ParentPackage")
                                              select x.Value).Distinct();

                foreach (var elem in queryParentPackageName)
                {
                    elementList = elem.ToString();
                    Linqlistbox.Items.Insert(0, elementList);
                    count++;
                }

                if (count == 0)
                {
                    relationshipNames = "No parent packages in the xml file";
                }

                Linqlistbox.Items.Insert(0, relationshipNames);
            }
            catch (Exception)
            {
                Console.WriteLine("Error in the xml file. Please check the input");
            }
        }

        /// <summary>
        /// This is used to execute the linq query to find 
        /// list of unique child packages. The child package 
        /// contains one or more types which are dependent on 
        /// some other types
        /// </summary>
        private void queryChildPackageName()
        {
            String relationshipNames = "The linq query for distinct child Package names ";
            String elementList = "";
            int count = 0;

            try
            {

                var queryParentPackageName = (from x in
                                                  doc.Descendants()
                                              where (x.Name == "ChildPackageName")
                                              select x.Value).Distinct();

                foreach (var elem in queryParentPackageName)
                {
                    elementList = elem.ToString();
                    Linqlistbox.Items.Insert(0, elementList);
                    count++;
                }

                if (count == 0)
                {
                    relationshipNames = "No child packages in the xml file";
                }

                Linqlistbox.Items.Insert(0, relationshipNames);
            }
            catch (Exception)
            {
                Console.WriteLine("Error in the xml file. Please check the input");
            }
        }
    }//end of class
}//end of namespace
