// MIT License.

namespace System.Web.Compilation;

using System;
using System.CodeDom.Compiler;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;

/*
 * This class describes a CodeDom compiler, along with the parameters that it uses.
 * The reason we need this class is that if two files both use the same language,
 * but ask for different command line options (e.g. debug vs retail), we will not
 * be able to compile them together.  So effectively, we need to treat them as
 * different languages.
 */
public sealed class CompilerType
{

    private readonly Type _codeDomProviderType;
    public Type CodeDomProviderType { get { return _codeDomProviderType; } }

    private readonly CompilerParameters _compilParams;
    public CompilerParameters CompilerParameters { get { return _compilParams; } }

    internal CompilerType(Type codeDomProviderType, CompilerParameters compilParams)
    {

        Debug.Assert(codeDomProviderType != null);
        _codeDomProviderType = codeDomProviderType;

        if (compilParams == null)
        {
            _compilParams = new CompilerParameters();
        }
        else
        {
            _compilParams = compilParams;
        }
    }

    internal CompilerType Clone()
    {
        // Clone the CompilerParameters to make sure the original is untouched
        return new CompilerType(_codeDomProviderType, CloneCompilerParameters());
    }

    private CompilerParameters CloneCompilerParameters()
    {

        CompilerParameters copy = new CompilerParameters();
        copy.IncludeDebugInformation = _compilParams.IncludeDebugInformation;
        copy.TreatWarningsAsErrors = _compilParams.TreatWarningsAsErrors;
        copy.WarningLevel = _compilParams.WarningLevel;
        copy.CompilerOptions = _compilParams.CompilerOptions;

        return copy;
    }

    [SuppressMessage("Microsoft.Usage", "CA2303:FlagTypeGetHashCode", Justification = "This is used on codeDomProviderTypes which are not com types.")]
    public override int GetHashCode()
    {
        return _codeDomProviderType.GetHashCode();
    }

    public override bool Equals(Object o)
    {
        CompilerType other = o as CompilerType;
        if (o == null)
        {
            return false;
        }

        return _codeDomProviderType == other._codeDomProviderType &&
            _compilParams.WarningLevel == other._compilParams.WarningLevel &&
            _compilParams.IncludeDebugInformation == other._compilParams.IncludeDebugInformation &&
            _compilParams.CompilerOptions == other._compilParams.CompilerOptions;
    }

#if PORT_ASSEMBLYBUILDER
    internal static AssemblyBuilder GetDefaultAssemblyBuilder(CompilationSection compConfig,
    ICollection referencedAssemblies, VirtualPath configPath, string outputAssemblyName) {

    return GetDefaultAssemblyBuilder(compConfig, referencedAssemblies,
        configPath, null /*generatedFilesDir*/, outputAssemblyName);
}

internal static AssemblyBuilder GetDefaultAssemblyBuilder(CompilationSection compConfig,
    ICollection referencedAssemblies, VirtualPath configPath,
    string generatedFilesDir, string outputAssemblyName) {

    CompilerType ctwp = GetDefaultCompilerTypeWithParams(compConfig, configPath);
    return ctwp.CreateAssemblyBuilder(compConfig, referencedAssemblies,
        generatedFilesDir, outputAssemblyName);
}
#endif
}
