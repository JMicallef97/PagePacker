using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;
using System.Text.RegularExpressions;

namespace PagePacker
{
    /// <summary>
    /// This class provides functions & methods to minifiy Javascript code. It is intended to maximally shrink the size of
    /// javascript code without causing unintended, breaking changes which can sometimes happen in other minifiers. The minifier
    /// does not do any error-checking or code verification, so it requires syntax-error-free input code to produce a proper
    /// output.
    /// </summary>
    public class JSMinifier
    {
        #region ENUMS

        /// <summary>
        /// Enumerates the options that the minifier can be configured with to adjust the level of minification & modification
        /// to the source code. Options can be combined by using the '|' operator and assigning it to a variable of the type
        /// MinifierOptions.
        /// 
        /// =====OPTION EXPLANATIONS=====
        /// 
        /// REMOVE_COMMENTS: Omits all single-line (started by '//' and terminated by a newline character) and multi-line (started by '/*' and
        /// terminated by */') comments from the minified code.
        /// 
        /// REMOVE_LINE_BREAKS: Omits all line breaks ('\n') from the minified code, making the code more compact but harder to read.
        /// 
        /// REMOVE_LOG_STATEMENTS: Omits all console.log("") and console.error("") statements, reducing code size.
        /// 
        /// SHORTEN_FUNCTION_NAMES: Replaces function names with 2- or 3-character identifiers. 3-character identifiers start being used when there
        /// are greater than 52 functions in the input code.
        /// 
        /// SHORTEN_PARAMETER_NAMES: Replaces function parameter names with 1-, 2- or 3-character identifiers. 2-character identifiers start being used
        /// when there are greater than 52 parameters and local variables within a function.
        /// 
        /// SHORTEN_VARIABLE_NAMES: Replaces global & local variable parameter names with shortened identifiers (1- to 2- character identifiers for
        /// local variables, and 2- to 3-character identifiers for global variables). Local variables share the same pool of identifiers as parameters,
        /// but global variables have a separate formatting scheme similar to how function names are shortened (except they are prefixed with 'g' instead
        /// of 'f' like functions are).
        /// </summary>
        [Flags]
        public enum MinifierOptions
        {
            REMOVE_COMMENTS          = 0,
            REMOVE_LINE_BREAKS       = 1,
            REMOVE_LOG_STATEMENTS    = 2,
            SHORTEN_FUNCTION_NAMES   = 4,
            SHORTEN_PARAMETER_NAMES  = 8,
            SHORTEN_VARIABLE_NAMES   = 16,
        }

        /// <summary>
        /// Enumerates the possible states that the code parser can be in when going through the code.
        /// </summary>
        enum CodeParserState
        {
            SL_COMMENT,     // parser is going through a single-line comment
            ML_COMMENT,     // parser is going through a multi-line comment
            DQ_STRING,      // parser is going through a string marked by double quotes (")
            SQ_STRING,      // parser is going through a string marked by single quotes (")
            BT_STRING,      // parser is going through a string marked by backticks (`)
            CODE            // parser is going through code that can be minified
        }

        /// <summary>
        /// Enumerates the possible states that the code token stream can take, used to detect & respond to specific minification opportunities
        /// </summary>
        enum CodeTokenStreamState
        {
            SCANNING,   // The default state. No special conditions (variable/function declarations, etc) detected.
            FUNC_DEC,   // A function declaration has been detected (identified by the 'function' keyword being seen in the token stream)
            VAR_DEC,    // A variable declaration has been detected (identified by the 'var', 'let', or 'const' keyword being seen in the token stream)
            PARAM_DEC   // A parameter declaration has been detected (the parser shifts to the PARAM_DEC state after processing a function declaration)
        }

        #endregion

        #region CONSTANTS

        public static readonly HashSet<char> JAVASCRIPT_OPERATOR_SYMBOLS
            = new HashSet<char>()
            {
                 // arithmetic operators
                '+', '-', '*', '/', '%',
                // assignment operator
                '=',
                // comparison & equality operators
                '<', '>', '!',
                // precedence operators
                '(', ')',
                // array & indexing operators
                '[', ']',
                // scoping operators
                '{', '}',
                // bitwise operators
                '&', '^', '|', '?',
                // end-of-statement marker
                ';',
                // parameter separator operator
                ',',
                // dot operator
                '.'
            };

        #endregion

        #region FIELDS

        /// <summary>
        /// -stores a series of intervals marking the sections of the code output by the minifier's first pass as minifiable code
        /// (primarily used to crunch function names after initial scanning)
        /// </summary>
        private List<Interval> firstPassOutputCodeSegments = new List<Interval>();

        /// <summary>
        /// The starting index of the current code interval being output in the first pass of the minifier.
        /// </summary>
        private int currentIntervalStartIndex = -1;

        /// <summary>
        /// The ending index of the current code interval being output in the first pass of the minifier.
        /// </summary>
        private int currentIntervalEndIndex = -1;

        /// <summary>
        /// A boolean indicating whether minifiable code is being output by the minifier or not (such as strings and comments).
        /// </summary>
        private bool isCodeSegmentBeingOutput = false;

        /// <summary>
        /// This dictionary associates the names of functions from the original unminified source code (an entry's key) with a shortened version
        /// (an entry's value) to enable substitution of symbols to reduce code size.
        /// </summary>
        private Dictionary<string, string> functionNameSymbolTable;

        /// <summary>
        /// This dictionary associates the names of global variables from the original unminified source code (an entry's key) with a shortened version
        /// (an entry's value) to enable substitution of symbols to reduce code size.
        /// </summary>
        private Dictionary<string, string> globalVariableSymbolTable;

        /// <summary>
        /// This dictionary associates the names of local variables from the original unminified source code (an entry's key) with a shortened version
        /// (an entry's value) to enable substitution of symbols to reduce code size. Each time a new function is encountered, this dictionary gets cleared.
        /// </summary>
        private Dictionary<string, string> localVariableSymbolTable;

        #endregion

        #region OPTION FLAGS

        /// <summary>
        /// If set to true, the minifier will strip out all comments (both single-line and multi-line) from the source code.
        /// </summary>
        private bool removeComments = false;

        /// <summary>
        /// If set to true, the minifier will remove line break characters, preserving the code formatting
        /// </summary>
        private bool removeLineBreaks = false;

        /// <summary>
        /// If set to true, the minifier will remove console.log statements from the code.
        /// </summary>
        private bool removeLogStatements = false;

        /// <summary>
        /// If set to true, the minifier will 'crunch' (shorten) function names by substituting them with a shorter name (like f0).
        /// </summary>
        private bool crunchFunctionNames = false;

        /// <summary>
        /// If set to true, the minifier will 'crunch' (shorten) variable names by substituting them with a shorter name (like g0).
        /// </summary>
        private bool crunchVariableNames = false;

        /// <summary>
        /// If set to true, the minifier will 'crunch' (shorten) parameter names by substituting them with a shorter name (like p0).
        /// </summary>
        private bool crunchParameterNames = false;

        #endregion

        #region CONSTRUCTORS

