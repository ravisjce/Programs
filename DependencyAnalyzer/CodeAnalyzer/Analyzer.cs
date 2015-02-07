//////////////////////////////////////////////////////////////////////////////////////////
// Analyzer.cs - Used for creating the relation analyzer object which finds various     //
// type and package dependency information                                              //
// Language:    C#, 2014, .Net Framework 4.5                                            //
// Platform:    Win8                                                                    //
// Application: Dependency Analyzer Tool to find type dependency and package dependency //
// Author:      Ravi Nagendra , Syracuse University                                     //
//////////////////////////////////////////////////////////////////////////////////////////

/*
 * Module Operations:
 * ------------------
 * This module Analyses the specified file. It creates the relationbuilder object
 * which uses parser module to find the type and package dependencies. Once the 
 * parser finds the type and package dependency, the results are stored in the 
 * relationbuilder object. This module is an intermediate layer for the executive
 * and parser module.
 * It requires the following 2 files
 *   Parser  - a collection of IRules
 *   RulesAndActions - collections of Rules and Actions
 */

using CodeAnalysis;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;

namespace CodeAnalyzer
{
    /// <summary>
    /// This class is used to create the code analyzer object
    /// which uses parser module to find the type and package dependencies.
    /// The final result will be stored in the code analyzer object which 
    /// acts as a centralised repository where all the results are present.
    /// </summary>
    public class Analyzer
    {
        //This contains the list of files for which analysis has to be performed.
        private List<string> files;

        /// <summary>
        /// Get and Set function for files 
        /// </summary>
        public List<string> Files
        {
            get { return files; }
            set { files = value; }
        }

        //This flag is used to check whether we need to find only type dependency or only package dependency
        private bool relationshipflag;

        /// <summary>
        /// Get and Set function for relationshipflag
        /// </summary>
        public bool Relationshipflag
        {
            get { return relationshipflag; }
            set { relationshipflag = value; }
        }

        /// <summary>
        /// This is the method where we create the code anlayser object.
        /// There will be only one code analyser object for all the files.
        /// It uses the parser module to find the relationship analysis.
        /// Once the results are generated, it will be stored in the centralised
        /// repo. We use this centralised repo from other module to know 
        /// the type and dependency analysis.
        /// </summary>
        /// <param name="serverName"></param>
        public void analyze(string serverName)
        {
            Console.Write("\n  CODE ANALYZER");
            Console.Write("\n ======================\n");

            CSsemi.CSemiExp semi = new CSsemi.CSemiExp();
            semi.displayNewLines = false;

            try
            {

                foreach (object file in files)
                {
                    Console.Write("\n\n  Processing file {0}\n", file as string);

                    if (!semi.open(file as string))
                    {
                        Console.Write("\n  Can't open {0}\n\n", file);
                        return;
                    }

                    Console.Write("\n  Type and Function Analysis");
                    Console.Write("\n ----------------------------\n");

                    BuildCodeAnalyzer builder = new BuildCodeAnalyzer(semi);
                    CodeAnalysis.Parser parser = builder.build();

                    Repository repo = Repository.getInstance();
                    repo.CurrentFileName = file.ToString();
                    Elem elem = getDefaultElemData(file.ToString(), serverName);
                    repo.locations.Add(elem);

                    try
                    {
                        while (semi.getSemi())
                            parser.parse(semi);
                    }
                    catch (Exception ex)
                    {
                        Console.Write("\n\n  {0}\n", ex.Message);
                    }
                    semi.close();
                }
            }
            catch (Exception)
            {
                Console.WriteLine("Error in the data. Exception thrown, pls check the input");
            }
        }

        /// <summary>
        /// This is an element which is added to the repository list which 
        /// indicates the package name for which analysis is performed.
        /// For each file there are many types and dependency results. 
        /// This element is later used to find the package name during 
        /// display and xml operation.
        /// </summary>
        /// <param name="package">Name of the package for which analysis is performed</param>
        /// <param name="serverName">The server name in which the package is found.
        /// Since there are multiple servers, we need to keep track of the server name
        /// for each file</param>
        /// <returns></returns>
        public Elem getDefaultElemData(string package, string serverName)
        {
            Elem temp = new Elem();
            temp.begin = 0;
            temp.end = 0;
            temp.type = "dummy";
            temp.packageName = package;
            temp.serverName = serverName;
            return temp;
        }

        /// <summary>
        /// This method is used for the second round of analysis. 
        /// Since there are multiple servers, we need to merge the partial type
        /// table of each of the server to find the depenendency across various 
        /// servers. Once the first parse is done, the client sends the type table
        /// of all other servers. At this point, we will merge the existing type
        /// table with other server type table and then use this updated table 
        /// to find the final dependeny results.
        /// </summary>
        public void analyzeSecondParse()
        {
            CSsemi.CSemiExp semi = new CSsemi.CSemiExp();
            semi.displayNewLines = false;

            try
            {
                foreach (object file in files)
                {
                    if (!semi.open(file as string))
                    {
                        Console.Write("\n  Can't open {0}\n\n", file);
                        return;
                    }

                    BuildCodeAnalyzerForRelationshipTypes builderreln = new BuildCodeAnalyzerForRelationshipTypes(semi);
                    CodeAnalysis.Parser parser = builderreln.build();

                    try
                    {
                        while (semi.getSemi())
                            parser.parse(semi);
                    }
                    catch (Exception ex)
                    {
                        Console.Write("\n\n  {0}\n", ex.Message);
                    }
                    semi.close();
                }
            }
            catch (Exception ex)
            {
                Console.Write("\n\n Error in the input data. Exception {0}. Please check the input data\n", ex.Message);
            }
        }
    }

#if(TEST_ANALYZE)
    class TestAnalyze
    {
        /// <summary>
        /// This is the test stub for the analyzer module.
        /// When the module is run as a standalone application
        /// then we call the main method and use the test stub
        /// to display the relevant information.
        /// </summary>
        /// <param name="args"></param>
        static void Main(string[] args)
        {
            List<string> paths = new List<string>();
            List<string> files = new List<string>();

            try
            {
                for (int i = 0; i < args.Length; i++)
                {
                    string[] newFiles = Directory.GetFiles(args[i], "*.cs");
                    ArrayList filesAL = new ArrayList();
                    for (int j = 0; j < newFiles.Length; ++j)
                    {
                        if (!newFiles[j].Contains("Temporary") && !newFiles[j].Contains("AssemblyInfo.cs"))
                            filesAL.Add(Path.GetFullPath(newFiles[j]));
                    }
                    files.AddRange((String[])filesAL.ToArray(typeof(string)));
                }

                Analyzer analyze = new Analyzer();
                analyze.Files = files;
                analyze.analyze("");
            }
            catch (Exception ex)
            {
                Console.Write("\n\n Error in the input data. Exception {0}. Please check the input data\n", ex.Message);
            }
        }
    }//end of class
#endif
}//end of namespace
