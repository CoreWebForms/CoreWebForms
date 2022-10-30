// MIT License.

using System.Reflection;

namespace System.Web;
internal static class MethodInvoker
{
    public static EventHandler Create(MethodInfo method, object target)
    {
        var parameters = method.GetParameters();

        if (parameters.Length == 0)
        {
            return (o, e) =>
            {
                try
                {
                    method.Invoke(target, null);
                }
                catch (TargetInvocationException ex) when (ex.InnerException is { } inner)
                {
                    throw inner;
                }
            };
        }
        else if (parameters.Length == 2 && parameters[0].ParameterType == typeof(object) && parameters[1].ParameterType == typeof(EventArgs))
        {
            return (o, e) =>
            {
                try
                {
                    method.Invoke(target, new[] { o, e });
                }
                catch (TargetInvocationException ex) when (ex.InnerException is { } inner)
                {
                    throw inner;
                }
            };
        }
        else
        {
            throw new InvalidOperationException();
        }
    }
}
