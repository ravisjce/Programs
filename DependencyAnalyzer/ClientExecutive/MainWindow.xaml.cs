//////////////////////////////////////////////////////////////////////////////////////////
// MainWindow.xaml.cs - Used for creating the GUI for the client process                //
// Language:    C#, 2014, .Net Framework 4.5                                            //
// Platform:    Win8                                                                    //
// Application: Dependency Analyzer Tool to find type dependency and package dependency //
// Author:      Ravi Nagendra , Syracuse University                                     //
//////////////////////////////////////////////////////////////////////////////////////////


/*
 * Module Operations:
 * ------------------
 * This module is the entry point for the client application. This is a WPF application
 * and user needs to enter the input data using the UI. This module uses the clientservice
 * package to create the channel for communication with the server. All the user input for
 * the client are specified via this module. It acts like an intermediate module between 
 * server and the clietservice module. All the processing for this module is performed in 
 * the clientservcie module. This module only displays the result.
 */

/*There is no test stub for this application since it is not a console based app
  * Its a WPF app and by default the UI will be launched on running this module
 * as a stand alone application
 */

using CodeAnalysis;
using HandCraftedService;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using CodeAnalyzer;

namespace ServiceClientExec
{
    /// <summary>
    /// WPF window for the client executive. This is the GUI for the client process
    /// </summary>
    public partial class MainWindow : Window
    {
        /// <summary>
        /// This is the client object which is used to 
        /// represent the client module in this project
        /// </summary>
        ClientService client = null;

        /// <summary>
        /// The service object which is used to communicate 
        /// with the server1
        /// </summary>
        IBasicService server1ServiceChannel = null;

        /// <summary>
        /// The service object which is used to communicate 
        /// with the server2
        /// </summary>
        IBasicService server2ServiceChannel = null;

        //Port Number for Server1
        string server1Port = "";

        //Port Number for Server2
        string server2Port = "";

        //Option to check whether the projects folder has to be traversed recursively in Server1
        bool server1recurse = false;

        //Option to check whether the projects folder has to be traversed recursively in Server2
        bool server2recurse = false;

        //List of projects present in Server1
        List<ProjectTree> projectfiles1 = null;

        //List of projects present in Server2
        List<ProjectTree> projectfiles2 = null;

        //List of files present in Server1
        List<string> server1Files = null;

        //List of files present in Server2
        List<string> server2Files = null;

        //List which contains partial Type of Server1
        List<Elem> server1TypeTable = null;

        //List which contains partial Type of Server2
        List<Elem> server2TypeTable = null;

        //List which contains the merged type tables of Server1 and Server2
        List<Elem> mergedTypeTable = null;

        //Repository for Server1
        Repository repo1Server = null;

        //Repository for Server2
        Repository repo2Server = null;

        //Repository which contains data of merged type tables
        Repository repoMerged = null;

        //Window to display the analysis result 
        ResultsWindow resultsWindow = null;

        /*constructor where all the member variables are initialised        
         *
         */
        ///
        public MainWindow()
        {
            try
            {
                InitializeComponent();
                Title = "Dependency Analyzer Client";
                AnalyzeButton.IsEnabled = false;
            }
            catch (Exception ex)
            {
                Console.Write("\n\n Error in the input data. Exception {0}. Please check the input data\n", ex.Message);
            }
        }

        /// <summary>        
        /// Callback function for the connect button of Server1. When the Client
        /// connects to a server successfully then the list of project files in the 
        /// Server list are fetched to the client and displayed on the GUI.
        /// </summary>
        /// <param name="sender">sender object returned by the platform</param>
        /// <param name="e">Event type returned by the sender object</param>
        private void ConnectButton1_Click(object sender, RoutedEventArgs e)
        {
            int projectFilesCount = 0;
            try
            {
                getConnection1();
                string msg = "Client is successfully connected";
                client.SendMessage(server1ServiceChannel, msg);
                msg = client.GetMessage(server1ServiceChannel);
                listBox1.Items.Insert(0, msg);
                ConnectButton1.IsEnabled = false;
                server1recurse = (bool)RecursiveSearch1.IsChecked;
                projectfiles1 = client.GetFilesList(server1ServiceChannel, server1ServiceChannel.getServerFilesPath(RemotePortTextBox1.Text), server1recurse);
                server1Files = new List<string>();

                foreach (ProjectTree prjtree in projectfiles1)
                {
                    listBox1.Items.Insert(projectFilesCount++, prjtree.Parent);
                    foreach (string file in prjtree.FilesList)
                    {
                        CheckBox itemCheckboxFiles = new CheckBox();
                        itemCheckboxFiles.Content = file;
                        listBox1.Items.Insert(projectFilesCount++, itemCheckboxFiles);
                    }
                }
                if (!AnalyzeButton.IsEnabled)
                    AnalyzeButton.IsEnabled = true;
            }
            catch (Exception ex)
            {
                showErrorWindow(ex.Message);
            }
        }

