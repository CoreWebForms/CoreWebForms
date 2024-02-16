// MIT License.

using System.Reflection;

#nullable disable

namespace System.Web.UI;

internal interface IScriptResourceDefinition
{
    string Path { get; }
    string DebugPath { get; }
    string CdnPath { get; }
    string CdnDebugPath { get; }
    string CdnPathSecureConnection { get; }
    string CdnDebugPathSecureConnection { get; }
    string ResourceName { get; }
    Assembly ResourceAssembly { get; }
}
