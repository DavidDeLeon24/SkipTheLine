using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using SkipTheLine.Enums;
using SkipTheLine.Models;

namespace SkipTheLine.Data
{
    public static class SeedData
    {
        public static async Task InitializeAsync(IServiceProvider serviceProvider)
        {
            using var scope = serviceProvider.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            var userManager = scope.ServiceProvider.GetRequiredService<UserManager<User>>();
            var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();

            // Create roles
            foreach (UserRole role in Enum.GetValues(typeof(UserRole)))
            {
                var roleName = role.ToString();
                if (!await roleManager.RoleExistsAsync(roleName))
                {
                    await roleManager.CreateAsync(new IdentityRole(roleName));
                }
            }

            // Create admin user
            var adminEmail = "admin@skiptheline.com";
            var adminUser = await userManager.FindByEmailAsync(adminEmail);

            if (adminUser == null)
            {
                adminUser = new User
                {
                    UserName = adminEmail,
                    Email = adminEmail,
                    FirstName = "Admin",
                    LastName = "User",
                    PhoneNumber = "1234567890",
                    CreatedAt = DateTime.UtcNow,
                    Role = UserRole.Admin
                };

                var result = await userManager.CreateAsync(adminUser, "Admin@123");
                if (result.Succeeded)
                {
                    await userManager.AddToRoleAsync(adminUser, UserRole.Admin.ToString());
                }
            }

            // Create restaurant owner
            var ownerEmail = "smilebakeryandcafe@gmail.com";
            var ownerUser = await userManager.FindByEmailAsync(ownerEmail);

            if (ownerUser == null)
            {
                ownerUser = new User
                {
                    UserName = ownerEmail,
                    Email = ownerEmail,
                    FirstName = "Smile",
                    LastName = "Owner",
                    PhoneNumber = "(825) 706-1388",
                    CreatedAt = DateTime.UtcNow,
                    Role = UserRole.RestaurantOwner
                };

                var result = await userManager.CreateAsync(ownerUser, "Owner@123");
                if (result.Succeeded)
                {
                    await userManager.AddToRoleAsync(ownerUser, UserRole.RestaurantOwner.ToString());
                }
            }

            // Create Smile Bakery & Cafe
            if (!context.Restaurants.Any())
            {
                var restaurant = new Restaurant
                {
                    Name = "Smile Bakery & Cafe",
                    Cuisine = "Bakery & Cafe",
                    Address = "Unit 3 7464 50 Avenue",
                    City = "Red Deer",
                    Province = "Alberta",
                    PostalCode = "T4P 1X7",
                    Country = "Canada",
                    PhoneNumber = "(825) 706-1388",
                    Email = "smilebakeryandcafe11@gmail.com",
                    Website = "https://www.smilebakeryandcafe.com",
                    Description = "A cozy bakery and cafe offering delicious pastries, fresh bread, specialty coffee, and delightful treats. Perfect for breakfast, lunch, or a relaxing afternoon coffee. Our friendly staff ensures a warm and welcoming atmosphere for all customers.",
                    OpeningTime = new TimeSpan(8, 0, 0),
                    ClosingTime = new TimeSpan(20, 0, 0),
                    MaxPartySize = 8,
                    OwnerId = ownerUser.Id,
                    CreatedAt = DateTime.UtcNow,
                    Rating = 5.0,
                    TotalReviews = 18
                };

                context.Restaurants.Add(restaurant);
                await context.SaveChangesAsync();

                // Create tables
                var tables = new List<Table>();

                // 2-seater tables (6 tables)
                for (int i = 1; i <= 6; i++)
                {
                    tables.Add(new Table
                    {
                        RestaurantId = restaurant.Id,
                        TableNumber = i,
                        Seats = 2,
                        IsActive = true
                    });
                }

                // 4-seater tables (4 tables)
                for (int i = 7; i <= 10; i++)
                {
                    tables.Add(new Table
                    {
                        RestaurantId = restaurant.Id,
                        TableNumber = i,
                        Seats = 4,
                        IsActive = true
                    });
                }

                // 6-seater tables (2 tables)
                for (int i = 11; i <= 12; i++)
                {
                    tables.Add(new Table
                    {
                        RestaurantId = restaurant.Id,
                        TableNumber = i,
                        Seats = 6,
                        IsActive = true
                    });
                }

                context.Tables.AddRange(tables);
                await context.SaveChangesAsync();
            }
        }
    }
}