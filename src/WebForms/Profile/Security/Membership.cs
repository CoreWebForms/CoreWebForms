namespace System.Web.Security;

// TODO: Migration
public class Membership
{
  public static bool ValidateUser(string username, string password)
  {
    return true;
  }

  public static bool CreateUser(string username, string password)
  {
    return true;
  }

  public static bool ChangePassword(string username, string oldPassword, string newPassword)
  {
    return true;
  }

  public static bool DeleteUser(string username)
  {
    return true;
  }

  public static bool UnlockUser(string username)
  {
    return true;
  }

  public static bool EnablePasswordReset
  {
    get => true;
  }

  public static bool EnablePasswordRetrieval
  {
    get => true;
  }

  public static bool RequiresQuestionAndAnswer
  {
    get => true;
  }

  public static bool RequiresUniqueEmail
  {
    get => true;
  }

  public static int MinRequiredPasswordLength
  {
    get => 8;
  }

  public static int MinRequiredNonAlphanumericCharacters
  {
    get => 1;
  }

  public static string PasswordStrengthRegularExpression
  {
    get => "^(?=.*[a-z])(?=.*[A-Z])(?=.*\\d)(?=.*[^\\da-zA-Z]).{8,}$";
  }

  public static string ResetPassword(string username, string answer)
  {
    return "password";
  }

  public static string GetUserNameByEmail(string email)
  {
    return "username";
  }

  public static string GetPassword(string username, string answer)
  {
    return "password";
  }

  public static string GeneratePassword(int length, int numberOfNonAlphanumericCharacters)
  {
    return "password";
  }
}