        /// <summary>
        /// Initializes a JSMinifier instance, configured with the options specified in the 'minifierOptions' parameter.
        /// </summary>
        /// <param name="minifierOptions">Configures the minifier to apply specific minification strategies to reduce the output code size.
        /// Options can be combined using the '|' operator.</param>
        public JSMinifier(MinifierOptions minifierOptions = 
            MinifierOptions.REMOVE_COMMENTS | MinifierOptions.REMOVE_LINE_BREAKS | MinifierOptions.REMOVE_LOG_STATEMENTS |
            MinifierOptions.SHORTEN_FUNCTION_NAMES | MinifierOptions.SHORTEN_PARAMETER_NAMES | MinifierOptions.SHORTEN_VARIABLE_NAMES)
        {
            // initialize instance variables
            this.functionNameSymbolTable = new Dictionary<string, string>();
            this.globalVariableSymbolTable = new Dictionary<string, string>();
            this.localVariableSymbolTable = new Dictionary<string, string>();
            this.firstPassOutputCodeSegments = new List<Interval>();

            // set minifier option flags based on minifierOptions
            removeComments = minifierOptions.HasFlag(MinifierOptions.REMOVE_COMMENTS);
            removeLineBreaks = minifierOptions.HasFlag(MinifierOptions.REMOVE_LINE_BREAKS);
            removeLogStatements = minifierOptions.HasFlag(MinifierOptions.REMOVE_LOG_STATEMENTS);
            crunchFunctionNames = minifierOptions.HasFlag(MinifierOptions.SHORTEN_FUNCTION_NAMES);
            crunchParameterNames = minifierOptions.HasFlag(MinifierOptions.SHORTEN_PARAMETER_NAMES);
            crunchVariableNames = minifierOptions.HasFlag(MinifierOptions.SHORTEN_VARIABLE_NAMES);
        }

        #endregion

        #region FUNCTIONS

