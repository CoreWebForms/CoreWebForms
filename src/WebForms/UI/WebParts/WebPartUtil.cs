// MIT License.

using System.Diagnostics;
using System.Reflection;
using Microsoft.Extensions.DependencyInjection;

namespace System.Web.UI.WebControls.WebParts
{
    internal static class WebPartUtil
    {
        // Called from WebPartManagerInternals and ConnectionsZone.
        internal static object CreateObjectFromType(Type type)
        {
            return ActivatorUtilities.CreateInstance(HttpRuntime.WebObjectActivator, type);
        }

        // We use BuildManager.GetType() instead of Type.GetType() so we can load types from the
        // Code directory, even if no assembly is specified.
        internal static Type DeserializeType(string typeName, bool throwOnError)
        {
            throw new InvalidOperationException("DeserializeType failed in WebPartUtil");
        }

        internal static Type[] GetTypesForConstructor(ConstructorInfo constructor)
        {
            Debug.Assert(constructor != null);
            ParameterInfo[] parameters = constructor.GetParameters();
            Type[] types = new Type[parameters.Length];
            for (int i = 0; i < parameters.Length; i++)
            {
                types[i] = parameters[i].ParameterType;
            }
            return types;
        }

        internal static bool IsConnectionPointTypeValid(Type connectionPointType, bool isConsumer)
        {
            if (connectionPointType == null)
            {
                return true;
            }

            if (!(connectionPointType.IsPublic || connectionPointType.IsNestedPublic))
            {
                return false;
            }

            Type baseType = isConsumer ? typeof(ConsumerConnectionPoint) : typeof(ProviderConnectionPoint);
            if (!connectionPointType.IsSubclassOf(baseType))
            {
                return false;
            }

            Type[] constructorTypes = isConsumer ? ConsumerConnectionPoint.ConstructorTypes :
                ProviderConnectionPoint.ConstructorTypes;
            ConstructorInfo constructor = connectionPointType.GetConstructor(constructorTypes);
            if (constructor == null)
            {
                return false;
            }

            return true;
        }

        // This helper method used to be needed to resolve types in the Code directory.  Since this
        // was fixed in VSWhidbey 380793, we can just use Type.AssemblyQualifiedName instead of
        // Type.FullName.  However, I am leaving this helper method in place in case we need to make
        // another fix in the future.
        internal static string SerializeType(Type type)
        {
            return type.FullName;
        }
    }
}
