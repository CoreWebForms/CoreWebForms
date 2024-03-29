// MIT License.

namespace System.Web.UI;

internal class Purpose
{
    internal static readonly Purpose WebForms_ClientScriptManager_EventValidation = new();

    private Purpose()
    {
    }

    internal static readonly Purpose WebForms_Page_PreviousPageID = new();
    internal static readonly Purpose User_ObjectStateFormatter_Serialize = new();
    internal static readonly Purpose WebForms_HiddenFieldPageStatePersister_ClientState = new();
    internal static readonly Purpose WebForms_ScriptManager_HistoryState = new();
    public static readonly Purpose WebForms_DetailsView_KeyTable = new();
    public static readonly Purpose WebForms_GridView_DataKeys = new();
    public static readonly Purpose WebForms_GridView_SortExpression = new();
}

internal interface IStateFormatter2 : IStateFormatter
{
    object Deserialize(string serializedState, Purpose purpose);

    string Serialize(object state, Purpose purpose);
}

public interface IStateFormatter
{
    object Deserialize(string serializedState);

    string Serialize(object state);
}
