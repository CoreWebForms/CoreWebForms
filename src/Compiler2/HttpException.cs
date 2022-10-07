//------------------------------------------------------------------------------
// <copyright file="HttpException.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>                                                                
//------------------------------------------------------------------------------

/*
 * Exception thrown by ASP.NET managed runtime
 * 
 * Copyright (c) 1998 Microsoft Corporation
 */

namespace System.Web
{
    using System.CodeDom.Compiler;
    using System.Collections;
    using System.Globalization;
    using System.Runtime.Serialization;
    using System.Security.Permissions;

    /// <devdoc>
    ///    <para> Exception thrown when a compilation error occurs.</para>
    /// </devdoc>
    [Serializable]
    public sealed class HttpCompileException : HttpException
    {

        private CompilerResults _results;
        private string _sourceCode;


        public HttpCompileException()
        {
        }


        public HttpCompileException(string message) : base(message)
        {
        }


        public HttpCompileException(String message, Exception innerException) : base(message, innerException)
        {
        }


        public HttpCompileException(CompilerResults results, string sourceCode)
        {
            _results = results;
            _sourceCode = sourceCode;

#if PORT_EXCEPTION_FORMATTER
            SetFormatter(new DynamicCompileErrorFormatter(this));
#endif
        }

        // Determines whether the compile exception should be cached
        private bool _dontCache;
        internal bool DontCache
        {
            get { return _dontCache; }
            set { _dontCache = value; }
        }

        // The virtualpath depdencies for current buildresult.
        private ICollection _virtualPathDependencies;
        internal ICollection VirtualPathDependencies
        {
            get { return _virtualPathDependencies; }
            set { _virtualPathDependencies = value; }
        }


        /// <devdoc>
        ///    <para>Serialize the object.</para>
        /// </devdoc>
        [SecurityPermissionAttribute(SecurityAction.Demand, SerializationFormatter = true)]
        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            base.GetObjectData(info, context);
            info.AddValue("_results", _results);
            info.AddValue("_sourceCode", _sourceCode);
        }

        private const string compileErrorFormat = "{0}({1}): error {2}: {3}";


        /// <devdoc>
        ///    <para> The first compilation error.</para>
        /// </devdoc>
        public override string Message
        {
            get
            {
                // Return the first compile error as the exception message
                CompilerError e = FirstCompileError;

                if (e == null)
                    return base.Message;

                string message = String.Format(CultureInfo.CurrentCulture, compileErrorFormat,
                    e.FileName, e.Line, e.ErrorNumber, e.ErrorText);

                return message;
            }
        }


        /// <devdoc>
        ///    <para> The CompilerResults object describing the compile error.</para>
        /// </devdoc>
        public CompilerResults Results
        {
            [AspNetHostingPermission(SecurityAction.Demand, Level = AspNetHostingPermissionLevel.High)]
            get
            {
                return _results;
            }
        }

        internal CompilerResults ResultsWithoutDemand
        {
            get
            {
                return _results;
            }
        }

        /// <devdoc>
        ///    <para> The source code that was compiled.</para>
        /// </devdoc>
        public string SourceCode
        {
            get
            {
                return _sourceCode;
            }
        }

        internal string SourceCodeWithoutDemand
        {
            get
            {
                return _sourceCode;
            }
        }

        // Return the first compile error, or null if there isn't one
        internal CompilerError FirstCompileError
        {
            get
            {
                if (_results == null || !_results.Errors.HasErrors)
                    return null;

                CompilerError e = null;

                foreach (CompilerError error in _results.Errors)
                {

                    // Ignore warnings
                    if (error.IsWarning) continue;

#if PORT_EXCEPTION
                    // If we found an error that's not in the generated code, use it
                    if (HttpRuntime.CodegenDirInternal != null && error.FileName != null &&
                        !StringUtil.StringStartsWith(error.FileName, HttpRuntime.CodegenDirInternal))
#endif
                    {
                        e = error;
                        break;
                    }

                    // The current error is in the generated code.  Keep track of
                    // it if it's the first one, but keep on looking in case we find another
                    // one that's not in the generated code (ASURT 62600)
                    if (e == null)
                        e = error;
                }

                return e;
            }
        }
    }


    /// <devdoc>
    ///    <para> Exception thrown when a parse error occurs.</para>
    /// </devdoc>
    [Serializable]
    public sealed class HttpParseException : HttpException
    {

        private VirtualPath _virtualPath;
        private int _line;
        private ParserErrorCollection _parserErrors;


        public HttpParseException()
        {
        }


        public HttpParseException(string message) : base(message)
        {
        }


        public HttpParseException(String message, Exception innerException) : base(message, innerException)
        {
        }


        public HttpParseException(string message, Exception innerException, string virtualPath,
            string sourceCode, int line) : this(message, innerException,
                System.Web.VirtualPath.CreateAllowNull(virtualPath), sourceCode, line)
        { }

        internal HttpParseException(string message, Exception innerException, VirtualPath virtualPath,
            string sourceCode, int line)
            : base(message, innerException)
        {

            _virtualPath = virtualPath;
            _line = line;

            string formatterMessage;
            if (innerException != null)
                formatterMessage = innerException.Message;
            else
                formatterMessage = message;

#if PORT_FORMATTER
            SetFormatter(new ParseErrorFormatter(this, System.Web.VirtualPath.GetVirtualPathString(virtualPath), sourceCode,
                line, formatterMessage));
#endif
        }

        /// <devdoc>
        ///    <para>Serialize the object.</para>
        /// </devdoc>
        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            base.GetObjectData(info, context);
            info.AddValue("_virtualPath", _virtualPath);
            info.AddValue("_line", _line);
            info.AddValue("_parserErrors", _parserErrors);
        }

