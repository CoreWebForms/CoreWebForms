// MIT License.

using System.Collections.Specialized;
using System.Diagnostics;
using System.Globalization;
using System.Security.Principal;
using System.Web.Util;
using Microsoft.Extensions.DependencyInjection;
using WebForms;

namespace System.Web.UI.WebControls.WebParts
{
    internal class WebPartsOptions
    {
        public WebPartsOptions()
        {
            IsUserAllowed = (_, name) => AllowedCapabilities.Contains(name);
        }

        public ICollection<string> AllowedCapabilities { get; } = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        public Func<IPrincipal, string, bool> IsUserAllowed { get; set; }

        public bool EnableExports { get; set; } = true;
    }

    public static class PersonalizationAdministration
    {
        internal static readonly DateTime DefaultInactiveSinceDate = DateTime.MaxValue;
        private const int _defaultPageIndex = 0;
        private const int _defaultPageSize = Int32.MaxValue;

        public static string ApplicationName
        {
            get
            {
                return Provider.ApplicationName;
            }
            set
            {
                Provider.ApplicationName = value;
            }
        }

        public static PersonalizationProvider Provider => HttpRuntimeHelper.Services.GetRequiredService<PersonalizationProvider>();

        public static PersonalizationProviderCollection Providers => HttpRuntimeHelper.Services.GetRequiredService<PersonalizationProviderCollection>();

        public static int ResetAllState(PersonalizationScope scope)
        {
            PersonalizationProviderHelper.CheckPersonalizationScope(scope);
            return ResetStatePrivate(scope, null, null);
        }

        public static int ResetState(PersonalizationStateInfoCollection data)
        {
            int count = 0;
            PersonalizationProviderHelper.CheckNullEntries(data, nameof(data));

            StringCollection sharedPaths = null;
            foreach (PersonalizationStateInfo stateInfo in data)
            {
                UserPersonalizationStateInfo userStateInfo = stateInfo as UserPersonalizationStateInfo;
                if (userStateInfo != null)
                {
                    if (ResetUserState(userStateInfo.Path, userStateInfo.Username))
                    {
                        count += 1;
                    }
                }
                else
                {
                    if (sharedPaths == null)
                    {
                        sharedPaths = new StringCollection();
                    }
                    sharedPaths.Add(stateInfo.Path);
                }
            }

            if (sharedPaths != null)
            {
                string[] sharedPathsArray = new string[sharedPaths.Count];
                sharedPaths.CopyTo(sharedPathsArray, 0);
                count += ResetStatePrivate(PersonalizationScope.Shared, sharedPathsArray, null);
            }
            return count;
        }

        public static bool ResetSharedState(string path)
        {
            path = StringUtil.CheckAndTrimString(path, nameof(path));
            string[] paths = [path];
            int count = ResetStatePrivate(PersonalizationScope.Shared, paths, null);
            Debug.Assert(count >= 0);
            if (count > 1)
            {
                throw new HttpException(SR.GetString(SR.PersonalizationAdmin_UnexpectedResetSharedStateReturnValue, count.ToString(CultureInfo.CurrentCulture)));
            }
            return (count == 1);
        }

        public static int ResetSharedState(string[] paths)
        {
            paths = PersonalizationProviderHelper.CheckAndTrimNonEmptyStringEntries(paths, nameof(paths), true, false, -1);
            return ResetStatePrivate(PersonalizationScope.Shared, paths, null);
        }

        public static int ResetUserState(string path)
        {
            path = StringUtil.CheckAndTrimString(path, nameof(path));
            string[] paths = [path];
            return ResetStatePrivate(PersonalizationScope.User, paths, null);
        }

        public static int ResetUserState(string[] usernames)
        {
            usernames = PersonalizationProviderHelper.CheckAndTrimNonEmptyStringEntries(usernames, nameof(usernames), true, true, -1);
            return ResetStatePrivate(PersonalizationScope.User, null, usernames);
        }

        public static bool ResetUserState(string path, string username)
        {
            path = StringUtil.CheckAndTrimString(path, nameof(path));
            username = PersonalizationProviderHelper.CheckAndTrimStringWithoutCommas(username, nameof(username));
            string[] paths = [path];
            string[] usernames = [username];
            int count = ResetStatePrivate(PersonalizationScope.User, paths, usernames);
            Debug.Assert(count >= 0);
            if (count > 1)
            {
                throw new HttpException(SR.GetString(SR.PersonalizationAdmin_UnexpectedResetUserStateReturnValue, count.ToString(CultureInfo.CurrentCulture)));
            }
            return (count == 1);
        }

