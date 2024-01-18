//MIT License.

using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
namespace System.Web.ModelBinding;

internal static class TypeDescriptorHelper {

    public static ICustomTypeDescriptor Get(Type type) {
        return new AssociatedMetadataTypeTypeDescriptionProvider(type).GetTypeDescriptor(type);
    }

}
