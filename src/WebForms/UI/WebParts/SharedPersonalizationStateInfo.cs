// MIT License.

namespace System.Web.UI.WebControls.WebParts
{
    [Serializable]
    public sealed class SharedPersonalizationStateInfo : PersonalizationStateInfo {

        private readonly int _sizeOfPersonalizations;
        private readonly int _countOfPersonalizations;

        public SharedPersonalizationStateInfo(string path,
                                              DateTime lastUpdatedDate,
                                              int size,
                                              int sizeOfPersonalizations,
                                              int countOfPersonalizations) :
                                              base(path, lastUpdatedDate, size) {
            PersonalizationProviderHelper.CheckNegativeInteger(sizeOfPersonalizations, nameof(sizeOfPersonalizations));
            PersonalizationProviderHelper.CheckNegativeInteger(countOfPersonalizations, nameof(countOfPersonalizations));
            _sizeOfPersonalizations = sizeOfPersonalizations;
            _countOfPersonalizations = countOfPersonalizations;
        }

        public int SizeOfPersonalizations {
            get {
                return _sizeOfPersonalizations;
            }
        }

        public int CountOfPersonalizations {
            get {
                return _countOfPersonalizations;
            }
        }
    }
}
