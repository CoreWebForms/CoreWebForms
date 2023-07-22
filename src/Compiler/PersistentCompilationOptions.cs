// MIT License.

using System.ComponentModel.DataAnnotations;
using Microsoft.CodeAnalysis;

namespace WebForms.Compiler;

public class PersistentCompilationOptions
{
    [Required]
    public string InputDirectory { get; set; } = null!;

    [Required]
    public string TargetDirectory { get; set; } = null!;

    public ICollection<string> References { get; } = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

    public ICollection<MetadataReference> MetadataReferences { get; } = new HashSet<MetadataReference>();
}
