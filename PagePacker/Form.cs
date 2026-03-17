using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Threading.Tasks;
using System.IO;
using System.Runtime.InteropServices;

namespace PagePacker
{
    // started project on 10 March 2026
    // Version 1 completed on 17 March 2026 @ 1220.

    public partial class mainForm : Form
    {
        // declare class variables
        private static string selectedRootFolderPath = "";
        private static string pageHTMLFilePath = "";
        private static string pageCSSFilePath = "";
        // -files contained within the root folder that can be packed into a webpage, sorted in arrays based on their fileType
        private static string[] enumeratedHTMLFiles = null;
        private static string[] enumeratedCSSFiles = null;
        private static string[] enumeratedJSFiles = null;
        private static string[] enumeratedJSONFiles = null;
        // status flag to indicate if files are being enumerated (i.e., after the user selects a root directory to enumerate files in)
        private static bool areFilesBeingEnumerated = false;

        public mainForm()
        {
            InitializeComponent();
        }

        // this function allows the user to select a folder that contains the files to pack
        private void selectSourceFolderBtn_Click(object sender, EventArgs e)
        {
            // declare a dialog to allow the user to select a folder that will contain the files to pack
            FolderBrowserDialog folderSelectDialog = new FolderBrowserDialog();
            // stores the folder that the user selected
            string selectedFolderPath = "";

            // check if the user successfully selected a folder
            if (folderSelectDialog.ShowDialog() == System.Windows.Forms.DialogResult.OK || true)
            {
                // get the path of the selected folder & place it in the textbox to show to the user
                selectedFolderPath = folderSelectDialog.SelectedPath;

                // prepare the user interface to indicate the file enumeration is ongoing
                tsStatusLbl.Text = "Enumerating files within the selected directory, please wait...";
                selectSourceFolderBtn.Enabled = false; // disable to avoid repeat enumeration requests (which could bog down the system)

                // create a task scheduler to launch the enumeration task
                TaskScheduler enumerationTaskScheduler = TaskScheduler.FromCurrentSynchronizationContext();

                // start a task to enumerate the files in the folder to pack
                Task<Tuple<string[], string[], string[], string[]>> enumerateFilesTask = Task.Factory.StartNew(() => FileEnumerator.enumerateFiles(selectedFolderPath, ref areFilesBeingEnumerated));
                enumerateFilesTask.ContinueWith(t =>
                {
                    // get a shallow copy of the result from the task
                    Tuple<string[], string[], string[], string[]> enumeratedFilePathArrays = t.Result;

                    // assign the arrays of filepaths to their corresponding arrays from the tuple
                    enumeratedHTMLFiles = enumeratedFilePathArrays.Item1.ToArray();
                    enumeratedCSSFiles = enumeratedFilePathArrays.Item2.ToArray();
                    enumeratedJSFiles = enumeratedFilePathArrays.Item3.ToArray();
                    enumeratedJSONFiles = enumeratedFilePathArrays.Item4.ToArray();

                    // check the validity of the enumerated files. Files must meet the minimum requirements for packing:
                    // 1. At least 1 HTML file exists
                    // -a CSS file is optional
                    if (enumeratedHTMLFiles.Length > 0)
                    {
                        // get enumerated file count to display in the task bar
                        int fileCount = enumeratedHTMLFiles.Length + enumeratedCSSFiles.Length + enumeratedJSFiles.Length + enumeratedJSONFiles.Length;

                        // update the UI indicators to indicate the enumeration task is completed
                        tsStatusLbl.Text = "File enumeration complete. " + fileCount + " files found.";

                        // place the folder path in a textbox so the user can see it
                        this.sourceFolderTB.Text = selectedFolderPath;
                        selectedRootFolderPath = selectedFolderPath;

                        // load the paths of the enumerated files to the 'files to include' checked listbox so the user can interact with it
                        loadFilepathsToUI();

                        // enable the 'pack files' and 'configuration export' buttons (since both functions can be accomplished with data correctly loaded)
                        packPageFilesBtn.Enabled = true;
                        exportPackingConfigBtn.Enabled = true;
                    }
                    else
                    {
                        // minimum requirements aren't met; display the error message
                        tsStatusLbl.Text = "Cannot pack webpage, no HTML file was found. Verify directory and try again.";
                        // clear selectedFolderPath to avoid errors with future loading attempts
                        selectedFolderPath = "";

                        // disable the UI controls (since selected data is invalid)
                        pageHTMLFileCB.Enabled = false;
                        pageHTMLFileCB.Items.Clear();
                        pageCSSFileCB.Enabled = false;
                        pageCSSFileCB.Items.Clear();
                        filesToPackCLB.Enabled = false;
                        filesToPackCLB.Items.Clear();
                        packPageFilesBtn.Enabled = false;
                        exportPackingConfigBtn.Enabled = false;
                    }

                    selectSourceFolderBtn.Enabled = true;

                }, enumerationTaskScheduler);
            }
        }

