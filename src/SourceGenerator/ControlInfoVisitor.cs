// MIT License.

using System.Collections.Generic;
using Microsoft.AspNetCore.SystemWebAdapters.Compiler;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

namespace Microsoft.AspNetCore.SystemWebAdapters.UI.Generator;

internal class ControlInfoVisitor : SymbolVisitor
{
    private readonly Compilation _compilation;
    private readonly INamedTypeSymbol _string;
    private readonly INamedTypeSymbol _delegate;
    private readonly ITypeSymbol _control;

    private readonly HashSet<IAssemblySymbol> _visited = new HashSet<IAssemblySymbol>(SymbolEqualityComparer.Default);

    public ControlInfoVisitor(Compilation compilation)
    {
        _compilation = compilation;
        _string = compilation.GetTypeByMetadataName("System.String")!;
        _delegate = compilation.GetTypeByMetadataName("System.Delegate")!;
        _control = compilation.GetTypeByMetadataName("System.Web.UI.Control")!;
    }

    public bool IsValid => _string is not null && _delegate is not null && _control is not null;

    public List<ControlInfo> List { get; } = new List<ControlInfo>();

    public override void VisitAssembly(IAssemblySymbol symbol)
    {
        if (!_visited.Add(symbol))
        {
            return;
        }

        foreach (var module in symbol.Modules)
        {
            module.Accept(this);
        }
    }

    public override void VisitModule(IModuleSymbol symbol)
    {
        symbol.GlobalNamespace.Accept(this);

        foreach (var r in symbol.ReferencedAssemblySymbols)
        {
            r.Accept(this);
        }
    }

    public override void VisitNamespace(INamespaceSymbol symbol)
    {
        foreach (var member in symbol.GetMembers())
        {
            member.Accept(this);
        }
    }

    public override void VisitNamedType(INamedTypeSymbol symbol)
    {
        if (_compilation.ClassifyConversion(symbol, _control) is { IsImplicit: true })
        {
            var info = new ControlInfo(symbol.ContainingNamespace.ToDisplayString(), symbol.Name);

            foreach (var attribute in symbol.GetAttributes())
            {
            }

            foreach (var member in symbol.GetMembers())
            {
                if (member is IPropertySymbol property)
                {
                    if (_compilation.ClassifyConversion(property.Type, _string) is { IsImplicit: true })
                    {
                        info.Strings.Add(property.Name);
                    }
                    else if (_compilation.ClassifyConversion(property.Type, _delegate) is { IsImplicit: true })
                    {
                        info.Events.Add(property.Name);
                    }
                    else
                    {
                        info.Other.Add(property.Name);
                    }
                }
            }

            List.Add(info);
        }
    }
}

