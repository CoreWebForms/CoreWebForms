namespace System.Web.UI.Features;

internal interface IUniqueIdGeneratorFeature
{
    string? GetUniqueIdGenerator(Control control);
}
