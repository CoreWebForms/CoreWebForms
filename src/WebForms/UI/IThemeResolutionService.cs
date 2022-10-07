namespace System.Web.UI
{
    using System;

    /// <summary>
    /// Summary description for IThemeResolutionService.
    /// </summary>
    public interface IThemeResolutionService
    {
#if PORT_THEMES
        ThemeProvider[] GetAllThemeProviders();

        ThemeProvider GetThemeProvider();
        ThemeProvider GetStylesheetThemeProvider();
#endif
    }
}