        public static int ResetUserState(string path, string[] usernames)
        {
            path = StringUtil.CheckAndTrimString(path, nameof(path));
            usernames = PersonalizationProviderHelper.CheckAndTrimNonEmptyStringEntries(usernames, nameof(usernames), true, true, -1);
            string[] paths = [path];
            return ResetStatePrivate(PersonalizationScope.User, paths, usernames);
        }

        // This private method assumes input parameters have been checked
        private static int ResetStatePrivate(PersonalizationScope scope, string[] paths, string[] usernames)
        {
            int count = Provider.ResetState(scope, paths, usernames);
            PersonalizationProviderHelper.CheckNegativeReturnedInteger(count, "ResetState");
            return count;
        }

        public static int ResetInactiveUserState(DateTime userInactiveSinceDate)
        {
            return ResetInactiveUserStatePrivate(null, userInactiveSinceDate);
        }

        public static int ResetInactiveUserState(string path,
                                                 DateTime userInactiveSinceDate)
        {
            path = StringUtil.CheckAndTrimString(path, nameof(path));
            return ResetInactiveUserStatePrivate(path, userInactiveSinceDate);
        }

        // This private method assumes input parameters have been checked
        private static int ResetInactiveUserStatePrivate(string path, DateTime userInactiveSinceDate)
        {
            int count = Provider.ResetUserState(path, userInactiveSinceDate);
            PersonalizationProviderHelper.CheckNegativeReturnedInteger(count, "ResetUserState");
            return count;
        }

        public static int GetCountOfState(PersonalizationScope scope)
        {
            return GetCountOfState(scope, null);
        }

        public static int GetCountOfState(PersonalizationScope scope, string pathToMatch)
        {
            PersonalizationProviderHelper.CheckPersonalizationScope(scope);
            pathToMatch = StringUtil.CheckAndTrimString(pathToMatch, nameof(pathToMatch), false);
            PersonalizationStateQuery stateQuery = new PersonalizationStateQuery();
            stateQuery.PathToMatch = pathToMatch;
            return GetCountOfStatePrivate(scope, stateQuery);
        }

        // This private method assumes input parameters have been checked
        private static int GetCountOfStatePrivate(PersonalizationScope scope,
                                                  PersonalizationStateQuery stateQuery)
        {
            int count = Provider.GetCountOfState(scope, stateQuery);
            PersonalizationProviderHelper.CheckNegativeReturnedInteger(count, "GetCountOfState");
            return count;
        }

        public static int GetCountOfUserState(string usernameToMatch)
        {
            usernameToMatch = StringUtil.CheckAndTrimString(usernameToMatch, nameof(usernameToMatch), false);
            PersonalizationStateQuery stateQuery = new PersonalizationStateQuery();
            stateQuery.UsernameToMatch = usernameToMatch;
            return GetCountOfStatePrivate(PersonalizationScope.User, stateQuery);
        }

        public static int GetCountOfInactiveUserState(DateTime userInactiveSinceDate)
        {
            return GetCountOfInactiveUserState(null, userInactiveSinceDate);
        }

        public static int GetCountOfInactiveUserState(string pathToMatch,
                                                      DateTime userInactiveSinceDate)
        {
            pathToMatch = StringUtil.CheckAndTrimString(pathToMatch, nameof(pathToMatch), false);
            PersonalizationStateQuery stateQuery = new PersonalizationStateQuery();
            stateQuery.PathToMatch = pathToMatch;
            stateQuery.UserInactiveSinceDate = userInactiveSinceDate;
            return GetCountOfStatePrivate(PersonalizationScope.User, stateQuery);
        }

        // This private method assumes input parameters have been checked
        private static PersonalizationStateInfoCollection FindStatePrivate(
                                                    PersonalizationScope scope,
                                                    PersonalizationStateQuery stateQuery,
                                                    int pageIndex,
                                                    int pageSize,
                                                    out int totalRecords)
        {
            return Provider.FindState(scope, stateQuery, pageIndex, pageSize, out totalRecords);
        }

        public static PersonalizationStateInfoCollection GetAllState(PersonalizationScope scope)
        {
            int totalRecords;
            return GetAllState(scope, _defaultPageIndex, _defaultPageSize, out totalRecords);
        }

        public static PersonalizationStateInfoCollection GetAllState(PersonalizationScope scope,
                                                                     int pageIndex, int pageSize,
                                                                     out int totalRecords)
        {
            PersonalizationProviderHelper.CheckPersonalizationScope(scope);
            PersonalizationProviderHelper.CheckPageIndexAndSize(pageIndex, pageSize);
            return FindStatePrivate(scope, null, pageIndex, pageSize, out totalRecords);
        }

