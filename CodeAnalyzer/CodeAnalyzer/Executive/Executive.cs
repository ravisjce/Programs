////////////////////////////////////////////////////////////////////////////////////////
// Executive.cs - Responsible for taking the input arguments and perform all oprations//
// Language:    C#, 2014, .Net Framework 4.5                                          //
// Platform:    Win8                                                                  //
// Application: Code Analyzer Tool to find type relationship and function complexity  //
// Author:      Ravi Nagendra , Syracuse University                                   //
////////////////////////////////////////////////////////////////////////////////////////
/*
 * Package Operations
 * ==================
 * This package is the entry point for the program. It takes the input parameter 
 * from the user and depending on the input options, various relationships are found.
 * This package interacts with commandlineparser, file manager, analyser, 
 * display and the xmlhandler
 */
using System;

namespace CodeAnalyzer
{
    public class Executive
    {
        CommandLineParser commandLineParser;
        FileMngr fileManager;
        Analyzer analyzer;
        Display display;
        XmlDisplay xmlDislay;
        
        string[] args;

        public string[] Args
        {
            get { return args; }
            set { args = value; }
        }

        public Display dislayInstance
        {
            get { return display; }
            set { display = value; }
        }

        //constructor in which all the member variables are initialised
        public Executive()
        {
            commandLineParser = new CommandLineParser();
            fileManager = new FileMngr();
            analyzer = new Analyzer();
            display = new Display();
            xmlDislay = new XmlDisplay();
        }
             
        //Method which starts the execution of all the functionalties in all the packages
        public bool startExecutive()
        {
            commandLineParser.Args = args;
            commandLineParser.parseCommandLines();

            fileManager.Patterns = commandLineParser.Patterns;
            fileManager.Recurse = commandLineParser.Recurse;

            foreach (string path in commandLineParser.DirectoryPaths)
                fileManager.findFiles(path);

            analyzer.Files = fileManager.getFiles();

            foreach (string path in commandLineParser.FilePaths)
                analyzer.Files.Add(path);

            analyzer.FindRelationship = commandLineParser.Relationships;
            analyzer.analyze();

            analyzer.generateXml = commandLineParser.Xml;

            display.displayTypesDetails();

            if (!commandLineParser.Relationships)
            {
                display.displayComplexityDetails();
            }
            else
            {
                display.displayRelationshipsDetails();
            }

            if (analyzer.generateXml == true)
            {
                xmlDislay.generateXmlTypesDetails();

                if (commandLineParser.Relationships)
                    xmlDislay.generateRelationshipsDetails();
                else
                    xmlDislay.generateXmlComplexityDetails();

                xmlDislay.saveXml();
            }
           
            return true;
        }
    }//end of class

#if(TEST_EXECUTIVE)
    class TestExecutive
    {
        //Entry point of the code analyzer tool
        static void Main(string[] args)
        {
            Executive mainExecutive = new Executive();

            try
            {
                if (args.Length == 0)
                {
                    mainExecutive.dislayInstance.displayNoInput();
                }
                else
                {
                    mainExecutive.Args = args;
                    mainExecutive.startExecutive();
                }
            }
            catch
            {
                Console.WriteLine("Error in execution. Check the input arguments");
            }
        }
    }//end of class
#endif
} // end of namespace
