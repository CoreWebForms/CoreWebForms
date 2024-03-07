// MIT License.

using System.Collections;

namespace System.Web.UI.WebControls.WebParts
{
    public sealed class EditorPartCollection : ReadOnlyCollectionBase {

        public static readonly EditorPartCollection Empty = new EditorPartCollection();

        public EditorPartCollection() {
        }

        public EditorPartCollection(ICollection editorParts) {
            Initialize(null, editorParts);
        }

        public EditorPartCollection(EditorPartCollection existingEditorParts, ICollection editorParts) {
            Initialize(existingEditorParts, editorParts);
        }

        public EditorPart this[int index] {
            get {
                return (EditorPart) InnerList[index];
            }
        }

        internal int Add(EditorPart value) {
            return InnerList.Add(value);
        }

        public bool Contains(EditorPart editorPart) {
            return InnerList.Contains(editorPart);
        }

        public void CopyTo(EditorPart[] array, int index) {
            InnerList.CopyTo(array, index);
        }

        public int IndexOf(EditorPart editorPart) {
            return InnerList.IndexOf(editorPart);
        }

        private void Initialize(EditorPartCollection existingEditorParts, ICollection editorParts) {
            if (existingEditorParts != null) {
                foreach (EditorPart existingEditorPart in existingEditorParts) {
                    // Don't need to check arg, since we know it is valid since it came
                    // from an EditorPartCollection.
                    InnerList.Add(existingEditorPart);
                }
            }

            if (editorParts != null) {
                foreach (object obj in editorParts) {
                    if (obj == null) {
                        throw new ArgumentException(SR.GetString(SR.Collection_CantAddNull), nameof(editorParts));
                    }
                    if (!(obj is EditorPart)) {
                        throw new ArgumentException(SR.GetString(SR.Collection_InvalidType, "EditorPart"), nameof(editorParts));
                    }
                    InnerList.Add(obj);
                }
            }
        }
    }
}
