//////////////////////////////////////////////////////////////////////////////////////////
// ClientService.cs - This module is responsible for the interaction between client     //
// and the server. It creates the channel required for the communication between client //
// and server. All the requests are sent and received via this module to the server     //
// Language:    C#, 2014, .Net Framework 4.5                                            //
// Platform:    Win8                                                                    //
// Application: Dependency Analyzer Tool to find type dependency and package dependency //
// Author:      Ravi Nagendra , Syracuse University                                     //
//////////////////////////////////////////////////////////////////////////////////////////

/*
 * Module Operations:
 * ------------------
 * This module is used to perform all the backend operation for
 * the client executive module. It is mainly responsible for creating 
 * the communication channel through which the client communicates with 
 * the server. All the requests are sent and received via the channel created
 * in this module. This module is also responsible for merging the type table
 * of all the servers. The client executive uses this module to access the 
 * merged table which is used to display the results.
 */

using CodeAnalysis;
using System;
using System.Collections.Generic;
using System.ServiceModel;

namespace HandCraftedService
{
    /// <summary>
    /// This class is responsible for creating the commnication channel 
    /// through which the requests are sent and received between client
    /// and server
    /// </summary>
    public class ClientService
    {
        /// <summary>
        /// Default constructor of the ProgClient Class
        /// </summary>        
        public ClientService()
        {

        }

        /// <summary>
        /// This method is used to create the communication channel 
        /// for the client. The client uses this channel for all the
        /// operation with the server. The channel is used to send and
        /// receive the requests from the server. 
        /// </summary>
        /// <param name="url"></param>
        /// <returns></returns>
        public IBasicService CreateSendChannel(string url)
        {
            BasicHttpBinding binding = new BasicHttpBinding();
            EndpointAddress address = new EndpointAddress(url);
            ChannelFactory<IBasicService> factory = new ChannelFactory<IBasicService>(binding, address);
            return factory.CreateChannel();
        }


        /// <summary>
        /// This method is used to send the request to a server from the
        /// client. Once the client is connected to the server, we send
        /// a test message to check whether connection was successful or
        /// not. The message are sent using the service object which is
        /// derived from the WCF interface classs IBasicService.
        /// </summary>
        /// <param name="serviceObject"></param>
        /// <param name="msg"></param>
        public void SendMessage(IBasicService serviceObject, string msg)
        {
            try
            {
                serviceObject.sendMessage(msg);
            }
            catch (Exception)
            {
                Console.Write("\n\n The service object is null. Please check the input");
            }
        }


        /// <summary>
        /// This method is used to get the response from a server
        /// Once the client is connected to the server, we get
        /// a test message from the server to check whether connection
        /// was successful or not. The message are received using the service object which is
        /// derived from the WCF interface classs IBasicService.
        /// </summary>
        /// <param name="serviceObject"></param>
        /// <returns></returns>
        public string GetMessage(IBasicService serviceObject)
        {
            try
            {
                return serviceObject.getMessage();
            }
            catch (Exception)
            {
                Console.Write("\n\n The service object is null. Please check the input");
                return "";
            }
        }


        /// <summary>
        /// This method is used to get the list of files from the server test
        /// folder. Initially all the files of the server are present in a 
        /// test folder which is not known to the client. When the client connects
        /// to the server, client uses this method to get the list of files 
        /// present in the server. Now user can select any file in this set for 
        /// which analysis will be performed. 
        /// </summary>
        /// <param name="serviceObject">The service object through which the 
        /// request is sent to the server</param>
        /// <param name="path">This indicates the path from which the files 
        /// have to fetched. This path is initially selected in the server.
        /// Once the path is set in the server, the client gets the path 
        /// of the server test folder and then uses it to get the full file list</param>
        /// <param name="recurse">This option is used to specify whether the files which
        /// are present in subdirectories of the server test folder needs to fetched.</param>
        /// <returns></returns>
        public List<ProjectTree> GetFilesList(IBasicService serviceObject, string path, bool recurse)
        {
            try
            {
                return serviceObject.getFilesList(path, recurse);
            }
            catch (Exception)
            {
                Console.Write("\n\n The service object is null. Please check the input");
                return null;
            }
        }


