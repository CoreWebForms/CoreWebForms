// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace System.Web.ModelBinding;

public static class ModelBinderProviders
{

    private static readonly ModelBinderProviderCollection _providers = CreateDefaultCollection();

    public static ModelBinderProviderCollection Providers
    {
        get
        {
            return _providers;
        }
    }

    private static ModelBinderProviderCollection CreateDefaultCollection()
    {
        return new ModelBinderProviderCollection()
        {
#if PORT_BINDINGPROVIDERS
            new TypeMatchModelBinderProvider(),
            new BinaryDataModelBinderProvider(),
            new KeyValuePairModelBinderProvider(),
            new ComplexModelBinderProvider(),
            new ArrayModelBinderProvider(),
            new DictionaryModelBinderProvider(),
            new CollectionModelBinderProvider(),
            new TypeConverterModelBinderProvider(),
            new MutableObjectModelBinderProvider()
#endif
        };
    }

}
