# Page compilation

The compilation system for ASP.NET Framework was MSBuild based and not something that we needed to bring-forward as-is as it was mostly an implementation detail. Instead, we are using Roslyn and creating compilations based on that.

## IWebFormsCompiler

The `IWebFormsCompiler` interface is used to access the heavy lifting of taking a collection of input files and compiling that aspx/ascx/etc into their appropriate types. It takes an `ICompilationStrategy` that is used to configure what the compilation should look like.

The main thing this does is to control the flow of the compilation to ensure we can produce all the assemblies:

- Identifying the order using the `DependencyParser` to figure out what dependencies exist between the pages/controls/etc
- Use code dom to generate the source for the file
- Set up a Roslyn compilation to compile the generated code. This code is embedded into the stream for use in debugging
- Tracks previously compiled items so that it can be used for dependent compilations if needed

## ICompilationStrategy

This is an interface that is intended to be used by IWebFormsCompiler to identify how to do certain behaviors we want to be able to control outside of the main processing unit. This includes things such as determining what the streams are that the assemblies or debug information should be written to. The main implementations of this are for in-memory (i.e. runtime) compilation compared to static compilation (i.e. aspnet_compiler).

## PageCompilationOptions

This type is used to configure various aspects of the general compilation system. This includes:

- *Parsers* - The implementations of various `DependencyParser` for a given extension. This type has been augmented from the ASP.NET Framework version to include the ability to generate a code parser and then a code generator from that as a starting point.
- *WebFormsFileProvider* This configures the location from which files are searched for compilation. By default, this is the environments content root

## IWebFormsCompilationFeature

This is a request feature that allows getting types (and additionally creating those types) for a given control/page/etc given its virtual path. On a system with dynamic compilation, it is important to access this information via the request features as it may change for a different request. For a static compilation, the same instance will be used for all requests as the compilation is already complete.

This is the replacement for the `BuildManager` class.
