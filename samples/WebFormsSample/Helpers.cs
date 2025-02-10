// MIT License.

using System.Data.Entity;

namespace WebFormsSample;

//
public class Helpers
{
    public static string Message = "Hello from a helper";
}

public class SampleContext : DbContext
{
    public SampleContext()
    {
        Database.SetInitializer<SampleContext>(null);
    }

    public DbSet<Author>? Authors { get; set; }
}

public class Author
{
    public int AuthorId { get; set; }
    public string? FirstName { get; set; }
    public string? LastName { get; set; }
}

