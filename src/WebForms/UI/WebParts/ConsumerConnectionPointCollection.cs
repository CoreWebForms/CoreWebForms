// MIT License.

using System.Collections;
using System.Collections.Specialized;

namespace System.Web.UI.WebControls.WebParts
{
    public sealed class ConsumerConnectionPointCollection : ReadOnlyCollectionBase {

        private readonly HybridDictionary _ids;

        public ConsumerConnectionPointCollection() {
        }

        public ConsumerConnectionPointCollection(ICollection connectionPoints) {
            if (connectionPoints == null) {
                throw new ArgumentNullException(nameof(connectionPoints));
            }

            _ids = new HybridDictionary(connectionPoints.Count, true /* caseInsensitive */);
            foreach (object obj in connectionPoints) {
                if (obj == null) {
                    throw new ArgumentException(SR.GetString(SR.Collection_CantAddNull), nameof(connectionPoints));
                }
                ConsumerConnectionPoint point = obj as ConsumerConnectionPoint;
                if (point == null) {
                    throw new ArgumentException(SR.GetString(SR.Collection_InvalidType, "ConsumerConnectionPoint"),
                                                nameof(connectionPoints));
                }
                string id = point.ID;
                if (!_ids.Contains(id)) {
                    InnerList.Add(point);
                    _ids.Add(id, point);
                }
                else {
                    throw new ArgumentException(SR.GetString(SR.WebPart_Collection_DuplicateID, "ConsumerConnectionPoint", id), nameof(connectionPoints));
                }
            }
        }

        public ConsumerConnectionPoint Default {
            get {
                return this[ConnectionPoint.DefaultID];
            }
        }

        public ConsumerConnectionPoint this[int index] {
            get {
                return (ConsumerConnectionPoint)InnerList[index];
            }
        }

        public ConsumerConnectionPoint this[string id] {
            get {
                return ((_ids != null) ? (ConsumerConnectionPoint)_ids[id] : null);
            }
        }

        public bool Contains(ConsumerConnectionPoint connectionPoint) {
            return InnerList.Contains(connectionPoint);
        }

        public int IndexOf(ConsumerConnectionPoint connectionPoint) {
            return InnerList.IndexOf(connectionPoint);
        }

        public void CopyTo(ConsumerConnectionPoint[] array, int index) {
            InnerList.CopyTo(array, index);
        }
    }
}

