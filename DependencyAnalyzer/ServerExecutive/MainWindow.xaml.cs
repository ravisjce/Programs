//////////////////////////////////////////////////////////////////////////////////////////
// MainWindow.xaml.cs - Used for creating the GUI for the server process                //
// Language:    C#, 2014, .Net Framework 4.5                                            //
// Platform:    Win8                                                                    //
// Application: Dependency Analyzer Tool to find type dependency and package dependency //
// Author:      Ravi Nagendra , Syracuse University                                     //
//////////////////////////////////////////////////////////////////////////////////////////

/*
 * Module Operations:
 * ------------------
 * This module represents the server GUI of the program. This is a WPF app
 * and when user lauches this module, server GUI will open where user can 
 * give the data. It accepts the portnumber and the url path of the channel 
 * which has to be created. All the client process mush use this channel address
 * to communicate with the server. 
 * In addition to this there is option to specify the folder in which test 
 * files for which analysis has to be performed are present. There is an option 
 * to select the file pattern which indicates that only files of those types have
 * to be anlaysed.
 */

/*There is no test stub for this application since it is not a console based app
  * Its a WPF app and by default the UI will be launched on running this module
 * as a stand alone application
 */
using HandCraftedService;
using System;
using System.Text;
using System.Threading;
using System.Windows;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Forms;
using System.IO;
using System.Collections;
using System.Collections.Generic;

namespace ServerExecutive
{
    /// <summary>
    /// WPF window for the server executive. This is the server for the client process
    /// </summary>
    public partial class MainWindow : Window
    {
        /// <summary>
        /// This is the host object which is responsible for creating 
        /// the communication channel for the server. It opens the server
        /// and makes it ready to accept the connection from client
        /// </summary>
        ServerHost server = null;

        /// <summary>
        ///  This is the default location where the file which 
        ///  stores the port number and the path of the test servers is created.        
        /// </summary>
        string textFilePath = System.IO.Directory.GetCurrentDirectory() + "\\" + "filepath.txt";

        /// <summary>
        /// There is an option for the user to browse the location of the test 
        /// folder in the server. This path where the test files in the server
        /// are present is stored in this member variable
        /// </summary>
        String selectedFolderPath = "";

        /*constructor where all the member variables are initialised        
        *
        */
        public MainWindow()
        {
            InitializeComponent();
            Title = "Dependency Analyzer Server";
            StopButton.IsEnabled = false;
        }

        /// <summary>
        /// Event handler for the listen button of the server GUI. When
        /// the user selects the listen button of the server, a host 
        /// channel is created and opened which will be used by the client
        /// to communicate with the server. The port number and the server
        /// IP address can be entered in the GUI.
        /// </summary>
        /// <param name="sender">The default sender object for which the callback is 
        /// associated</param>
        /// <param name="e">Platform event object for the button click operation</param>
        private void ListenButton1_Click(object sender, RoutedEventArgs e)
        {
            string localPort1 = RemotePortTextBox1.Text;
            string endpoint1 = RemoteAddressTextBox1.Text + localPort1 + "/IBasicService";

            try
            {
                if (localPort1.Equals("") || RemoteAddressTextBox1.Text.Equals(""))
                {
                    throw new Exception("Input URI not proper. Please provide valid URL");
                }
                String projectLocation = "";
                server = new ServerHost();
                server.CreateReceiveChannel(endpoint1);
                listBox1.Items.Insert(0, "Started.. Waiting for a Client");
                ListenButton1.IsEnabled = false;
                StopButton.IsEnabled = true;
                projectLocation = "The test files directory is " + Directory.GetCurrentDirectory();
                listBox1.Items.Insert(1, projectLocation);
                BrowseButton.IsEnabled = true;
                selectedFolderPath = Directory.GetCurrentDirectory();
                UpdateFileContents();
            }
            catch (Exception ex)
            {
                Window temp = new Window();
                StringBuilder msg = new StringBuilder(ex.Message);
                msg.Append("\nport = ");
                msg.Append(localPort1.ToString());
                temp.Content = msg.ToString();
                temp.Height = 100;
                temp.Width = 500;
                temp.Show();
            }
        }

