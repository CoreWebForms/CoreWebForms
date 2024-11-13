namespace System.Web.Profile;

[Serializable]
public class ProfileInfo : ProfileBase
{
    public ProfileInfo(string username, bool isAnonymous, DateTime lastActivityDate, DateTime lastUpdatedDate, int size)
    {
        if( username != null )
        {
            username = username.Trim();
        }

        _UserName = username;
        if (lastActivityDate.Kind == DateTimeKind.Local) {
            lastActivityDate = lastActivityDate.ToUniversalTime();
        }
        _LastActivityDate = lastActivityDate;
        if (lastUpdatedDate.Kind == DateTimeKind.Local) {
            lastUpdatedDate = lastUpdatedDate.ToUniversalTime();
        }
        _LastUpdatedDate = lastUpdatedDate;
        _IsAnonymous = isAnonymous;
        _Size = size;
    }

    protected ProfileInfo() { }

    public virtual string    UserName         { get { return _UserName;} }
    public virtual DateTime  LastActivityDate { get { return _LastActivityDate.ToLocalTime();} }
    public virtual DateTime  LastUpdatedDate  { get { return _LastUpdatedDate.ToLocalTime(); } }
    public virtual bool      IsAnonymous      { get { return _IsAnonymous;} }
    public virtual int       Size             { get { return _Size; } }


    private string   _UserName;
    private DateTime _LastActivityDate;
    private DateTime _LastUpdatedDate;
    private bool     _IsAnonymous;
    private int      _Size;
}


