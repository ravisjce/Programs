/*
 * FileMgr.cs - Prototype of Pr#2 FileMgr
 * 
 * Platform:    Surface Pro 3, Win 8.1 pro, Visual Studio 2013
 * Application: CSE681 - SMA Helper
 * Author:      Jim Fawcett, yada, yada, yada
 *
 * Modified by: Ravi Nagendra
 * 
 * Maintenance History:
 * --------------------
 * ver 1.1 : 13 october 2014
 * -Added a check to skip the file addition to the list if
 * the input file is a temporary file or assemblyinfo file
 * 
 * -Added get and set method to access the private variables 
 * from other class.
 * 
 * -Added exception handing which handles various scenarios 
 * where input filename is invalid
 */
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;

namespace CodeAnalyzer
{
    public class FileMngr
    {
        List<string> files;
        private bool recurse;
        private List<string> patterns;

        //constructor in which all the member variables are initialised
        public FileMngr()
        {
            files = new List<string>();
            patterns = new List<string>();
        }
        public List<string> Files
        {
            get { return files; }
            set { files = value; }
        }

        public bool Recurse
        {
            get { return recurse; }
            set { recurse = value; }
        }        

        public List<string> Patterns
        {
            get { return patterns; }
            set { patterns = value; }
        }

        //returns the list which contains all the files which needs to be analysed
        public List<string> getFiles()
        {
            return files;
        }

        /*Used to create the list of files from the given input. If /S option is
        * specified in the input then the subdirectory files are also added
        */
        public void findFiles(string path)
        {
            try
            {
                foreach (string pattern in patterns)
                {
                    string[] newFiles = Directory.GetFiles(path, pattern);
                    ArrayList filesAL = new ArrayList();
                    for (int i = 0; i < newFiles.Length; ++i)
                    {
                        if (!newFiles[i].Contains("Temporary") && !newFiles[i].Contains("AssemblyInfo.cs"))
                            filesAL.Add(Path.GetFullPath(newFiles[i]));
                    }
                    files.AddRange((String[])filesAL.ToArray(typeof(string)));
                }
                if (recurse)
                {
                    string[] dirs = Directory.GetDirectories(path);
                    foreach (string dir in dirs)
                        findFiles(dir);
                }
            }
            catch (DirectoryNotFoundException)
            {
                Console.WriteLine("\nThe given directory cannot be found {0}\n", path);
            }
            catch (FileNotFoundException)
            {
                Console.WriteLine("\nThe given file cannot be found {0}\n", path);
            }            
            catch (PathTooLongException)
            {
                Console.WriteLine("\nThe given path is too long to read {0}\n", path);
            }
            catch (Exception)
            {
                Console.WriteLine("\nError in {0}\n", path);
            }
        }
    }//end of class

#if(TEST_FILEMGR)
    class TestFileMgr
    {
        //Test Stub
        static void Main(string[] args)
        {
            /* If the package is run as stand alone application
             * then add the default values for the member variables
             */ 
            try
            {
                Console.Write("\n  Testing File Manager Class");
                Console.Write("\n =======================\n");

                FileMngr fm = new FileMngr();
                fm.Patterns.Add("*.cs");
                fm.findFiles(".");
                List<string> files = fm.getFiles();
                foreach (string file in files)
                    Console.Write("\n  {0}", file);
                Console.Write("\n\n");
                Console.ReadLine();
            }
            catch
            {
                Console.Write("\n Error in filemanager. check the input parameters");               
            }
        }
#endif
    }//end of class
} // end of namespace