        /// <summary>
        /// This function minifies a collection of strings containing javascript (provided for the 'jsFileText' parameter) and returns them combined into
        /// a single string.
        /// </summary>
        /// <param name="jsFileText">An array of strings containing javascript text. If null or empty, an empty string will be returned.</param>
        /// <param name="jsEntryPointFunctionName">The name of a function which will not be modified (allowing code in other parts of the HTML file
        /// to still be able to reference the function after the code has been minified).</param>
        public string minifyJSFiles(string[] jsFileText, string jsEntryPointFunctionName)
        {
            #region ERROR CHECK PARAMETERS

            // check if jsFileText is null to avoid a null exception
            if (jsFileText == null)
            {
                return "";
            }

            if (jsFileText.Length == 0)
            {
                return "";
            }

            #endregion

            #region FUNCTION VARIABLES

            // stores the code that has been combined from all entries in the jsFileText array and minified
            string minifiedCode = "";
            // stores non-whitespace characters parsed from the input source code
            string currentToken = "";
            // a string that stores whitespace characters encountered by the parser after it parsed a token
            string whitespaceBucket = "";
            // stores non-whitespace minified characters parsed from the input source code, without accumulating operator symbols
            string currentTokenSymbol = "";
            // stores a copy of currentTokenSymbol for use when parsing parameters
            string currentParameterTokenSymbol = "";

            // stores the previous character in the code stream, used to check if a boundary string has been encountered
            char precedingChar = ' ';
            // stores the character that is currently being evaluated
            char currentChar = ' ';
            // stores a character which marks the boundary between tokens, used to determine when the parser tries identifying symbols in the
            // input stream for specific minification opportunities
            char currentTokenBoundaryCharacter = ' ';

            // The number of lines that make up the source code (specifically the number of line break characters detected)
            int lineCount = 0;
            // Represents the scope depth within the segment of code that the parser is currently processing, used to distinguish variable types and
            // function definitions vs function calls.
            int currentScopeDepth = 0;
            // Represents the 'scope depth' of parenthesis within the segment of code that the parser is currently processing, used when parsing
            // parameter definitions in function declarations to determine when the function declaration has ended.
            int parenthesisScopeDepth = 0;
            // Stores the scope depth at the start of a template literal ('backticked string') code expression, used to determine when 
            int templateLiteralExprStartingScopeDepth = 0;

            // stores the parser's current state, used to determine the action taken
            CodeParserState parserState = CodeParserState.CODE;

            // stores the current state of the token stream, used to detect or identify certain conditions in the token stream that are opportunities
            // for minification
            CodeTokenStreamState tokenStreamState = CodeTokenStreamState.SCANNING;

            // a boolean indicating if the parser is currently encountering whitespace characters, used to mark the transition between tokens and determine if whitespace
            // can be omitted
            bool isParsingWhitespace = false;
            // a boolean indicating if the parser has just encountered a string (to trigger token processing)
            bool wasStartOfStringEncountered = false;
            // a boolean indicating if the parser has just encountered the end of a string (to avoid triggering token processing)
            bool wasEndOfStringEncountered = false;
            // a flag that if set to true causes the parser to not output the current character to the output (minified code). For example,
            // when the end of a multiline comment is reached, it is undesirable to output the '/' of the multiline comment end marker '*/'.
            bool skipCharOutput = false;
            // a flag that is set to true if currentToken ended in a character which is a javascript operator. It is used to determine if white space is needed
            // between the current token and the next one.
            bool didCurrentTokenEndAsOperator = false;
            // a flag that is set to true if currentToken ended in a character which is a javascript operator. It is used to determine if white space is needed
            // between the current token and the next one.
            bool isCurrentCharAnOperator = false;
            // a flag that is set to true if a console function is being called (i.e., console. has been found in the token stream)
            bool consoleFuncStatementDetected = false;
            // a flag that is set to true if a log (console.log) statement is being processed
            bool isParsingLogStatement = false;
            // a flag that is set to true if a code expression is encountered in a template literal (a backticked string), used to signal the parser to process
            // the characters it finds as code to ensure proper variable & function name crunching & minification.
            bool isParsingTemplateLiteralCodeExpression = false;
            // a flag set to true when the start of a template literal code expression was found
            bool wasTLCEStartEncountered = false;
            // a flag set to false when the end of a template literal code expression was found
            bool wasTLCEEndEncountered = false;
            // a flag set to true if the parser is scanning a parameter which has a default expression assigned to it
            bool doesScannedParamHaveDefaultExpr = false;

            #endregion

            // reset instance variables
            isCodeSegmentBeingOutput = false;

            // iterate through the code file text
            for (int m = 0; m < jsFileText.Length; m++)
            {
                for (int z = 0; z < jsFileText[m].Length; z++)
                {
                    // reset flags
                    skipCharOutput = false;
                    wasStartOfStringEncountered = false;
                    wasEndOfStringEncountered = false;
                    wasTLCEStartEncountered = false;
                    wasTLCEEndEncountered = false;

                    // extract the current character from the input stream & store it in a local variable to simplify code
                    currentChar = jsFileText[m][z];

                    // identify the current state of the code (is either in a minifiable code segment or not)
                    #region IDENTIFY & UPDATE PARSER STATE

                    // identify the current state by looking at the previous character
                    switch (currentChar)
                    {
                        case '"':
                            #region EVAL '"'
                            // " (double quote)
                            // -if not preceded by \, is a string literal marker

                            // check if the character is escaped or not
                            if (precedingChar != '\\')
                            {
                                // take action based on what state the parser is in
                                if (parserState == CodeParserState.CODE)
                                {
                                    // start of a string has been encountered; adjust parser to state to reflect
                                    parserState = CodeParserState.DQ_STRING;
                                    wasStartOfStringEncountered = true;
                                }
                                else if (parserState == CodeParserState.DQ_STRING)
                                {
                                    // the end of a string has been encountered; adjust parser state to reflect
                                    parserState = CodeParserState.CODE;
                                    wasEndOfStringEncountered = true;
                                }
                            }
                            #endregion
                            break;
                        case '\'':
                            #region EVAL '''
                            // ' (single quote)
                            // -if not preceded by \, is a string literal marker

                            // check if the character is escaped or not
                            if (precedingChar != '\\')
                            {
                                // take action based on what state the parser is in
                                if (parserState == CodeParserState.CODE)
                                {
                                    // start of a string has been encountered; adjust parser to state to reflect
                                    parserState = CodeParserState.SQ_STRING;
                                    wasStartOfStringEncountered = true;
                                }
                                else if (parserState == CodeParserState.SQ_STRING)
                                {
                                    // the end of a string has been encountered; adjust parser state to reflect
                                    parserState = CodeParserState.CODE;
                                    wasEndOfStringEncountered = true;
                                }
                            }
                            #endregion
                            break;
                        case '`':
                            #region EVAL '`'
                            // ` (backtick single quote)
                            // -if not preceded by \, is a string literal marker

                            // check if the character is escaped or not
                            if (precedingChar != '\\')
                            {
                                // take action based on what state the parser is in
                                if (parserState == CodeParserState.CODE)
                                {
                                    // start of a string has been encountered; adjust parser to state to reflect
                                    parserState = CodeParserState.BT_STRING;
                                    wasStartOfStringEncountered = true;
                                }
                                else if (parserState == CodeParserState.BT_STRING)
                                {
                                    // the end of a string has been encountered; adjust parser state to reflect
                                    parserState = CodeParserState.CODE;
                                    wasEndOfStringEncountered = true;
                                }
                            }

                            #endregion
                            break;
                        case '/':
                            #region EVAL '/'
                            // / (single-line comment or end of multi-line comment)
                            // -if preceded by a /, then the current line is a comment
                            // -if preceded by a *, is the end of a multi-line comment

                            // check if the parser is in an applicable state
                            if (parserState == CodeParserState.CODE)
                            {
                                // check if the preceding character indicates a single-line comment
                                if (precedingChar == '/')
                                {
                                    // a single-line comment marker '//' was encountered; adjust parser state accordingly
                                    parserState = CodeParserState.SL_COMMENT;

                                    // -first, check if the caller requested comment removal (since otherwise comment marker characters shouldn't be deleted)
                                    if (removeComments)
                                    {
                                        if (minifiedCode.Length > 0 && minifiedCode[minifiedCode.Length - 1] == '/')
                                        {
                                            // remove the comment marker character
                                            //minifiedCode = minifiedCode.Substring(0, minifiedCode.Length - 1);
                                            backspaceMC(1,ref minifiedCode,false);
                                        }

                                        if (currentToken.Length > 0 && currentToken[currentToken.Length - 1] == '/')
                                        {
                                            // remove the comment marker character
                                            currentToken = currentToken.Substring(0, currentToken.Length - 1);
                                        }

                                        // set the 'skip char output' flag to true to indicate that the '/' character read during this iteration should not be output
                                        // to the code (to avoid syntax errors in the minified code)
                                        skipCharOutput = true;
                                    }
                                }
                            }
                            else if (parserState == CodeParserState.ML_COMMENT)
                            {
                                // check if the preceding character indicates a multi-line comment
                                if (precedingChar == '*')
                                {
                                    //System.Windows.Forms.MessageBox.Show(minifiedCode + " IN HERE");
                                    //test = true;

                                    // an end marker for a multi-line comment (*/) was encountered; adjust parser state accordingly
                                    parserState = CodeParserState.CODE;

                                    // -first, check if the caller requested comment removal (since otherwise comment marker characters shouldn't be deleted)
                                    if (removeComments)
                                    {
                                        if (minifiedCode.Length > 0 && minifiedCode[minifiedCode.Length - 1] == '*')
                                        {
                                            // remove the comment marker character
                                            backspaceMC(1,ref minifiedCode,false);
                                        }

                                        if (currentToken.Length > 0 && currentToken[currentToken.Length - 1] == '*')
                                        {
                                            // remove the comment marker character
                                            currentToken = currentToken.Substring(0, currentToken.Length - 1);
                                        }

                                        // set the 'skip char output' flag to true to indicate that the '*' character read during this iteration should not be output
                                        // to the code (to avoid syntax errors in the minified code)
                                        skipCharOutput = true;
                                    }
                                }
                            }

                            #endregion
                            break;
                        case '*':
                            #region EVAL '*'
                            // * (start of multi-line comment)
                            // -if preceded by a /, is the start of a multi-line ecomment

                            // check if the parser is in an applicable state
                            if (parserState == CodeParserState.CODE)
                            {
                                // check if the preceding character indicates the start of a multi-line comment
                                if (precedingChar == '/')
                                {
                                    // a multi-line comment start marker '/*' was encountered; adjust parser state accordingly
                                    parserState = CodeParserState.ML_COMMENT;
                                    
                                    // -first, check if the caller requested comment removal (since otherwise comment marker characters shouldn't be deleted)
                                    if (removeComments)
                                    {
                                        if (minifiedCode.Length > 0 && minifiedCode[minifiedCode.Length - 1] == '/')
                                        {
                                            // remove the comment marker character
                                            backspaceMC(1,ref minifiedCode,false);
                                        }

                                        if (currentToken.Length > 0 && currentToken[currentToken.Length - 1] == '/')
                                        {
                                            // remove the comment marker character
                                            currentToken = currentToken.Substring(0, currentToken.Length - 1);
                                        }

                                        // set the 'skip char output' flag to true to indicate that the '/' character read during this iteration should not be output
                                        // to the code (to avoid syntax errors in the minified code)
                                        skipCharOutput = true;
                                    }
                                }
                            }

                            #endregion
                            break;
                        case '\n':
                            #region EVAL '\n'

                            // check if the caller requested line break removal by setting 'preserveLineBreaks' to false
                            if (removeLineBreaks)
                            {
                                // skip outputting the character since the caller requested line breaks to be removed
                                skipCharOutput = true;
                            }

                            // check if the parser was going through a single-line comment
                            // (if it was, its effect is canceled the moment a new line is encountered)
                            if (parserState == CodeParserState.SL_COMMENT)
                            {
                                parserState = CodeParserState.CODE;

                                // set the 'skip output' flag to true, since there is no need to output a newline character for a commented line
                                // (since the line has nothing)
                                // -first, check if the caller requested comment removal (since otherwise newline characters should not be removed, which would
                                // mess up the code formatting)
                                if (removeComments)
                                {
                                    skipCharOutput = true;
                                }
                            }

                            // increment the lineCount variable to keep track of the number of lines
                            lineCount++;

                            #endregion
                            break;
                        case '{':
                            #region EVAL '{'

                            // check if the parser is inside a backticked string, which may contain code
                            if (parserState == CodeParserState.BT_STRING && !isParsingTemplateLiteralCodeExpression)
                            {
                                // check if a code expression was found
                                // (code expressions can be contained inside a backticked string ('template literal') bounded by the character sequence ${ }.
                                // Need to alert the parser to process characters inside code expressions to ensure variable/function name crunching and minification
                                // occurs properly)
                                if (currentChar == '{' && precedingChar == '$')
                                {
                                    // a code expression has been found; set a flag to indicate to the parser that a code expression is being parsed
                                    isParsingTemplateLiteralCodeExpression = true;
                                    wasTLCEStartEncountered = true;
                                    // record the scope depth at the start of the expression to be able to accurately determine when the expression
                                    // is finished and revert the parser back to the string-processing state
                                    templateLiteralExprStartingScopeDepth = currentScopeDepth;
                                }
                                // otherwise { is part of a string or non-code segment and is not relevant
                            }

                            // ensure bracket is from a code segment (or a code expression inside a template literal),
                            // not a comment or string (to avoid erroneous scope depth changes)
                            if (parserState == CodeParserState.CODE || 
                                (parserState == CodeParserState.BT_STRING && isParsingTemplateLiteralCodeExpression))
                            {
                                // code is entering a new block/scope level, increment the scope level
                                currentScopeDepth++;
                            }

                            #endregion
                            break;
                        case '}':
                            #region EVALUATE '}'

                            // ensure bracket is from a code segment, not a comment or string (to avoid erroneous scope depth changes)
                            if (parserState == CodeParserState.CODE ||
                                (parserState == CodeParserState.BT_STRING && isParsingTemplateLiteralCodeExpression))
                            {
                                // code is exiting a block, decrement the scope level
                                currentScopeDepth--;
                            }

                            // check if the parser is inside a backticked string, which may contain code
                            if (parserState == CodeParserState.BT_STRING && isParsingTemplateLiteralCodeExpression)
                            {
                                // check if the end of the code expression has been found (only valid if the scope depth is the same as when the
                                // code segment began)
                                if (currentScopeDepth == templateLiteralExprStartingScopeDepth)
                                {
                                    // set a flag indicating the end of the template literal code expression was found (to ensure proper handling of code)
                                    wasTLCEEndEncountered = true;

                                    // reset variables
                                    isParsingTemplateLiteralCodeExpression = false;
                                    templateLiteralExprStartingScopeDepth = 0;
                                }
                            }

                            #endregion
                            break;
                        case ';':
                            #region EVALUATE ';'
                            // ensure bracket is from a code segment, not a comment or string (to avoid erroneous scope depth changes)
                            if (parserState == CodeParserState.CODE)
                            {
                                if (isParsingLogStatement)
                                {
                                    // reset flags
                                    isParsingLogStatement = false;
                                    // prevent outputting the semicolon to avoid a semicolon without a statement (which would cause a syntax error)
                                    skipCharOutput = true;
                                }
                            }
                            #endregion
                            break;
                    }

                    // update preceding char, to ensure the next comparison works properly
                    precedingChar = currentChar;
                    
                    #endregion

                    #region PROCESS/HANDLE TOKEN SYMBOLS

                    // check if the parser is in the middle of parsing through a code segment
                    if ((parserState == CodeParserState.CODE || ((isParsingTemplateLiteralCodeExpression && 
                        !wasTLCEStartEncountered) || wasTLCEEndEncountered)) 
                            && !wasStartOfStringEncountered
                            && !wasEndOfStringEncountered)
                    {
                        #region IDENTIFY & SUBSTITUTE SYMBOLS (if requested)

                        // check if the current character in the input stream is a token boundary symbol or not
                        if (!char.IsWhiteSpace(currentChar) && !JAVASCRIPT_OPERATOR_SYMBOLS.Contains(currentChar))
                        {
                            // accumulate characters
                            currentTokenSymbol += currentChar;
                        }
                        else
                        {
                            #region IDENTIFY/UPDATE TOKEN STREAM STATE

                            // update the token boundary character
                            currentTokenBoundaryCharacter = currentChar;

                            switch (tokenStreamState)
                            {
                                case CodeTokenStreamState.SCANNING:
                                    #region SCAN FOR KEYWORDS

                                    // check if the token is a symbol name (variable name, function name, etc)
                                    // check the token at the top of the stack
                                    switch (currentTokenSymbol)
                                    {
                                        case "let":
                                        case "var":
                                        case "const":
                                            // check if the caller requested to crunch variable names 
                                            // (otherwise there is no point in tracking variable declarations)
                                            if (crunchVariableNames)
                                            {
                                                // variable declaration has occurred
                                                tokenStreamState = CodeTokenStreamState.VAR_DEC;
                                            }
                                            break;
                                        case "function":
                                            // check if the caller requested to crunch function names 
                                            // (otherwise there is no point in tracking function declarations)
                                            if (crunchVariableNames)
                                            {
                                                // possible function declaration has occurred
                                                tokenStreamState = CodeTokenStreamState.FUNC_DEC;
                                            }
                                            break;
                                        case "console":
                                            if (currentTokenBoundaryCharacter == '.')
                                            {
                                                // a console function statement has been identified; set a flag to indicate a console function statement
                                                // was detected
                                                consoleFuncStatementDetected = true;
                                            }
                                            break;
                                        case "log":
                                        case "error":
                                            #region TRY REMOVE CONSOLE STATEMENT (if requested by caller)
                                            // check if the 'consoleFuncStatementDetected' flag is set to true (indicating console. was seen previously)
                                            // and the caller gave authorization to remove the log statement (by setting 'removeLogStatements' to true)
                                            if (removeLogStatements && consoleFuncStatementDetected)
                                            {
                                                // set a flag to indicate a log statement is being parsed
                                                isParsingLogStatement = true;
                                                
                                                // reset the flag
                                                consoleFuncStatementDetected = false;

                                                // remove 'console.log' (console. + currentTokenSymbol) from currentToken, to avoid it being added to the output stream
                                                currentToken = "";
                                                //System.Windows.Forms.MessageBox.Show(minifiedCode.Substring(minifiedCode.Length - 100, 100));
                                            }
                                            #endregion
                                            break;
                                        default:
                                            // check if the symbol matches any variable or function names
                                            if (crunchVariableNames)
                                            {
                                                // check if currentTokenSymbol matches a variable name
                                                if (localVariableSymbolTable.ContainsKey(currentTokenSymbol))
                                                {
                                                    // check if currentToken is just the variable name (surrounded on both sides by whitespace) or if the
                                                    // variable name is contained within a larger token
                                                    if (currentToken.Length == currentTokenSymbol.Length)
                                                    {
                                                        // swap the original variable name for the crunched version
                                                        currentToken = localVariableSymbolTable[currentTokenSymbol];
                                                    }
                                                    else if (currentToken.Length > currentTokenSymbol.Length)
                                                    {
                                                        // ensure that currentTokenSymbol isn't preceded by a dot (to avoid function accessed by a dot operator
                                                        // with the same name as local variable names being replaced by the crunched variable name, causing a syntax
                                                        // error)
                                                        if (currentToken[currentToken.Length - currentTokenSymbol.Length - 1] != '.')
                                                        {
                                                            // swap the original variable name for the crunched version
                                                            currentToken = currentToken.Substring(0, currentToken.Length - currentTokenSymbol.Length);
                                                            currentToken += localVariableSymbolTable[currentTokenSymbol];
                                                        } 
                                                        // otherwise a dot precedes the variable name. Don't overwrite since the symbol might be a function name that happens to share the name
                                                        // with a declared variable
                                                    }
                                                }
                                            }

                                            // function name crunching must happen at the end (since functions can appear in any order)
                                            #region CRUNCH FUNCTION NAMES (commented)
                                            if (false)
                                            {
                                                // check for function names to crunch (minify)
                                                if (crunchFunctionNames)
                                                {
                                                    // check if the current token is a recognized function name
                                                    if (functionNameSymbolTable.ContainsKey(currentTokenSymbol))
                                                    {
                                                        //System.Windows.Forms.MessageBox.Show(currentTokenSymbol);

                                                        // check if currentToken is just the function name (surrounded on both sides by whitespace) or if the
                                                        // function name is contained within a larger token
                                                        if (currentToken.Length == currentTokenSymbol.Length)
                                                        {
                                                            // swap the original function name for the crunched version
                                                            currentToken = functionNameSymbolTable[currentTokenSymbol];
                                                        }
                                                        else
                                                        {
                                                            // ensure that currentTokenSymbol isn't preceded by a dot (to avoid functions accessed by a dot operator
                                                            // with the same name as a declared function name being replaced by the crunched function name, causing a syntax
                                                            // error or unexpected behavior)
                                                            if (currentToken[currentToken.Length - currentTokenSymbol.Length - 1] != '.')
                                                            {
                                                                // swap the crunched function name in for the 
                                                                currentToken = currentToken.Substring(0, currentToken.Length - currentTokenSymbol.Length);
                                                                currentToken += functionNameSymbolTable[currentTokenSymbol];
                                                            }
                                                        }
                                                    }
                                                }
                                            }

                                            #endregion
                                            break;
                                    }

                                    #endregion
                                    break;
                                case CodeTokenStreamState.FUNC_DEC:
                                    #region PROCESS FUNCTION DECLARATION

                                    // ensure the caller requested function name crunching/minification, that the scope is at the correct level for function 
                                    // declarations (which should be scope level 0, since functions are not contained within blocks) and the function name isn't 
                                    // the javascript entry point function name (which needs to be preserved to ensure that other javascript code within the 
                                    // HTML document that is unaffected by minification can still call the function)
                                    if (currentScopeDepth == 0 && currentTokenSymbol != jsEntryPointFunctionName)
                                    {
                                        // clear the local variable symbol table (since a new function has been encountered, meaning all the variables in
                                        // functionNameSymbolTable have gone out of scope and aren't being kept anymore)
                                        localVariableSymbolTable.Clear();

                                        // ensure the function name hasn't already been added to the function table to avoid a 'key already contained' error
                                        if (!functionNameSymbolTable.ContainsKey(currentTokenSymbol) && crunchFunctionNames)
                                        {
                                            // add the function name to the function symbol table
                                            //functionNameSymbolTable.Add(currentTokenSymbol, "f" + functionNameSymbolTable.Count);
                                            functionNameSymbolTable.Add(currentTokenSymbol, "f" + getCrunchedSymbolName(functionNameSymbolTable.Count));
                                        }

                                        // reset currentTokenSymbol
                                        currentTokenSymbol = "";

                                        // check if the caller wants to crunch parameter names or not
                                        if (crunchParameterNames)
                                        {
                                            // reset parenthesis scope depth (will be used in the PARAM_DEC state)
                                            // -check if the parenthesis is immediately after the function name, or separated by whitespace (which will affect
                                            // parameter parsing)
                                            if (currentChar == '(')
                                            {
                                                // no whitespace separates the function name; set scope depth to 0 to indicate parameter parsing can begin
                                                // immediately.
                                                parenthesisScopeDepth = 0;
                                            }
                                            else
                                            {
                                                // whitespace separates the function name from the start of the parameter declaration section. Paramter
                                                // parsing must wait until the opening parenthesis is found.
                                                parenthesisScopeDepth = -1;
                                            }

                                            // switch token stream state to the 'PARAM_DEC' state (since a parameter declaration might follow the function name)
                                            tokenStreamState = CodeTokenStreamState.PARAM_DEC;
                                        }
                                        else
                                        {
                                            // no need to parse/analyze parameter contents of function; skip to the scanning state for improved efficiency
                                            tokenStreamState = CodeTokenStreamState.SCANNING;
                                        }

                                    } // otherwise code is not a function declaration
                                    else
                                    {
                                        // revert back to scanning, since it's not a function declaration
                                        tokenStreamState = CodeTokenStreamState.SCANNING;
                                    }

                                    #endregion
                                    break;
                                case CodeTokenStreamState.VAR_DEC:
                                    #region PROCESS VARIABLE DECLARATION

                                    // ensure the caller specifically requested variable crunching before proceeding
                                    if (crunchVariableNames)
                                    {
                                        // add the variable to the appropriate symbol table based on the current scope (which determines what kind of variable it is)
                                        if (currentScopeDepth == 0)
                                        {
                                            // add to the global symbol table
                                            globalVariableSymbolTable.Add(currentTokenSymbol, "g" + getCrunchedSymbolName(globalVariableSymbolTable.Count));
                                            // -replace currentToken with the new variable name (to ensure the crunched variable name gets output to the output stream)
                                            currentToken = globalVariableSymbolTable[currentTokenSymbol];
                                        }
                                        else if (currentScopeDepth > 0)
                                        {
                                            // ensure the token symbol length is greater than 0 (such as the cases of unconventional declarations,
                                            // like 'for (const [tableName, requiredFields] of Object.entries(schema.dbTables)) {'
                                            if (currentTokenSymbol.Length > 0)
                                            {
                                                // ensure the variable hasn't already been added to the function table
                                                // (even if it has, there's no problem using the same variable name since in javascript variables in deeper scopes will
                                                // shadow variables in less deep scopes, and the code being input to the parser is expected to be syntactically correct).
                                                if (!localVariableSymbolTable.ContainsKey(currentTokenSymbol))
                                                {
                                                    // add to the function symbol table
                                                    localVariableSymbolTable.Add(currentTokenSymbol, getCrunchedSymbolName(localVariableSymbolTable.Count));
                                                }

                                                // reset flag (to ensure correct behavior when parsing parameter names)
                                                doesScannedParamHaveDefaultExpr = false;

                                                //System.Windows.Forms.MessageBox.Show(currentToken);
                                                // -replace currentToken with the new variable name (to ensure the crunched variable name gets output to the output stream)
                                                currentToken = localVariableSymbolTable[currentTokenSymbol];
                                            }
                                        }
                                    }

                                    // revert back to the 'scanning' state (since the variable declaration was recorded)
                                    tokenStreamState = CodeTokenStreamState.SCANNING;
                                    #endregion
                                    break;
                                case CodeTokenStreamState.PARAM_DEC:
                                    #region PROCESS PARAMETER DECLARATION

                                    // parameters will be assigned to the localVariableSymbolTable, since scope-wise they correspond
                                    // only to the function

                                    // check if the scope depth is 0 (otherwise the parser is inside a nested expression which is an expression assigned to a
                                    // parameter with a default value, and is thus of no importance for parameter parsing)
                                    if (currentScopeDepth == 0)
                                    {
                                        // check if the parenthesis scope depth is 0 (ensuring the parser doesn't accumulate characters from within nested
                                        // parenthesis expressions or exits early from the PARAM_DEC state)
                                        if (parenthesisScopeDepth == 0)
                                        {
                                            // need to retain currentTokenSymbol (in case it is separated by whitespace characters from its boundary symbol)
                                            // -Do this by recording a copy of currentTokenSymbol. Once one of the separator symbols are found, use that value.
                                            // (don't copy currentTokenSymbol if it is blank)
                                            if (currentTokenSymbol.Length != 0)
                                            {
                                                currentParameterTokenSymbol = currentTokenSymbol;
                                            }

                                            // check if the parameter is assigned a default expression or not
                                            if (currentChar == '=')
                                            {
                                                //if (currentTokenSymbol.Length > 0)
                                                if (currentParameterTokenSymbol.Length > 0)
                                                {
                                                    // save the parameter name (stored in currentToken) to the symbol table
                                                    localVariableSymbolTable.Add(currentParameterTokenSymbol, getCrunchedSymbolName(localVariableSymbolTable.Count));

                                                    // replace currentToken with the crunched variable name to ensure the parameter renaming is applied to
                                                    // the output minified code.
                                                    if (currentToken.Length > 0)
                                                    {
                                                        currentToken = currentToken.Remove(currentToken.Length - currentParameterTokenSymbol.Length,
                                                            currentParameterTokenSymbol.Length);
                                                        currentToken += localVariableSymbolTable[currentParameterTokenSymbol];
                                                    }
                                                    else
                                                    {
                                                        // old parameter name was written to the output; remove it and insert the crunched parameter name
                                                        backspaceMC(currentParameterTokenSymbol.Length, ref minifiedCode, true);
                                                        addMC(localVariableSymbolTable[currentParameterTokenSymbol], ref minifiedCode, true);
                                                    }
                                                }

                                                // set the flag to true (to indicate to the parser to not record the end of the expression as a
                                                // parameter name when the comma ',' separating the parameters is found.
                                                doesScannedParamHaveDefaultExpr = true;
                                            }
                                            else if (currentChar == ',')
                                            {
                                                if (!doesScannedParamHaveDefaultExpr)
                                                {
                                                    if (currentParameterTokenSymbol.Length == 0 &&
                                                        !JAVASCRIPT_OPERATOR_SYMBOLS.Contains(currentToken[currentToken.Length - 1]))
                                                    {
                                                        currentParameterTokenSymbol = currentToken;
                                                    }

                                                    // ensure currentParameterTokenSymbol is valid
                                                    if (currentParameterTokenSymbol.Length > 0)
                                                    {
                                                        // save the parameter name to the symbol table
                                                        localVariableSymbolTable.Add(currentParameterTokenSymbol, 
                                                            getCrunchedSymbolName(localVariableSymbolTable.Count));

                                                        // replace currentToken with the crunched variable name to ensure the parameter renaming is applied to
                                                        // the output minified code.
                                                        if (currentToken.Length > 0)
                                                        {
                                                            currentToken = currentToken.Remove(currentToken.Length - currentParameterTokenSymbol.Length,
                                                                currentParameterTokenSymbol.Length);
                                                            currentToken += localVariableSymbolTable[currentParameterTokenSymbol];
                                                        }
                                                        else
                                                        {
                                                            // old parameter name was written to the output; remove it and insert the crunched parameter name
                                                            backspaceMC(currentParameterTokenSymbol.Length, ref minifiedCode, true);
                                                            addMC(localVariableSymbolTable[currentParameterTokenSymbol], ref minifiedCode, true);
                                                        }
                                                    }
                                                }
                                                else
                                                {
                                                    // don't add to the symbol table; currentToken contains the end of the default expression assigned
                                                    // to the parameter.
                                                    // -reset the flag to ensure subsequent parameters are properly scanned
                                                    doesScannedParamHaveDefaultExpr = false;
                                                }
                                            }
                                            else if (currentChar == ')')
                                            {
                                                if (!doesScannedParamHaveDefaultExpr)
                                                {
                                                    if (currentParameterTokenSymbol.Length == 0 &&
                                                        !JAVASCRIPT_OPERATOR_SYMBOLS.Contains(currentToken[currentToken.Length - 1]))
                                                    {
                                                        currentParameterTokenSymbol = currentToken;
                                                    }

                                                    // ensure currentParameterTokenSymbol is valid
                                                    if (currentParameterTokenSymbol.Length > 0)
                                                    {
                                                        // save the parameter name to the symbol table
                                                        localVariableSymbolTable.Add(currentParameterTokenSymbol,
                                                            getCrunchedSymbolName(localVariableSymbolTable.Count));

                                                        // replace currentToken with the crunched variable name to ensure the parameter renaming is applied to
                                                        // the output minified code.
                                                        if (currentToken.Length > 0)
                                                        {
                                                            currentToken = currentToken.Remove(currentToken.Length - currentParameterTokenSymbol.Length,
                                                                currentParameterTokenSymbol.Length);
                                                            currentToken += localVariableSymbolTable[currentParameterTokenSymbol];
                                                        }
                                                        else
                                                        {
                                                            // old parameter name was written to the output; remove it and insert the crunched parameter name
                                                            backspaceMC(currentParameterTokenSymbol.Length, ref minifiedCode, true);
                                                            addMC(localVariableSymbolTable[currentParameterTokenSymbol], ref minifiedCode, true);
                                                        }
                                                    }
                                                }
                                                else
                                                {
                                                    // don't add to the symbol table; currentToken contains the end of the default expression assigned
                                                    // to the parameter.
                                                    // -reset the flag to ensure subsequent parameters are properly scanned
                                                    doesScannedParamHaveDefaultExpr = false;
                                                }

                                                // reset currentParameterTokenSymbol (since the end of the function declaration has been reached; there is no
                                                // need to retain the value)
                                                currentParameterTokenSymbol = "";

                                                // the end of the function declaration has been found
                                                // -switch token stream state to the 'scanning' state (since the parenthesis marking the end of the function
                                                // declaration has been fully parsed
                                                tokenStreamState = CodeTokenStreamState.SCANNING;
                                            }
                                        }

                                        // update the parenthesis scope depth (to ensure that the parser only accumulates characters that correspond
                                        // to parameter names, and not characters that are inside nested expressions)
                                        if (currentChar == '(')
                                        {
                                            parenthesisScopeDepth++;
                                        }
                                        else if (currentChar == ')')
                                        {
                                            parenthesisScopeDepth = Math.Max(parenthesisScopeDepth - 1, 0);
                                        }
                                    }
                                    else
                                    {
                                        if (currentChar == '(')
                                        {
                                            parenthesisScopeDepth = 0;
                                        }
                                    }

                                    #endregion
                                    break;
                            }

                            #endregion

                            // reset the current token symbol to avoid spillover into the next token analysis decision
                            currentTokenSymbol = "";
                        }

                        #endregion
                    }

                    #endregion

                    #region MANAGE TOKENS & WHITESPACE

                    if ((parserState == CodeParserState.CODE || wasStartOfStringEncountered || 
                        ((isParsingTemplateLiteralCodeExpression && !wasTLCEStartEncountered) || wasTLCEEndEncountered))
                        && !wasEndOfStringEncountered && !skipCharOutput && !isParsingLogStatement)
                    {
                        // set the 'skipCharOutput' flag to true (since this section handles character/string injection into the output stream)
                        // if a string wasn't encountered (otherwise the string literal marker will be output at the end of the string, rather
                        // than at the start which will cause a syntax error)
                        skipCharOutput = true && !wasStartOfStringEncountered && !wasTLCEEndEncountered;

                        #region PROCESS CODE SEGMENT
                        if (wasStartOfStringEncountered)
                        {
                            // can add the token directly in front of the string
                            addMC(currentToken, ref minifiedCode, true); // outputs code
                            currentToken = "";
                        }
                        else
                        {
                            // check if the current character is whitespace or not
                            if (!char.IsWhiteSpace(currentChar))
                            {
                                #region HANDLE NON-WHITESPACE CHARACTERS

                                // check if the parser was just parsing whitespace (indicating the whitespace between 2 non-whitespace tokens was encountered),
                                // indicating the next token to be processed. Alternatively, check if the parser reached the end of a code expression (which may or may not
                                // have whitespace to signal a transition)
                                if (isParsingWhitespace || wasTLCEEndEncountered)
                                {
                                    if (currentToken.Length > 0)
                                    {
                                        // get the status of the character on the end of the current token and the current character
                                        // (to determine if whitespace is necessary)
                                        didCurrentTokenEndAsOperator = currentToken.Length > 0 && JAVASCRIPT_OPERATOR_SYMBOLS.Contains(currentToken.Last()); // todo; handle case where currentToken length is 0
                                        isCurrentCharAnOperator = JAVASCRIPT_OPERATOR_SYMBOLS.Contains(currentChar);

                                        if (didCurrentTokenEndAsOperator || isCurrentCharAnOperator)
                                        {
                                            // preserve line break characters (\n) within the string, if it contains any
                                            if (whitespaceBucket.Contains('\n') && !removeLineBreaks)
                                            {
                                                // preserve only newline characters
                                                whitespaceBucket = Regex.Replace(whitespaceBucket, @"[^\r\n]+", "");
                                            }
                                            else
                                            {
                                                // no newline characters to preserve; string can be set to empty since whitespace isn't needed
                                                whitespaceBucket = "";
                                            }
                                        }
                                        else
                                        {
                                            // no operators exist between the tokens; whitespace is needed to avoid a syntax error
                                            // preserve line break characters (\n) within the string, if it contains any
                                            if (whitespaceBucket.Contains('\n') && !removeLineBreaks)
                                            {
                                                // preserve only newline characters
                                                whitespaceBucket = Regex.Replace(whitespaceBucket, @"[^\r\n]+", "");
                                            }
                                            else
                                            {
                                                // no newline characters to preserve; only a single space is needed
                                                whitespaceBucket = " ";
                                            }
                                        }

                                        // add the current token and the whitespace provided to the minified code
                                        addMC(currentToken, ref minifiedCode, true);
                                        addMC(whitespaceBucket, ref minifiedCode, true);
                                    }

                                    // reset the flag to reset the parser state
                                    isParsingWhitespace = false;

                                    // add the next character to currentToken and whitespaceBucket to prepare for the next token
                                    currentToken = "";
                                    whitespaceBucket = "";
                                }

                                // add the current character to the token to keep processing the code
                                // -don't add the string literal marker at the start of the token (to avoid an extra string marker being added at the end of the string,
                                // causing a syntax error
                                // -don't add the } character encountered at the end of a template literal code expression (TLCE) to avoid a syntax error
                                if (!wasStartOfStringEncountered  && !wasTLCEEndEncountered)
                                {
                                    // accumulate the character in currentToken
                                    currentToken += currentChar;
                                }

                                #endregion
                            }
                            else
                            {
                                #region HANDLE WHITESPACE CHARACTER

                                // accumulate the character in the whitespaceBucket
                                whitespaceBucket += currentChar;
                                // set the 'isParsingWhitespace' flag to indicate the parser's current state
                                isParsingWhitespace = true;

                                #endregion
                            }
                        }

                        #endregion
                    }

                    #endregion

                    #region APPLY SETTINGS

                    // check if the comment removal option applies, and combine its result with the 'skip output' flag
                    skipCharOutput = skipCharOutput || ((removeComments && (parserState == CodeParserState.SL_COMMENT || parserState == CodeParserState.ML_COMMENT)));

                    // check if the log statement removal option applies, and combine its results with the 'skip output' flag
                    skipCharOutput = skipCharOutput || ((removeLogStatements && isParsingLogStatement));

                    #endregion

                    // check to ensure that code didn't set the 'skip output' flag before outputting to the minified code
                    if (!skipCharOutput)
                    {
                        // append the current character from the input stream to the output stream
                        //minifiedCode += currentChar;
                        addMC(currentChar.ToString(), ref minifiedCode, 
                            (parserState == CodeParserState.CODE || isParsingTemplateLiteralCodeExpression)); // may or may not be code
                    }
                }
            }

            // check if currentToken contains any more characters
            if (currentToken.Length > 0)
            {
                // add the current token to the minified code to ensure code is properly processed (and to avoid a syntax error)
                addMC(currentToken, ref minifiedCode, (parserState == CodeParserState.CODE));
            }

            // check if there is still an open code segment (which will almost certainly occur since javascript code is organized into
            // functions, which won't end in a string
            if (currentIntervalStartIndex != -1)
            {
                // close the final segment to ensure all code segments are recorded
                firstPassOutputCodeSegments.Add(new Interval(currentIntervalStartIndex, minifiedCode.Length-1));  // - 1
            }

            // verified code segment detection works properly @ 1137 16 March 26
            #region TEST CODE SEGMENT DETECTION (commented)

            //StringBuilder firstPassMinifiedCode = new StringBuilder(minifiedCode);
            //for (int m = 0; m < firstPassOutputCodeSegments.Count; m++)
            //{
            //    // mark code segments
            //    // -verified successful detection of code segments (strings properly delineated, all non-newline code characters marked)
            //    // @ 1137 16 March 26
            //    if (true)
            //    {
            //        for (int z = firstPassOutputCodeSegments[m].IntervalStart; z <= firstPassOutputCodeSegments[m].IntervalEnd; z++)
            //        {
            //            if (firstPassMinifiedCode[z] != '\n')
            //            {
            //                firstPassMinifiedCode[z] = '#';
            //            }
            //        }
            //    }

            //    // mark non-code segments (strings)
            //    // -verified successful detection of code segments (no syntax errors) @ 1133 16 March 2026
            //    if (false)
            //    {
            //        if (m < firstPassOutputCodeSegments.Count - 1)
            //        {
            //            for (int z = firstPassOutputCodeSegments[m].IntervalEnd + 2;
            //                z < firstPassOutputCodeSegments[m + 1].IntervalStart - 1;
            //                z++)
            //            {
            //                firstPassMinifiedCode[z] = '#';
            //            }
            //        }
            //        else
            //        {
            //            for (int z = firstPassOutputCodeSegments[m].IntervalEnd;
            //                z < minifiedCode.Length; z++)
            //            {
            //                firstPassMinifiedCode[z] = '#';
            //            }
            //        }
            //    }
            //}

            //minifiedCode = firstPassMinifiedCode.ToString();

            #endregion

            // check if the caller requested to crunch function names
            if (crunchFunctionNames)
            {
                #region CRUNCH FUNCTION NAMES

                // reset currentCha & currentToken for its use in the parsing loop
                currentChar = ' ';
                currentToken = "";

                // stores the preceding operator character before the current token (used to ensure symbol names are only replaced in valid,
                // safe circumstances)
                char precedingOperator = ' ';

                // an offset which accounts for the shrinkage that occurs as the minified code is minified even further in the second pass
                // (when function names are crunched)
                int minificationOffset = 0;

                // place minifiedCode into a StringBuilder to make a copy and enable substitution of characters
                StringBuilder minifiedCode2ndPass = new StringBuilder(minifiedCode);

                for (int m = 0; m < firstPassOutputCodeSegments.Count; m++)
                {
                    for (int z = firstPassOutputCodeSegments[m].IntervalStart; z <= firstPassOutputCodeSegments[m].IntervalEnd; z++)
                    {
                        // record currentChar (to determine if the end of a possible symbol has been reached)
                        currentChar = minifiedCode2ndPass[z - minificationOffset];

                        if (!JAVASCRIPT_OPERATOR_SYMBOLS.Contains(currentChar) && !char.IsWhiteSpace(currentChar))
                        {
                            // character is part of a token; add the character to currentToken
                            currentToken += currentChar;
                        }
                        else
                        {
                            // Ensure the symbol is not preceded by a dot '.', which would indicate a function or variable name shares the same name as 
                            // a system function or object property name. In that case the symbol should not be replaced, otherwise syntax errors or 
                            // unexpected behavior could occur.
                            if (precedingOperator != '.')
                            {
                                // check if the conditions are right for a function name
                                if (functionNameSymbolTable.ContainsKey(currentToken))
                                {
                                    // replace the function name symbol with the shortened entry in the symbol table
                                    minifiedCode2ndPass = minifiedCode2ndPass.Remove(z - currentToken.Length - minificationOffset, currentToken.Length);
                                    minifiedCode2ndPass = minifiedCode2ndPass.Insert(z - currentToken.Length - minificationOffset, functionNameSymbolTable[currentToken]);

                                    // add the length difference to minificationOffset
                                    minificationOffset += (currentToken.Length - functionNameSymbolTable[currentToken].Length);
                                }
                                else if (globalVariableSymbolTable.ContainsKey(currentToken) && currentChar != '(')
                                {
                                    // replace the variable name symbol with the shortened entry in the symbol table
                                    minifiedCode2ndPass = minifiedCode2ndPass.Remove(z - currentToken.Length - minificationOffset, currentToken.Length);
                                    minifiedCode2ndPass = minifiedCode2ndPass.Insert(z - currentToken.Length - minificationOffset, globalVariableSymbolTable[currentToken]);

                                    // add the length difference to minificationOffset
                                    minificationOffset += (currentToken.Length - globalVariableSymbolTable[currentToken].Length);
                                }
                            }
                            
                            // reset currentToken (since symbols can't contain operators)
                            currentToken = "";

                            // record precedingChar (so the previous operator can be examined during symbol substitution checks, to prevent replacement
                            // of symbols which would cause syntax errors due to functions or variables having the same name as other functions or variables
                            precedingOperator = currentChar;
                        }
                    }
                }

                // copy the code minified through the second pass into the minifiedCode variable to ensure it is output from the function.
                minifiedCode = minifiedCode2ndPass.ToString();

                #endregion
            }

            // return the resultant minified code.
            return minifiedCode;
        }

