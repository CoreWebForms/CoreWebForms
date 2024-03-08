using System.Collections.ObjectModel;
using System.Collections.Specialized;

namespace System.Web.Security;

// TODO: Implement
public class MembershipUser
{
  public string UserName { get; set; }
  public string Email { get; set; }
}

// TODO: Implement
public class MembershipUserCollection : Collection<MembershipUser>
{
}

// TODO: Implement
public enum MembershipPasswordFormat
{
  Clear = 0,
  Hashed = 1,
  Encrypted = 2
}

// TODO: Implement
public enum MembershipCreateStatus
{
  Success = 0,
  InvalidUserName = 1,
  InvalidPassword = 2,
  InvalidQuestion = 3,
  InvalidAnswer = 4,
  InvalidEmail = 5,
  DuplicateUserName = 6,
  DuplicateEmail = 7,
  UserRejected = 8,
  InvalidProviderUserKey = 9,
  DuplicateProviderUserKey = 10,
  ProviderError = 11,
  UserNotCreated = 12
}

// TODO: Implement
public class MembershipProvider
{
  public virtual bool EnablePasswordReset
  {
    get => true;
  }

  public virtual bool EnablePasswordRetrieval
  {
    get => true;
  }

  public virtual bool RequiresQuestionAndAnswer
  {
    get => true;
  }

  public virtual bool RequiresUniqueEmail
  {
    get => true;
  }

  public virtual int MinRequiredPasswordLength
  {
    get => 8;
  }

  public virtual int MinRequiredNonAlphanumericCharacters
  {
    get => 1;
  }

  public virtual int MaxInvalidPasswordAttempts
  {
    get => 3;
  }
  public virtual int PasswordAttemptWindow
  {
    get => 10;
  }

  public virtual string PasswordStrengthRegularExpression
  {
    get => ".*";
  }

  public virtual string ApplicationName { get; set; }

  public virtual MembershipPasswordFormat PasswordFormat
  {
    get => MembershipPasswordFormat.Clear;
  }

  public virtual MembershipUser CreateUser(string username, string password, string email, string passwordQuestion,
    string passwordAnswer, bool isApproved, object providerUserKey, out MembershipCreateStatus status)
  {
    throw new NotImplementedException();
  }

  public virtual bool ChangePasswordQuestionAndAnswer(string username, string password, string newPasswordQuestion,
    string newPasswordAnswer)
  {
    throw new NotImplementedException();
  }

  public virtual string GetPassword(string username, string answer)
  {
    throw new NotImplementedException();
  }

  public virtual bool ChangePassword(string username, string oldPassword, string newPassword)
  {
    throw new NotImplementedException();
  }

  public virtual string ResetPassword(string username, string answer)
  {
    throw new NotImplementedException();
  }

  public virtual void UpdateUser(MembershipUser user)
  {
    throw new NotImplementedException();
  }

  public virtual bool ValidateUser(string username, string password)
  {
    throw new NotImplementedException();
  }

  public virtual bool UnlockUser(string userName)
  {
    throw new NotImplementedException();
  }

  public virtual MembershipUser GetUser(object providerUserKey, bool userIsOnline)
  {
    throw new NotImplementedException();
  }

  public virtual MembershipUser GetUser(string username, bool userIsOnline)
  {
    throw new NotImplementedException();
  }

  public virtual string GetUserNameByEmail(string email)
  {
    throw new NotImplementedException();
  }

  public virtual bool DeleteUser(string username, bool deleteAllRelatedData)
  {
    throw new NotImplementedException();
  }

  public virtual MembershipUserCollection GetAllUsers(int pageIndex, int pageSize, out int totalRecords)
  {
    throw new NotImplementedException();
  }

  public virtual int GetNumberOfUsersOnline()
  {
    throw new NotImplementedException();
  }

  public virtual MembershipUserCollection FindUsersByName(string usernameToMatch, int pageIndex, int pageSize,
    out int totalRecords)
  {
    throw new NotImplementedException();
  }

  public virtual MembershipUserCollection FindUsersByEmail(string emailToMatch, int pageIndex, int pageSize,
    out int totalRecords)
  {
    throw new NotImplementedException();
  }

  public virtual void Initialize(string name, NameValueCollection config)
  {
    throw new NotImplementedException();
  }
}