        /// <summary>        
        /// Callback function for the connect button of Server2. When the Client
        /// connects to a server successfully then the list of project files in the 
        /// Server list are fetched to the client and displayed on the GUI.
        /// </summary>
        /// <param name="sender">sender object returned by the platform</param>
        /// <param name="e">Event type returned by the sender object</param>

        private void ConnectButton2_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                getConnection2();
                int projectFilesCount = 0;
                string msg = "Client is successfully connected";
                client.SendMessage(server2ServiceChannel, msg);
                server2ServiceChannel.SetPortNumber(RemotePortTextBox2.Text);
                msg = client.GetMessage(server2ServiceChannel);
                listBox2.Items.Insert(0, msg);
                ConnectButton2.IsEnabled = false;
                server2recurse = (bool)RecursiveSearch2.IsChecked;
                projectfiles2 = client.GetFilesList(server2ServiceChannel, server2ServiceChannel.getServerFilesPath(RemotePortTextBox2.Text), server2recurse);
                server2Files = new List<string>();
                foreach (ProjectTree prjtree in projectfiles2)
                {
                    listBox2.Items.Insert(projectFilesCount++, prjtree.Parent);
                    foreach (string file in prjtree.FilesList)
                    {
                        CheckBox itemCheckboxFiles = new CheckBox();
                        itemCheckboxFiles.Content = file;
                        listBox2.Items.Insert(projectFilesCount++, itemCheckboxFiles);
                    }
                }
                if (!AnalyzeButton.IsEnabled)
                    AnalyzeButton.IsEnabled = true;
            }
            catch (Exception ex)
            {
                showErrorWindow(ex.Message);
            }
        }

        /// <summary>
        /// Used to clear the fileList of different Servers. This is required when the user 
        /// selects analysis for the second and subsequent times for different files
        /// </summary>        
        private void ClearServerFilesList()
        {
            try
            {
                if (server1Files != null)
                {
                    server1Files.Clear();
                }
                if (server2Files != null)
                {
                    server2Files.Clear();
                }
            }
            catch (Exception ex)
            {
                Console.Write("\n\n Error in the input data. Exception {0}. Please check the input data\n", ex.Message);
            }
        }

        private void ClearTypeTables()
        {
            if (server2TypeTable != null)
            {
                server2TypeTable.Clear();
            }
            if (server1TypeTable != null)
            {
                server1TypeTable.Clear();
            }
        }
        /// <summary>
        /// This method is used to send the selected file lists for Server1.
        /// User can select any file present in the Test folder of Server1.
        /// Analysis has to be performed only for these selected files.       
        /// </summary>
        private bool AnalyzeServer1()
        {
            try
            {
                if (server1ServiceChannel != null)
                {
                    client.SetRepo(server1ServiceChannel);

                    if (server1Files.Count > 0)
                    {
                        server1TypeTable = client.SetFilesList(server1ServiceChannel, server1Files, server1Port);
                    }
                    else
                    {
                        if (server2ServiceChannel == null || server2Files.Count == 0)
                        {
                            listBox1.Items.Insert(0, "Please select a file to analyze");
                            return false;
                        }
                    }
                }
                return true;
            }
            catch (Exception ex)
            {
                Console.Write("\n\n Error in the input data. Exception {0}. Please check the input data\n", ex.Message);
                return false;
            }
        }

        /// <summary>
        /// This method is used to send the selected file lists for Server2.
        /// User can select any file present in the Test folder of Server2.
        /// Analysis has to be performed only for these selected files.       
        /// </summary>
        private bool AnalyzeServer2()
        {
            try
            {
                if (server2ServiceChannel != null)
                {
                    client.SetRepo(server2ServiceChannel);

                    if (server2Files.Count > 0)
                        server2TypeTable = client.SetFilesList(server2ServiceChannel, server2Files, server2Port);
                    else
                    {
                        if (server1ServiceChannel == null || server1Files.Count == 0)
                        {
                            listBox2.Items.Insert(0, "Please select a file to analyze");
                            return false;
                        }
                    }
                }
                return true;
            }
            catch (Exception ex)
            {
                Console.Write("\n\n Error in the input data. Exception {0}. Please check the input data\n", ex.Message);
                return false;
            }
        }

        /// <summary>
        /// This method is to create the list of files for which analysis has to be performed
        /// User can select files from both server1 and server2. This method udpates the list
        /// of files for both Server1 and Server2 depending on whether the file is selected 
        /// using the checkbox option.
        /// </summary>
        /// <param name="serverNumber">Server for which the fileslist has to created</param>
        /// <param name="currentListBox">This is to check whether the checkbox for the
        /// files are selected or not.
        /// </param>
        private void CreateServerFilesList(int serverNumber, ListBox currentListBox)
        {
            String parentDirectory = "";

            try
            {
                foreach (object obj in currentListBox.Items)
                {
                    try
                    {
                        CheckBox selectedItems = (CheckBox)obj;

                        if (selectedItems != null)
                        {
                            if (selectedItems.IsChecked == true)
                            {
                                String currentname = selectedItems.Content.ToString();
                                String currentParentDirectory = parentDirectory + "\\" + currentname;

                                if (File.Exists(currentParentDirectory))
                                {
                                    if (serverNumber == 1)
                                        server1Files.Add(currentParentDirectory);
                                    else
                                        server2Files.Add(currentParentDirectory);
                                }
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine("Directory found", ex.ToString());
                        parentDirectory = (String)obj.ToString();
                    }
                }
            }
            catch (Exception ex)
            {
                Console.Write("\n\n Error in the input data. Exception {0}. Please check the input data\n", ex.Message);
            }
        }

        /// <summary>
        /// This is responsible for creating the XML file which contains the result
        /// of the type dependency as well as package dependency. The generated xml
        /// file is stored in the debug folder of the client. This file is later used
        /// by the LINQ query to display the result of XML in the GUI
        /// </summary>
        private String createXmlData()
        {
            String xmlSavedDataString = "";
            XmlDisplay xmlDislay = new XmlDisplay();
            xmlDislay.SetMergedRepo(repoMerged);
            xmlDislay.generateRelationshipsDetails();
            xmlDislay.generatePackageDependencyDetails();
            xmlSavedDataString = xmlDislay.saveXml();
            return xmlSavedDataString;
        }

        /// <summary>
        /// This method is called when the user selects the analyze button 
        /// in the client GUI. Once the analyze method is called, the following 
        /// are the steps involved to find the dependencies
        /// 1. List of files for which analysis is needed is stored
        /// 2. If there are 2 servers then there will be two list for 2 different servers
        /// 3. List of files are sent to individual servers where the partial type tables are created
        /// 4. The partial type tables are sent back from each of the server to the client
        /// 5. These partial type tables are merged in the client and again the merged tables are sent to 
        /// individual servers
        /// 6. using the merged table, each of the server do the analysis for the second pass
        /// 7. Each of the server sends the merged type tables to the client
        /// 8. Client uses this merged type tables of the servers to create the final merged list
        /// which contains the details of type and package dependencies
        /// </summary>
        /// <param name="sender">Default sender object returned by the platform</param>
        /// <param name="e"></param>
        private void AnalyzeButton_Click(object sender, RoutedEventArgs e)
        {
            String xmlString = "";
            try
            {
                ClearServerFilesList();
                ClearTypeTables();
                CreateServerFilesList(1, listBox1);
                CreateServerFilesList(2, listBox2);
                
                if (!AnalyzeServer1()) return;
                if (!AnalyzeServer2()) return;
                
                if (server1ServiceChannel != null && server2ServiceChannel != null)
                {
                    if (mergedTypeTable != null)
                     mergedTypeTable.Clear(); 
                    mergedTypeTable = client.mergeTypeTables(server1TypeTable, server2TypeTable);
                    if (server1Files.Count != 0)
                     client.SendMergedTypeTable(server1ServiceChannel, mergedTypeTable); 
                    if (server2Files.Count != 0)
                        client.SendMergedTypeTable(server2ServiceChannel, mergedTypeTable);
                }

                mergeFinalRepoTable();
                xmlString = createXmlData();
                resultsWindow = new ResultsWindow(repoMerged, xmlString);
                resultsWindow.Show();
            }
            catch (Exception ex)
            {
                Console.Write("\n\n Error in the input data. Exception {0}. Please check the input data\n", ex.Message);
            }
        }

        /// <summary>
        /// This method is used to merge the final repo which is obtained 
        /// from individual servers
        /// </summary>
        private void mergeFinalRepoTable()
        {
            if (server1ServiceChannel != null)
            {
                client.GetAnalyzedData(server1ServiceChannel, server1Files);
                repo1Server = client.GetRepo(server1ServiceChannel);
            }
            if (server2ServiceChannel != null)
            {
                client.GetAnalyzedData(server2ServiceChannel, server2Files);
                repo2Server = client.GetRepo(server2ServiceChannel);
            }
            if (server1ServiceChannel != null && server2ServiceChannel != null)
            {
                if (server2Files.Count != 0) repoMerged = client.mergeRepo(repo1Server, repo2Server);
                else repoMerged = repo1Server;
            }
            else if (server1ServiceChannel != null && server2ServiceChannel == null)
                repoMerged = repo1Server;
            else if (server1ServiceChannel == null && server2ServiceChannel != null)
                repoMerged = repo2Server;
        }

        /// <summary>
        /// This is used to create a proxy channel for the client to the server1
        /// The client uses this channel to communicate with server1.
        /// </summary>
        void getConnection1()
        {
            try
            {
                if (ConnectButton1.IsEnabled)
                {
                    server1Port = RemotePortTextBox1.Text;
                    string endpoint = RemoteAddressTextBox1.Text + server1Port + "/IBasicService";
                    try
                    {
                        if (client == null)
                            client = new ClientService();
                        server1ServiceChannel = client.CreateSendChannel(endpoint);

                    }
                    catch (Exception ex)
                    {
                        showErrorWindow(ex.Message, server1Port);
                    }
                }
            }
            catch (Exception ex)
            {
                Console.Write("\n\n Error in the input data. Exception {0}. Please check the input data\n", ex.Message);
            }
        }

        /// <summary>
        ///  /// This is used to create a proxy channel for the client to the server2
        /// The client uses this channel to communicate with server2.       
        /// </summary>
        void getConnection2()
        {
            try
            {
                if (ConnectButton2.IsEnabled)
                {
                    server2Port = RemotePortTextBox2.Text;
                    string endpoint = RemoteAddressTextBox2.Text + server2Port + "/IBasicService";
                    try
                    {
                        if (client == null)
                            client = new ClientService();
                        server2ServiceChannel = client.CreateSendChannel(endpoint);
                    }
                    catch (Exception ex)
                    {
                        showErrorWindow(ex.Message, server2Port);
                    }
                }
            }
            catch (Exception ex)
            {
                Console.Write("\n\n Error in the input data. Exception {0}. Please check the input data\n", ex.Message);
            }
        }

        /// <summary>
        /// This method is used to create the error message window whenever
        /// there is an exception. 
        /// </summary>
        /// <param name="message">the corresponding error message</param>
        /// <param name="portNo">the port number of the server where the
        /// exception was thrown
        /// </param>
        void showErrorWindow(string message, string portNo)
        {
            try
            {
                Window temp = new Window();
                StringBuilder msg = new StringBuilder(message);
                msg.Append("\nport = ");
                msg.Append(portNo);
                temp.Content = msg.ToString();
                temp.Height = 100;
                temp.Width = 500;
                temp.Show();
            }
            catch (Exception ex)
            {
                Console.Write("\n\n Error in the input data. Exception {0}. Please check the input data\n", ex.Message);
            }
        }

        /// <summary>
        /// Used to create a error window which displays the 
        /// error info to the user
        /// </summary>
        /// <param name="message">The message indicating the exception type</param>
        void showErrorWindow(string message)
        {
            try
            {
                Window temp = new Window();
                temp.Content = message;
                temp.Height = 100;
                temp.Width = 500;
                temp.Show();
            }
            catch (Exception ex)
            {
                Console.Write("\n\n Error in the input data. Exception {0}. Please check the input data\n", ex.Message);
            }
        }

        /// <summary>
        /// This method is called when the client executive GUI is closed.
        /// When the main client GUI is closed, we need to close the results 
        /// window if it is active. 
        /// </summary>
        /// <param name="sender">The sender object returned by the platform</param>
        /// <param name="e">The event handler arguments which is sent by the platform
        /// when the window is closed
        /// </param>
        private void Window_Closed(object sender, EventArgs e)
        {
            if (resultsWindow != null)
            {
                if (resultsWindow.IsActive)
                    resultsWindow.Close();
            }
        }
    }//end of class
}//end of namespace