        /// <summary>
        /// This function removes the last character from minifiedCode, similar to backspacing once.
        /// </summary>
        /// <param name="minifiedCode"></param>
        private void backspaceMC(int backSpaceCount, ref string minifiedCode, bool isMinifiableCode)
        {
            if (minifiedCode.Length >= backSpaceCount)
            {
                // remove the last character from the output stream
                minifiedCode = minifiedCode.Substring(0, minifiedCode.Length - backSpaceCount);
            }
            else
            {
                throw new Exception("Backspace count exceeded minified code length.");
            }

            // since backspacing only occurs within non-code segments (mainly comment markers), it does not affect code segments
        }

        /// <summary>
        /// This function adds the string provided for the 'input' parameter to minifiedCode.
        /// </summary>
        /// <param name="input">The text to add to minifiedCode (the output stream)</param>
        /// <param name="minifiedCode">A reference to the 'minifiedCode' variable within the main minifier function, which
        /// accumulates minified source code.</param>
        /// <param name="isMinifiableCode">A boolean indicating whether or not the text being added to minifiedCode is minifiable code
        /// (code that can have minification applied to it) or not (usually strings and/or comments).</param>
        private void addMC(string input, ref string minifiedCode, bool isMinifiableCode)
        {
            // check if code is 
            if (!isMinifiableCode && isCodeSegmentBeingOutput)
            {
                // code segment has ended
                currentIntervalEndIndex = minifiedCode.Length - 1;
                endCodeSegmentInterval(currentIntervalEndIndex);

                // update flag
                isCodeSegmentBeingOutput = false;
            }
            else if (isMinifiableCode && !isCodeSegmentBeingOutput)
            {
                //System.Windows.Forms.MessageBox.Show(input.Length.ToString() + " LENGTH");

                // code segment has started
                currentIntervalStartIndex = Math.Max(minifiedCode.Length - 1, 0);

                // increment 2 to account for the string literal start/end quotes
                if (currentIntervalStartIndex > 0)
                {
                    currentIntervalStartIndex += 2;
                }

                startOrUpdateCodeSegmentInterval(currentIntervalStartIndex);

                // update flag
                isCodeSegmentBeingOutput = true;
            }

            //if (minifiedCode.Length > 1088 && currentIntervalStartIndex == -1)
            //{
            //    Console.WriteLine(firstPassOutputCodeSegments.Last().IntervalStart.ToString() + ", " +
            //        firstPassOutputCodeSegments.Last().IntervalEnd.ToString());
            //    System.Windows.Forms.MessageBox.Show(currentIntervalStartIndex.ToString());
            //}

            // last 2 characters need to be added

            // add the input data to minifiedCode
            minifiedCode += input;
        }

