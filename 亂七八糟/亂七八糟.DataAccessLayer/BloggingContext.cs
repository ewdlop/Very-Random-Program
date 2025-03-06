using Microsoft.EntityFrameworkCore;

namespace 亂七八糟.DataAccessLayer;

/// <summary>
/// <seealso href="https://learn.microsoft.com/en-us/ef/core/get-started/overview/first-app?tabs=netcore-cli""/>
/// </summary>
public class BloggingContext : DbContext
{
    //public static readonly BloggingContext Default = new BloggingContext();
    //public required DbSet<Blog> Blogs { get; set; } 
    //public required DbSet<Post>? Posts { get; set; }
    public DbSet<Blog> Blogs { get; set; }
    public DbSet<Post> Posts { get; set; }
    public string? DbPath { get; } = string.Empty;

    public BloggingContext()
    {
        Environment.SpecialFolder folder = Environment.SpecialFolder.LocalApplicationData;
        string? path = Environment.GetFolderPath(folder);
        DbPath = System.IO.Path.Join(path, "blogging.db");
    }

    public BloggingContext(DbContextOptions<BloggingContext> options) : base(options)
    {
        Environment.SpecialFolder folder = Environment.SpecialFolder.LocalApplicationData;
        string? path = Environment.GetFolderPath(folder);
        DbPath = System.IO.Path.Join(path, "blogging.db");
    }

    public BloggingContext(BloggingContext context)
    {
        Blogs = context.Set<Blog>();
        Posts = context.Set<Post>();
        DbPath = context.DbPath;
    }

    public BloggingContext(BloggingContext context, string dbPath)
    {
        Blogs = context.Set<Blog>();
        Posts = context.Set<Post>();
        DbPath = dbPath;
    }

    public BloggingContext(string dbPath)
    {
        Initialize();
        DbPath = dbPath;
    }


    protected virtual void Initialize()
    {
        Blogs = Set<Blog>();
        Posts = Set<Post>();
    }

    //public BloggingContext(string dbPath)
    //{
    //    DbPath = dbPath;
    //}

    // The following configures EF to create a Sqlite database file in the
    // special "local" folder for your platform.
    protected override void OnConfiguring(DbContextOptionsBuilder options)
    {
        if(!options.IsConfigured)
            options.UseSqlite($"Data Source={DbPath}");
    }
}