        public static PersonalizationStateInfoCollection GetAllInactiveUserState(DateTime userInactiveSinceDate)
        {
            int totalRecords;
            return GetAllInactiveUserState(userInactiveSinceDate, _defaultPageIndex, _defaultPageSize, out totalRecords);
        }

        public static PersonalizationStateInfoCollection GetAllInactiveUserState(DateTime userInactiveSinceDate,
                                                                                 int pageIndex, int pageSize,
                                                                                 out int totalRecords)
        {
            PersonalizationProviderHelper.CheckPageIndexAndSize(pageIndex, pageSize);
            PersonalizationStateQuery stateQuery = new PersonalizationStateQuery();
            stateQuery.UserInactiveSinceDate = userInactiveSinceDate;
            return FindStatePrivate(PersonalizationScope.User, stateQuery, pageIndex, pageSize, out totalRecords);
        }

        public static PersonalizationStateInfoCollection FindSharedState(string pathToMatch)
        {
            int totalRecords;
            return FindSharedState(pathToMatch, _defaultPageIndex, _defaultPageSize, out totalRecords);
        }

        public static PersonalizationStateInfoCollection FindSharedState(string pathToMatch,
                                                                         int pageIndex, int pageSize,
                                                                         out int totalRecords)
        {
            pathToMatch = StringUtil.CheckAndTrimString(pathToMatch, nameof(pathToMatch), false);
            PersonalizationProviderHelper.CheckPageIndexAndSize(pageIndex, pageSize);
            PersonalizationStateQuery stateQuery = new PersonalizationStateQuery();
            stateQuery.PathToMatch = pathToMatch;
            return FindStatePrivate(PersonalizationScope.Shared, stateQuery, pageIndex, pageSize, out totalRecords);
        }

        public static PersonalizationStateInfoCollection FindUserState(string pathToMatch,
                                                                       string usernameToMatch)
        {
            int totalRecords;
            return FindUserState(pathToMatch, usernameToMatch, _defaultPageIndex,
                                 _defaultPageSize, out totalRecords);
        }

        public static PersonalizationStateInfoCollection FindUserState(string pathToMatch,
                                                                       string usernameToMatch,
                                                                       int pageIndex, int pageSize,
                                                                       out int totalRecords)
        {
            pathToMatch = StringUtil.CheckAndTrimString(pathToMatch, nameof(pathToMatch), false);
            usernameToMatch = StringUtil.CheckAndTrimString(usernameToMatch, nameof(usernameToMatch), false);
            PersonalizationProviderHelper.CheckPageIndexAndSize(pageIndex, pageSize);
            PersonalizationStateQuery stateQuery = new PersonalizationStateQuery();
            stateQuery.PathToMatch = pathToMatch;
            stateQuery.UsernameToMatch = usernameToMatch;
            return FindStatePrivate(PersonalizationScope.User, stateQuery, pageIndex,
                                    pageSize, out totalRecords);
        }

        public static PersonalizationStateInfoCollection FindInactiveUserState(string pathToMatch,
                                                                               string usernameToMatch,
                                                                               DateTime userInactiveSinceDate)
        {
            int totalRecords;
            return FindInactiveUserState(pathToMatch, usernameToMatch, userInactiveSinceDate,
                                         _defaultPageIndex, _defaultPageSize, out totalRecords);
        }

        public static PersonalizationStateInfoCollection FindInactiveUserState(string pathToMatch,
                                                                               string usernameToMatch,
                                                                               DateTime userInactiveSinceDate,
                                                                               int pageIndex, int pageSize,
                                                                               out int totalRecords)
        {
            pathToMatch = StringUtil.CheckAndTrimString(pathToMatch, nameof(pathToMatch), false);
            usernameToMatch = StringUtil.CheckAndTrimString(usernameToMatch, nameof(usernameToMatch), false);
            PersonalizationProviderHelper.CheckPageIndexAndSize(pageIndex, pageSize);
            PersonalizationStateQuery stateQuery = new PersonalizationStateQuery();
            stateQuery.PathToMatch = pathToMatch;
            stateQuery.UsernameToMatch = usernameToMatch;
            stateQuery.UserInactiveSinceDate = userInactiveSinceDate;
            return FindStatePrivate(PersonalizationScope.User, stateQuery, pageIndex,
                                    pageSize, out totalRecords);
        }
    }
}

