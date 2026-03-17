using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace PagePacker
{
    /// <summary>
    /// This class provides methods to shrink text containing JSON data.
    /// </summary>
    public class JSONMinifier
    {
        #region FUNCTIONS

        /// <summary>
        /// This function minifies a string containing JSON (provided for the 'jsonFileText' parameter) and returns it in a minified form.
        /// </summary>
        /// <param name="jsFileText">A string containing JSON text. If null or empty, an empty string will be returned.</param>
        public string minifyJSONFile(string jsonFileText)
        {
            #region ERROR CHECK PARAMETERS

            // check if jsonFileText is null to avoid a null exception
            if (jsonFileText == null)
            {
                return "";
            }

            if (jsonFileText.Length == 0)
            {
                return "";
            }

            #endregion

            #region FUNCTION VARIABLES

            // stores the JSON that has been combined from all entries in the jsFileText array and minified
            string minifiedJSON = "";

            // stores the character that is currently being evaluated
            char currentChar = ' ';
            // stores the previous character in the code stream, used to check if a boundary string has been encountered
            char precedingChar = ' ';

            // a flag indicating if the parser is iterating through a string (which shouldn't have whitespace removed, since it is formatted)
            bool isParsingString = false;

            // a flag that if set to true causes the parser to not output the current character to the output (minifiedJSON). For example,
            // when a whitespace character is identified it should not be added to the output (since minification's goal is to shrink the size
            // of the JSON string)
            bool skipCharOutput = false;

            #endregion

            // iterate through the code file text
            for (int z = 0; z < jsonFileText.Length; z++)
            {
                // extract the current character from the input stream & store it in a local variable to simplify code
                currentChar = jsonFileText[z];

                // check if the parser is currently inside a string or not
                if (!isParsingString)
                {
                    // Need to remove whitespace characters, but also look for the start of a string (since strings are formatted and should
                    // not have whitespace removed)
                    if (char.IsWhiteSpace(currentChar))
                    {
                        // skip character output since unnecessary whitespace should be trimmed
                        skipCharOutput = true;
                    }
                    else if (currentChar == '"')
                    {
                        // the starting string marker was found; output it to ensure the output JSON is correctly formatted
                        skipCharOutput = false;

                        // start of string has been found, so set the flag to switch the parser state to look for the end of the
                        // string.
                        isParsingString = true;
                    }
                    else
                    {
                        // non-whitespace character was found; output it (since it is part of the JSON file structure)
                        skipCharOutput = false;
                    }
                }
                else
                {
                    // check for the end of the string (will be a double quote that is not preceded by a '\', which is an escaped
                    // double quote and not a string marker)
                    if (currentChar == '"' && precedingChar != '\\')
                    {
                        isParsingString = false;
                    }

                    // make sure to output every character within a string to ensure the string is properly copied.
                    skipCharOutput = false;
                }

                // update preceding char, to ensure the next comparison works properly
                precedingChar = currentChar;

                // if not suppressed, add the current character to build up the minified JSON output
                if (!skipCharOutput)
                {
                    minifiedJSON += currentChar;
                }
            }

            // return the result
            return minifiedJSON;
        }

        #endregion
    }
}
