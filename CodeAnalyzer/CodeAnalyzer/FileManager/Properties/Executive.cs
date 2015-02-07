//////////////////////////////////////////////////////////////////////////////////
// Executive.cs -                                                       //
// Language:    C#, 2014, .Net Framework 4.5                                    //
// Platform:    Win8                                                            //
// Application: Code Analyzer Tool, Project #2, Fall 2014                       //
// Author:      Ravi Nagendra, Syracuse University                              //
//////////////////////////////////////////////////////////////////////////////////
/*
 * Module Operations:
 * ------------------
 * 
 * 
 */
using System;

namespace CodeAnalyzer
{
    public class Executive
    {
        private CommandLineParser clp;
        private FileMngr fm;
        private Analyzer analyzer;
        private Display display;
        private string[] args;

        public string[] Args
        {
            get { return args; }
            set { args = value; }
        }

        public Executive()
        {
            clp = new CommandLineParser();
            fm = new FileMngr();
            analyzer = new Analyzer();
            display = new Display();
        }

        /// <summary>
        /// calls all the core logic of the tool
        /// </summary>
        /// <returns></returns>
        public bool runExecutive()
        {
            clp.Args = args;
            clp.parseCommandLines();

            fm.Patterns = clp.Patterns;
            fm.Recurse = clp.Recurse;
            foreach (string path in clp.DirectoryPaths)
                fm.findFiles(path);

            analyzer.Files = fm.getFiles();

            foreach (string path in clp.FilePaths)
                analyzer.Files.Add(path);

            analyzer.FindRelationship = clp.Relationships;
            analyzer.analyze();

            display.displayTypesDetails();

            if (!clp.Relationships)
            {
                display.displayComplexityDetails();
            }
            else
            {
                display.displayRelationshipsDetails();
            }

            return true;
        }
    }

#if(TEST_EXECUTIVE)
    class TestExecutive
    {
        static void Main(string[] args)
        {
            Executive ex = new Executive();
            ex.Args = args;
            ex.runExecutive();            
        }
    }
#endif
}
