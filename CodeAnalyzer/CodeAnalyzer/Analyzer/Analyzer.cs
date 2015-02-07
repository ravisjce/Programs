////////////////////////////////////////////////////////////////////////////////////////
// Analyzer.cs - Used for calling type relationship analysis                          //
// Language:    C#, 2014, .Net Framework 4.5                                          //
// Platform:    Win8                                                                  //
// Application: Code Analyzer Tool to find type relationship and function complexity  //
// Author:      Ravi Nagendra , Syracuse University                                   //
////////////////////////////////////////////////////////////////////////////////////////
/*
 * Package Operations
 * ==================
 * The analyzer module is used to call the relationship analysis 
 * and function complexties analysis for each file in the given 
 * input arguments.
 * For finding the relationship analysis, there is a 2nd parse of the 
 * same files used in the first parse.
 * Public Interface
 * ================
 * Analyzer analyze = new Analyzer();   // constructs Analyzer object
 * List<string> files = new List<string>(); // List which contains input filepath
 */


using CodeAnalysis;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;

namespace CodeAnalyzer
{
    public class Analyzer
    {
        List<string> files;     
        bool findRelationship;  //The flag is set to true if user gives /R option in the input
        bool enableXmlOutput;   //The flag is set to true if user gives /X option in the input

        public List<string> Files
        {
            get { return files; }
            set { files = value; }
        }
        
        public bool FindRelationship
        {
            get { return findRelationship; }
            set { findRelationship = value; }
        }

        public bool generateXml
        {
            get { return enableXmlOutput; }
            set { enableXmlOutput = value; }
        }

        /* This method is used to call the relationship and complexity analysis 
         * for each file in the input set         
         */
        public void analyze()
        {
            CSsemi.CSemiExp semi = new CSsemi.CSemiExp();
            semi.displayNewLines = false;
            /* These are the supported file formats in this tool. If input file is not in this format, 
             * then analysis will not be done*/
            string[] supportedFileFormatList = { ".cs", ".c", ".cpp", ".java", ".txt", ".bat"}; 

            foreach (object file in files)
            {
                string fileExtension = Path.GetExtension(file.ToString());
                bool supportedFileFormat = false;
               
                foreach (string currentFileExtension in supportedFileFormatList)
                {
                    if (fileExtension == currentFileExtension)
                        supportedFileFormat = true;
                }

                if (!supportedFileFormat)
                {
                    Console.WriteLine("\nThe file {0} is of unsupported format", file.ToString());
                    continue;
                }
                if (!semi.open(file as string))
                {
                    Console.Write("\n  Can't open {0}\n\n", file);
                    return;
                }
                
                BuildCodeAnalyzer builder = new BuildCodeAnalyzer(semi);
                CodeAnalysis.Parser parser = builder.build();

                Repository repo = Repository.getInstance();
                Elem elem = getDefaultElemData(file.ToString());
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

                //Only when the user specifies the /R option, we do relationship analysis
                if (findRelationship)
                {
                    semi = new CSsemi.CSemiExp();
                    semi.displayNewLines = false;
                    if (!semi.open(file as string))
                    {
                        Console.Write("\n  Can't open {0}\n\n", file);
                        return;
                    }

                    BuildCodeAnalyzerForRelationshipTypes builderreln = new BuildCodeAnalyzerForRelationshipTypes(semi);
                    parser = builderreln.build();
                                      
                    try
                    {
                        while (semi.getSemi())
                            parser.parse(semi);
                    }
                    catch (Exception ex)
                    {
                        Console.Write("\n\n  {0}\n", ex.Message);
                    }
                }
                semi.close();
            }
        }
        /*This element will be inserted in the repository as the first element for each file.
         * This is used to find the filename in which the specified type is present
        */		
        public Elem getDefaultElemData(string fullFilePath)
        {
            Elem elem = new Elem();
            elem.begin = 0;
            elem.type = "dummy";
            elem.end = 0;
            elem.filepath = fullFilePath.ToString();
            return elem;
        }	      
    }//end of class

#if(TEST_ANALYZE)
    class TestAnalyze
    {
        //Test Stub
        static void Main(string[] args)
        {
            List<string> files = new List<string>();

            if (args.Length == 0)
            {
                Console.WriteLine("Nothing to display");
            }

            else
            {
                try
                {
                    /* If the package is run as stand alone application
                     * then add the default values for the member variables
                     */ 
                    Analyzer analyze = new Analyzer();
                    string[] newFiles = Directory.GetFiles(args[0], "*.cs");
                    ArrayList filesAL = new ArrayList();

                    for (int j = 0; j < newFiles.Length; ++j)
                    {
                        if (!newFiles[j].Contains("Temporary") && !newFiles[j].Contains("AssemblyInfo.cs"))
                            filesAL.Add(Path.GetFullPath(newFiles[j]));
                    }
                    files.AddRange((String[])filesAL.ToArray(typeof(string)));

                    analyze.Files = files;
                    analyze.analyze();
                    Repository rep = Repository.getInstance();
                    List<Elem> table = rep.locations;

                    Console.WriteLine("\n\nType             Name                Begin       End    ");
                    Console.Write("==============================================================");
                    foreach (Elem e in table)
                    {
                        if (e.begin != 0 && e.end != 0)
                            Console.Write("\n  {0,10}, {1,25}, {2,5}, {3,5}", e.type, e.name, e.begin, e.end);
                    }
                    Console.WriteLine();
                }
                catch
                {
                    Console.WriteLine("Error in analyser. Check the input arguments");
                }
            }
        }
    }//end of class
#endif
} // end of namespace
