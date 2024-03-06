// MIT License.

using System.Collections;

namespace System.Web.UI.WebControls.WebParts
{
    public sealed class CatalogPartCollection : ReadOnlyCollectionBase {

        public static readonly CatalogPartCollection Empty = new CatalogPartCollection();

        public CatalogPartCollection() {
        }

        public CatalogPartCollection(ICollection catalogParts) {
            Initialize(null, catalogParts);
        }

        public CatalogPartCollection(CatalogPartCollection existingCatalogParts, ICollection catalogParts) {
            Initialize(existingCatalogParts, catalogParts);
        }

        public CatalogPart this[int index] {
            get {
                return (CatalogPart) InnerList[index];
            }
        }

        public CatalogPart this[string id] {
            get {
                foreach (CatalogPart catalogPart in InnerList) {
                    if (String.Equals(catalogPart.ID, id, StringComparison.OrdinalIgnoreCase)) {
                        return catalogPart;
                    }
                }

                return null;
            }
        }

        internal int Add(CatalogPart value) {
            return InnerList.Add(value);
        }

        public bool Contains(CatalogPart catalogPart) {
            return InnerList.Contains(catalogPart);
        }

        public void CopyTo(CatalogPart[] array, int index) {
            InnerList.CopyTo(array, index);
        }

        public int IndexOf(CatalogPart catalogPart) {
            return InnerList.IndexOf(catalogPart);
        }

        private void Initialize(CatalogPartCollection existingCatalogParts, ICollection catalogParts) {
            if (existingCatalogParts != null) {
                foreach (CatalogPart existingCatalogPart in existingCatalogParts) {
                    // Don't need to check arg, since we know it is valid since it came
                    // from a CatalogPartCollection.
                    InnerList.Add(existingCatalogPart);
                }
            }

            if (catalogParts != null) {
                foreach (object obj in catalogParts) {
                    if (obj == null) {
                        throw new ArgumentException(SR.GetString(SR.Collection_CantAddNull), nameof(catalogParts));
                    }
                    if (!(obj is CatalogPart)) {
                        throw new ArgumentException(SR.GetString(SR.Collection_InvalidType, "CatalogPart"), nameof(catalogParts));
                    }
                    InnerList.Add(obj);
                }
            }
        }
    }
}
