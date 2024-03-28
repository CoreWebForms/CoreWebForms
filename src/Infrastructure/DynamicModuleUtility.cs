using System.Web;

namespace Microsoft.Web.Infrastructure.DynamicModuleHelper;

public static class DynamicModuleUtility
{
    public static void RegisterModule(Type moduleType) => HttpApplication.RegisterModule(moduleType);
}