        /// <summary>
        /// This function returns a string containing a 'crunched' (reduced in size) symbol name to aid in code minification, given an index value (representing
        /// the order in which the variable was found. For example, index 0 would be the first variable located). Returned names will correspond with the following 
        /// pattern:
        /// [Entries 0-25]: A lowercase letter between 'a' and 'z'
        /// [Entries 26-51]: An uppercase letter between 'A' and 'Z'
        /// [Entries 52-77]: A lowercase letter between 'a' and 'z' followed by '0'
        /// [Entries 52 and above]: Will follow the same pattern as above, but with a number appended.
        /// </summary>
        /// <returns></returns>
        private string getCrunchedSymbolName(int variableIndex)
        {
            // stores the returned variable name
            string crunchedVarName = "";

            // uppercase letter range (A-Z) is char codes 65 to 90
            // lowercase letter range (a-z) is char codes 97 to 122
            // -offset is 97 - (25 * ((variableIndex / 25) % 2)) + (variableIndex % 25))

            crunchedVarName += (char)(
                (97                                     // -starting offset (the start of the uppercase letter range)
                - (32 * ((variableIndex / 26) % 2))     // -selects uppercase or lowercase characters based on the index range
                                                        // (configured so variableIndex values of 0-25 returns lowercase letters 
                                                        // & variableIndex values of 26-51 return uppercase letters)
                + (variableIndex % 26)));               // -selects the specific letter

            //if (crunchedVarName == "G")
            //{
            //    System.Windows.Forms.MessageBox.Show("Test");
            //}

            // -integer divide by 52

            // check if a number is needed at the end (to distinguish it from previous variable names)
            if (variableIndex > 51)
            {
                // append a number to the end

                /* Pattern should follow:
                    0-25: no number
                    26-51: no number
                    52-77: 0
                    78-103: 0
                    104-129: 1
                    130-155: 1
                 */

                // to accomplish, integer divide by 52 to get the number, and subtract 1 to offset the first number to 0 (to ensure
                // all single-digit numbers are used before incrementing)
                crunchedVarName += ((variableIndex / 52) - 1).ToString();
            }

            // return result
            return crunchedVarName;
        }

