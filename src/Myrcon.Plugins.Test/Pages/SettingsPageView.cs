﻿#region Copyright
// Copyright 2014 Myrcon Pty. Ltd.
// 
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
// 
//     http://www.apache.org/licenses/LICENSE-2.0
// 
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
#endregion

// ------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//     Runtime Version: 10.0.0.0
//  
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
// ------------------------------------------------------------------------------

namespace Myrcon.Plugins.Test.Pages
{
#line 1 "C:\Users\P\Documents\Projects\clients\myrcon\procon\Procon-2\src\TestPlugin\Pages\SettingsPageView.tt"
    [System.CodeDom.Compiler.GeneratedCodeAttribute("Microsoft.VisualStudio.TextTemplating", "10.0.0.0")]
    public partial class SettingsPageView : SettingsPageViewBase
    {
        public virtual string TransformText()
        {
            this.Write("\r\n\r\n<html>\r\n\t<head>\r\n\t\t<title>This is my title</title>\r\n\t</head>\r\n\t<body>\r\n\t\t<div" +
                    " id=\"content\">\r\n\t\t\t<h1>Settings</h1>\r\n\t\t\tThis is the settings page for this plug" +
                    "in. Check out the <a href=\"/\">Index</a>.\r\n\r\n\t\t\t<button id=\"javascript-navigation" +
                    "-test\">Javascript Navigation to Index</button>\r\n\r\n\t\t\t<button id=\"javascript-data" +
                    "-test\">Javascript Command with JSON response</button>\r\n\t\t</div>\r\n\r\n\t\t<script typ" +
                    "e=\"application/javascript\">\r\n\r\n\t\t\tdefine(\'settings_view\', [\r\n\t\t\t\t\'jquery\',\r\n\t\t\t\t" +
                    "\'lodash\',\r\n\t\t\t\t\'backbone\',\r\n\t\t\t\t\'proxy\'\r\n\t\t\t], function($, _, Backbone, proxy) {" +
                    "\r\n\t\t\t\treturn Backbone.View.extend({\r\n\t\t\t\t\tel: null,\r\n\t\t\t\t\tmodel: proxy.model,\r\n\t" +
                    "\t\t\t\tevents: {\r\n\t\t\t\t\t\t\'click #javascript-navigation-test\': \'_on_javascript_naviga" +
                    "tion_test_click\',\r\n\t\t\t\t\t\t\'click #javascript-data-test\': \'_on_javascript_data_tes" +
                    "t_click\'\r\n\t\t\t\t\t},\r\n\t\t\t\t\tinitialize: function() {\r\n\t\t\t\t\t\tconsole.log(\"Settings ->" +
                    " view -> initialize\");\r\n\t\t\t\t\t\t\r\n\t\t\t\t\t\tthis.listenTo(this.model, \'response\', this" +
                    "._on_response);\r\n\t\t\t\t\t\tthis.listenTo(this.model, \'destructing\', this._on_destruc" +
                    "ting);\r\n\t\t\t\t\t},\r\n\t\t\t\t\t_on_javascript_navigation_test_click: function() {\r\n\t\t\t\t\t\t" +
                    "console.log(\"Settings -> view -> _on_javascript_navigation_test_click\");\r\n\r\n\t\t\t\t" +
                    "\t\tthis.model.request({\r\n\t\t\t\t\t\t\tcommand: \'/\'\r\n\t\t\t\t\t\t});\r\n\t\t\t\t\t},\r\n\t\t\t\t\t_on_javasc" +
                    "ript_data_test_click: function() {\r\n\t\t\t\t\t\tconsole.log(\"Settings -> view -> _on_j" +
                    "avascript_data_test_click\");\r\n\r\n\t\t\t\t\t\tthis.model.request({\r\n\t\t\t\t\t\t\tcommand: \'Tes" +
                    "tPluginsCommandsZeroParameters\'\r\n\t\t\t\t\t\t});\r\n\t\t\t\t\t},\r\n\t\t\t\t\t_on_response: function" +
                    "(request, response) {\r\n\t\t\t\t\t\tconsole.log(\"Settings -> view -> _on_response\");\r\n\t" +
                    "\t\t\t\t\t\r\n\t\t\t\t\t\tconsole.log(request);\r\n\t\t\t\t\t\tconsole.log(response);\r\n\t\t\t\t\t},\r\n\t\t\t\t\t" +
                    "_on_destructing: function() {\r\n\t\t\t\t\t\tconsole.log(\"Settings -> destructing\");\r\n\r\n" +
                    "\t\t\t\t\t\tthis.remove();\r\n\t\t\t\t\t}\r\n\t\t\t\t});\r\n\t\t\t});\r\n\r\n\t\t\trequire([\r\n\t\t\t\t\'jquery\',\r\n\t\t" +
                    "\t\t\'lodash\',\r\n\t\t\t\t\'backbone\',\r\n\t\t\t\t\'proxy\',\r\n\t\t\t\t\'settings_view\'\r\n\t\t\t], function(" +
                    "$, _, Backbone, proxy, Settings_view) {\r\n\t\t\t\tconsole.log(\"Settings -> Loaded\");\r" +
                    "\n\r\n\t\t\t\tvar view = new Settings_view({\r\n\t\t\t\t\t// Rebind as the original element ma" +
                    "y not exist when this view is instantiated twice.\r\n\t\t\t\t\tel: $(\'#content\')\r\n\t\t\t\t}" +
                    ");\r\n\t\t\t});\r\n\t\t</script>\r\n\t</body>\r\n</html>");
            return this.GenerationEnvironment.ToString();
        }
    }
    
