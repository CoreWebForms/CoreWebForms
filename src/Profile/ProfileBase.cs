namespace System.Web;

// TODO: Implement
public class ProfileBase
{
    public string UserName { get; set; }
    public bool IsAnonymous { get; set; }

    public void Initialize(string username, bool isAuthenticated)
    {
        throw new NotImplementedException();
    }

    public static ProfileBase Create(string username, bool isAuthenticated)
    {
        throw new NotImplementedException();
    }

    public object GetPropertyValue(string propertyName)
    {
        throw new NotImplementedException();
    }

    public void SetPropertyValue(string propertyName, object propertyValue)
    {
        throw new NotImplementedException();
    }

    public void Save()
    {
        throw new NotImplementedException();
    }
}
