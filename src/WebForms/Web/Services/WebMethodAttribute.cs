//------------------------------------------------------------------------------
// <copyright file="WebMethodAttribute.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//------------------------------------------------------------------------------

using System.Transactions;

namespace System.Web.Services {

    using System;
    // using EnterpriseServices;

    /// <include file='doc\WebMethodAttribute.uex' path='docs/doc[@for="WebMethodAttribute"]/*' />
    /// <devdoc>
    ///    <para> The WebMethod attribute must be placed on a method in a Web Service class to mark it as available
    ///       to be called via the Web. The method and class must be marked public and must run inside of
    ///       an ASP.NET Web application.</para>
    /// </devdoc>
    [AttributeUsage(AttributeTargets.Method)]
    public sealed class WebMethodAttribute : Attribute {
        private int transactionOption; // this is an int to prevent system.enterpriseservices.dll from getting loaded
        private bool enableSession;
        private int cacheDuration;
        private bool bufferResponse;
        private string description;
        private string messageName;

        private bool transactionOptionSpecified;
        private bool enableSessionSpecified;
        private bool cacheDurationSpecified;
        private bool bufferResponseSpecified;
        private bool descriptionSpecified;
        private bool messageNameSpecified;

        /// <include file='doc\WebMethodAttribute.uex' path='docs/doc[@for="WebMethodAttribute.WebMethodAttribute"]/*' />
        /// <devdoc>
        /// <para>Initializes a new instance of the <see cref='System.Web.Services.WebMethodAttribute'/>
        /// class.</para>
        /// </devdoc>
        public WebMethodAttribute() {
            enableSession = false;
            transactionOption = 0; // TransactionOption.Disabled
            cacheDuration = 0;
            bufferResponse = true;
        }

        /// <include file='doc\WebMethodAttribute.uex' path='docs/doc[@for="WebMethodAttribute.WebMethodAttribute1"]/*' />
        /// <devdoc>
        /// <para>Initializes a new instance of the <see cref='System.Web.Services.WebMethodAttribute'/>
        /// class.</para>
        /// </devdoc>
        public WebMethodAttribute(bool enableSession)
            : this() {
            EnableSession = enableSession;
        }

        /// <include file='doc\WebMethodAttribute.uex' path='docs/doc[@for="WebMethodAttribute.WebMethodAttribute2"]/*' />
        /// <devdoc>
        /// <para>Initializes a new instance of the <see cref='System.Web.Services.WebMethodAttribute'/>
        /// class.</para>
        /// </devdoc>
        public WebMethodAttribute(bool enableSession, TransactionScopeOption transactionOption)
            : this() {
            EnableSession = enableSession;
            this.transactionOption = (int)transactionOption;
            transactionOptionSpecified = true;
        }

        /// <include file='doc\WebMethodAttribute.uex' path='docs/doc[@for="WebMethodAttribute.WebMethodAttribute3"]/*' />
        /// <devdoc>
        /// <para>Initializes a new instance of the <see cref='System.Web.Services.WebMethodAttribute'/>
        /// class.</para>
        /// </devdoc>
        public WebMethodAttribute(bool enableSession, TransactionScopeOption transactionOption, int cacheDuration) {
            EnableSession = enableSession;
            this.transactionOption = (int)transactionOption;
            transactionOptionSpecified = true;
            CacheDuration = cacheDuration;
            BufferResponse = true;
        }

        /// <include file='doc\WebMethodAttribute.uex' path='docs/doc[@for="WebMethodAttribute.WebMethodAttribute4"]/*' />
        /// <devdoc>
        /// <para>Initializes a new instance of the <see cref='System.Web.Services.WebMethodAttribute'/>
        /// class.</para>
        /// </devdoc>
        public WebMethodAttribute(bool enableSession, TransactionScopeOption transactionOption, int cacheDuration, bool bufferResponse) {
            EnableSession = enableSession;
            this.transactionOption = (int)transactionOption;
            transactionOptionSpecified = true;
            CacheDuration = cacheDuration;
            BufferResponse = bufferResponse;
        }

        /// <include file='doc\WebMethodAttribute.uex' path='docs/doc[@for="WebMethodAttribute.Description"]/*' />
        /// <devdoc>
        ///    <para> A message that describes the Web service method.
        ///       The message is used in description files generated for a Web Service, such as the Service Contract and the Service Description page.</para>
        /// </devdoc>
        public string Description {
            get {
                return (description == null) ? string.Empty : description;
            }

            set {
                description = value;
                descriptionSpecified = true;
            }
        }
        internal bool DescriptionSpecified { get { return descriptionSpecified; } }

        /// <include file='doc\WebMethodAttribute.uex' path='docs/doc[@for="WebMethodAttribute.EnableSession"]/*' />
        /// <devdoc>
        ///    <para>Indicates wheter session state is enabled for a Web service Method. The default is false.</para>
        /// </devdoc>
        public bool EnableSession {
            get {
                return enableSession;
            }

            set {
                enableSession = value;
                enableSessionSpecified = true;
            }
        }
        internal bool EnableSessionSpecified { get { return enableSessionSpecified; } }

        /// <include file='doc\WebMethodAttribute.uex' path='docs/doc[@for="WebMethodAttribute.CacheDuration"]/*' />
        /// <devdoc>
        ///    <para>Indicates the number of seconds the response should be cached. Defaults to 0 (no caching).
        ///          Should be used with caution when requests are likely to be very large.</para>
        /// </devdoc>
        public int CacheDuration {
            get {
                return cacheDuration;
            }

            set {
                cacheDuration = value;
                cacheDurationSpecified = true;
            }
        }
        internal bool CacheDurationSpecified { get { return cacheDurationSpecified; } }

        /// <include file='doc\WebMethodAttribute.uex' path='docs/doc[@for="WebMethodAttribute.BufferResponse"]/*' />
        /// <devdoc>
        ///    <para>Indicates whether the response for this request should be buffered. Defaults to false.</para>
        /// </devdoc>
        public bool BufferResponse {
            get {
                return bufferResponse;
            }

            set {
                bufferResponse = value;
                bufferResponseSpecified = true;
            }
        }
        internal bool BufferResponseSpecified { get { return bufferResponseSpecified; } }

        /// <include file='doc\WebMethodAttribute.uex' path='docs/doc[@for="WebMethodAttribute.TransactionOption"]/*' />
        /// <devdoc>
        ///    <para>
        ///       Indicates the transaction participation mode of a Web Service Method. </para>
        /// </devdoc>
        public TransactionScopeOption TransactionOption {
            get {
                return (TransactionScopeOption)transactionOption;
            }
            set {
                transactionOption = (int)value;
                transactionOptionSpecified = true;
            }
        }
        internal bool TransactionOptionSpecified { get { return transactionOptionSpecified; } }

        internal bool TransactionEnabled {
            get {
                return transactionOption != 0;
            }
        }

        /// <include file='doc\WebMethodAttribute.uex' path='docs/doc[@for="WebMethodAttribute.MessageName"]/*' />
        /// <devdoc>
        ///    <para>The name used for the request and response message containing the
        ///    data passed to and returned from this method.</para>
        /// </devdoc>
        public string MessageName {
            get {
                return messageName == null ? string.Empty : messageName;
            }

            set {
                messageName = value;
                messageNameSpecified = true;
            }
        }
        internal bool MessageNameSpecified { get { return messageNameSpecified; } }
    }
}
