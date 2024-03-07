// MIT License.

namespace System.Web.UI.WebControls.WebParts
{
    public class WebPartDescription {

        private readonly string _id;
        private readonly string _title;
        private readonly string _description;
        private readonly string _imageUrl;
        private readonly WebPart _part;

        private WebPartDescription() {
        }

        public WebPartDescription(string id, string title, string description, string imageUrl) {
            if (String.IsNullOrEmpty(id)) {
                throw new ArgumentNullException(nameof(id));
            }
            if (String.IsNullOrEmpty(title)) {
                throw new ArgumentNullException(nameof(title));
            }
            _id = id;
            _title = title;
            _description = (description != null) ? description : String.Empty;
            _imageUrl = (imageUrl != null) ? imageUrl : String.Empty;
        }

        public WebPartDescription(WebPart part) {
            string id = part.ID;
            if (String.IsNullOrEmpty(id)) {
                throw new ArgumentException(SR.GetString(SR.WebPartManager_NoWebPartID), nameof(part));
            }

            _id = id;

            string displayTitle = part.DisplayTitle;
            _title = (displayTitle != null) ? displayTitle : String.Empty;

            string description = part.Description;
            _description = (description != null) ? description : String.Empty;

            string imageUrl = part.CatalogIconImageUrl;
            _imageUrl = (imageUrl != null) ? imageUrl : String.Empty;

            _part = part;
        }

        public string CatalogIconImageUrl {
            get {
                return _imageUrl;
            }
        }

        public string Description {
            get {
                return _description;
            }
        }

        public string ID {
            get {
                return _id;
            }
        }

        public string Title {
            get {
                return _title;
            }
        }

        internal WebPart WebPart {
            get {
                return _part;
            }
        }
    }
}
