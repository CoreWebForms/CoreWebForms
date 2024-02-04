# Registering Types

Registering tag prefixes was something done in [`web.config`](https://learn.microsoft.com/dotnet/api/system.web.configuration.tagprefixinfo) in framework. This won't work going forward, so we need a way to communicate what namespaces a prefix refers to.

For context, this is what the registration looked like in `web.config`:

```xml
<system.web>  
  <pages>  
    <controls>  
      <!-- Searches all linked assemblies for the namespace -->  
      <add tagPrefix="MyTags1" namespace=" MyNameSpace "/>  
      <!-- Uses a specified assembly -->  
      <add tagPrefix="MyTags2" namespace="MyNameSpace"   
        assembly="MyAssembly"/>  
      <!-- Uses the specified source for the user control -->  
      <add tagprefix="MyTags3" tagname="MyCtrl" src="MyControl.ascx"/>  
    </controls>  
   </pages>  
</system.web>
```

## Design

The initial step is to allow prefixes to be registered by supplying a type from the namespace which will then be included in the compilation:

```cs
public static class WebFormsCompilerExtensions
{
    public static IWebFormsBuilder AddPrefix<T>(this IWebFormsBuilder builder, string prefix);
}

builder.Services.AddWebForms()
    .AddDynamicPages()
    .AddPrefix<ScriptManager>("asp") // For WebForms.Extensions
    .AddPrefix<ListView>("asp") // For WebForms.Extensions
    .AddPrefix<BundleReference>("webopt"); // For WebForms.Optimization
```

See #116 for a tracking issue to improve this experience.
