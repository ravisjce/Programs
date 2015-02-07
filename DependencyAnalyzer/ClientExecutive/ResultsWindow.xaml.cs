//////////////////////////////////////////////////////////////////////////////////////////
// ResultsWindow.xaml.cs - Used to display the result of the analysis in GUI            //
// Language:    C#, 2014, .Net Framework 4.5                                            //
// Platform:    Win8                                                                    //
// Application: Dependency Analyzer Tool to find type dependency and package dependency //
// Author:      Ravi Nagendra , Syracuse University                                     //
//////////////////////////////////////////////////////////////////////////////////////////

/*
 * Module Operations:
 * ------------------
 * This module is used to display the type and package dependency result in the UI.
 * This is a WPF application and there is no processing involved in the module.
 * There is an option to select only type and only package dependency result in the 
 * UI. Also there is option to select the LINQ query operation from the UI. This
 * acts like a display module for the client where the results are displayed.
 */
/*There is no test stub for this application since it is not a console based app
  * Its a WPF app and by default the UI will be launched on running this module
 * as a stand alone application
 */
using CodeAnalysis;
using System.Windows;
using System;
using System.IO;

namespace ServiceClientExec
{
    /// <summary>
    /// This class is used to create the GUI to display the results
    /// of the analysis. The result contains both type dependency as well
    /// as package dependencies across packages. There is an option to 
    /// execute LINQ query which fetches the result from the XML file which
    /// contains the complete analysis result
    /// </summary>
    public partial class ResultsWindow : Window
    {
        //This is used to to store the list which contains the complete analysis result
        Repository repos;
        String xmlString;

        /*constructor where all the member variables are initialised        
        *
        */
        public ResultsWindow(Repository repos, String xmlData)
        {
            try
            {
                this.repos = repos;
                String xmlPath = "";
                InitializeComponent();
                Title = "Package and Dependency Analysis Results";
                TypeAnalysisCheckBox.IsChecked = true;
                PackageDependencyCheckBox.IsChecked = true;
                showResults();
                xmlPath = "The xml file is generated in the path " + Directory.GetCurrentDirectory() + "\\" + "DependencyAnalyzer.xml";
                XMLOutput.Items.Insert(0, xmlPath);
                xmlString = xmlData;
            }
            catch (Exception ex)
            {
                Console.Write("\n\n Error in the input data. Exception {0}. Please check the input data\n", ex.Message);
            }
        }

        /// <summary>
        /// This method is used to populate the results of various type dependency.
        /// There are 4 types of dependencies which can exist between different types.
        /// Inheritance, Aggregation, composition,using dependency. Each of the type
        /// dependency is then displayed used corresponding methods. Apart from the 
        /// type dependency, the result also contains package dependency data.
        /// </summary>
        public void showResults()
        {
            showInheritance();
            showAggregation();
            showComposition();
            showUsing();
            showPackages();
        }

        /// <summary>
        /// Used to display the inheritance type dependency analysis for 
        /// all the files across different servers. 
        /// </summary>
        public void showInheritance()
        {
            try
            {
                foreach (InheritanceElement ie in repos.inheritancedataList)
                {
                    string msg = "";
                    foreach (ChildTypeDetails child in ie.ChildDetails)
                    {
                        msg = "Child Class " + child.Name + " has a dependency on Parent Class " + ie.parent;
                        InheritanceListBox.Items.Insert(0, msg);
                    }
                }
            }
            catch (Exception)
            {
                Console.Write("\n\n No elements in the inheritance list");
            }
        }

        /// <summary>
        /// Used to display the Aggregation type dependency analysis for 
        /// all the files across different servers.
        /// </summary>
        public void showAggregation()
        {
            try
            {
                foreach (AggregationElement ae in repos.aggregationdataList)
                {
                    string msg = "";

                    foreach (ChildTypeDetails child in ae.ChildDetails)
                    {
                        msg = "Child Class " + child.Name + " has a dependency on Parent Class " + ae.aggregator;
                        AggregationListBox.Items.Insert(0, msg);
                    }
                }
            }
            catch (Exception)
            {
                Console.Write("\n\n No elements in the aggregation list");
            }
        }

