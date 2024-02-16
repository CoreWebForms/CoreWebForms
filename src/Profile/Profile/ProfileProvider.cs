using System.Collections;
using System.Collections.Specialized;
using System.Configuration;

namespace System.Web.Profile;

// TODO: Implement
public class ProfileProvider
{
    public virtual int DeleteInactiveProfiles(ProfileAuthenticationOption authenticationOption, DateTime userInactiveSinceDate)
    {
        throw new NotImplementedException();
    }

    public virtual int DeleteProfiles(string[] usernames)
    {
        throw new NotImplementedException();
    }

    public virtual int DeleteProfiles(ProfileInfoCollection profiles)
    {
        throw new NotImplementedException();
    }

    public virtual ProfileInfoCollection FindInactiveProfilesByUserName(ProfileAuthenticationOption authenticationOption, string usernameToMatch, DateTime userInactiveSinceDate, int pageIndex, int pageSize, out int totalRecords)
    {
        throw new NotImplementedException();
    }

    public virtual ProfileInfoCollection FindProfilesByUserName(ProfileAuthenticationOption authenticationOption, string usernameToMatch, int pageIndex, int pageSize, out int totalRecords)
    {
        throw new NotImplementedException();
    }

    public virtual ProfileInfoCollection GetAllInactiveProfiles(ProfileAuthenticationOption authenticationOption, DateTime userInactiveSinceDate, int pageIndex, int pageSize, out int totalRecords)
    {
        throw new NotImplementedException();
    }

    public virtual ProfileInfoCollection GetAllProfiles(ProfileAuthenticationOption authenticationOption, int pageIndex, int pageSize, out int totalRecords)
    {
        throw new NotImplementedException();
    }

    public virtual int GetNumberOfInactiveProfiles(ProfileAuthenticationOption authenticationOption, DateTime userInactiveSinceDate)
    {
        throw new NotImplementedException();
    }

    public virtual string ApplicationName { get; set; }
    public virtual SettingsPropertyValueCollection GetPropertyValues(SettingsContext context, SettingsPropertyCollection collection)
    {
        throw new NotImplementedException();
    }

    public virtual void SetPropertyValues(SettingsContext context, SettingsPropertyValueCollection collection)
    {
        throw new NotImplementedException();
    }

    public virtual void Initialize(string name, NameValueCollection config)
    {
        throw new NotImplementedException();
    }


}
