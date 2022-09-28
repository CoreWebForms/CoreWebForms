// MIT License.

using System.Collections.Specialized;

#nullable disable

namespace System.Web.UI;
public class HiddenFieldPageStatePersister : PageStatePersister
{
    public HiddenFieldPageStatePersister(Page page) : base(page)
    {
    }

    public override void Load()
    {
        NameValueCollection requestValueCollection = Page.RequestValueCollection;
        if (requestValueCollection == null)
        {
            return;
        }

        string viewStateString = null;
        try
        {
            viewStateString = Page.RequestViewStateString;

            // VSWhidbey 160556
            if (!String.IsNullOrEmpty(viewStateString) || !String.IsNullOrEmpty(Page.ViewStateUserKey))
            {
                var combinedState = (Pair)StateFormatter2.Deserialize(viewStateString, Purpose.WebForms_HiddenFieldPageStatePersister_ClientState);
                ViewState = combinedState.First;
                ControlState = combinedState.Second;
            }
        }
        catch (Exception e)
        {
            // throw if this is a wrapped ViewStateException -- mac validation failed
            if (e.InnerException is ViewStateException)
            {
                throw;
            }

            ViewStateException.ThrowViewStateError(e, viewStateString);
        }
    }

    /// <devdoc>
    ///     To be supplied.
    /// </devdoc>
    public override void Save()
    {
        if (ViewState != null || ControlState != null)
        {
            Page.ClientState = StateFormatter2.Serialize(new Pair(ViewState, ControlState), Purpose.WebForms_HiddenFieldPageStatePersister_ClientState);
        }
    }
}
