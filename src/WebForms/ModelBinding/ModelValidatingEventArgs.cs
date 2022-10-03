// MIT License.

using System.ComponentModel;

namespace System.Web.ModelBinding;
public sealed class ModelValidatingEventArgs : CancelEventArgs
{

    public ModelValidatingEventArgs(ModelBindingExecutionContext modelBindingExecutionContext, ModelValidationNode parentNode)
    {
        if (modelBindingExecutionContext == null)
        {
            throw new ArgumentNullException(nameof(modelBindingExecutionContext));
        }

        ModelBindingExecutionContext = modelBindingExecutionContext;
        ParentNode = parentNode;
    }

    public ModelBindingExecutionContext ModelBindingExecutionContext
    {
        get;
        private set;
    }

    public ModelValidationNode ParentNode
    {
        get;
        private set;
    }

}
