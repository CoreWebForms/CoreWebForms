// MIT License.

using System.Collections;
using System.Collections.Specialized;

namespace System.Web.UI.WebControls.WebParts
{
    public sealed class WebPartDescriptionCollection : ReadOnlyCollectionBase {

        private readonly HybridDictionary _ids;

        public WebPartDescriptionCollection() {
        }

        public WebPartDescriptionCollection(ICollection webPartDescriptions) {
            if (webPartDescriptions == null) {
                throw new ArgumentNullException(nameof(webPartDescriptions));
            }

            _ids = new HybridDictionary(webPartDescriptions.Count, true /* caseInsensitive */);
            foreach (object obj in webPartDescriptions) {
                if (obj == null) {
                    throw new ArgumentException(SR.GetString(SR.Collection_CantAddNull), nameof(webPartDescriptions));
                }
                WebPartDescription description = obj as WebPartDescription;
                if (description == null) {
                    throw new ArgumentException(SR.GetString(SR.Collection_InvalidType, "WebPartDescription"),
                                                nameof(webPartDescriptions));
                }
                string id = description.ID;
                if (!_ids.Contains(id)) {
                    InnerList.Add(description);
                    _ids.Add(id, description);
                }
                else {
                    throw new ArgumentException(SR.GetString(SR.WebPart_Collection_DuplicateID, "WebPartDescription", id), nameof(webPartDescriptions));
                }
            }
        }

        public bool Contains(WebPartDescription value) {
            return InnerList.Contains(value);
        }

        public int IndexOf(WebPartDescription value) {
            return InnerList.IndexOf(value);
        }

        public WebPartDescription this[int index] {
            get {
                return (WebPartDescription) InnerList[index];
            }
        }

        public WebPartDescription this[string id] {
            get {
                return ((_ids != null) ? (WebPartDescription)_ids[id] : null);
            }
        }

        public void CopyTo(WebPartDescription[] array, int index) {
            InnerList.CopyTo(array, index);
        }
    }
}
