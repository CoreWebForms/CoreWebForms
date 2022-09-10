namespace System.Web.UI.Features;

internal static class UniqueIdGeneratorExtensions
{
    public static void EnableUniqueIdGenerator(this Control control)
    {
        control.Features.Set<IUniqueIdGeneratorFeature>(new UniqueIdGeneratorFeature(control));
    }
}
