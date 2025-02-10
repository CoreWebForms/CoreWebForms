// MIT License.

using System.Data.Entity;
using Newtonsoft.Json.Linq;

namespace WebFormsSample;

//
public class Helpers
{
    public static string Message = "Hello from a helper";
}

public class SampleContext : DbContext
{
    public DbSet<Book> Books { get; set; }
    public DbSet<Author> Authors { get; set; }
}

public class Author
{
    public int AuthorId { get; set; }
    public string FirstName { get; set; }
    public string LastName { get; set; }
    public ICollection<Book> Books { get; set; }
}
public class Book
{
    public int BookId { get; set; }
    public string Title { get; set; }
    public Author Author { get; set; }
    public int AuthorId { get; set; }
    public string MyTest()
    {
        string json = @"{
  CPU: 'Intel',
  Drives: [
    'DVD read/writer',
    '500 gigabyte hard drive'
  ]
}";

        JObject o = JObject.Parse(json);
        return o.GetType().ToString();
    }

}

