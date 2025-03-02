// MIT License.

namespace System.Web.Script.Serialization;
[
  AttributeUsage(AttributeTargets.Property | AttributeTargets.Field, AllowMultiple = false, Inherited = true)
  ]
public sealed class ScriptIgnoreAttribute : Attribute
{
    public ScriptIgnoreAttribute()
    {
    }

    public bool ApplyToOverrides
    {
        get;
        set;
    }
}
