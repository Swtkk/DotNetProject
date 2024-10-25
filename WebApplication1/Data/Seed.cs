
using WebApplication1.Models;
using WebApplication1.Models;

namespace WebApplication1.Data
{
    public class Seed
    {
        public static void SeedData(IApplicationBuilder applicationBuilder)
        {
            using (var serviceScope = applicationBuilder.ApplicationServices.CreateScope())
            {
                var context = serviceScope.ServiceProvider.GetService<ApplicationDbContext>();

                context.Database.EnsureCreated();

                if (!context.Posts.Any())
                {
                    context.Posts.AddRange(new List<Post>()
                    {
                        new Post()
                        {
                            Title = "Title",
                            Description = "Description",

                        },
                        new Post()
                        {
                            Title = "Title2",
                            Description = "Description2",

                        }


                    });
                    context.SaveChanges();
                }
            }
        }
    }                
}