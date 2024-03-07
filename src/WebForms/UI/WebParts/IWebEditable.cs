// MIT License.

namespace System.Web.UI.WebControls.WebParts {

    public interface IWebEditable {

        object WebBrowsableObject { get; }

        EditorPartCollection CreateEditorParts();
    }
}
