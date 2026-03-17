using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace PagePacker
{
    /// <summary>
    /// This class provides methods to shrink text containing CSS data.
    /// </summary>
    public class CSSMinifier
    {
        #region VARIABLES

        /// <summary>
        /// Enumerates the options that the minifier can be configured with to adjust the level of minification & modification
        /// to the source code. Options can be combined by using the '|' operator and assigning it to a variable of the type
        /// MinifierOptions.
        /// 
        /// =====OPTION EXPLANATIONS=====
        /// 
        /// REMOVE_COMMENTS: Omits all comments (started by '/*' and terminated by */') from the minified code.
        /// 
        /// REMOVE_LINE_BREAKS: Omits all line breaks ('\n') from the minified code, making the code more compact but harder to read.
        /// </summary>
        [Flags]
        public enum MinifierOptions
        {
            REMOVE_COMMENTS = 0,
            REMOVE_LINE_BREAKS = 1,
        }

        #endregion

        #region FIELDS

        /// <summary>
        /// A constrained version of the javascript minifier used to minify the CSS data
        /// </summary>
        private JSMinifier minifier;

        #endregion

        #region CONSTRUCTOR

        /// <summary>
        /// Initializes a CSSMinifier instance, configured with the options specified in the 'minifierOptions' parameter.
        /// </summary>
        /// <param name="minifierOptions">Configures the minifier to apply specific minification strategies to reduce the output CSS size.
        /// Options can be combined using the '|' operator.</param>
        public CSSMinifier(CSSMinifier.MinifierOptions minifierOptions =
            CSSMinifier.MinifierOptions.REMOVE_COMMENTS | CSSMinifier.MinifierOptions.REMOVE_LINE_BREAKS)
        {
            // set up instance variables
            // -configure the options for the minifier
            JSMinifier.MinifierOptions internalMinifierOptions = 0;

            if (minifierOptions.HasFlag(CSSMinifier.MinifierOptions.REMOVE_COMMENTS))
            {
                internalMinifierOptions = internalMinifierOptions | JSMinifier.MinifierOptions.REMOVE_COMMENTS;
            }

            if (minifierOptions.HasFlag(CSSMinifier.MinifierOptions.REMOVE_LINE_BREAKS))
            {
                internalMinifierOptions = internalMinifierOptions | JSMinifier.MinifierOptions.REMOVE_LINE_BREAKS;
            }

            this.minifier = new JSMinifier(internalMinifierOptions);
        }

        #endregion

        #region FUNCTIONS

        /// <summary>
        /// This function minifies a string containing CSS (provided for the 'cssFileText' parameter) and returns it in a minified form.
        /// </summary>
        /// <param name="cssFileText">An array of strings containing CSS text. If null or empty, an empty string will be returned.</param>
        public string minifyCSSFile(string cssFileText)
        {
            #region ERROR CHECK PARAMETERS

            // check if jsFileText is null to avoid a null exception
            if (cssFileText == null)
            {
                return "";
            }

            if (cssFileText.Length == 0)
            {
                return "";
            }

            #endregion

            #region VARIABLES

            // stores the CSS that has been combined from all entries in the cssFileText array and minified
            string minifiedCSS = "";

            #endregion

            // apply minification to the CSS file text
            minifiedCSS = minifier.minifyJSFiles(new string[] { cssFileText }, "");

            // return the minified CSS
            return minifiedCSS;
        }

        #endregion
    }
}
