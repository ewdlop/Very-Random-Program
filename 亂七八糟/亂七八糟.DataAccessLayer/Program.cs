using System;
using System.Linq;
using 亂七八糟.DataAccessLayer;

namespace 亂七八糟.DataAccessLayer;

public class Program
{
    /// <summary>
    /// <seealso href="https://learn.microsoft.com/en-us/ef/core/get-started/overview/first-app?tabs=netcore-cli"></see>
    /// </summary>
    /// <param name="args"></param>
    public static async Task Main(string[] args)
    {
        await using var db = new BloggingContext();

        // Note: This sample requires the database to be created before running.
        Console.WriteLine($"Database path: {db.DbPath}.");

        // Create
        Console.WriteLine("Inserting a new blog");
        db.Add(new Blog { Url = "http://blogs.msdn.com/adonet" });
        db.SaveChanges();

        // Read
        Console.WriteLine("Querying for a blog");
        var blog = db.Blogs
            .OrderBy(b => b.BlogId)
            .First();

        // Update
        Console.WriteLine("Updating the blog and adding a post");
        blog.Url = "https://devblogs.microsoft.com/dotnet";
        blog.Posts.Add(
            new Post { Title = "Hello World", Content = "I wrote an app using EF Core!" });
        db.SaveChanges();

        // Delete
        Console.WriteLine("Delete the blog");
        db.Remove(blog);
        db.SaveChanges();

    }
}
