namespace System.Web.Profile;

// TODO: Implement
public class ProfileInfo : ProfileBase
{
  public ProfileInfo(string username, bool isAnonymous, DateTime lastActivityDate, DateTime lastUpdatedDate, int size)
  {
    UserName = username;
    IsAnonymous = isAnonymous;
    LastActivityDate = lastActivityDate;
    LastUpdatedDate = lastUpdatedDate;
    Size = size;
  }

  public DateTime LastActivityDate { get; }
  public DateTime LastUpdatedDate { get; }
  public int Size { get; }
}
