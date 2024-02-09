// MIT License.

namespace System.Web.UI;

using System;
using System.Web.Util;

[AttributeUsage(AttributeTargets.Assembly, AllowMultiple = true)]
public sealed class TagPrefixAttribute : Attribute
{
    public TagPrefixAttribute(string namespaceName, string tagPrefix)
    {
        if (string.IsNullOrEmpty(namespaceName))
        {
            throw ExceptionUtil.ParameterNullOrEmpty("namespaceName");
        }
        if (string.IsNullOrEmpty(tagPrefix))
        {
            throw ExceptionUtil.ParameterNullOrEmpty("tagPrefix");
        }

        NamespaceName = namespaceName;
        TagPrefix = tagPrefix;
    }

    public string NamespaceName { get; }

    public string TagPrefix { get; }
}
