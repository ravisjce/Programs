//////////////////////////////////////////////////////////////////////////////////////////
// ServerHost.cs - This module performs all the backend operation of the server         //
// Language:    C#, 2014, .Net Framework 4.5                                            //
// Platform:    Win8                                                                    //
// Application: Dependency Analyzer Tool to find type dependency and package dependency //
// Author:      Ravi Nagendra , Syracuse University                                     //
//////////////////////////////////////////////////////////////////////////////////////////

/*
 * Module Operations:
 * ------------------
 * This is the module which represents the server of the tool.
 * All the service contracts are defined in this module. The service 
 * contracts exposes the functions of server to the client. All the 
 * requests from the clients are processed by this module. It creates
 * the server channel through which clients can communicate with the 
 * server. It accepts the requests and sends the response to the client. 
 */

using CodeAnalysis;
using CodeAnalyzer;
using System;
using System.IO;
using System.Collections.Generic;
using System.ServiceModel;

namespace HandCraftedService
{
    /// <summary>
    /// The class which represents the server module of the
    /// application. It defines all the methods which are exposed
    /// to the client.
    /// </summary>
    public class ServerHost : IBasicService
    {
        //The host object which opens the connection for the server. Client connects to this host channel
        ServiceHost host = null;

        //The port number of the server. Client need this port number to connect to the server.
        String serverPortNumber = "";

        //The path of the folder where the test files is stored in the server
        String serverFilesPath = "";

        //The message sent from the server to the client to indicate connection is established
        String message = "";

        //This list stores the list of all the test files in the server and its parent folder
        List<ProjectTree> filestree;

        //This contains the list of files for which analysis has to be performed. Client sets the list
        List<String> files;

        //The type table which contains the result of the type analysis.
        List<Elem> localTypeTable = null;

        //Stores the file patterns which defines what type of files are to be analysed
        List<string> patterns = null;

        //This is used for creating the list of files which are present in the server test folder
        FileManager fm = null;

        //This object is responsible for doing the analysis on the test files of the server
        Analyzer analyzer;

        /// <summary>
        /// Default constructor where all the data members are initialised
        /// </summary>
        public ServerHost()
        {
            localTypeTable = new List<Elem>();
            patterns = new List<string>();
        }

        /// <summary>
        /// Creates the server communication channel which will be used
        /// by the clients to send and receive messages.
        /// </summary>
        /// <param name="url">URL which specifies the URI of the server</param>
        public void CreateReceiveChannel(string url)
        {
            BasicHttpBinding binding = new BasicHttpBinding();
            Uri address = new Uri(url);
            Type service = typeof(ServerHost);
            host = new ServiceHost(service, address);
            host.AddServiceEndpoint(typeof(IBasicService), binding, address);
            host.Open();
        }

        /// <summary>
        /// This method is used by the client to send request to the server
        /// </summary>
        /// <param name="message">Message which is sent by the client</param>      
        public void sendMessage(string msg)
        {
            this.message = "Service received message: " + msg;
        }

        /// <summary>
        /// This method is used by the client to get a response from the server
        /// </summary>
        /// <returns>Response message from the server</returns>
        public string getMessage()
        {
            return "Connected...";
        }

        /// <summary>
        /// This method is used by the client to get the list of test files
        /// which are present in the server
        /// </summary>
        /// <param name="path">The path in which the server files are present</param>
        /// <param name="recurse">Specifies whether the server files present 
        /// in the subdirectories of test folder needs to sent from the server
        /// to the client</param>
        /// <returns>Returns the list of files present in the server test folder</returns>
        public List<ProjectTree> getFilesList(string path, bool recurse)
        {
            fm = new FileManager();
            fm.Patterns.Add("*.cs");
            fm.Recurse = recurse;
            fm.findFiles(path);
            fm.createProjectTree();
            filestree = fm.getFiles();

            return filestree;
        }

        /// <summary>
        /// This is used by the client to the list of files
        /// for which analysis needs to be performed by the server
        /// </summary>
        /// <param name="files">List of files for which analysis has to be done</param>
        /// <param name="serverName">Name of the server for which the file list are set</param>
        /// <returns>Returns the repo which is created after analyzing all the files</returns>
        public List<Elem> setFilesList(List<string> files, string serverName)
        {
            this.files = files;
            analyze(serverName);
            Repository rep = Repository.getInstance();
            List<Elem> table = rep.locations;
            return table;
        }

        /// <summary>
        /// used to initiate the analysis of the selected files
        /// </summary>
        /// <param name="serverName">Name of the server where the analysis is done</param>
        public void analyze(string serverName)
        {
            analyzer = new Analyzer();
            analyzer.Files = files;
            analyzer.Relationshipflag = true;
            analyzer.analyze(serverName);
        }

        /// <summary>
        /// This method is used by the client to send the merged type table 
        /// to each of the server. The client merges the partial type table
        /// obtained from each of the servers and sends this merged type table
        /// to the servers. The individual server then uses this merged type table
        /// to do the second round of analysis
        /// </summary>
        /// <param name="typeTable">The merged type table which is sent by the client to the server</param>
        public void sendMergedTypeTable(List<Elem> typeTable)
        {
            Repository repo = Repository.getInstance();
            repo.locations = typeTable;
        }