        /// <summary>
        /// Used to display the composition type dependency analysis for 
        /// all the files across different servers.
        /// </summary>
        public void showComposition()
        {
            try
            {
                foreach (CompositionElement ce in repos.compositiondataList)
                {
                    string msg = "";
                    foreach (ChildTypeDetails child in ce.ChildDetails)
                    {
                        msg = "Child Class " + child.Name + " has a dependency on Parent Class " + ce.compositor;
                        CompositionListBox.Items.Insert(0, msg);
                    }
                }
            }
            catch (Exception)
            {
                Console.Write("\n\n No elements in the composition list");
            }
        }

        /// <summary>
        /// Used to display the using type dependency analysis for 
        /// all the files across different servers.
        /// </summary>
        public void showUsing()
        {
            try
            {
                foreach (UsingElement ue in repos.usingdataList)
                {
                    string msg = "";
                    foreach (ChildTypeDetails child in ue.ChildDetails)
                    {
                        msg = "Child Class " + child.Name + " has a dependency on Parent Class " + ue.parent;
                        UsingListBox.Items.Insert(0, msg);
                    }
                }
            }
            catch (Exception)
            {
                Console.Write("\n\n No elements in the using type list");
            }
        }

        /// <summary>
        /// /// Used to display the package dependency analysis for 
        /// all the files across different servers.
        /// </summary>
        public void showPackages()
        {
            try
            {
                foreach (PackageDependencyElement pe in repos.packagedata)
                {
                    string message = "";
                    string child = pe.childPackageName;
                    //foreach (string child in pe.childrenPackage)
                    {
                        message = "Package " + child + " has a dependency on " + pe.parentPackage;
                        PackageListBox.Items.Insert(0, message);
                    }
                }
            }
            catch (Exception)
            {
                Console.Write("\n\n No elements in the package analysis list");
            }
        }

        /// <summary>
        /// This method is used to display only the type analysis results for the files.
        /// By default both type analysis and package analysis results are displayed
        /// in the GUI. User can then select only the type analysis results display
        /// by selecting the checkbox present in the results window.
        /// </summary>
        /// <param name="sender">Default sender object returned by the platform</param>
        /// <param name="e">The event generated when user selects the checkbox</param>
        private void TypeAnalysisCheckBox_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if ((bool)TypeAnalysisCheckBox.IsChecked)
                {
                    Grid1.Visibility = Visibility.Visible;
                    Grid2.Visibility = Visibility.Visible;
                    Grid3.Visibility = Visibility.Visible;
                    Grid4.Visibility = Visibility.Visible;
                }
                else
                {
                    Grid1.Visibility = Visibility.Collapsed;
                    Grid2.Visibility = Visibility.Collapsed;
                    Grid3.Visibility = Visibility.Collapsed;
                    Grid4.Visibility = Visibility.Collapsed;
                }
            }
            catch (Exception ex)
            {
                Console.Write("\n\n Error in the input data. Exception {0}. Please check the input data\n", ex.Message);
            }
        }

        /// <summary>
        /// This method is used to display only the package analysis results.
        /// By default both type analysis and package analysis results are displayed
        /// in the GUI. User can then select only the package analysis results display
        /// by selecting the checkbox present in the results window.

        /// </summary>
        /// <param name="sender">Default sender object returned by the platform</param>
        /// <param name="e">The event generated when user selects the checkbox</param>
        private void PackageDependencyCheckBox_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if ((bool)PackageDependencyCheckBox.IsChecked)
                    Grid5.Visibility = Visibility.Visible;
                else
                    Grid5.Visibility = Visibility.Collapsed;
            }
            catch (Exception ex)
            {
                Console.Write("\n\n Error in the input data. Exception {0}. Please check the input data\n", ex.Message);
            }
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            LINQResults linqWindow = new LINQResults(xmlString);
            linqWindow.Show();
        }

    }//end of class
}//end of namespace