        /// <summary>
        /// The client uses this method to send the list of files for which analysis 
        /// has to be performed. Initally the client gets the list of all the files
        /// present in the test folder of the server. User can then select individual 
        /// files from this list. The analysis will be performed only for these selected
        /// files. So this method sends the final files list from which analysis needs to
        /// be performed.
        /// </summary>
        /// <param name="serviceObject">The service object through which filelist is 
        /// sent to the server from the client</param>
        /// <param name="files">List of files for which analysis needs to be performed.</param>
        /// <param name="serverName">Name of the server in which analysis has to be done.
        /// Since there are multiple servers, they are identified using the servername</param>
        /// <returns>Returns the analysis result which is sent by the server.</returns>
        public List<Elem> SetFilesList(IBasicService serviceObject, List<string> files, string serverName)
        {
            try
            {
                List<Elem> table = serviceObject.setFilesList(files, serverName);
                return table;
            }
            catch (Exception)
            {
                Console.Write("\n\n The service object is null. Please check the input");
                return null;
            }
        }

        /// <summary>
        /// This method is used to merge the partial type table of obtained from all
        /// the servers. There may be type or package dependency across servres. In this
        /// scenario, the client has to merge all the partial tables so get the final 
        /// analysis result. Once the partial type tables are merged, the client sends
        /// this merged table to the individual servers where the second round of analysis
        /// will be done. 
        /// </summary>
        /// <param name="typeTable1">The partial type table of server1</param>
        /// <param name="typeTable2">The partial type table of server2</param>
        /// <returns></returns>
        public List<Elem> mergeTypeTables(List<Elem> typeTable1, List<Elem> typeTable2)
        {
            try
            {
                List<Elem> mergedList = new List<Elem>();
                if (typeTable1 == null)
                {
                    return typeTable2;
                }

                if (typeTable2 == null)
                {
                    return typeTable1;
                }

                foreach (Elem elem in typeTable1)
                {
                    mergedList.Add(elem);
                }

                foreach (Elem elem in typeTable2)
                {
                    mergedList.Add(elem);
                }

                return mergedList;
            }
            catch (Exception)
            {
                Console.Write("\n\n Error in the input type tables. Please check the input");
                return null;
            }
        }

        /// <summary>
        /// This method is used to send the merged type table to individual servers.
        /// Since ther are multiple servers involved in this project, there will be 
        /// partial type tables from each of the servers. These partial table needs
        /// to be merged. This is done by the client. Once client merges the type 
        /// table, it is sent back to each of the individual servers.After this 
        /// the server does a second round of analysis using the merged table
        /// </summary>
        /// <param name="serviceObject">The service object through which communication
        /// between client and server are carried out</param>
        /// <param name="typeTableMerged">Merged type table which contains list of 
        /// all the types of all the servers.
        /// </param>
        public void SendMergedTypeTable(IBasicService serviceObject, List<Elem> typeTableMerged)
        {
            try
            {
                serviceObject.sendMergedTypeTable(typeTableMerged);
            }
            catch (Exception)
            {
                Console.Write("\n\n The service object is null. Please check the input");
            }
        }


        /// <summary>
        /// This method is used to start the second round of analysis for the files.
        /// Since there are multiple servers involved in the system, second round of 
        /// analysis is required once partial type table for each of the server is 
        /// available. The client merges these partial type tables and then sends
        /// it to the server. After the server gets this merged type table, the 
        /// server has to start the second round of analysis on this merged type table.
        /// This method is used to start the second round of analysis in the server.
        /// </summary>
        /// <param name="serverObject">The server object in which the analysis has to be done</param>
        /// <param name="files">List of files for which analysis is needed</param>
        public void GetAnalyzedData(IBasicService serverObject, List<string> files)
        {
            try
            {
                serverObject.startSecondAnalysis(files);
            }
            catch (Exception)
            {
                Console.Write("\n\n The service object is null. Please check the input");
            }
        }

        /// <summary>
        /// This is used to the repository of the servers. Client needs the reporitory
        /// of each of the server to create the merged type table. Every time when the
        /// server is done with the analysis, the client needs to get the updated 
        /// repository from the server. 
        /// </summary>
        /// <param name="serviceObject">The server object in which the repo is present</param>
        /// <returns></returns>
        public Repository GetRepo(IBasicService serviceObject)
        {
            try
            {
                return serviceObject.getRepo();
            }
            catch (Exception)
            {
                Console.Write("\n\n The service object is null. Please check the input");
                return null;
            }
        }

