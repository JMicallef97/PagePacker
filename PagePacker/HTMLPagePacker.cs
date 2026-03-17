using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Runtime.InteropServices;

namespace PagePacker
{
    /// <summary>
    /// This class contains methods used to pack a series of webpage-related files (HTML, CSS, JS, JSON, etc) into a single HTML file for maximum portability
    /// </summary>
    public static class HTMLPagePacker
    {
        /// <summary>
        /// This function accepts a folder path (the root directory of the files to include), a list of file paths (organized into arrays based on their file extension), 
        /// a page HTML file path (a path relative to the root directory, like  \index.html), a list of file paths of files that should have minification applied to them, 
        /// and optionally a page CSS file path and returns a string containing the contents of an HTML file with the contents of all files incorporated into it. If 
        /// any required parameter is missing (page HTML file path or root directory folder path is invalid), a blank string will be returned and the parameter 
        /// 'failureReason' will be populated with a reason for the function failing to return a packed HTML file.
        /// </summary>
        /// <returns></returns>
        public static string packFilesIntoHTMLPage(string rootDirectory, string pageHTMLFilePath, string pageCSSFilePath, string[] htmlFilePaths, string[] cssFilePaths, 
            string[] javascriptFilePaths, string[] jsonFilePaths, string[] minifyFilePaths, string jsEntryPointFuncName, bool minifyJS, bool minifyJSON, bool minifyCSS,
            JSMinifier.MinifierOptions jsMinificationOptions, CSSMinifier.MinifierOptions cssMinificationOptions, out string failureReason)
        {
            // declare local variables to simplify code
            string packedPageHTML = ""; // stores the final formatted HTML
            string[] basePageHTMLLines = null; // stores the HTML from the page HTML file
            string currentlyIteratedBPHLine = "";
            int currentBasePageHTMLLineIndex = 0;
            JSMinifier jsMinifier = new JSMinifier(jsMinificationOptions);
            JSONMinifier jsonMinifier = new JSONMinifier();
            CSSMinifier cssMinifier = new CSSMinifier(cssMinificationOptions);

            #region VALIDATE INPUTS

            if (!DirectoryExistsLongPath(rootDirectory))
            {
                failureReason = "Invalid root directory";
                return "";
            }

            Console.WriteLine(rootDirectory + pageHTMLFilePath);

            if (!FileExistsLongPath(rootDirectory + pageHTMLFilePath))
            {
                failureReason = "Invalid page HTML file path";
                return "";
            }

            try
            {
                // load the base page HTML into local variables (basePageHTML)
                basePageHTMLLines = LongPathReadAllLines(rootDirectory + pageHTMLFilePath);
            }
            catch (Exception e)
            {
                System.Windows.Forms.MessageBox.Show(e.ToString());
                failureReason = "Could not open page HTML file";
                return "";
            }

            #endregion

            #region INITIALIZE PACKED PAGE HTML

            // populate the packed page HTML with default data
            packedPageHTML = @"<!DOCTYPE html>
<html lang=""en"">" + Environment.NewLine;

            #region INCLUDE CSS FILE (if exists)

            // ensure the file path actually exists before trying to include the CSS file
            if (FileExistsLongPath(rootDirectory + pageCSSFilePath))
            {
                // include the CSS file
                try
                {
                    string pageCSSFileText = LongPathReadAllText(rootDirectory + pageCSSFilePath);

                    // add a CSS block in the page HTML by using a 'style' tag
                    packedPageHTML += "<style>" + Environment.NewLine;

                    // check if the user requested to minify the javascript
                    if (minifyCSS)
                    {
                        // minify the CSS & add it to the packed page HTML
                        pageCSSFileText = cssMinifier.minifyCSSFile(pageCSSFileText);
                    } 
                    // otherwise don't apply minification

                    // add the page css file text to the HTML file text to complete CSS file inclusion
                    packedPageHTML += pageCSSFileText + Environment.NewLine;

                    // close out the CSS block
                    packedPageHTML += "</style>" + Environment.NewLine;
                }
                catch (Exception e)
                {
                    System.Windows.Forms.MessageBox.Show(e.ToString());
                    failureReason = "Could not open page CSS file";
                    return "";
                }
            }
            // otherwise no CSS file was selected; don't include any

            #endregion

            // copy lines from the head of the base page HTML into the packed page HTML (being sure to skip external links to files, since those won't be usable)
            // [assumed to begin at line 3]

            #region PARSE BASE HTML FILE HEAD

            // start the region off with a 'head' tag
            packedPageHTML += "<head>" + Environment.NewLine;

            currentBasePageHTMLLineIndex = 3;
            while (basePageHTMLLines[currentBasePageHTMLLineIndex-1] != "</head>")
            {
                // get the current line
                currentlyIteratedBPHLine = basePageHTMLLines[currentBasePageHTMLLineIndex];

                // check if the line is not a link to an external resource (which can't be used in a packed file) and the line isn't whitespace
                if (!(currentlyIteratedBPHLine.Contains("    <link rel") || 
                    currentlyIteratedBPHLine.Contains("    <script src") ||
                    currentlyIteratedBPHLine.Trim() == ""))
                {
                    // add the line to the output
                    packedPageHTML += currentlyIteratedBPHLine + Environment.NewLine;
                }

                // increment to advance to the next line
                currentBasePageHTMLLineIndex++;
            }

            #endregion

            #region PARSE BASE HTML FILE BODY

            // parse the body of the base HTML file
            while (basePageHTMLLines[currentBasePageHTMLLineIndex] != "</body>")
            {
                // get the current line
                currentlyIteratedBPHLine = basePageHTMLLines[currentBasePageHTMLLineIndex];

                // check if the line is not whitespace
                if (currentlyIteratedBPHLine.Trim() != "")
                {
                    // add the line to the output
                    packedPageHTML += currentlyIteratedBPHLine + Environment.NewLine;
                }

                // increment to advance to the next line
                currentBasePageHTMLLineIndex++;
            }

            #endregion

            if (javascriptFilePaths != null && javascriptFilePaths.Length > 0)
            {
                #region INCLUDE JAVASCRIPT

                // start a javascript section by adding a 'script' tag
                packedPageHTML += "<script>" + Environment.NewLine;

                // declare local variables to simplify code
                string currentJSFileText = "";
                string minifiedJSFileText = "";
                bool jsProcessingSucceeded = true;
                List<string> combinedJSFileText = new List<string>();

                // include the javascript files
                for (int m = 0; m < javascriptFilePaths.Length; m++)
                {
                    // try loading the file
                    try
                    {
                        currentJSFileText = LongPathReadAllText(rootDirectory + javascriptFilePaths[m]);

                        // minify the javascript if the option is selected
                        if (minifyJS && minifyFilePaths.Contains(javascriptFilePaths[m]))
                        {
                            // add the file text to the list of files (since the minifier processes an entire batch of files at once)
                            combinedJSFileText.Add(currentJSFileText);
                        }
                        else
                        {
                            // include javascript as-is
                            packedPageHTML += currentJSFileText + Environment.NewLine;
                        }
                    }
                    catch (Exception e)
                    {
                        // report the error & set a flag to exclude the javascript (since it contains errors)
                        System.Windows.Forms.MessageBox.Show(e.ToString());
                        failureReason = "Couldn't load or format Javascript file '" + Path.GetFileNameWithoutExtension(javascriptFilePaths[m]) + "'";
                        jsProcessingSucceeded = false;
                        return "";
                    }
                }

                if (jsProcessingSucceeded)
                {
                    minifiedJSFileText = jsMinifier.minifyJSFiles(combinedJSFileText.ToArray(), jsEntryPointFuncName);
                    packedPageHTML += minifiedJSFileText + Environment.NewLine;
                }

                // end the javascript section by adding a /script tag
                packedPageHTML += "</script>" + Environment.NewLine;

                #endregion
            }

            if (jsonFilePaths != null && jsonFilePaths.Length > 0)
            {
                #region INCLUDE JSON

                // declare local variables to simplify code
                string currentJSONFileText = "";
                string pageJSONBody = "";
                bool jsonProcessingSucceeded = true;
                List<string> combinedJSONFileText = new List<string>();

                for (int m = 0; m < jsonFilePaths.Length; m++)
                {
                    // try loading the file
                    try
                    {
                        // load the file text
                        currentJSONFileText = LongPathReadAllText(rootDirectory + jsonFilePaths[m]);

                        // add a JSON section header to contain the JSON
                        pageJSONBody += @"<script id=""" + Path.GetFileNameWithoutExtension(jsonFilePaths[m]) + @""" type=""application/json"">" + Environment.NewLine;

                        // minify the javascript if the option is selected
                        if (minifyJSON)
                        {
                            // add the json file text to the list to be minified (since the minifier processes the JSON in batches)
                            pageJSONBody += jsonMinifier.minifyJSONFile(currentJSONFileText) + Environment.NewLine;
                        }
                        else
                        {
                            // include javascript as-is
                            pageJSONBody += currentJSONFileText + Environment.NewLine;

                        }

                        // close the section off with a </script> tag
                        pageJSONBody += "</script>" + Environment.NewLine;
                    }
                    catch (Exception e)
                    {
                        // report the error & set a flag to exclude the javascript (since it contains errors)
                        System.Windows.Forms.MessageBox.Show(e.ToString());
                        failureReason = "Couldn't load or format JSON file '" + Path.GetFileNameWithoutExtension(javascriptFilePaths[m]) + "'";
                        jsonProcessingSucceeded = false;
                        return "";
                    }
                }

                if (jsonProcessingSucceeded)
                {
                    // add the minified JSON text to the packed page HTML to ensure it is output
                    packedPageHTML += pageJSONBody;
                }

                #endregion
            }

            #region END FILE

            packedPageHTML += @"</body>
</html>";

            #endregion

            #endregion

            // if this point was reached, no errors occurred; set failureReason to blank since no error occurred & return the packed page HTML
            // to be written to a file
            failureReason = "";
            return packedPageHTML;
        }

        #region HELPER FUNCTIONS

        // DLL imports that contain Win32 calls used by helper functions

        [DllImport("kernel32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
        static extern uint GetFileAttributes(string lpFileName);

        [DllImport("kernel32.dll", SetLastError = true, CharSet = CharSet.Unicode)]
        private static extern IntPtr CreateFile(
            string lpFileName,
            uint dwDesiredAccess,
            uint dwShareMode,
            IntPtr lpSecurityAttributes,
            uint dwCreationDisposition,
            uint dwFlagsAndAttributes,
            IntPtr hTemplateFile);

        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern bool ReadFile(
            IntPtr hFile,
            [Out] byte[] lpBuffer,
            uint nNumberOfBytesToRead,
            out uint lpNumberOfBytesRead,
            IntPtr lpOverlapped);

        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern bool CloseHandle(IntPtr hObject);

        // Win32 constants
        private const uint GENERIC_READ = 0x80000000;
        private const uint FILE_SHARE_READ = 0x00000001;
        private const uint OPEN_EXISTING = 3;
        private const uint FILE_ATTRIBUTE_NORMAL = 0x80;
        private const uint INVALID_FILE_ATTRIBUTES = 0xFFFFFFFF;
        private const uint FILE_ATTRIBUTE_DIRECTORY = 0x10;

        /// <summary>
        /// This function checks if a directory with a potentially long path (>260 characters) exists in the file system or not.
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        public static bool DirectoryExistsLongPath(string path)
        {
            uint attr = GetFileAttributes(path);
            return (attr & FILE_ATTRIBUTE_DIRECTORY) != 0;
        }

        /// <summary>
        /// Checks if a file exists, supporting long paths (>260 chars). Required due to windows not allowing file paths longer than 
        /// 260 characters in System.IO calls
        /// </summary>
        public static bool FileExistsLongPath(string path)
        {
            // Add \\?\ prefix if not present
            if (!path.StartsWith(@"\\?\"))
                path = @"\\?\" + path;

            uint attr = GetFileAttributes(path);

            // Ensure it is NOT a directory
            return (attr & FILE_ATTRIBUTE_DIRECTORY) == 0;
        }

        /// <summary>
        /// Reads all lines from a local file, supporting extended paths (\\?\) using Win32 APIs to avoid .NET path restrictions.  
        /// Required due to windows not allowing file paths longer than 260 characters in System.IO calls
        /// </summary>
        public static string[] LongPathReadAllLines(string path)
        {
            if (string.IsNullOrWhiteSpace(path))
                throw new ArgumentException("Path is null or empty.");

            // Ensure the path is absolute. Win32 APIs require fully qualified paths.
            if (!Path.IsPathRooted(path))
            {
                path = Path.GetFullPath(path);
            }

            // Prepend \\?\ if not already
            if (!path.StartsWith(@"\\?\"))
            {
                path = @"\\?\" + path;
            }

            IntPtr hFile = CreateFile(
                path,
                GENERIC_READ,
                FILE_SHARE_READ,
                IntPtr.Zero,
                OPEN_EXISTING,
                FILE_ATTRIBUTE_NORMAL,
                IntPtr.Zero);

            if (hFile == new IntPtr(-1))
                throw new IOException("Unable to open file due to Win32 error: " + Marshal.GetLastWin32Error());

            try
            {
                List<string> lines = new List<string>();
                const int bufferSize = 4096;
                byte[] buffer = new byte[bufferSize];
                uint bytesRead;
                StringBuilder sb = new StringBuilder();

                while (ReadFile(hFile, buffer, bufferSize, out bytesRead, IntPtr.Zero) && bytesRead > 0)
                {
                    string chunk = Encoding.UTF8.GetString(buffer, 0, (int)bytesRead);
                    sb.Append(chunk);

                    string[] split = sb.ToString().Split(new[] { "\r\n", "\n" }, StringSplitOptions.None);

                    // Keep last incomplete line in StringBuilder
                    for (int i = 0; i < split.Length - 1; i++)
                    {
                        lines.Add(split[i]);
                    }

                    sb.Clear();
                    sb.Append(split[split.Length - 1]);
                }

                // Add any remaining line
                if (sb.Length > 0)
                    lines.Add(sb.ToString());

                return lines.ToArray();
            }
            finally
            {
                CloseHandle(hFile);
            }
        }

        /// <summary>
        /// Reads the entire text from a local file using Win32 calls, supporting extended paths and special characters.  
        /// Required due to windows not allowing file paths longer than 260 characters in System.IO calls
        /// </summary>
        public static string LongPathReadAllText(string path)
        {
            if (string.IsNullOrWhiteSpace(path))
                throw new ArgumentException("Path is null or empty.");

            // Convert relative paths to absolute
            if (!Path.IsPathRooted(path))
            {
                path = Path.GetFullPath(path);
            }

            // Prepend \\?\ for long paths if not already present
            if (!path.StartsWith(@"\\?\"))
            {
                path = @"\\?\" + path;
            }

            // Open file with Win32 CreateFile
            IntPtr hFile = CreateFile(
                path,
                GENERIC_READ,
                FILE_SHARE_READ,
                IntPtr.Zero,
                OPEN_EXISTING,
                FILE_ATTRIBUTE_NORMAL,
                IntPtr.Zero);

            if (hFile == new IntPtr(-1))
                throw new IOException("Unable to open file. Win32 error: " + Marshal.GetLastWin32Error());

            try
            {
                const int bufferSize = 4096;
                byte[] buffer = new byte[bufferSize];
                uint bytesRead;
                StringBuilder sb = new StringBuilder();

                // Read file in chunks
                while (ReadFile(hFile, buffer, bufferSize, out bytesRead, IntPtr.Zero) && bytesRead > 0)
                {
                    sb.Append(Encoding.UTF8.GetString(buffer, 0, (int)bytesRead));
                }

                return sb.ToString();
            }
            finally
            {
                CloseHandle(hFile);
            }
        }

        #endregion
    }
}