        // this function checks if the user disabled an HTML or CSS file that was selected, updating other UI components as required
        private void filesToPackCLB_ItemCheck(object sender, ItemCheckEventArgs e)
        {
            // clear status label (in case there was a selection error before & now there won't be)
            tsStatusLbl.Text = "";

            // get the selected item, with the file path removed
            string selectedItem = filesToPackCLB.Items[e.Index].ToString();
            string fileExtension = selectedItem.Substring(1, selectedItem.IndexOf(") ") - 1).ToLower();
            string selectedItem_RelativePath = selectedItem.Remove(0, selectedItem.IndexOf(") ") + 2);

            // only fire if the user unchecked an item (since that is the only time this event needs to occur)
            if (e.NewValue == CheckState.Unchecked && fileExtension == ".html")
            {
                // if the file is selected in the combobox, don't allow it to be unchecked (would cause problems with the packer)
                if (pageHTMLFileCB.Items.Count > 0 && pageHTMLFileCB.SelectedItem.ToString() == selectedItem_RelativePath)
                {
                    // re-check the item to avoid it being excluded (despite being selected in the combobox as the page's HTML file)
                    e.NewValue = CheckState.Checked;

                    // print a message in the status bar to tell the user the issue
                    tsStatusLbl.Text = "Cannot uncheck (file selected as page HTML file). Change page HTML file and try again.";
                }

            }
            else if (fileExtension == ".css")
            {
                // check if the user unchecked (excluded) the selected CSS file (which is not required to pack the page)
                if (pageCSSFileCB.Items.Contains(selectedItem_RelativePath) && e.NewValue == CheckState.Unchecked)
                {
                    // remove the item from the combobox, since the user doesn't want to include it
                    pageCSSFileCB.Items.Remove(selectedItem_RelativePath);
                    pageCSSFileCB.Text = "";
                    pageCSSFilePath = "";

                    if (pageCSSFileCB.Items.Count == 0) { pageCSSFileCB.Enabled = false; }
                }
                else if (e.NewValue == CheckState.Checked)
                {
                    if (!pageCSSFileCB.Items.Contains(selectedItem_RelativePath))
                    {
                        // add the item to the combobox, since the user wants to include it
                        pageCSSFileCB.Items.Add(selectedItem_RelativePath);
                    }

                    pageCSSFileCB.Enabled = true;

                    // set the item as the selected item if no item is selected (to ensure a CSS file [if located in the directory] is included as the page CSS file)
                    if (pageCSSFileCB.SelectedIndex == -1)
                    {
                        pageCSSFileCB.SelectedIndex = pageCSSFileCB.Items.Count - 1;
                    }
                }
            }
            else if (fileExtension == ".js" || fileExtension == ".json")
            {
                //MessageBox.Show(selectedItem);
                CheckState newState = CheckState.Indeterminate;

                if ((!minifyJSCB.Checked && selectedItem.Contains(".JS")) ||
                    (selectedItem.Contains("(.JSON)") && !minifyJSONCB.Checked))
                {
                    newState = CheckState.Unchecked;
                }
                else
                {
                    newState = e.NewValue;
                }

                if (filesToMinifyCLB.Items.Contains(selectedItem))
                {
                    
                    // copy check state to item in the 'files to minify' clb
                    filesToMinifyCLB.SetItemCheckState(
                        filesToMinifyCLB.Items.IndexOf(selectedItem),
                        newState);
                }
            }
        }

        // this function updates the value of the pageHTMLFilePath based on the value selected in the page HTML file combobox
        private void pageHTMLFileCB_SelectedValueChanged(object sender, EventArgs e)
        {
            if (pageHTMLFileCB.SelectedIndex >= 0)
            {
                // identify which HTML file in the enumeratedHTMLFiles array contains the selected item
                string selectedItem = pageHTMLFileCB.SelectedItem.ToString();

                // strip out the leading characters (.../) since those indicate a relative path & aren't included in the actual filepath
                selectedItem = selectedItem.Remove(0, 4);

                // search each string within the enumeratedHTMLFiles array to find a matching file; if a match is found, the filepath is found
                for (int m = 0; m < enumeratedHTMLFiles.Length; m++)
                {
                    if (enumeratedHTMLFiles[m].Contains(selectedItem))
                    {
                        // assign the value
                        pageHTMLFilePath = enumeratedHTMLFiles[m];
                        break;
                    }
                }
            }
        }

        // this function updates the value of the pageCSSFilePath based on the value selected in the page CSS file combobox
        private void pageCSSFileCB_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (pageCSSFileCB.SelectedIndex >= 0)
            {
                // identify which CSS file in the enumeratedCSSFiles array contains the selected item
                string selectedItem = pageCSSFileCB.SelectedItem.ToString();

                // strip out the leading characters (.../) since those indicate a relative path & aren't included in the actual filepath
                selectedItem = selectedItem.Remove(0, 4);

                // search each string within the enumeratedCSSFiles array to find a matching file; if a match is found, the filepath is found
                for (int m = 0; m < enumeratedCSSFiles.Length; m++)
                {
                    if (enumeratedCSSFiles[m].Contains(selectedItem))
                    {
                        // assign the value
                        pageCSSFilePath = enumeratedCSSFiles[m];
                        break;
                    }
                }
            }
            else
            {
                pageCSSFilePath = "";
            }
        }

