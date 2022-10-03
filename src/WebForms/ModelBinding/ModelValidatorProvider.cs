// MIT License.

namespace System.Web.ModelBinding;
public abstract class ModelValidatorProvider
{
    public abstract IEnumerable<ModelValidator> GetValidators(ModelMetadata metadata, ModelBindingExecutionContext context);
}
