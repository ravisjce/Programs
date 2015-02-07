//////////////////////////////////////////////////////////////////////////////////////////
// FileManager.cs - Used for creating the GUI for the client process                //
// Language:    C#, 2014, .Net Framework 4.5                                            //
// Platform:    Win8                                                                    //
// Application: Dependency Analyzer Tool to find type dependency and package dependency //
// Author:      Ravi Nagendra , Syracuse University                                     //
//////////////////////////////////////////////////////////////////////////////////////////

/*
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

/* Module operation: This module is responsible for creating the list of files 
 * which are present in a directory. It creates the list of files in the parent directory
 * or in the subdirectories depending on the input parameter. 
 * The parent folder is considered as one project. All the files present in the directory
 * is considered as the packages of this project
 */


using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;

namespace CodeAnalysis
{
    /// <summary>
    /// The class structure used to reprent the project and its packages.
    /// Each folder is considered as a package and all the files in this
    /// folder is considered as its package.
    /// </summary>
    public class ProjectTree
    {
        /// <summary>
        /// The parent folder which contains all the packages.
        /// </summary>
        String parent;

        /// <summary>
        /// List of files which are present in a given project.
        /// </summary>
        private List<String> filesList;

        /// <summary>
        /// set and get method for the parent
        /// </summary>
        public String Parent
        {
            get { return parent; }
            set { parent = value; }
        }

        /// <summary>
        /// set and get method for the filelist
        /// </summary>
        public List<String> FilesList
        {
            get { return filesList; }
            set { filesList = value; }
        }

        /// <summary>
        /// Constructor for the class where the member variables are initialised
        /// </summary>
        public ProjectTree()
        {
            filesList = new List<String>();
        }
    }

    /// <summary>
    /// This class is used to create the list of files present in a particular folder.
    /// Each folder is considered as a project in this tool. There can be multiple files
    /// in the folder and each file is a package. The class contains the parent name 
    /// and list of files which are present in this folder. This structure will be used
    /// by the client to populate the list of projects and its packages in the GUI.
    /// </summary>
    public class FileManager
    {
        /// <summary>
        /// List of files present in a folder
        /// </summary>
        List<string> files;

        /// <summary>
        /// The structure used to the store the project and its packages.
        /// </summary>
        List<ProjectTree> projectList;

        /// <summary>
        /// set and get method for files
        /// </summary>
        public List<string> Files
        {
            get { return files; }
            set { files = value; }
        }

        /// <summary>
        /// The flag specifies whether the directories have to traversed recursively 
        /// to find the file list
        /// </summary>
        private bool recurse;

        /// <summary>
        /// set and get method for recurse
        /// </summary>
        public bool Recurse
        {
            get { return recurse; }
            set { recurse = value; }
        }

        /// <summary>
        /// The list of pattern given by the user. This class finds
        /// only those kinds of file whose extension matches with this
        /// pattern
        /// </summary>
        private List<string> patterns;

        /// <summary>
        /// get and set method for patterns
        /// </summary>
        public List<string> Patterns
        {
            get { return patterns; }
            set { patterns = value; }
        }

        /// <summary>
        /// constructor where all the member variables are initialised   
        /// </summary>
        public FileManager()
        {
            files = new List<string>();
            patterns = new List<string>();
            projectList = new List<ProjectTree>();
        }

        /// <summary>
        /// Method used to get the list of projecttree constructed using this module.
        /// It contains the parent folder and all its files.
        /// </summary>
        /// <returns></returns>
        public List<ProjectTree> getFiles()
        {
            return projectList;
        }

        /// <summary>
        /// This method creates the list of projects and packages. The
        /// list is created such that the folder which contains the files
        /// is represented as parent and all the files in this folder is 
        /// represented as its child. This is useful to populate the list of
        /// files in the client executive. 
        /// </summary>
        public void createProjectTree()
        {
            try
            {
                for (int j = 0; j < files.Count; ++j)
                {
                    bool parentpresent = false;
                    string currentFilename = System.IO.Path.GetFileName(files[j]);
                    String currentParentPath = System.IO.Path.GetDirectoryName(files[j]);
                    String fullParentPath = System.IO.Path.GetFullPath(currentParentPath);

                    for (int r = 0; r < projectList.Count; r++)
                    {
                        ProjectTree temporary = projectList[r];
                        if (temporary.Parent == fullParentPath)
                        {
                            parentpresent = true;
                            temporary.FilesList.Add(currentFilename);
                        }
                    }
                    if (!parentpresent)
                    {
                        ProjectTree currentTree = new ProjectTree();
                        currentTree.Parent = fullParentPath;
                        currentTree.FilesList.Add(currentFilename);
                        projectList.Add(currentTree);
                    }
                }
            }
            catch (Exception)
            {
                Console.Write("\n\n Error in creating the project list. Please check the input directory");
            }
        }

        /// <summary>
        /// This method finds list of all the files in a folder
        /// If the recurse option is enabled then the list will be
        /// created for the subdirectories as well
        /// </summary>
        /// <param name="path">Path of the folder for which the files list 
        /// has to created</param>
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
            catch (DirectoryNotFoundException) { Console.WriteLine("\nThe provided directory cannot be found {0}\n", path); }
            catch (FileNotFoundException) { Console.WriteLine("\nThe provided file cannot be found {0}\n", path); }
            catch (FileLoadException) { Console.WriteLine("\nError Loading file {0}\n", path); }
            catch (PathTooLongException) { Console.WriteLine("\nThe provided path is too long to read. Try moving the file to another location {0}\n", path); }
            catch (Exception) { Console.WriteLine("\nError in {0}\n", path); }
        }
    }

#if(TEST_FILEMGR)
    class TestFileMgr
    {
        /// <summary>
        /// Test stub for the module. when the module is launched 
        /// as a standalone application this is the main entry point
        /// for the application
        /// </summary>
        /// <param name="args"></param>
        static void Main(string[] args)
        {
            Console.Write("\n  Testing File Manager Class");
            Console.Write("\n =======================\n");

            FileManager fm = new FileManager();
            fm.Patterns.Add("*.cs");
            fm.findFiles(".");
            foreach (string file in fm.Files)
            {
                Console.Write("\n  {0}", file);
            }
            Console.Write("\n\n");
            Console.ReadLine();
        }
    }//end of class
#endif
}//end of namespace
