using System;
using System.Collections.Generic;
using System.Linq;

namespace EnumFailureTest
{
    class Program
    {
        public static void Main()
        {
            using (var db = new BloggingContext())
            {
                //Cleanup
                foreach (var blog in db.Blogs)
                {
                    db.Remove(blog);
                }

                //Add Test Data
                db.Blogs.Add(new Blog
                {
                    Url = "http://blogs.msdn.com/adonet",
                    Posts = new List<Post>
                    {
                        new Post { Title = "Example Post 1", Type = BlogType.Technical},
                    }
                });
                var count = db.SaveChanges();
                Console.WriteLine("{0} records saved to database", count);

                Console.WriteLine();
                Console.WriteLine("Success - All posts in database:");
                foreach (var blog in db.Posts)
                {
                    Console.WriteLine(" - {0} - {1}", blog.Title, blog.Type);
                }

                Console.WriteLine();
                Console.WriteLine("Fails - Projecting first post in a blog");

                var result = from blog in db.Blogs
                    select new
                    {
                        BlogId = blog.BlogId,
                        Type = blog.Posts.OrderBy(b => b.Title).FirstOrDefault().Type,
                        PostTitle = blog.Posts.OrderBy(b => b.Title).FirstOrDefault().Title
                    };
                foreach (var projection in result)
                {
                    Console.WriteLine(" - {0}", projection.Type);
                }
            }
        }
    }
}