        /// <summary>
        /// This method is used to merge the repositories of all the servers. After 
        /// the first round of analysis in all the servers, the client merges these
        /// partial type talbes and sends them to each of the server. After this the
        /// server performs second round of analysis and generates the final repo which
        /// has the complete result. Since each of the servers has this repo, the client
        /// needs to merge these final repos from each of the server. The client appends
        /// the repo of one server to other
        /// </summary>
        /// <param name="repo1">The repository of the first server</param>
        /// <param name="repo2">The repository of the second server</param>
        /// <returns></returns>
        public Repository mergeRepo(Repository repo1, Repository repo2)
        {
            try
            {
                List<PackageDependencyElement> packageElement1 = repo1.packagedata;
                List<PackageDependencyElement> packageElement2 = repo2.packagedata;

                foreach (PackageDependencyElement pe in packageElement2)
                    packageElement1.Add(pe);

                repo1.inheritancedataList = getMergedInheritanceList(repo1, repo2);
                repo1.aggregationdataList = getMergedAggregationList(repo1, repo2);
                repo1.compositiondataList = getMergedCompositionList(repo1, repo2);
                repo1.usingdataList = getMergedUsingList(repo1, repo2);
                repo1.packagedata = packageElement1;

                return repo1;
            }
            catch (Exception)
            {
                Console.Write("\n\n Input repository is not proper. Please check the data");
                return null;
            }
        }


        /// <summary>
        /// This method is used to merge the inheritance type tables of the
        /// individual servers. Once all the servers are done with the second
        /// round of analysis, the updated type tables have to be merged. This 
        /// method merges the inheritance list from all the servers.
        /// </summary>
        /// <param name="repo1">The repository of the first server</param>
        /// <param name="repo2">The repository of the second server</param>
        /// <returns>The merged list which contains all the unique inheritance element</returns>
        private List<InheritanceElement> getMergedInheritanceList(Repository repo1, Repository repo2)
        {
            List<InheritanceElement> inheritancedata1 = repo1.inheritancedataList;
            List<InheritanceElement> inheritancedata2 = repo2.inheritancedataList;
            bool present = false;

            try
            {
                if (inheritancedata2.Count > inheritancedata1.Count)
                {
                    inheritancedata1 = repo2.inheritancedataList;
                    inheritancedata2 = repo1.inheritancedataList;
                }

                foreach (InheritanceElement ie in inheritancedata2)
                {
                    foreach (InheritanceElement temp in inheritancedata1)
                    {
                        if (temp.ToString().Equals(ie.ToString()))
                        {
                            present = true;
                            break;
                        }
                    }
                    if (!present)
                        inheritancedata1.Add(ie);
                    present = false;
                }

                return inheritancedata1;
            }
            catch (Exception)
            {
                Console.WriteLine("Error while merging ");
                return null;
            }
        }

        /// <summary>
        /// This method is used to merge the aggregation type tables of the
        /// individual servers. Once all the servers are done with the second
        /// round of analysis, the updated type tables have to be merged. This 
        /// method merges the aggregation list from all the servers.
        /// </summary>
        /// <param name="repo1">The repository of the first server</param>
        /// <param name="repo2">The repository of the second server</param>
        /// <returns>The merged list which contains all the unique aggregation element</returns>
        private List<AggregationElement> getMergedAggregationList(Repository repo1, Repository repo2)
        {
            List<AggregationElement> aggregationdata1 = repo1.aggregationdataList;
            List<AggregationElement> aggregationdata2 = repo2.aggregationdataList;
            bool present = false;

            try
            {
                if (aggregationdata2.Count > aggregationdata1.Count)
                {
                    aggregationdata1 = repo2.aggregationdataList;
                    aggregationdata2 = repo1.aggregationdataList;
                }

                foreach (AggregationElement ae in aggregationdata2)
                {
                    foreach (AggregationElement temp in aggregationdata1)
                    {
                        if (temp.ToString().Equals(ae.ToString()))
                        {
                            present = true;
                            break;
                        }
                    }
                    if (!present)
                        aggregationdata1.Add(ae);
                    present = false;
                }

                return aggregationdata1;
            }
            catch (Exception)
            {
                Console.WriteLine("Error while merging ");
                return null;
            }
        }

