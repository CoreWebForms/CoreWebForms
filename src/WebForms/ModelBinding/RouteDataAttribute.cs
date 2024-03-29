//MIT license

namespace System.Web.ModelBinding; 

[AttributeUsage(AttributeTargets.Parameter, Inherited = false, AllowMultiple = false)]
public sealed class RouteDataAttribute : ValueProviderSourceAttribute {

    public string Key {
        get;
        private set;
    }

    public RouteDataAttribute()
        : this(null) {
    }

    public RouteDataAttribute(string key) {
        Key = key;
    }

    public override IValueProvider GetValueProvider(ModelBindingExecutionContext modelBindingExecutionContext) {
        if (modelBindingExecutionContext == null) {
            throw new ArgumentNullException(nameof(modelBindingExecutionContext));
        }

        return new RouteDataValueProvider(modelBindingExecutionContext);
    }

    public override string GetModelName() {
        return Key;
    }

}
