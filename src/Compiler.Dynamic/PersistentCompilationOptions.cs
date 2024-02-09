// MIT License.

using System.ComponentModel.DataAnnotations;

namespace WebForms.Compiler.Dynamic;

public class PersistentCompilationOptions
{
    [Required]
    public string InputDirectory { get; set; } = null!;

    [Required]
    public string TargetDirectory { get; set; } = null!;
}