        /// <summary>
        /// This method is used to merge the composition type tables of the
        /// individual servers. Once all the servers are done with the second
        /// round of analysis, the updated type tables have to be merged. This 
        /// method merges the composition list from all the servers.
        /// </summary>
        /// <param name="repo1">The repository of the first server</param>
        /// <param name="repo2">The repository of the second server</param>
        /// <returns>The merged list which contains all the unique composition element</returns>
        private List<CompositionElement> getMergedCompositionList(Repository repo1, Repository repo2)
        {
            List<CompositionElement> compositiondataList1 = repo1.compositiondataList;
            List<CompositionElement> compositiondataList2 = repo2.compositiondataList;
            bool present = false;

            try
            {
                if (compositiondataList2.Count > compositiondataList1.Count)
                {
                    compositiondataList1 = repo2.compositiondataList;
                    compositiondataList2 = repo1.compositiondataList;
                }

                foreach (CompositionElement ce in compositiondataList2)
                {
                    foreach (CompositionElement temp in compositiondataList1)
                    {
                        if (temp.ToString().Equals(ce.ToString()))
                        {
                            present = true;
                            break;
                        }
                    }
                    if (!present)
                        compositiondataList1.Add(ce);
                    present = false;
                }

                return compositiondataList1;
            }
            catch (Exception)
            {
                Console.WriteLine("Error while merging ");
                return null;
            }
        }

        /// <summary>
        /// This method is used to merge the using type tables of the
        /// individual servers. Once all the servers are done with the second
        /// round of analysis, the updated type tables have to be merged. This 
        /// method merges the using list from all the servers.
        /// </summary>
        /// <param name="repo1">The repository of the first server</param>
        /// <param name="repo2">The repository of the second server</param>
        /// <returns>The merged list which contains all the unique using element</returns>
     
        private List<UsingElement> getMergedUsingList(Repository repo1, Repository repo2)
        {
            List<UsingElement> usingdata1 = repo1.usingdataList;
            List<UsingElement> usingdata2 = repo2.usingdataList;
            bool present = false;

            try
            {
                if (usingdata2.Count > usingdata1.Count)
                {
                    usingdata1 = repo2.usingdataList;
                    usingdata2 = repo2.usingdataList;
                }

                foreach (UsingElement ue in usingdata2)
                {
                    foreach (UsingElement temp in usingdata1)
                    {
                        if (temp.ToString().Equals(ue.ToString()))
                        {
                            present = true;
                            break;
                        }
                    }
                    if (!present)
                        usingdata1.Add(ue);
                    present = false;
                }

                return usingdata1;
            }
            catch (Exception)
            {
                Console.WriteLine("Error while merging ");
                return null;
            }
        }

        /// <summary>
        /// This is used to set the repository of the servers. When server process
        /// a request to do the analysis, it will store the result in its repo. If
        /// a new client makes a connection then the repository contents of each of 
        /// the server should be set. This method does this task.
        /// </summary>
        /// <param name="svc"></param>
        public void SetRepo(IBasicService svc)
        {
            try
            {
                svc.setRepo();
            }
            catch (Exception)
            {
                Console.Write("\n\n The service object is null. Please check the input");
            }
        }

        /// <summary>
        /// Test stud for the program. This is the entry point of the program 
        /// when the module is run as a stand alone application.
        /// </summary>
        /// <param name="args">Command line parameters</param>
        static void Main(string[] args)
        {
            Console.Title = "BasicHttp Client";
            Console.Write("\n  Starting Programmatic Basic Service Client");
            Console.Write("\n ============================================\n");

            string url = "http://localhost:4000/IBasicService";
            ClientService client = new ClientService();
            while (true)
            {
                try
                {
                    IBasicService svc = client.CreateSendChannel(url);

                    string msg = "This is a test message from client";
                    client.SendMessage(svc, msg);

                    msg = client.GetMessage(svc);
                    Console.Write("\n  Message recieved from Service: {0}\n\n", msg);
                    List<ProjectTree> files = client.GetFilesList(svc, ".", false);
                    //foreach (string file in files)
                    //  Console.WriteLine("{0}\n", file);
                    //client.SetFilesList(svc, files, "server1");
                    break;
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Exception thrown " + ex.Message);
                    Console.WriteLine("Retrying...");
                }
            }
            Console.ReadKey();
        }

    }//end of class
}//end of namespace