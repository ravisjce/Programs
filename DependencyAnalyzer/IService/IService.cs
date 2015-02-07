//////////////////////////////////////////////////////////////////////////////////////////
// IService.cs - Used for creating the GUI for the client process                //
// Language:    C#, 2014, .Net Framework 4.5                                            //
// Platform:    Win8                                                                    //
// Application: Dependency Analyzer Tool to find type dependency and package dependency //
// Author:      Ravi Nagendra , Syracuse University                                     //
//////////////////////////////////////////////////////////////////////////////////////////

/*
 * Module Operations:
 * ------------------
 * This is an interface module which defines the service contract for the WCF. The 
 * server and client use this service contract to communicate with each other.
 * The client can access only those methods of the server for which operation contract
 * is defined. All the methods in the interface are defined by the server. 
 */

//This package does not contain any test stub as it a an interface for ServiceContract

using CodeAnalysis;
using System.Collections.Generic;
using System.ServiceModel;
using System;


namespace HandCraftedService
{
    /// <summary>
    /// Service Contract for the namespace
    /// </summary>
    [ServiceContract(Namespace = "HandCraftedService")]
    public interface IBasicService
    {
        /// <summary>
        /// This method is used by the client to send request to the server
        /// </summary>
        /// <param name="message">Message which is sent by the client</param>
        [OperationContract]
        void sendMessage(string message);


        /// <summary>
        /// This method is used by the client to get a response from the server
        /// </summary>
        /// <returns>Response message from the server</returns>
        [OperationContract]
        string getMessage();

        /// <summary>
        /// This method is used by the client to get the list of test files
        /// which are present in the server
        /// </summary>
        /// <param name="path">The path in which the server files are present</param>
        /// <param name="recurse">Specifies whether the server files present 
        /// in the subdirectories of test folder needs to sent from the server
        /// to the client</param>
        /// <returns>Returns the list of files present in the server test folder</returns>
        [OperationContract]
        List<ProjectTree> getFilesList(string path, bool recurse);

        /// <summary>
        /// This is used by the client to the list of files
        /// for which analysis needs to be performed by the server
        /// </summary>
        /// <param name="files">List of files for which analysis has to be done</param>
        /// <param name="serverName">Name of the server for which the file list are set</param>
        /// <returns>Returns the repo which is created after analyzing all the files</returns>
        [OperationContract]
        List<Elem> setFilesList(List<string> files, string serverName);


        /// <summary>
        /// This method is used by the client to send the merged type table 
        /// to each of the server. The client merges the partial type table
        /// obtained from each of the servers and sends this merged type table
        /// to the servers. The individual server then uses this merged type table
        /// to do the second round of analysis
        /// </summary>
        /// <param name="typeTable">The merged type table which is sent by the client to the server</param>
        [OperationContract]
        void sendMergedTypeTable(List<Elem> typeTable);

        /// <summary>
        /// This method is used to indicate the server to start the
        /// second round of analysis. Once the server receives the merged type 
        /// table from the client, it has to start the second round of analysis.
        /// Once the server starts the second round of analysis, it will create 
        /// the final repo table which will be used by the client.
        /// </summary>
        /// <param name="files">List of files for which the analysis has to be performed</param>
        [OperationContract]
        void startSecondAnalysis(List<string> files);


        /// <summary>
        /// This method is used by the client to the repository of the servers.
        /// The client does the merging of the partial type table of all the servers.
        /// So client uses this method to fetch the repo from the server. 
        /// </summary>
        /// <returns>Returns the repository of the server to the client</returns>
        [OperationContract]
        Repository getRepo();

        /// <summary>
        /// This method is used to set the repository of the individual servers.
        /// When the server is active and if there are requests from multiple 
        /// clients then the repository has to reinitialised or else the result 
        /// will be appened on top of previous data.
        /// </summary>
        [OperationContract]
        void setRepo();

        /// <summary>
        /// This method is used by the client to find the list of 
        /// all the files present in the test folder of the server
        /// </summary>
        /// <param name="portNumber">port number to identify the server</param>
        /// <returns>Returns the list of all the files present in the test folder</returns>
        [OperationContract]
        string getServerFilesPath(String portNumber);

        /// <summary>
        /// This method is used by the client to get the list of 
        /// patterns which is used by the server to generate the file list.
        /// </summary>
        /// <returns>Returns the list of patterns which specifies the type
        /// of files for which analysis will be done</returns>
        [OperationContract]
        List<string> getPatterns();

        /// <summary>
        /// This method is used by the client to set the port number 
        /// for any any requests. Since a client can connect to multiple 
        /// servers, whenever the client wants to send a request to the 
        /// server, it has to identify the server name. This is done by
        /// using the portnumber of the server
        /// </summary>
        /// <param name="portnumber">The server portnumber from which
        /// response is excepted</param>
        [OperationContract]
        void SetPortNumber(String portnumber);

        /// <summary>
        /// This method is used by the client to identify the
        /// port number from which a response was sent. Since a 
        /// client can connect to multiple server, the client uses this
        /// method to find which server sent the response
        /// </summary>
        /// <returns>retuns the port number of the server</returns>
        [OperationContract]
        String GetPortNumber();
    }//end of class
}//end of namespace