        /// <summary>
        /// This method is used to close the server. Once the user
        /// selects the stop button, the server will close and user 
        /// can open up a new connection channel for the new server
        /// </summary>
        /// <param name="sender">The default sender object for which the callback is 
        /// associated</param>
        /// <param name="e">Platform event object for the button click operation</param>
        private void StopButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                server.close();
                ListenButton1.IsEnabled = true;
                StopButton.IsEnabled = false;
                BrowseButton.IsEnabled = false;
                listBox1.Items.Clear();
            }
            catch (Exception)
            {
                Console.WriteLine("Server could not be stopped");
            }
        }

        /// <summary>
        /// This method is used to create a dialogbox where the user
        /// can select the path of the folder where the test files of 
        /// the server are stored.
        /// </summary>
        /// <param name="sender">The default sender object for which the callback is 
        /// associated</param>
        /// <param name="e">Platform event object for the button click operation</param>
        private void Button_Browse(object sender, RoutedEventArgs e)
        {
            FolderBrowserDialog dlg = new FolderBrowserDialog();
            string path = AppDomain.CurrentDomain.BaseDirectory;
            dlg.SelectedPath = path;
            DialogResult result = dlg.ShowDialog();
            try
            {
                if (result == System.Windows.Forms.DialogResult.OK)
                {
                    String currentPathSet = "Test files directory is " + dlg.SelectedPath;
                    listBox1.Items.Clear();
                    listBox1.Items.Insert(0, currentPathSet);
                    selectedFolderPath = dlg.SelectedPath;
                    UpdateFileContents();
                }
            }
            catch
            {
                Console.WriteLine("Error while browsing the folders");
            }
        }

        /// <summary>
        /// This method is used to update the file contents which stores
        /// the path of the test folder of the server. Every time the 
        /// server port is changed, it will be added in the file stored 
        /// in the default path
        /// </summary>
        private void UpdateFileContents()
        {
            try
            {
                String[] fileContents = null;
                List<String> finalFileContents = new List<String>();
                int lineNumber = 0;
                bool portAlreadyPresent = false;
                if (File.Exists(textFilePath))
                    fileContents = System.IO.File.ReadAllLines(textFilePath);

                if (fileContents != null)
                {
                    for (int i = 0; i < fileContents.Length; i++)
                    {
                        if (fileContents[i].Contains("PortNumber"))
                        {
                            string[] portNumber = fileContents[i].Split(' ');

                            if (portNumber[1].Equals(RemotePortTextBox1.Text))
                            {
                                lineNumber = i;
                                portAlreadyPresent = true;
                            }
                        }
                    }
                    if (portAlreadyPresent)
                    {
                        fileContents[lineNumber + 1] = "*.cs";
                        fileContents[lineNumber + 2] = selectedFolderPath;
                        System.IO.File.WriteAllLines(textFilePath, fileContents);
                    }
                    else
                    {
                        for (int i = 0; i < fileContents.Length; i++)
                        {
                            finalFileContents.Add(fileContents[i]);
                        }
                        WriteToFile(finalFileContents);
                    }
                }
                else
                    WriteToFile(finalFileContents);
            }
            catch (Exception)
            {
                Console.WriteLine("Error while writing to file");
            }
        }

        /// <summary>
        /// This method is used to write the server details to 
        /// a text file. The server details consists of the path
        /// where the test files are present. It also has the info
        /// about the server port number and the file patterns.
        /// </summary>
        /// <param name="fileContents">Server Details which contains
        /// port number, file patterns and location of test folder</param>
        private void WriteToFile(List<string> fileContents)
        {
            try
            {
                String updatePortNumber = "PortNumber " + RemotePortTextBox1.Text;

                fileContents.Add(updatePortNumber);
                fileContents.Add("*.cs");
                fileContents.Add(selectedFolderPath);
                System.IO.File.WriteAllLines(textFilePath, fileContents);
            }
            catch (Exception)
            {
                Console.WriteLine("Could not write to file");
            }
        }
    }//end of class
}//end of namespace
