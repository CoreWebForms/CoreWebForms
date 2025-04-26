// MIT License.

using System.Web.UI;
using System;
using System.ComponentModel;
using System.Collections.Generic;
using WebFormsSample;
using System.Data.Entity;

namespace SystemWebUISample.Pages;

public partial class ef : Page
{
    public void CallDB()
    {
        var author = new Author
        {
            FirstName = "William",
            LastName = "Shakespeare"
        };

        using (var context = new SampleContext())
        {
            context.Authors.Add(author);
        }
        txt.Text = "EF Loaded.";
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

}
