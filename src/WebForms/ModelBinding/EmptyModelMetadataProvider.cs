//MIT License.

namespace System.Web.ModelBinding;

public class EmptyModelMetadataProvider : AssociatedMetadataProvider {
    protected override ModelMetadata CreateMetadata(IEnumerable<Attribute> attributes, Type containerType, Func<object> modelAccessor, Type modelType, string propertyName) {
        return new ModelMetadata(this, containerType, modelAccessor, modelType, propertyName);
    }
}
