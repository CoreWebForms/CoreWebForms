// MIT License.

namespace System.Web.UI;

/// <summary>
/// Summary description for IThemeResolutionService.
/// </summary>
public interface IThemeResolutionService
{
    ThemeProvider[] GetAllThemeProviders();
    ThemeProvider GetThemeProvider();
    ThemeProvider GetStylesheetThemeProvider();
}
