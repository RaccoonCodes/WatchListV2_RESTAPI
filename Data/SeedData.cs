using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using WatchListV2.Constants;
using WatchListV2.Models;

namespace WatchListV2.Data
{
    public static class SeedData
    {
        public static async Task EnsurePopulated (IApplicationBuilder app)
        {
            using var scope = app.ApplicationServices.CreateScope();
            var services = scope.ServiceProvider;
            
            var context = services.GetRequiredService<ApplicationDbContext>();
            var userManager = services.GetRequiredService<UserManager<ApiUsers>>();
            var roleManager = services.GetRequiredService<RoleManager<IdentityRole>>();

            //Apply any pending migrations
            if (context.Database.GetPendingMigrations().Any())
            {
                context.Database.Migrate();
            }
            // Ensure roles are created
            await EnsureRolesAsync(roleManager);

            // Ensure the test users are created and assigned to roles
            var testUser = await EnsureUserAsync(userManager, "TestUser", "test-user@email.com", new[] { RoleNames.User });
            var testAdmin = await EnsureUserAsync(userManager, "TestAdministrator", "test-admin@email.com", new[] { RoleNames.Administrator });

            // Seed series data
            await SeedSeriesDataAsync(context, testUser);

        }
        private static async Task EnsureRolesAsync(RoleManager<IdentityRole> roleManager)
        {
            foreach (var roleName in new[] { RoleNames.Administrator, RoleNames.User })
            {
                if (!await roleManager.RoleExistsAsync(roleName))
                {
                    await roleManager.CreateAsync(new IdentityRole(roleName));
                }
            }
        }
        private static async Task<ApiUsers> EnsureUserAsync(UserManager<ApiUsers> userManager, string userName, string email, string[] roles)
        {
            var user = await userManager.FindByNameAsync(userName);
            if (user == null)
            {
                user = new ApiUsers
                {
                    UserName = userName,
                    Email = email,
                    NormalizedUserName = userName.ToUpper(),
                    NormalizedEmail = email.ToUpper()
                };
                var password = new PasswordHasher<ApiUsers>().HashPassword(user, "MyVeryOwnTestPassword123$");
                user.PasswordHash = password;
                await userManager.CreateAsync(user);
            }

            // Ensure the user has the required roles
            await EnsureUserRolesAsync(userManager, user, roles);

            return user;
        }
        private static async Task EnsureUserRolesAsync(UserManager<ApiUsers> userManager, ApiUsers user, string[] roles)
        {
            foreach (var roleName in roles)
            {
                if (!await userManager.IsInRoleAsync(user, roleName))
                {
                    await userManager.AddToRoleAsync(user, roleName);
                }
            }
        }
        private static async Task SeedSeriesDataAsync(ApplicationDbContext context, ApiUsers user)
        {
            if (!context.Series.Any())
            {
                context.Series.AddRange(
                    new SeriesModel
                    {
                        UserID = user.Id,
                        TitleWatched = "Suzume",
                        SeasonWatched = "1",
                        ProviderWatched = "CrunchyRoll",
                        Genre = "Slice of Life"
                    },
                    new SeriesModel
                    {
                        UserID = user.Id,
                        TitleWatched = "The Great Cleric",
                        SeasonWatched = "1",
                        ProviderWatched = "CrunchyRoll",
                        Genre = "Fantasy"
                    }
                );
                await context.SaveChangesAsync();
            }
        }
    }
}

/*
 * public static async Task EnsurePopulated (IApplicationBuilder app)
        {
            using var scope = app.ApplicationServices.CreateScope();
            var services = scope.ServiceProvider;

            var context = services.GetRequiredService<ApplicationDbContext>();

            var userManager = services.GetRequiredService<UserManager<ApiUsers>>();
            var roleManager = services.GetRequiredService<RoleManager<IdentityRole>>();

            //Apply any pending migrations
            if (context.Database.GetPendingMigrations().Any())
            {
                context.Database.Migrate();
            }

            var user = await userManager.FindByNameAsync("TestUser");

            //Checking if user does not exist
            if (user == null) {
                user = new ApiUsers
                {
                    UserName = "TestUser",
                    Email = "test-user@email.com",
                    NormalizedUserName = "TESTUSER",
                    NormalizedEmail = "TEST-USER@EMAIL.COM"


                };
                var password = new PasswordHasher<ApiUsers>().HashPassword(user, "MyVeryOwnTestPassword123$");
                user.PasswordHash = password;
                await userManager.CreateAsync(user);
            }

            //Seed Series
            if (!context.Series.Any()) {
                context.Series.AddRange(
                    new SeriesModel
                    {
                        
                        UserID = user.Id,
                        TitleWatched = "Suzume",
                        SeasonWatched = "1",
                        ProviderWatched = "CrunchyRoll",
                        Genre = "Slice of Life"
                    },
                    new SeriesModel
                    {
                       
                        UserID = user.Id,
                        TitleWatched = "The Great Cleric",
                        SeasonWatched = "1",
                        ProviderWatched = "CrunchyRoll",
                        Genre = "Fantasy"
                    }
                );
               await context.SaveChangesAsync();
            
            }

        }
 * 
 * 
 */