        /// <summary>
        /// This method is used to indicate the server to start the
        /// second round of analysis. Once the server receives the merged type 
        /// table from the client, it has to start the second round of analysis.
        /// Once the server starts the second round of analysis, it will create 
        /// the final repo table which will be used by the client.
        /// </summary>
        /// <param name="files">List of files for which the analysis has to be performed</param>
        public void startSecondAnalysis(List<string> files)
        {
            analyzer = new Analyzer();
            analyzer.Files = files;
            analyzer.analyzeSecondParse();
        }

        /// <summary>
        /// This method is used by the client to the repository of the servers.
        /// The client does the merging of the partial type table of all the servers.
        /// So client uses this method to fetch the repo from the server. 
        /// </summary>
        /// <returns>Returns the repository of the server to the client</returns>
        public Repository getRepo()
        {
            return Repository.getInstance();
        }

        /// <summary>
        /// This method is used by the client to find the list of 
        /// all the files present in the test folder of the server
        /// </summary>
        /// <param name="portNumber">port number to identify the server</param>
        /// <returns>Returns the list of all the files present in the test folder</returns>
        public string getServerFilesPath(String portNumber)
        {
            try
            {
                string textFilePath = System.IO.Directory.GetCurrentDirectory();
                textFilePath = textFilePath + "\\" + "filepath.txt";
                serverFilesPath = "";
                if (!File.Exists(textFilePath))
                {
                    return serverFilesPath;
                }
                string[] lines = System.IO.File.ReadAllLines(textFilePath);

                for (int i = 0; i < lines.Length; i++)
                {
                    if (lines[i].Contains("PortNumber"))
                    {
                        String[] portDetails = lines[i].Split(' ');
                        if (portDetails[1].Equals(portNumber))
                        {
                            serverFilesPath = lines[i + 2];
                            break;
                        }
                    }
                }

                return serverFilesPath;
            }
            catch (Exception)
            {
                Console.WriteLine("could not get the file list from the server");
                return "";
            }
        }

        /// <summary>
        /// This method is used by the client to get the list of 
        /// patterns which is used by the server to generate the file list.
        /// </summary>
        /// <returns>Returns the list of patterns which specifies the type
        /// of files for which analysis will be done</returns>
        public List<string> getPatterns()
        {
            string textFilePath = System.IO.Directory.GetCurrentDirectory();
            textFilePath = textFilePath + "\\" + "filepath.txt";
            string[] lines = System.IO.File.ReadAllLines(textFilePath);
            patterns.Clear();
            for (int i = 0; i < lines.Length - 1; i++)
            {
                patterns.Add(lines[i]);
            }
            return patterns;
        }

        /// <summary>
        /// This method is used by the client to set the port number 
        /// for any any requests. Since a client can connect to multiple 
        /// servers, whenever the client wants to send a request to the 
        /// server, it has to identify the server name. This is done by
        /// using the portnumber of the server
        /// </summary>
        /// <param name="portnumber">The server portnumber from which
        /// response is excepted</param>
        public void SetPortNumber(String portnumber)
        {
            serverPortNumber = portnumber;
        }

        /// <summary>
        /// This method is used to set the repository of the individual servers.
        /// When the server is active and if there are requests from multiple 
        /// clients then the repository has to reinitialised or else the result 
        /// will be appened on top of previous data.
        /// </summary>
        public void setRepo()
        {
            Repository repo = Repository.getInstance();
            if (repo != null)
                repo.setInstance();
        }

        /// <summary>
        /// This method is used by the client to identify the
        /// port number from which a response was sent. Since a 
        /// client can connect to multiple server, the client uses this
        /// method to find which server sent the response
        /// </summary>
        /// <returns>retuns the port number of the server</returns>
        public String GetPortNumber()
        {
            return serverPortNumber;
        }

        /// <summary>
        /// used to close the server host.
        /// </summary>
        public void close()
        {
            if (host != null)
                host.Close();
        }

        /// <summary>
        /// Test stub for the module. When the module is run as 
        /// a stand alone application then this is main entry point
        /// of the program
        /// </summary>
        /// <param name="args">Arguments for the program</param>
        static void Main(string[] args)
        {
            Console.Title = "BasicHttp Service Host";
            Console.Write("\n  Starting Programmatic Basic Service");
            Console.Write("\n =====================================\n");

            ServerHost server = null;
            try
            {
                server = new ServerHost();
                server.CreateReceiveChannel("http://localhost:4000/IBasicService");
                Console.Write("\n  Started BasicService - Press key to exit:\n");
                Console.ReadKey();
            }
            catch (Exception ex)
            {
                Console.Write("\n\n  {0}\n\n", ex.Message);
                Console.ReadKey();
                return;
            }

            server.close();
        }
    }//end of class
}//end of namespace