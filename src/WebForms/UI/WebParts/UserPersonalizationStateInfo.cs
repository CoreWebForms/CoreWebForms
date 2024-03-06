// MIT License.

using System.Web.Util;

namespace System.Web.UI.WebControls.WebParts
{
    [Serializable]
    public sealed class UserPersonalizationStateInfo : PersonalizationStateInfo {

        private readonly string _username;
        private readonly DateTime _lastActivityDate;

        public UserPersonalizationStateInfo(string path,
                                            DateTime lastUpdatedDate,
                                            int size,
                                            string username,
                                            DateTime lastActivityDate) :
                                            base(path, lastUpdatedDate, size) {
            _username = StringUtil.CheckAndTrimString(username, nameof(username));
            _lastActivityDate = lastActivityDate.ToUniversalTime();
        }

        public string Username {
            get {
                return _username;
            }
        }

        public DateTime LastActivityDate {
            get {
                return _lastActivityDate.ToLocalTime();
            }
        }
    }
}
