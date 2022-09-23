// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace System.Web.Util
{
    public interface IWebObjectFactory {
        object CreateInstance();
    }

    internal interface ITypedWebObjectFactory : IWebObjectFactory {
        // Type that will be instantiated by CreateInstance.  This is to allow the caller
        // to check base type validity *before* actually creating the instance.
        Type InstantiatedType { get; }
    }
}