    #line default
    #line hidden
    #region Base class
    /// <summary>
    /// Base class for this transformation
    /// </summary>
    [System.CodeDom.Compiler.GeneratedCodeAttribute("Microsoft.VisualStudio.TextTemplating", "10.0.0.0")]
    public class SettingsPageViewBase
    {
        #region Fields
        private global::System.Text.StringBuilder generationEnvironmentField;
        private global::System.CodeDom.Compiler.CompilerErrorCollection errorsField;
        private global::System.Collections.Generic.List<int> indentLengthsField;
        private string currentIndentField = "";
        private bool endsWithNewline;
        private global::System.Collections.Generic.IDictionary<string, object> sessionField;
        #endregion
        #region Properties
        /// <summary>
        /// The string builder that generation-time code is using to assemble generated output
        /// </summary>
        protected System.Text.StringBuilder GenerationEnvironment
        {
            get
            {
                if ((this.generationEnvironmentField == null))
                {
                    this.generationEnvironmentField = new global::System.Text.StringBuilder();
                }
                return this.generationEnvironmentField;
            }
            set
            {
                this.generationEnvironmentField = value;
            }
        }
        /// <summary>
        /// The error collection for the generation process
        /// </summary>
        public System.CodeDom.Compiler.CompilerErrorCollection Errors
        {
            get
            {
                if ((this.errorsField == null))
                {
                    this.errorsField = new global::System.CodeDom.Compiler.CompilerErrorCollection();
                }
                return this.errorsField;
            }
        }
        /// <summary>
        /// A list of the lengths of each indent that was added with PushIndent
        /// </summary>
        private System.Collections.Generic.List<int> indentLengths
        {
            get
            {
                if ((this.indentLengthsField == null))
                {
                    this.indentLengthsField = new global::System.Collections.Generic.List<int>();
                }
                return this.indentLengthsField;
            }
        }
        /// <summary>
        /// Gets the current indent we use when adding lines to the output
        /// </summary>
        public string CurrentIndent
        {
            get
            {
                return this.currentIndentField;
            }
        }
        /// <summary>
        /// Current transformation session
        /// </summary>
        public virtual global::System.Collections.Generic.IDictionary<string, object> Session
        {
            get
            {
                return this.sessionField;
            }
            set
            {
                this.sessionField = value;
            }
        }
        #endregion
        #region Transform-time helpers
        /// <summary>
        /// Write text directly into the generated output
        /// </summary>
        public void Write(string textToAppend)
        {
            if (string.IsNullOrEmpty(textToAppend))
            {
                return;
            }
            // If we're starting off, or if the previous text ended with a newline,
            // we have to append the current indent first.
            if (((this.GenerationEnvironment.Length == 0) 
                        || this.endsWithNewline))
            {
                this.GenerationEnvironment.Append(this.currentIndentField);
                this.endsWithNewline = false;
            }
            // Check if the current text ends with a newline
            if (textToAppend.EndsWith(global::System.Environment.NewLine, global::System.StringComparison.CurrentCulture))
            {
                this.endsWithNewline = true;
            }
            // This is an optimization. If the current indent is "", then we don't have to do any
            // of the more complex stuff further down.
            if ((this.currentIndentField.Length == 0))
            {
                this.GenerationEnvironment.Append(textToAppend);
                return;
            }
            // Everywhere there is a newline in the text, add an indent after it
            textToAppend = textToAppend.Replace(global::System.Environment.NewLine, (global::System.Environment.NewLine + this.currentIndentField));
            // If the text ends with a newline, then we should strip off the indent added at the very end
            // because the appropriate indent will be added when the next time Write() is called
            if (this.endsWithNewline)
            {
                this.GenerationEnvironment.Append(textToAppend, 0, (textToAppend.Length - this.currentIndentField.Length));
            }
            else
            {
                this.GenerationEnvironment.Append(textToAppend);
            }
        }
        /// <summary>
        /// Write text directly into the generated output
        /// </summary>
        public void WriteLine(string textToAppend)
        {
            this.Write(textToAppend);
            this.GenerationEnvironment.AppendLine();
            this.endsWithNewline = true;
        }
        /// <summary>
        /// Write formatted text directly into the generated output
        /// </summary>
        public void Write(string format, params object[] args)
        {
            this.Write(string.Format(global::System.Globalization.CultureInfo.CurrentCulture, format, args));
        }
        /// <summary>
        /// Write formatted text directly into the generated output
        /// </summary>
        public void WriteLine(string format, params object[] args)
        {
            this.WriteLine(string.Format(global::System.Globalization.CultureInfo.CurrentCulture, format, args));
        }
        /// <summary>
        /// Raise an error
        /// </summary>
        public void Error(string message)
        {
            System.CodeDom.Compiler.CompilerError error = new global::System.CodeDom.Compiler.CompilerError();
            error.ErrorText = message;
            this.Errors.Add(error);
        }
        /// <summary>
        /// Raise a warning
        /// </summary>
        public void Warning(string message)
        {
            System.CodeDom.Compiler.CompilerError error = new global::System.CodeDom.Compiler.CompilerError();
            error.ErrorText = message;
            error.IsWarning = true;
            this.Errors.Add(error);
        }
        /// <summary>
        /// Increase the indent
        /// </summary>
        public void PushIndent(string indent)
        {
            if ((indent == null))
            {
                throw new global::System.ArgumentNullException("indent");
            }
            this.currentIndentField = (this.currentIndentField + indent);
            this.indentLengths.Add(indent.Length);
        }
        /// <summary>
        /// Remove the last indent that was added with PushIndent
        /// </summary>
        public string PopIndent()
        {
            string returnValue = "";
            if ((this.indentLengths.Count > 0))
            {
                int indentLength = this.indentLengths[(this.indentLengths.Count - 1)];
                this.indentLengths.RemoveAt((this.indentLengths.Count - 1));
                if ((indentLength > 0))
                {
                    returnValue = this.currentIndentField.Substring((this.currentIndentField.Length - indentLength));
                    this.currentIndentField = this.currentIndentField.Remove((this.currentIndentField.Length - indentLength));
                }
            }
            return returnValue;
        }
        /// <summary>
        /// Remove any indentation
        /// </summary>
        public void ClearIndent()
        {
            this.indentLengths.Clear();
            this.currentIndentField = "";
        }
        #endregion
        #region ToString Helpers
        /// <summary>
        /// Utility class to produce culture-oriented representation of an object as a string.
        /// </summary>
        public class ToStringInstanceHelper
        {
            private System.IFormatProvider formatProviderField  = global::System.Globalization.CultureInfo.InvariantCulture;
            /// <summary>
            /// Gets or sets format provider to be used by ToStringWithCulture method.
            /// </summary>
            public System.IFormatProvider FormatProvider
            {
                get
                {
                    return this.formatProviderField ;
                }
                set
                {
                    if ((value != null))
                    {
                        this.formatProviderField  = value;
                    }
                }
            }
            /// <summary>
            /// This is called from the compile/run appdomain to convert objects within an expression block to a string
            /// </summary>
            public string ToStringWithCulture(object objectToConvert)
            {
                if ((objectToConvert == null))
                {
                    throw new global::System.ArgumentNullException("objectToConvert");
                }
                System.Type t = objectToConvert.GetType();
                System.Reflection.MethodInfo method = t.GetMethod("ToString", new System.Type[] {
                            typeof(System.IFormatProvider)});
                if ((method == null))
                {
                    return objectToConvert.ToString();
                }
                else
                {
                    return ((string)(method.Invoke(objectToConvert, new object[] {
                                this.formatProviderField })));
                }
            }
        }
        private ToStringInstanceHelper toStringHelperField = new ToStringInstanceHelper();
        public ToStringInstanceHelper ToStringHelper
        {
            get
            {
                return this.toStringHelperField;
            }
        }
        #endregion
    }
    #endregion
}
