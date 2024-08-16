using Microsoft.EntityFrameworkCore;
using WatchListV2.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Microsoft.Net.Http.Headers;
using Microsoft.Extensions.Options;
using Microsoft.OpenApi.Models;
using WatchListV2.Data;
using WatchListV2.Attribute;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Cors;
using WatchListV2.Constants;
using WatchListV2.Controllers;
using Microsoft.AspNetCore.Authentication.Cookies;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers(opts =>
{
    opts.CacheProfiles.Add("NoCache", new CacheProfile() {
        NoStore = true 
    });
    opts.CacheProfiles.Add("Any-60", new CacheProfile() { 
        Location = ResponseCacheLocation.Any, 
        Duration = 60 
    });
    
});
builder.Services.AddControllersWithViews();

builder.Services.AddHttpClient("ApiClient", client =>
{
    var apiBaseUrl = builder.Configuration["ApiBaseUrl"];
    client.BaseAddress = new Uri(apiBaseUrl);
});

builder.Services.AddRazorPages();
builder.Services.AddEndpointsApiExplorer();


builder.Services.AddSwaggerGen(opts =>
{
    //Add Authorization and test endpoints for client side
    opts.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        In = ParameterLocation.Header,
        Description = "Please enter token",
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        BearerFormat = "JWT",
        Scheme = "bearer"
    });
    //Change this later for cleaner view
    opts.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference{ Type = ReferenceType.SecurityScheme, Id = "Bearer"}
            },
            Array.Empty<string>()
        }
    });
    opts.EnableAnnotations();
});
builder.Services.AddDistributedSqlServerCache(opts =>
{
    opts.ConnectionString = builder.Configuration.GetConnectionString("DefaultConnection");
    opts.SchemaName = "dbo";
    opts.TableName = "AppCache";
});

builder.Services.AddDbContext<ApplicationDbContext>(opts =>
{
    opts.UseSqlServer(builder.Configuration["ConnectionStrings:DefaultConnection"]);
});


//Configuration for password requirements
builder.Services.AddIdentity<ApiUsers, IdentityRole>(opts =>
{
    opts.Password.RequireDigit = true;
    opts.Password.RequireLowercase = true;
    opts.Password.RequireUppercase = true;
    opts.Password.RequireNonAlphanumeric = true;
    opts.Password.RequiredLength = 10;
}).AddEntityFrameworkStores<ApplicationDbContext>().AddDefaultUI();
//This helps reduce forgery tokens in third party attacks
builder.Services.AddAuthentication(opts =>
{
    opts.DefaultAuthenticateScheme =
    opts.DefaultChallengeScheme = 
    opts.DefaultForbidScheme = 
    opts.DefaultScheme =
    opts.DefaultSignInScheme =
    opts.DefaultSignOutScheme =
    JwtBearerDefaults.AuthenticationScheme;
}).AddJwtBearer(opts =>
{
    opts.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidIssuer = builder.Configuration["JWT:Issuer"],
        ValidateAudience = true,
        ValidAudience = builder.Configuration["JWT:Audience"],
        ValidateIssuerSigningKey = true,
        IssuerSigningKey = new SymmetricSecurityKey(
            System.Text.Encoding.UTF8.GetBytes(builder.Configuration["JWT:SigningKey"]))
    };
});

builder.Services.AddSession(opts =>
{
    opts.IdleTimeout = TimeSpan.FromMinutes(30); // Set the session timeout
    opts.Cookie.HttpOnly = true;
    opts.Cookie.IsEssential = true;
});

builder.Services.AddAuthorization();

builder.Services.AddScoped<IUserService,EUserService>();
builder.Services.AddScoped<ISeriesService,ESeriesService>();
builder.Services.AddScoped<IAdminSeriesService,EAdminSeriesService>();




var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
    
    app.MapGet("/auth/test/1", 
    [Authorize]
    [ResponseCache(NoStore = true)] 
    () =>
    {
        return Results.Ok("You are authorized!");
    });

    app.MapGet("/auth/test/rbac", 
        [ResponseCache(CacheProfileName = "NoCache")]
        [Authorize]
    (HttpContext httpContext) =>
    {
        var user = httpContext.User;
        if (user.IsInRole(RoleNames.Administrator))
        {
            return Results.Ok("You are Authorized as Admin!");
        }
        else if (user.IsInRole(RoleNames.User))
        {
            return Results.Ok("You are not an Admin! Go back!");
        }

        return Results.Forbid();
    });
}
app.UseSession();

app.UseRouting();
app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();

app.Use((context, next) =>
{
    context.Response.Headers["cache-control"] = "no-cache, no-store";
    return next.Invoke();
});

app.UseStaticFiles();
app.MapDefaultControllerRoute();    
app.MapControllers();

app.MapGet("/", () => "Hello World");
await SeedData.EnsurePopulated(app);

app.Run();

/*
 * Notes:
 * Added EF core
 * Added EF Core Identity
 * Added Swashbuckle 
 * Added JWT 
 * Added Swashbuckle Annotations 6.4.0
 * Added OpenAPIAnalyzers 
 * Added SQL Server Cache 
 * Added SQL server tool : dotnet tool install --global dotnet-sql-cache -version 6.0.11 (for creating AppCache DB Table)
 * Added Dynamic LINQ for SeriesController on Query = query.order 
 */