using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace PagePacker
{
    // this class contains methods to enumerate files relevant to the packing process contained in a root folder (or subdirectories within the root folder)
    public static class FileEnumerator
    {
        /// <summary>
        /// Indicates the maximum depth (in directories) from the root directory that the enumerateFiles function will search.
        /// </summary>
        const int MAX_ENUMERATION_LAYERS = 50;

        // this function returns a tuple containing arrays of file paths used in web pages (such as .html, .css, .js) contained within the directory whose path is passed as the 'baseDirectory' parameter,
        // or contained within directories contained within the base directory. If baseDirectory is not a valid folder directory, an empty array will be returned.
        // -Note: The tuple returns file path arrays in the following order: HTML files, CSS files, JS (Javascript) files, and JSON files.
        // -Note, this function blocks, so should be run on a thread separately from the user interface to avoid blocking.
        public static Tuple<string[], string[], string[], string[]> enumerateFiles(string baseDirectory, ref bool isStillEnumerating)
        {
            // check if the base directory is invalid; if so, exit the function since there is no way to enumerate files with an invalid base directory
            if (!Directory.Exists(baseDirectory)) { return new Tuple<string[], string[], string[], string[]>(new string[] { }, new string[] { }, new string[] { }, new string[] { }); }

            // declare local variables
            // -the lists containing the paths of all enumerated files, based on their type
            List<string> htmlFilePaths = new List<string>();
            List<string> cssFilePaths = new List<string>();
            List<string> jsFilePaths = new List<string>();
            List<string> jsonFilePaths = new List<string>();
            // counts the number of enumeration layers, used to force an end to enumeration to avoid an endless loop for a directory that is too deep
            int enumerationLayers = 0;
            // stores a list of files that are located within the current layer
            List<string> currentLayerFiles = new List<string>();
            // stores a list of directories located within the current layer (that will be enumerated)
            List<string> currentLayerDirectories = new List<string>();
            // stores the directories encountered while enumerating directories located in currentLayerDirectories
            List<string> nextLayerDirectories = new List<string>();
            // stores the extension of the current file, used to filter files returned to only those that can be packed
            string currentFileExtension = "";

            // begin the process by adding the base directory to the list of current layer directories
            currentLayerDirectories.Add(baseDirectory);

            // check if baseDirectory is valid
            if (Directory.Exists(baseDirectory))
            {
                // begin enumerating the files contained within (not stopping until the enumeration layer limit is reached or there are no more directories to enumerate)
                while (enumerationLayers < MAX_ENUMERATION_LAYERS && currentLayerDirectories.Count > 0)
                {
                    // iterate through all the directories in the current layer, enumerating files contained within and adding them to currentLayerFiles for addition to the final list
                    for (int m = 0; m < currentLayerDirectories.Count; m++)
                    {
                        // enumerate the files in the current directory; they will be filtered later
                        currentLayerFiles.AddRange(Directory.GetFiles(currentLayerDirectories[m]));

                        // enumerate directories contained within the current directory
                        nextLayerDirectories.AddRange(Directory.GetDirectories(currentLayerDirectories[m]));
                    }

                    // filter the enumerated files to only the types that can be packed (.html, .css, .js and .json)
                    for (int m = 0; m < currentLayerFiles.Count; m++)
                    {
                        // get the file extension of the current file
                        currentFileExtension = Path.GetExtension(currentLayerFiles[m]);
                        //System.Windows.Forms.MessageBox.Show(currentLayerFiles[m]);

                        // assign the file to a list based on its type (if it is one of the supported file types)
                        switch (currentFileExtension)
                        {
                            case ".html":
                                htmlFilePaths.Add(currentLayerFiles[m]);
                                break;
                            case ".css":
                                cssFilePaths.Add(currentLayerFiles[m]);
                                break;
                            case ".js":
                                jsFilePaths.Add(currentLayerFiles[m]);
                                break;
                            case ".json":
                                jsonFilePaths.Add(currentLayerFiles[m]);
                                break;
                            // otherwise the file is not of interest; discard
                        }
                    }

                    // clear out the list of enumerated files (since it is no longer needed)
                    currentLayerFiles.Clear();

                    // swap the list of recently enumerated directories into the currentLayerDirectory list, to ensure the directories will be enumerated in the next loop iteration
                    currentLayerDirectories.Clear();
                    currentLayerDirectories.AddRange(nextLayerDirectories);
                    nextLayerDirectories.Clear();

                    // increment the enumerationLayers variable to keep track of the number of enumerated layers (to avoid an endless loop or excessive enumeration time)
                    enumerationLayers++;
                }
            }

            // return the result
            return new Tuple<string[], string[], string[], string[]>(
                htmlFilePaths.ToArray(),
                cssFilePaths.ToArray(),
                jsFilePaths.ToArray(),
                jsonFilePaths.ToArray());
        }
    }
}