        /// <summary>
        /// This function adds a new Interval object (or updates it) to the firstPassOutputCodeSegments list, 
        /// populated with the provided charIndex representing the index of the character within the output 
        /// code that the interval starts at.
        /// </summary>
        /// <param name="charIndex">The index of the character within the code output by the first pass of the
        /// minifier that indicates where the interval begins.</param>
        private void startOrUpdateCodeSegmentInterval(int charIndex)
        {
            if (currentIntervalStartIndex == -1)
            {
                currentIntervalStartIndex = charIndex;
            }
            else
            {
                currentIntervalEndIndex = charIndex;
            }
        }

        /// <summary>
        /// This function finalizes the last Interval object in the firstPassOutputCodeSegments list, populated with the
        /// provided charIndex representing the index of the character within the output code that the interval ends at.
        /// </summary>
        /// <param name="charIndex">The index of the character within the code output by the first pass of the
        /// minifier that indicates where the interval ends.</param>
        private void endCodeSegmentInterval(int charIndex)
        {
            if (currentIntervalStartIndex != -1 && currentIntervalEndIndex != -1)
            {
                // push the current interval indices to the list of code segments
                firstPassOutputCodeSegments.Add(new Interval(currentIntervalStartIndex, currentIntervalEndIndex));

                // reset the interval start/end index variables (to ensure correct operation)
                currentIntervalStartIndex = -1;
                currentIntervalEndIndex = -1;
            }
        }

        #endregion
    }
}