        // this function exports the packing details (root folder, file paths, page HTML & CSS files, and options) in a file to simplify the process of packing a page with updated files.
        private void exportPackingConfigBtn_Click(object sender, EventArgs e)
        {
            /* packing configuration format:
             * ==PAGE HTML FILE==
             * [relative page HTML file path here]
             * ==PAGE CSS FILE==
             * [relative CSS file path here, or nothing (if no CSS file included)]
             * ==JAVASCRIPT ENTRY POINT (FUNCTION NAME)==
             * [the name of a javascript function that is executed on page launch, or nothing (if the user didn't type anything)]
             * ==FILES TO INCLUDE==
             * [include the relative file path of each file to include]
             * ==FILES TO MINIFY==
             * [include the relative file path of each file to include, or nothing (if no files to minify)]
             * ==MINIFY JAVASCRIPT==
             * [include either 'true' or 'false' here]
             * ==MINIFY JSON==
             * [include either 'true' or 'false' here]
             * ==MINIFY CSS==
             * [include either 'true' or 'false' here]
             * ==JS MINIFICATION OPTIONS==
             * [include a string containing 6 digits, 1 for each javascript minification option. Use '1' for true or '0' for false]
             * ==CSS MINIFICATION OPTIONS==
             * [include a string containing 2 digits, 1 for each CSS minification option. Use '1' for true or '0' for false]
            */

            // file paths are all relative; the root folder path will be assumed to be the folder that the configuration file is contained in to promote portability)

            // declare local variables to store the file as it is being built
            string packingConfigFileText = "";
            string pageHTMLFile = pageHTMLFileCB.SelectedItem.ToString();
            string pageCSSFile = "";
            
            // add the section for the page HTML file
            packingConfigFileText += "==PAGE HTML FILE==" + Environment.NewLine;
            if (pageHTMLFile.Substring(0, 3) == "...") { pageHTMLFile = pageHTMLFileCB.SelectedItem.ToString().Remove(0, 3); }
            packingConfigFileText += pageHTMLFile + Environment.NewLine;

            // add the section for the page CSS file path
            packingConfigFileText += "==PAGE CSS FILE==" + Environment.NewLine;

            // check if a page CSS file is selected (since the page files may not contain one)
            if (pageCSSFileCB.SelectedIndex >= 0)
            {
                if (pageCSSFile.Length > 0)
                {
                    if (pageCSSFile.Substring(0, 3) == "...") { pageCSSFile = pageCSSFileCB.SelectedItem.ToString().Remove(0, 3); }
                }

                packingConfigFileText += pageCSSFile + Environment.NewLine;
            }
            else
            {
                // leave the line blank to signal no css file will be used
                packingConfigFileText += " " + Environment.NewLine;
            }

            // add the section for the javascript entry point function name
            packingConfigFileText += "==JAVASCRIPT ENTRY POINT (FUNCTION NAME)==" + Environment.NewLine;
            packingConfigFileText += jsEntryPointFuncNameTB.Text + Environment.NewLine;

            // add the section for the files to include
            packingConfigFileText += "==FILES TO INCLUDE==" + Environment.NewLine;
            for (int m = 0; m < filesToPackCLB.Items.Count; m++)
            {
                if (filesToPackCLB.GetItemCheckState(m) == CheckState.Checked)
                {
                    packingConfigFileText += filesToPackCLB.Items[m].ToString().Remove(0, filesToPackCLB.Items[m].ToString().IndexOf(") ") + 2) + Environment.NewLine;
                }
            }

            // add the section for files to minify
            packingConfigFileText += "==FILES TO MINIFY==" + Environment.NewLine;
            for (int m = 0; m < filesToMinifyCLB.Items.Count; m++)
            {
                if (filesToMinifyCLB.GetItemCheckState(m) == CheckState.Checked)
                {
                    packingConfigFileText += filesToMinifyCLB.Items[m].ToString().Remove(0, filesToMinifyCLB.Items[m].ToString().IndexOf(") ") + 2) + Environment.NewLine;
                }
            }

            // add the option section for minifying javascript
            packingConfigFileText += "==MINIFY JAVASCRIPT==" + Environment.NewLine;
            packingConfigFileText += minifyJSCB.Checked.ToString().ToLower() + Environment.NewLine;

            // add the option section for minifying json
            packingConfigFileText += "==MINIFY JSON==" + Environment.NewLine;
            packingConfigFileText += minifyJSONCB.Checked.ToString().ToLower() + Environment.NewLine;

            // add the option section for minifying CSS
            packingConfigFileText += "==MINIFY CSS==" + Environment.NewLine;
            packingConfigFileText += minifyCSSCB.Checked.ToString().ToLower() + Environment.NewLine;

            // add the option section for javascript minification options
            packingConfigFileText += "==JS MINIFICATION OPTIONS==" + Environment.NewLine;
            packingConfigFileText += (jsCommentRemoveCB.Checked ? "1" : "0");
            packingConfigFileText += (jsLBRemoveCB.Checked ? "1" : "0");
            packingConfigFileText += (jsCrunchVarNameCB.Checked ? "1" : "0");
            packingConfigFileText += (jsRemoveLogStmtCB.Checked ? "1" : "0");
            packingConfigFileText += (jsCrunchFuncNameCB.Checked ? "1" : "0");
            packingConfigFileText += (jsCrunchParamNameCB.Checked ? "1" : "0");
            packingConfigFileText += Environment.NewLine;

            // add the option section for CSS minification options
            packingConfigFileText += "==CSS MINIFICATION OPTIONS==" + Environment.NewLine;
            packingConfigFileText += (cssCommentRemoveCB.Checked ? "1" : "0");
            packingConfigFileText += (cssLBRemoveCB.Checked ? "1" : "0");
            packingConfigFileText += Environment.NewLine;

            // open a save file dialog pointed to the root folder
            SaveFileDialog saveConfigFileDialog = new SaveFileDialog();
            saveConfigFileDialog.Filter = "PagePacker Configuration File | *.pcf";
            saveConfigFileDialog.DefaultExt = ".pcf";
            saveConfigFileDialog.InitialDirectory = selectedRootFolderPath;

            // check if the user made a valid choice in saving the file
            if (saveConfigFileDialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                try
                {
                    // write the file to the disk
                    File.WriteAllText(saveConfigFileDialog.FileName, packingConfigFileText);
                    // write text to the status bar
                    tsStatusLbl.Text = "Saved config file '" + Path.GetFileNameWithoutExtension(saveConfigFileDialog.FileName) + "' to the root directory. (" + DateTime.Now.ToShortTimeString() + ")";
                }
                catch (Exception ex)
                {
                    // show the exception to give the user an idea of what went wrong
                    MessageBox.Show(ex.ToString());
                    // print message in status bar
                    tsStatusLbl.Text = "Unable to export configuration; file I/O error occurred. (" + DateTime.Now.ToShortTimeString() + ")";
                }

               
            }
        }

