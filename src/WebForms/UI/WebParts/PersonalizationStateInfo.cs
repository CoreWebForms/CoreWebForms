// MIT License.

using System.Web.Util;

namespace System.Web.UI.WebControls.WebParts
{
    [Serializable]
    public abstract class PersonalizationStateInfo {

        private readonly string _path;
        private readonly DateTime _lastUpdatedDate;
        private readonly int _size;

        // We only want our assembly to inherit this class, so make it internal
        internal PersonalizationStateInfo(string path, DateTime lastUpdatedDate, int size) {
            _path = StringUtil.CheckAndTrimString(path, nameof(path));
            PersonalizationProviderHelper.CheckNegativeInteger(size, nameof(size));
            _lastUpdatedDate = lastUpdatedDate.ToUniversalTime();
            _size = size;
        }

        public string Path {
            get {
                return _path;
            }
        }

        public DateTime LastUpdatedDate {
            get {
                return _lastUpdatedDate.ToLocalTime();
            }
        }

        public int Size {
            get {
                return _size;
            }
        }
    }
}
