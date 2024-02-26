// MIT License.

using System.CodeDom;
using System.CodeDom.Compiler;
using System.Collections;

namespace System.Web.Compilation;

public class AssemblyBuilder
{
    public string CultureName { get; set; }
    public Type CodeDomProviderType { get; set; }
    public bool IsBatchFull { get; set; }

    public void AddBuildProvider(BuildProvider buildProvider)
    {
        throw new NotImplementedException();
    }

    public CompilerResults Compile()
    {
        throw new NotImplementedException();
    }

    public CompilerParameters GetCompilerParameters()
    {
        throw new NotImplementedException();
    }

    public bool ContainsTypeNames(ICollection typeNames)
    {
        throw new NotImplementedException();
    }

    public void AddTypeNames(ICollection typeNames)
    {
        throw new NotImplementedException();
    }

    internal void AddCodeCompileUnit(SourceFileBuildProvider sourceFileBuildProvider, CodeSnippetCompileUnit snippetCompileUnit)
    {
        throw new NotImplementedException();
    }
}