#if PORT_MAPPATH
        /// <devdoc>
        ///    <para> The physical path to source file that has the error.</para>
        /// </devdoc>
        public string FileName
        {
            get
            {
                string physicalPath = _virtualPath.MapPathInternal();

                if (physicalPath == null)
                    return null;

                // Demand path discovery before returning the path (ASURT 123798)
                InternalSecurityPermissions.PathDiscovery(physicalPath).Demand();
                return physicalPath;
            }
        }
#endif

        /// <devdoc>
        ///    <para> The virtual path to source file that has the error.</para>
        /// </devdoc>
        public string VirtualPath
        {
            get
            {
                return System.Web.VirtualPath.GetVirtualPathString(_virtualPath);
            }
        }

        internal VirtualPath VirtualPathObject
        {
            get
            {
                return _virtualPath;
            }
        }

        /// <devdoc>
        ///    <para> The CompilerResults object describing the compile error.</para>
        /// </devdoc>
        public int Line
        {
            get { return _line; }
        }

        // The set of parser errors
        public ParserErrorCollection ParserErrors
        {
            get
            {
                if (_parserErrors == null)
                {
                    _parserErrors = new ParserErrorCollection();
                    ParserError thisError = new ParserError(Message, _virtualPath, _line);
                    _parserErrors.Add(thisError);
                }

                return _parserErrors;
            }
        }
    }


    /// <devdoc>
    ///    <para> Exception thrown when a potentially unsafe input string is detected (ASURT 122278)</para>
    /// </devdoc>
    [Serializable]
    public sealed class HttpRequestValidationException : HttpException
    {


        public HttpRequestValidationException()
        {
        }


        public HttpRequestValidationException(string message) : base(message)
        {
#if PORT_FORMATTER
            SetFormatter(new UnhandledErrorFormatter(
                this, SR.GetString(SR.Dangerous_input_detected_descr), null));
#endif
        }


        public HttpRequestValidationException(String message, Exception innerException) : base(message, innerException)
        {
        }
    }

    [Serializable]
    public sealed class ParserError
    {
        private int _line;
        private VirtualPath _virtualPath;
        private string _errorText;
        private Exception _exception;

        public ParserError()
        {
        }

        public ParserError(string errorText, string virtualPath, int line)
            : this(errorText, System.Web.VirtualPath.CreateAllowNull(virtualPath), line)
        {
        }

        internal ParserError(string errorText, VirtualPath virtualPath, int line)
        {
            _virtualPath = virtualPath;
            _line = line;
            _errorText = errorText;
        }

        // The original exception that introduces the Parser Error
        internal Exception Exception
        {
            get { return _exception; }
            set { _exception = value; }
        }

        // The virtualPath where the parser error occurs.
        public string VirtualPath
        {
            get { return System.Web.VirtualPath.GetVirtualPathString(_virtualPath); }
            set { _virtualPath = System.Web.VirtualPath.Create(value); }
        }

        // The description error text of the error.
        public string ErrorText
        {
            get { return _errorText; }
            set { _errorText = value; }
        }

        // The line where the parser error occurs.
        public int Line
        {
            get { return _line; }
            set { _line = value; }
        }
    }

    [Serializable]
    public sealed class ParserErrorCollection : CollectionBase
    {
        public ParserErrorCollection()
        {
        }

        public ParserErrorCollection(ParserError[] value)
        {
            this.AddRange(value);
        }

        public ParserError this[int index]
        {
            get { return ((ParserError)List[index]); }
            set { List[index] = value; }
        }

        public int Add(ParserError value)
        {
            return List.Add(value);
        }

        public void AddRange(ParserError[] value)
        {
            if (value == null)
            {
                throw new ArgumentNullException("value");
            }

            for (int i = 0; i < value.Length; i++)
            {
                this.Add(value[i]);
            }
        }

        public void AddRange(ParserErrorCollection value)
        {
            if (value == null)
            {
                throw new ArgumentNullException("value");
            }

            foreach (ParserError parserError in value)
            {
                this.Add(parserError);
            }
        }

        public bool Contains(ParserError value)
        {
            return List.Contains(value);
        }

        public void CopyTo(ParserError[] array, int index)
        {
            List.CopyTo(array, index);
        }

        public int IndexOf(ParserError value)
        {
            return List.IndexOf(value);
        }

        public void Insert(int index, ParserError value)
        {
            List.Insert(index, value);
        }

        public void Remove(ParserError value)
        {
            List.Remove(value);
        }
    }
}
