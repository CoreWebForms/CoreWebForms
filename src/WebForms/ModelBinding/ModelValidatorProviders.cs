// MIT License.

namespace System.Web.ModelBinding;

public static class ModelValidatorProviders
{

#if PORT_MODELBINDING
    private static readonly ModelValidatorProviderCollection _providers = new ModelValidatorProviderCollection() {
        new DataAnnotationsModelValidatorProvider(),
#if UNDEF
        new DataErrorInfoModelValidatorProvider(),
        new ClientDataTypeModelValidatorProvider()
#endif
    };

    public static ModelValidatorProviderCollection Providers {
        get {
            return _providers;
        }
    }
#else
    public static ModelValidatorProviderCollection Providers => throw new NotImplementedException();
#endif

}
