//MIT license

namespace System.Web.UI.WebControls; 

public interface IFieldControl {
    IAutoFieldGenerator FieldsGenerator {
        get;
        set;
    }
}
