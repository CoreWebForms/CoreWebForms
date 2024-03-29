// MIT License.

using System.Collections;
using System.Collections.Specialized;

namespace System.Web.UI.WebControls.WebParts
{
    public sealed class ProviderConnectionPointCollection : ReadOnlyCollectionBase {

        private readonly HybridDictionary _ids;

        public ProviderConnectionPointCollection() {
        }

        public ProviderConnectionPointCollection(ICollection connectionPoints) {
            if (connectionPoints == null) {
                throw new ArgumentNullException(nameof(connectionPoints));
            }

            _ids = new HybridDictionary(connectionPoints.Count, true /* caseInsensitive */);
            foreach (object obj in connectionPoints) {
                if (obj == null) {
                    throw new ArgumentException(SR.GetString(SR.Collection_CantAddNull), nameof(connectionPoints));
                }
                ProviderConnectionPoint point = obj as ProviderConnectionPoint;
                if (point == null) {
                    throw new ArgumentException(SR.GetString(SR.Collection_InvalidType, "ProviderConnectionPoint"),
                                                nameof(connectionPoints));
                }
                string id = point.ID;
                if (!_ids.Contains(id)) {
                    InnerList.Add(point);
                    _ids.Add(id, point);
                }
                else {
                    throw new ArgumentException(SR.GetString(SR.WebPart_Collection_DuplicateID, "ProviderConnectionPoint", id), nameof(connectionPoints));
                }
            }
        }

        public ProviderConnectionPoint Default {
            get {
                return this[ConnectionPoint.DefaultID];
            }
        }

        public ProviderConnectionPoint this[int index] {
            get {
                return (ProviderConnectionPoint)InnerList[index];
            }
        }

        public ProviderConnectionPoint this[string id] {
            get {
                return ((_ids != null) ? (ProviderConnectionPoint)_ids[id] : null);
            }
        }

        public bool Contains(ProviderConnectionPoint connectionPoint) {
            return InnerList.Contains(connectionPoint);
        }

        public int IndexOf(ProviderConnectionPoint connectionPoint) {
            return InnerList.IndexOf(connectionPoint);
        }

        public void CopyTo(ProviderConnectionPoint[] array, int index) {
            InnerList.CopyTo(array, index);
        }
    }
}