        // this function imports a file (assumed to be a packing configuration file) and verifies its validity before applying its data to the user interface
        private void loadPackingConfigFileBtn_Click(object sender, EventArgs e)
        {
            // declare local variables to simplify code
            string[] loadedFileLines = null;
            int validatedIncludeFiles = 0;
            int missingIncludeFiles = 0;
            string rootFileDirectory = "";
            string pageHTMLFilePath = "";
            string pageCSSFilePath = "";
            string javascriptEntrypointFuncName = "";
            List<string> includeFileNames = new List<string>();
            List<string> minifyFileNames = new List<string>();
            string configFileName = "";
            int currentFileLineIndex = 0;

            // packing settings
            bool minifyJSSetting = true;
            bool minifyJSONSetting = true;
            bool minifyCSSSetting = true;
            string jsMinificationSettingString = "";
            string cssMinificationSettingString = "";

            // packing file is assumed to be in the root directory (where the 'export' function places it)
            // -first, have the user select the file
            OpenFileDialog openPackingConfigFileDialog = new OpenFileDialog();
            openPackingConfigFileDialog.Filter = "PagePacker Configuration File | *.pcf";
            openPackingConfigFileDialog.DefaultExt = ".pcf";
            openPackingConfigFileDialog.InitialDirectory = selectedRootFolderPath;

            // check if the user selected a file
            if (openPackingConfigFileDialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                rootFileDirectory = Path.GetDirectoryName(openPackingConfigFileDialog.FileName);
                configFileName = Path.GetFileNameWithoutExtension(openPackingConfigFileDialog.FileName);

                try
                {
                    // load the file contents into a local array to simplify code
                    loadedFileLines = File.ReadAllLines(openPackingConfigFileDialog.FileName);
                }
                catch (Exception ex)
                {
                    // show the exception to give the user an idea of what went wrong
                    MessageBox.Show(ex.ToString());
                    // print message in status bar
                    tsStatusLbl.Text = "Unable to import configuration; file I/O error occurred. (" + DateTime.Now.ToShortTimeString() + ")";
                    // return since the file couldn't be imported, which is an assumption the rest of the code depends on
                    return;
                }
            }

            // if this point is reached, the file's contents were successfully loaded
            // -first, check if the file meets the minimum length (minimum number of lines) of 10 lines (1 for each section's title, 1 for a value for each section)
            if (loadedFileLines == null)
            {
                tsStatusLbl.Text = "Cannot import configuration; file is empty.";
                return;
            }

            // check if the file length is less than expected
            if (loadedFileLines.Length < 20)
            {
                tsStatusLbl.Text = "Cannot import configuration; file is missing configuration data.";
                return;
            }

            // assemble the page HTML file and CSS file paths
            pageHTMLFilePath = rootFileDirectory + loadedFileLines[1];
            pageCSSFilePath = rootFileDirectory + loadedFileLines[3];
            javascriptEntrypointFuncName = loadedFileLines[5];

            // check if the HTML file path doesn't exist
            if (!File.Exists(pageHTMLFilePath))
            {
                tsStatusLbl.Text = "Cannot import configuration; page HTML file doesn't exist in root directory.";
                return;
            }
            else
            {
                // reset the page HTML file path to be relative
                pageHTMLFilePath = "..." + loadedFileLines[1];
            }

            // check if the CSS file path doesn't exist
            if (File.Exists(pageCSSFilePath))
            {
                // clear pageCSSFilePath (since the file doesn't exist)
                pageCSSFilePath = "";
            }
            else
            {
                // reset the page CSS file path to be relative
                pageCSSFilePath = "..." + loadedFileLines[3];
            }

            // populate the javascript entry point function name to the UI (loaded from the file)
            this.jsEntryPointFuncNameTB.Text = javascriptEntrypointFuncName;

            currentFileLineIndex = 7;
            while (currentFileLineIndex < loadedFileLines.Length && loadedFileLines[currentFileLineIndex] != "==FILES TO MINIFY==")
            {
                // check if the file exists; if so, increment validatedIncludeFiles. Otherwise increment missingIncludeFiles
                if (File.Exists(rootFileDirectory + loadedFileLines[currentFileLineIndex]))
                {
                    includeFileNames.Add(loadedFileLines[currentFileLineIndex]);
                    validatedIncludeFiles++;
                }
                else
                {
                    missingIncludeFiles++;
                }

                currentFileLineIndex++;
            }

            // check for an error state
            if (currentFileLineIndex == loadedFileLines.Length)
            {
                tsStatusLbl.Text = "Cannot import configuration; 'files to include' section has bad formatting.";
                return;
            }
            else
            {
                // increment to skip past the section title for the 'files to minify' section
                currentFileLineIndex++;
            }

            // print status message
            tsStatusLbl.Text = "Verified existence of " + validatedIncludeFiles.ToString() + " of " + (validatedIncludeFiles + missingIncludeFiles).ToString() + " files to include. Missing files will be excluded.";

            // load 'files to minify'
            while (currentFileLineIndex < loadedFileLines.Length && loadedFileLines[currentFileLineIndex] != "==MINIFY JAVASCRIPT==")
            {
                // check if the file exists; if so, increment validatedIncludeFiles. Otherwise increment missingMinifyFiles
                if (File.Exists(rootFileDirectory + loadedFileLines[currentFileLineIndex]))
                {
                    minifyFileNames.Add(loadedFileLines[currentFileLineIndex]);
                }

                currentFileLineIndex++;
            }

            // check for an error state
            if (currentFileLineIndex == loadedFileLines.Length)
            {
                tsStatusLbl.Text = "Cannot import configuration; 'files to minify' section has bad formatting.";
                return;
            }

            // try loading settings
            if (!bool.TryParse(loadedFileLines[currentFileLineIndex + 1], out minifyJSSetting))
            {
                // print an error message
                tsStatusLbl.Text = "Cannot import configuration; option for setting 'Minify Javascript' is incorrectly formatted.";
                return;
            }

            if (!bool.TryParse(loadedFileLines[currentFileLineIndex + 3], out minifyJSONSetting))
            {
                // print an error message
                tsStatusLbl.Text = "Cannot import configuration; option for setting 'Minify JSON' is incorrectly formatted.";
                return;
            }

            if (!bool.TryParse(loadedFileLines[currentFileLineIndex + 5], out minifyCSSSetting))
            {
                // print an error message
                tsStatusLbl.Text = "Cannot import configuration; option for setting 'Minify CSS' is incorrectly formatted.";
                return;
            }

            // load the javascript minification settings
            jsMinificationSettingString = loadedFileLines[currentFileLineIndex + 7];
            cssMinificationSettingString = loadedFileLines[currentFileLineIndex + 9];

            // error-check the javascript minification setting string
            // -check if data is missing (string not long enough)
            if (jsMinificationSettingString.Length < 6)
            {
                // js minification string is incorrectly formatted (missing data). Print an error message
                tsStatusLbl.Text = "Cannot import configuration; javascript minification setting is missing data.";
                return;
            }
            // check if the string is incorrectly formatted
            if (!jsMinificationSettingString.Substring(0,6).All(c => c == '0' || c == '1'))
            {
                // js minification string has invalid data (an unexpected character)
                tsStatusLbl.Text = "Cannot import configuration; javascript minification setting has invalid data.";
                return;
            }

            // error-check the CSS minification setting string
            // -check if data is missing (string not long enough)
            if (cssMinificationSettingString.Length < 2)
            {
                // js minification string is incorrectly formatted (missing data). Print an error message
                tsStatusLbl.Text = "Cannot import configuration; CSS minification setting is missing data.";
                return;
            }
            // check if the string is incorrectly formatted
            if (!cssMinificationSettingString.Substring(0, 2).All(c => c == '0' || c == '1'))
            {
                // js minification string has invalid data (an unexpected character)
                tsStatusLbl.Text = "Cannot import configuration; CSS minification setting has invalid data.";
                return;
            }

            // if this point is reached, loading was successful. Load data to the user interface
            // -first, clear out all user interface components
            sourceFolderTB.Text = "";
            pageHTMLFileCB.Items.Clear();
            pageCSSFileCB.Items.Clear();
            filesToPackCLB.Items.Clear();

            // begin populating data
            sourceFolderTB.Text = rootFileDirectory;

            List<string> enumeratedHTMLFiles = new List<string>();
            List<string> enumeratedCSSFiles = new List<string>();
            List<string> enumeratedJSFiles = new List<string>();
            List<string> enumeratedJSONFiles = new List<string>();

            string fileExtension = "";
            for (int m = 0; m < includeFileNames.Count; m++)
            {
                fileExtension = Path.GetExtension(includeFileNames[m]);
                //includeFileNames[m] = includeFileNames[m].Remove(0, 3);

                switch (fileExtension)
                {
                    case ".html":
                        enumeratedHTMLFiles.Add(includeFileNames[m]);
                        break;
                    case ".css":
                        enumeratedCSSFiles.Add(includeFileNames[m]);
                        break;
                    case ".js":
                        enumeratedJSFiles.Add(includeFileNames[m]);
                        break;
                    case ".json":
                        enumeratedJSONFiles.Add(includeFileNames[m]);
                        break;
                }
            }

            // load the file names into the arrays
            mainForm.enumeratedHTMLFiles = enumeratedHTMLFiles.ToArray();
            mainForm.enumeratedCSSFiles = enumeratedCSSFiles.ToArray();
            mainForm.enumeratedJSFiles = enumeratedJSFiles.ToArray();
            mainForm.enumeratedJSONFiles = enumeratedJSONFiles.ToArray();
            mainForm.selectedRootFolderPath = rootFileDirectory;

            // call the function to load the filepaths to the UI
            loadFilepathsToUI();

            string currentFileName = "";
            // manually set the 'files to minify' settings based on the configuration file
            for (int m = 0; m < filesToMinifyCLB.Items.Count; m++)
            {
                // default all files to not minified (check state will be reset to 'checked' if the file is on the list of files to minify)
                filesToMinifyCLB.SetItemCheckState(m, CheckState.Unchecked);

                // format the file name to match the format it is stored in within the configuration file
                currentFileName = filesToMinifyCLB.Items[m].ToString();
                currentFileName = currentFileName.Substring(currentFileName.IndexOf("..."),
                    currentFileName.Length - (currentFileName.IndexOf("...")));

                //MessageBox.Show("'" + currentFileName + "'" + Environment.NewLine + "'" + minifyFileNames[m] + "'");

                // check if the currently examined file name is among the list of files to minify
                if (minifyFileNames.Contains(currentFileName))
                {
                    // restore the check state, to correspond with the configuration data in the configuration file
                    filesToMinifyCLB.SetItemCheckState(m, CheckState.Checked);
                }

                //MessageBox.Show(currentFileName);
            }

            // manually set the settings
            minifyJSCB.Checked = minifyJSSetting;
            minifyJSONCB.Checked = minifyJSONSetting;
            minifyCSSCB.Checked = minifyCSSSetting;

            // manually set the HTML and CSS file paths
            pageHTMLFileCB.SelectedItem = pageHTMLFilePath;

            if (pageCSSFilePath.Trim() != "")
            {
                pageCSSFileCB.SelectedItem = pageCSSFilePath;
            }

            // set the javascript and CSS minification options
            jsCommentRemoveCB.Checked = jsMinificationSettingString[0] == '1';
            jsLBRemoveCB.Checked = jsMinificationSettingString[1] == '1';
            jsCrunchFuncNameCB.Checked = jsMinificationSettingString[2] == '1';
            jsRemoveLogStmtCB.Checked = jsMinificationSettingString[3] == '1';
            jsCrunchFuncNameCB.Checked = jsMinificationSettingString[4] == '1';
            jsCrunchParamNameCB.Checked = jsMinificationSettingString[5] == '1';

            cssCommentRemoveCB.Checked = cssMinificationSettingString[0] == '1';
            cssLBRemoveCB.Checked = cssMinificationSettingString[1] == '1';

            // re-enable export config & pack file buttons (since UI contains valid data)
            exportPackingConfigBtn.Enabled = true;
            packPageFilesBtn.Enabled = true;

            // write a status message
            tsStatusLbl.Text = "Successfully imported configuration file '" + configFileName + "' (" + DateTime.Now.ToShortTimeString() + ")";
        }

