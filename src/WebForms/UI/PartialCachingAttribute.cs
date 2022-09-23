// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Diagnostics.CodeAnalysis;

/*
 * Fragment caching attribute
 *
 * Copyright (c) 1999 Microsoft Corporation
 */

namespace System.Web.UI
{
    /*
     * This class defines the PartialCachingAttribute attribute that can be placed on
     * user controls classes to enable the fragmant caching feature.
     */

    /// <devdoc>
    ///    <para>[To be supplied.]</para>
    /// </devdoc>
    [AttributeUsage(AttributeTargets.Class)]
    [SuppressMessage("Microsoft.Design", "CA1019:DefineAccessorsForAttributeArguments", Justification = "Optional arguments have already shipped public overloads")]
    public sealed class PartialCachingAttribute : Attribute
    {
        private int _duration;

        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        public int Duration
        {
            get
            {
                return _duration;
            }
            set
            {
                _duration = value;
            }
        }

        private string _varyByParams;

        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        public string VaryByParams
        {
            get
            {
                return _varyByParams;
            }
            set
            {
                _varyByParams = value;
            }
        }

        private string _varyByControls;

        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        public string VaryByControls
        {
            get
            {
                return _varyByControls;
            }
            set
            {
                _varyByControls = value;
            }
        }

        private string _varyByCustom;

        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        public string VaryByCustom
        {
            get
            {
                return _varyByCustom;
            }
            set
            {
                _varyByCustom = value;
            }
        }

        private string _sqlDependency;

        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        public string SqlDependency
        {
            get
            {
                return _sqlDependency;
            }
            set
            {
                _sqlDependency = value;
            }
        }

        private bool _shared;

        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        public bool Shared
        {
            get
            {
                return _shared;
            }
            set
            {
                _shared = value;
            }
        }

        private string _providerName;

        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        public string ProviderName
        {
            get
            {
#if PORT_OUTPUTCACHE
                if (_providerName == null)
                {
                    return OutputCache.ASPNET_INTERNAL_PROVIDER_NAME;
                }
                else
#endif
                {
                    return _providerName;
                }
            }
            set
            {
#if PORT_OUTPUTCACHE
                if (value == OutputCache.ASPNET_INTERNAL_PROVIDER_NAME)
                {
                    value = null;
                }
#endif
                _providerName = value;
            }
        }

        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        public PartialCachingAttribute(int duration)
        {
            _duration = duration;
        }

        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        public PartialCachingAttribute(int duration, string varyByParams,
            string varyByControls, string varyByCustom)
            : this(duration, varyByParams, varyByControls, varyByCustom, null, false)
        {
        }

        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        public PartialCachingAttribute(int duration, string varyByParams,
            string varyByControls, string varyByCustom, bool shared)
            : this(duration, varyByParams, varyByControls, varyByCustom, null, shared)
        {
        }

        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        public PartialCachingAttribute(int duration, string varyByParams,
            string varyByControls, string varyByCustom, string sqlDependency, bool shared)
        {
            _duration = duration;
            _varyByParams = varyByParams;
            _varyByControls = varyByControls;
            _varyByCustom = varyByCustom;
            _shared = shared;
            _sqlDependency = sqlDependency;
        }
    }

}
