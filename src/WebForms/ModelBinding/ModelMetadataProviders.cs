// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace System.Web.ModelBinding;

public static class ModelMetadataProviders
{
#if PORT_MODELBINDING
    private static ModelMetadataProvider _current = new DataAnnotationsModelMetadataProvider();

    public static ModelMetadataProvider Current
    {
        get
        {
            return _current;
        }
        set
        {
            _current = value ?? new EmptyModelMetadataProvider();
        }
    }
#else
    public static ModelMetadataProvider Current => throw new NotImplementedException();
#endif
}