        // this function initiates the page packing process, which packs all included files into a single HTML file for maximum portability
        private void packPageFilesBtn_Click(object sender, EventArgs e)
        {
            // write a message to the status label to indicate the packing process is beginning
            tsStatusLbl.Text = "Packing include files into HTML file, please wait...";

            // disable the 'pack files' button (to avoid the user overwhelming the system by firing off multiple requests)
            packPageFilesBtn.Enabled = false;

            string packedHTMLFileText = "";
            string packingErrorMsg = "";
            List<string> htmlFilesToPack = new List<string>();
            List<string> cssFilesToPack = new List<string>();
            List<string> jsFilesToPack = new List<string>();
            List<string> jsonFilesToPack = new List<string>();
            List<string> filesToMinify = new List<string>();

            // -create a task scheduler to launch the file packing task
            TaskScheduler enumerationTaskScheduler = TaskScheduler.FromCurrentSynchronizationContext();

            // load the file paths of the files selected for packing in the filesToPackCLB
            string currentPackingFileName = "";
            string currentPackingFileExtension = "";
            for (int m = 0; m < filesToPackCLB.Items.Count; m++)
            {
                if (filesToPackCLB.GetItemCheckState(m) == CheckState.Checked)
                {
                    currentPackingFileName = filesToPackCLB.Items[m].ToString();

                    // get the file extension to determine what collection to put the file in
                    currentPackingFileExtension = currentPackingFileName.Substring(0, currentPackingFileName.IndexOf(")") + 1);

                    // strip out the file extension and relative path indicator (...) to ensure the file path being passed is valid
                    currentPackingFileName = currentPackingFileName.Remove(0, currentPackingFileName.IndexOf("...") + 3);

                    //MessageBox.Show(currentPackingFileName);

                    // extract the file type
                    switch (currentPackingFileExtension)
                    {
                        case "(.HTML)":
                            htmlFilesToPack.Add(currentPackingFileName);
                            break;
                        case "(.CSS)":
                            cssFilesToPack.Add(currentPackingFileName);
                            break;
                        case "(.JS)":
                            jsFilesToPack.Add(currentPackingFileName);
                            break;
                        case "(.JSON)":
                            jsonFilesToPack.Add(currentPackingFileName);
                            break;
                        default:
                            MessageBox.Show("Unsupported file type '" + currentPackingFileExtension + "'");
                            break;
                    }
                }
            }

            // get a list of files that the user requested should have minification applied
            string currentFilePath = "";
            for (int m = 0; m < filesToMinifyCLB.Items.Count; m++)
            {
                if (filesToMinifyCLB.GetItemCheckState(m) == CheckState.Checked)
                {
                    currentFilePath = filesToMinifyCLB.Items[m].ToString();
                    currentFilePath = currentFilePath.Substring(currentFilePath.IndexOf("...") + 3, currentFilePath.Length - (currentFilePath.IndexOf("...") + 3));
                    filesToMinify.Add(currentFilePath);
                }
            }

            // compile the javascript and css minification options
            JSMinifier.MinifierOptions jsMinifyOptions = 0;
            CSSMinifier.MinifierOptions cssMinifyOptions = 0;

            if (jsCommentRemoveCB.Checked) { jsMinifyOptions = jsMinifyOptions | JSMinifier.MinifierOptions.REMOVE_COMMENTS; }
            if (jsLBRemoveCB.Checked) { jsMinifyOptions = jsMinifyOptions | JSMinifier.MinifierOptions.REMOVE_LINE_BREAKS; }
            if (jsCrunchVarNameCB.Checked) { jsMinifyOptions = jsMinifyOptions | JSMinifier.MinifierOptions.SHORTEN_VARIABLE_NAMES; }
            if (jsRemoveLogStmtCB.Checked) { jsMinifyOptions = jsMinifyOptions | JSMinifier.MinifierOptions.REMOVE_LOG_STATEMENTS; }
            if (jsCrunchFuncNameCB.Checked) { jsMinifyOptions = jsMinifyOptions | JSMinifier.MinifierOptions.SHORTEN_FUNCTION_NAMES; }
            if (jsCrunchParamNameCB.Checked) { jsMinifyOptions = jsMinifyOptions | JSMinifier.MinifierOptions.SHORTEN_PARAMETER_NAMES; }

            if (cssCommentRemoveCB.Checked) { cssMinifyOptions = cssMinifyOptions | CSSMinifier.MinifierOptions.REMOVE_COMMENTS; }
            if (cssLBRemoveCB.Checked) { cssMinifyOptions = cssMinifyOptions | CSSMinifier.MinifierOptions.REMOVE_LINE_BREAKS; }

            // create a task to pack the file (since it may take a while to complete and to avoid locking up the UI)
            Task<string> packFilesTask = Task.Factory.StartNew(() => HTMLPagePacker.packFilesIntoHTMLPage(NormalizeLongPath(selectedRootFolderPath), pageHTMLFilePath, pageCSSFilePath,
                htmlFilesToPack.ToArray(), 
                cssFilesToPack.ToArray(),
                jsFilesToPack.ToArray(),
                jsonFilesToPack.ToArray(),
                filesToMinify.ToArray(),
                jsEntryPointFuncNameTB.Text,
                minifyJSCB.Checked,
                minifyJSONCB.Checked,
                minifyCSSCB.Checked,
                jsMinifyOptions,
                cssMinifyOptions,
                out packingErrorMsg));
            packFilesTask.ContinueWith(t =>
            {
                // move the result into a function variable
                packedHTMLFileText = packFilesTask.Result;

                // check if an error occurred
                if (packingErrorMsg != "")
                {
                    // an error occurred; print an error message & exit the function (since the text returned from the packing function is invalid)
                    tsStatusLbl.Text = "Error occurred while trying to pack files: " + packingErrorMsg;
                    return;
                }
                else
                {
                    // update the status label
                    tsStatusLbl.Text = "Successfully packed files (file size: " + FormatBytes(packedHTMLFileText.Length) + ") (" + DateTime.Now.ToShortTimeString() + ")";

                    // open a file dialog for the user to save the file
                    SaveFileDialog savePackedHTMLFileDialog = new SaveFileDialog();
                    savePackedHTMLFileDialog.Filter = "HTML Files | *.html";
                    savePackedHTMLFileDialog.InitialDirectory = selectedRootFolderPath;

                    if (savePackedHTMLFileDialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                    {
                        // try saving the file
                        try
                        {
                            // write the file to the disk
                            File.WriteAllText(savePackedHTMLFileDialog.FileName, packedHTMLFileText);

                            // update the status label to indicate the file was succesfully written
                            tsStatusLbl.Text = "Successfully packed files, output is " + Path.GetFileName(savePackedHTMLFileDialog.FileName)
                                + " (" + FormatBytes(packedHTMLFileText.Length) + ") (" + DateTime.Now.ToShortTimeString() + ")";
                        }
                        catch (Exception ex)
                        {
                            MessageBox.Show(ex.ToString());
                            tsStatusLbl.Text = "Could not save file due to I/O error, try again.";
                        }
                    }
                 }

                // reenable the 'pack files' button (since the current file packing request is over)
                packPageFilesBtn.Enabled = true;

            }, enumerationTaskScheduler);
        }

        #region FORMATTING-RELATED CONTROLS EVENT HANDLERS

        private void minifyJSCB_CheckedChanged(object sender, EventArgs e)
        {
            // update the minify checked listbox entries to correspond with the minify javascript setting if
            // this function wasn't programmatically called (when sender == null)
            if (sender != null)
            {
                updateMinifyCLBEnableStatus();
            }

            // update the enable status of the javascript minification options
            jsCommentRemoveCB.Enabled = minifyJSCB.Checked;
            jsCrunchFuncNameCB.Enabled = minifyJSCB.Checked;
            jsCrunchParamNameCB.Enabled = minifyJSCB.Checked;
            jsCrunchVarNameCB.Enabled = minifyJSCB.Checked;
            jsLBRemoveCB.Enabled = minifyJSCB.Checked;
            jsRemoveLogStmtCB.Enabled = minifyJSCB.Checked;
        }

        private void minifyJSCB_EnabledChanged(object sender, EventArgs e)
        {
            // check if the checkbox is checked
            if (minifyJSCB.Enabled)
            {
                // update the status of the javascript formatting options
                minifyJSCB_CheckedChanged(null, null);
            }
        }

        private void minifyJSONCB_CheckedChanged(object sender, EventArgs e)
        {
            updateMinifyCLBEnableStatus();
        }

        private void minifyCSSCB_CheckedChanged(object sender, EventArgs e)
        {
            // update the enable status of the CSS minification options
            cssCommentRemoveCB.Enabled = minifyCSSCB.Checked;
            cssLBRemoveCB.Enabled = minifyCSSCB.Checked;
        }

        private void minifyCSSCB_EnabledChanged(object sender, EventArgs e)
        {
            // check if the checkbox is checked
            if (minifyCSSCB.Enabled)
            {
                // update the status of the CSS formatting options
                minifyCSSCB_CheckedChanged(null, null);
            }
        }

        #endregion

        #region HELPER FUNCTIONS

        // this function loads the filepaths of webpage files enumerated when the user selected a root directory to the user interface (like the checked listbox and comboboxes)
        private void loadFilepathsToUI()
        {
            // clear the listbox to make room for the new items
            this.filesToPackCLB.Items.Clear();
            this.filesToMinifyCLB.Items.Clear();

            // disable option checkboxes (reset them)
            minifyJSCB.Enabled = false;
            jsCommentRemoveCB.Enabled = false;
            jsCrunchFuncNameCB.Enabled = false;
            jsCrunchParamNameCB.Enabled = false;
            jsCrunchVarNameCB.Enabled = false;
            jsLBRemoveCB.Enabled = false;
            jsRemoveLogStmtCB.Enabled = false;

            minifyJSONCB.Enabled = false;

            minifyCSSCB.Enabled = false;
            cssCommentRemoveCB.Enabled = false;
            cssLBRemoveCB.Enabled = false;

            // load the items to the listbox, only displaying the relative path (since there is no need to show the root path, since all files have the same one)
            // -place the arrays into a list to simplify loading code
            List<string[]> enumeratedFilePaths = new List<string[]>();
            enumeratedFilePaths.Add(enumeratedHTMLFiles);
            enumeratedFilePaths.Add(enumeratedCSSFiles);
            enumeratedFilePaths.Add(enumeratedJSFiles);
            enumeratedFilePaths.Add(enumeratedJSONFiles);

            // declare variables to simplify code within the formatting loop
            string fileExtension = "";
            string formattedFilePath = "";

            // format & load the file paths into the listbox
            for (int m = 0; m < enumeratedFilePaths.Count; m++)
            {
                for (int x = 0; x < enumeratedFilePaths[m].Length; x++)
                {
                    // assign the currently iterated file path's details into local variables
                    formattedFilePath = enumeratedFilePaths[m][x];
                    fileExtension = Path.GetExtension(formattedFilePath);

                    // first, convert the filepath to a relative path by stripping out the root directory (no need for it since all files share it)
                    if (formattedFilePath.Length > selectedRootFolderPath.Length)
                    {
                        // convert the file format to a relative form & replace it in the array
                        enumeratedFilePaths[m][x] = formattedFilePath.Remove(0, selectedRootFolderPath.Length);
                        // add a ... to the displayed file path to indicate a relative path
                        formattedFilePath = "..." + formattedFilePath.Remove(0, selectedRootFolderPath.Length);
                    }

                    switch (fileExtension)
                    {
                        case ".html":
                            // add it to the HTML combobox to allow the user to select it
                            pageHTMLFileCB.Items.Add(formattedFilePath);
                            break;
                        case ".css":
                            // add it to the CSS combobox to allow the user to select it
                            pageCSSFileCB.Items.Add(formattedFilePath);
                            // unlock the CSS minification options (since at least 1 CSS file exists)
                            minifyCSSCB.Enabled = true;
                            break;
                        case ".js":
                            // add it to the 'files to minify' combobox (since it is a file type that can be minified)
                            filesToMinifyCLB.Items.Add( "(" + fileExtension.ToUpper() + ") " + formattedFilePath);
                            // check the item if the listbox is enabled (since the user will probably want to minify the files by default)
                            filesToMinifyCLB.SetItemChecked(filesToMinifyCLB.Items.Count - 1, filesToMinifyCLB.Enabled);
                            // unlock the javascript minification options (since at least 1 JS file exists)
                            minifyJSCB.Enabled = true;
                            break;
                        case ".json":
                            // add it to the 'files to minify' listbox (since it is a file type that can be minified)
                            filesToMinifyCLB.Items.Add("(" + fileExtension.ToUpper() + ") " + formattedFilePath);
                            // check the item if the listbox is enabled (since the user will probably want to minify the files by default)
                            filesToMinifyCLB.SetItemChecked(filesToMinifyCLB.Items.Count - 1, filesToMinifyCLB.Enabled);
                            // unlock the JSON minification options (since at least 1 JSON file exists)
                            minifyJSONCB.Enabled = true;
                            break;
                    }

                    // prepend the file extension (to make it easier to notice the type of file)
                    formattedFilePath = "(" + fileExtension.ToUpper() + ") " + formattedFilePath;

                    // add the file path to the listbox
                    this.filesToPackCLB.Items.Add(formattedFilePath);
                    this.filesToPackCLB.SetItemChecked(this.filesToPackCLB.Items.Count - 1, true);
                }
            }

            // select & enable the first HTML & CSS files in the combobox, if populated
            // (remove first 3 dots
            if (pageHTMLFileCB.Items.Count > 0)
            {
                pageHTMLFileCB.SelectedIndex = 0;
                pageHTMLFilePath = pageHTMLFileCB.SelectedItem.ToString();
                if (pageHTMLFilePath.Substring(0, 3) == "...") { pageHTMLFilePath = pageHTMLFilePath.Remove(0, 3); }
                pageHTMLFileCB.Enabled = true;
            }

            if (pageCSSFileCB.Items.Count > 0)
            {
                pageCSSFileCB.SelectedIndex = 0;
                pageCSSFilePath = pageCSSFileCB.SelectedItem.ToString();
                if (pageCSSFilePath.Substring(0, 3) == "...") { pageCSSFilePath = pageCSSFilePath.Remove(0, 3); }
                pageCSSFileCB.Enabled = true;
            }
            else
            {
                pageCSSFileCB.Enabled = false;
            }

            // enable user input controls, so the user can interact with it
            this.filesToPackCLB.Enabled = true;
            this.filesToMinifyCLB.Enabled = true;
            this.jsEntryPointFuncNameTB.Enabled = true;
        }

        [DllImport("Shlwapi.dll", CharSet = CharSet.Auto)]
        static extern long StrFormatByteSize(
            long fileSize,
            StringBuilder buffer,
            int bufferSize);

        static string FormatBytes(long bytes)
        {
            StringBuilder sb = new StringBuilder(32);
            StrFormatByteSize(bytes, sb, sb.Capacity);
            return sb.ToString();
        }

        // this function updates the status of the 'files to minify' CLB based on the status of the options (so if the user disables minification by unchecking
        // all minification options, the 'files to minify clb' is disabled since the user explictly requested no minification occur)
        private void updateMinifyCLBEnableStatus()
        {
            filesToMinifyCLB.Enabled = minifyJSCB.Checked || minifyJSONCB.Checked;

            string currentItem = "";
            string currentItemFileExtension = "";

            // update check state of files based on the option the user selected
            for (int m = 0; m < filesToMinifyCLB.Items.Count; m++)
            {
                currentItem = filesToMinifyCLB.Items[m].ToString();
                currentItemFileExtension = currentItem.Substring(0, currentItem.IndexOf(")")+1);
                //MessageBox.Show(currentItemFileExtension);

                switch (currentItemFileExtension)
                {
                    case "(.JS)":
                        filesToMinifyCLB.SetItemChecked(m, filesToPackCLB.GetItemCheckState(filesToPackCLB.Items.IndexOf(currentItem)) == CheckState.Checked &&
                            minifyJSCB.Checked);
                        break;
                    case "(.JSON)":
                        filesToMinifyCLB.SetItemChecked(m, filesToPackCLB.GetItemCheckState(filesToPackCLB.Items.IndexOf(currentItem)) == CheckState.Checked &&
                            minifyJSONCB.Checked);
                        break;
                }
            }
        }

        public static string NormalizeLongPath(string path)
        {
            // Absolute path
            path = System.IO.Path.GetFullPath(path);

            // Trim trailing backslash for files
            if (!System.IO.Directory.Exists(path) && path.EndsWith(@"\"))
                path = path.TrimEnd('\\');

            // Add \\?\ prefix if not already
            if (!path.StartsWith(@"\\?\"))
                path = @"\\?\" + path;

            return path;
        }

        #endregion
    }
}
