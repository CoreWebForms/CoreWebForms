// MIT License.

namespace System.Web.UI.WebControls.WebParts
{
    public interface IWebPart {

        string CatalogIconImageUrl { get; set; }

        string Description { get; set; }

        string Subtitle { get; }

        string Title { get; set; }

        string TitleIconImageUrl { get; set; }

        string TitleUrl { get; set; }
    }
}
