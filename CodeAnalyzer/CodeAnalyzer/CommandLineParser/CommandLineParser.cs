////////////////////////////////////////////////////////////////////////////////////////
// CommandLineParser.cs - Used for processing the input arguments                     //
// Language:    C#, 2014, .Net Framework 4.5                                          //
// Platform:    Win8                                                                  //
// Application: Code Analyzer Tool to find type relationship and function complexity  //
// Author:      Ravi Nagendra , Syracuse University                                   //
////////////////////////////////////////////////////////////////////////////////////////
/*
 * Package Operations
 * ==================
 * This package is responsible for processing the input argument list and creating two lists
 * namely, a list which contains all the filenames and a list which contains different patterns.
 * In addition there are flag variables which determine what all list needs to be displayed
 * 
 * Public Interface
 * ================
 * CommandLineParser clp = new CommandLineParser(); // constructs CommandLineParser object
 * clp.Args = args;   initialises the arguments list of CommandLineParser object
 */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace CodeAnalyzer
{
    public class CommandLineParser
    {
        bool recurse;   //This flag indicates whether the directories needs to be searched recursively     
        bool relationships;  //This flag indicates whether the type relationship has to be found or not
        bool xml; //This flag indicates whether the xml output needs to be generated or not
        List<string> patterns; //Contains list of patterns specified in the input file 
        string[] args;
        List<string> directoryPaths;
        List<string> filePaths;

        public bool Recurse
        {
            get { return recurse; }
            set { recurse = value; }
        }
          
        public bool Relationships
        {
            get { return relationships; }
            set { relationships = value; }
        } 
        public bool Xml
        {
            get { return xml; }
            set { xml = value; }
        }
        
        public List<string> Patterns
        {
            get { return patterns; }
            set { patterns = value; }
        }      

        public List<string> DirectoryPaths
        {
            get { return directoryPaths; }
            set { directoryPaths = value; }
        }

        public List<string> FilePaths
        {
            get { return filePaths; }
            set { filePaths = value; }
        }

        public string[] Args
        {
            get { return args; }
            set { args = value; }
        }

        public CommandLineParser()
        {
            recurse = false;
            relationships = false;
            xml = false;
            patterns = new List<string>();
            directoryPaths = new List<string>();
            filePaths = new List<string>();
        }

        /* The method is responsible for
         * creating an arraylist which contains list of patterns,
         * creating an arraylist which contains list of files,
         * sets the flag values depending on the input arguments
         */
        public bool parseCommandLines()
        {
            if (args.Length == 0)
            {
                Console.WriteLine("No Command Lines specified");
                return false;
            }

            for (int i = 0;i< args.Length;i++)
            {
                if (args[i].ToLower().Equals("/s"))
                {
                    recurse = true;
                }
                else if (args[i].ToLower().Equals("/r"))
                {
                    relationships = true;
                }
                else if (args[i].ToLower().Equals("/x"))
                {
                    xml = true;
                }
                else if (args[i].StartsWith("*."))
                {
                    patterns.Add(args[i]);
                }
                else if (Directory.Exists(args[i]) && i<args.Length)
                {
                    if (Directory.Exists(args[i + 1]) || args[i+1].ToLower().Equals("/s") || args[i+1].ToLower().Equals("/r")
                        || args[i + 1].ToLower().Equals("/x") || args[i + 1].StartsWith("*.") || (i == args.Length-1))
                    {
                        directoryPaths.Add(args[i]);
                    }
                    else
                    {
                        String filePath = args[i] + "/" + args[i + 1];
                        filePaths.Add(Path.GetFullPath(filePath));
                        i++;
                    }                        
                }
                else
                {
                    int directoryPathIndex = i-1;
                    for (int j = i-1; j >= 0; j--)
                        if (Directory.Exists(args[j]))
                        {
                            directoryPathIndex = j;
                            break;
                        }
                    String filePath = args[directoryPathIndex] + "/" + args[i];

                    try
                    {
                        filePaths.Add(Path.GetFullPath(filePath));
                    }
                    catch
                    {
                        Console.WriteLine("Error in input parameter. Check the input list");
                        return false;
                    }
                }
            }          
            return true;
        } 

        //Used to represent all the class members in string format
        public override string ToString()
        {
            StringBuilder temp = new StringBuilder();
            temp.Append(String.Format("\n/X present: {0}", this.xml));
            temp.Append(String.Format("\n/S present: {0}", this.recurse));
            temp.Append(String.Format("\n/R present: {0}", this.relationships));
            temp.Append("\npattern name specified:");
            foreach (string pattern in this.patterns)
                temp.Append(String.Format(" {0} ", pattern));
            temp.Append("\nPaths are:");
            foreach (string path in this.directoryPaths)
                temp.Append(String.Format("{0} ", path));

            return temp.ToString();
        }
    }//end of class

#if(TEST_CLPARSER)
    class TestCLParser
    {
        //Test Stub
        static void Main(string[] args)
        {
            CommandLineParser clp = new CommandLineParser();
            clp.Args = args;
            Console.WriteLine("Command Line Arguements ");

            /* If the package is run as stand alone application
             * then add the default values for the member variables
            */ 
            try
            {
                foreach (string arg in args)
                {
                    Console.Write("  {0}", arg);
                }
                Console.Write("\n");
                if (clp.parseCommandLines())
                {
                    Console.WriteLine("\nParsed Command Line Data");
                    Console.WriteLine(clp.ToString());
                }
                else
                {
                    Console.WriteLine("Nothing to Parse");
                }
            }
            catch 
            {
                Console.WriteLine("Error during command line parsing");
            }
        }
    }//end of class
#endif
} // end of namespace
