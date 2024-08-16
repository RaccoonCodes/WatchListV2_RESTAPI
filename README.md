# WatchListV2 API Controller
The Full Website is still in progress, regardless, here is the API Controller

## Overview
This project is developed in .NET 6 that follows CRUD Operations that manages your series and videos that you have personally have seen. There are user and Admin login with Registration included. This API Controller relies on JWT Authentication and distributed SQL server Caching

I have set up a Swagger endpoint to test API functionality

Access the Swagger UI at:
- **URL**: `https://localhost:44322/swagger/index.html`

### Test Logins

You can use the following credentials for testing:

- **User:**
  - `userName`: `TestUser`
  - `password`: `MyVeryOwnTestPassword123$`

- **Admin:**
  - `userName`: `TestAdministrator`
  - `password`: `MyVeryOwnTestPassword123$`

After a successful login, a JWT token will be returned. Copy this token and use the "Authorize" button in Swagger to simulate a proper login.

- **User Access:** Limited access to functionality.
- **Admin Access:** Full access to all functionality.

### Important Note

Please change the connection strings to `LocalConnection` instead of `DefaultConnection` in Program.cs file if you wish to use this code locally.

- **Replace:**
  - `opts.ConnectionString = builder.Configuration.GetConnectionString("DefaultConnection");`
  - `opts.UseSqlServer(builder.Configuration["ConnectionStrings:DefaultConnection"]);`
 
## Project Structure
**Attribute**: These files contains helper or extension class to Validation that are used in DTO Folder. They Provide Sort by Order and Sort by entity(ex: id, Genre, Provider) 

**Constants**: This is primarily used for Authorization when, registering or signing in users, will apply or check their roles. I used a strongly type approach to avoid human error when applying roles in code.  

**Controllers**: Each file in the Controllers folder inherits from `ControllerBase`, which provides the minimum functionality needed for controllers that do not handle any view files since they are APIs.

**Data**: In this Folder, it contains a SeedData that will be used when there is no data in the database and needs to populate some data for testing

**DTO**: The DTO (Data Transfer Object) folder is used for API communication, reducing the amount of data sent by only providing the necessary information for API calls. This folder also includes a file that hosts descriptive links for HATEOAS.

**Extensions**: This Folders contains a class for serializing and deserializing JSON for distributed cache

**Migrations**: This houses files used for Entity Framework
**Models**: Used for interfaces, implementation, Entity and Identity setup, and Series Model 

## Key Features

- **JWT Authentication**: The project uses JWT tokens for secure authentication and authorization.
- **Caching**: Implements distributed SQL Server caching to improve performance.
- **Identity Integration**: Integrated with ASP.NET Core Identity for user management, including login, registration, and role-based access control.
- **EF Core**: Configuration and communication with the database (SQL or LocalDb).
- **Swashbuckle**: API testing using Swagger.
- **OpenAPI Analyzers**: Ensures that status codes are used and declared properly in API controllers.
- **Dynamic LINQ**: Facilitates complex LINQ queries.

## Configuration

### User Secrets

I have configured the database to use a local machine setup (assuming you have a SQL database installed). If you wish to use your own database, please configure your connection string using User Secrets as described above in **Important Note**.

### Seperation of Concerns (SoC)
In this project, the Separation of Concerns (SoC) principle is applied to ensure that business logic and HTTP responses are handled separately. This approach improves code maintainability, readability, and testability by keeping different aspects of the application isolated from one another. Decoupling from each other further. Below, I gave an example of seperating and Decoupling Business logic and HTTP reponses. Easier for Maintining, Testing, and Readability 

## Code Explanation
I have added comments throughout the project to help you understand the code. Below are some key parts of the code:

### Program.cs

```csharp
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
```
**Caching Profiles**: Two cache profiles are added: `NoCache` is used in the `AccountController` to ensure no caching during login or registration. `Any-60` is used in the `SeriesController` and `AdminController` to cache series data for 60 seconds, reducing redundant API calls when the database has no changes.

```csharp
builder.Services.AddSwaggerGen(opts =>
{
    opts.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        In = ParameterLocation.Header,
        Description = "Please enter token",
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        BearerFormat = "JWT",
        Scheme = "bearer"
    });
    opts.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference { Type = ReferenceType.SecurityScheme, Id = "Bearer" }
            },
            Array.Empty<string>()
        }
    });
    opts.EnableAnnotations();
});
```
**Swagger Setup**: This configuration enables JWT authentication within Swagger, allowing you to test secure endpoints by providing a token.

### Controllers

### AccountController
The `AccountController` handles authentication and user management functions, including login, registration, and role management. This also pplays a big role in managing security of the application by using JWT tokens.

**Key Functions**
**Registration**: Allows new users to register by providing necessary details such as username, password, and email. Upon successful registration, the user is assigned a role, either as a regular user or an administrator, based on the registration process.

**Login**: Handles user authentication. The user provides their username and password, and if the credentials are valid, the controller generates and returns a JWT token. This token must be included in subsequent API requests to authenticate the user.

**Role Management**: Assigns roles to users during registration or login. These roles determine the user's access level within the application. Admins have full access to all functionality, while regular users have limited access.

**Claims Handling**: The controller manages claims associated with the JWT token, including user roles and other relevant information, ensuring that each API request is authenticated and authorized appropriately.

**Registration**
```csharp
 public async Task <ActionResult> Register(RegisterDTO input)
```
In my code I used _userService.RegisterAsync() to call an implementation that i made for Registration. Reason why i Seperated business logic and HTTP Responses are mentioned above.
```csharp
 public async Task<IdentityResult> RegisterAsync(RegisterDTO input)
 {
     var newUser = new ApiUsers();
     newUser.UserName = input.UserName;
     newUser.Email = input.Email;

     var result = await _userManager.CreateAsync(newUser, input.Password);

     if (result.Succeeded)
     {
         await _userManager.AddToRoleAsync(newUser,RoleNames.User);
     }
     return result;

 }
```
Here, the method is an asynchronous method takes in a DTO and returns IdentityResult with a public accessor. Within it, var `newUser` object is created for `ApiUsers`. then we create and object that will hold the IdentityResult after creating User via `UserManager` (please look at EUserService for full code). If it is sucessful, then assign the user with User role then return Result.

Back at `Register` method in `AccountController`, if the Results from `RegisterAsync()` is sucessful, then return status code 201, otherwise it will return a badRequest.

### SeriesController
The Controller manages CRUD operations for user's series. This controller is accessable only by signing in as User or Administrator.

**Key Functions**

**Get Series By ID**: Retrieves a list of series that the user has watched  Supports pagination, filtering, and sorting.
 
**Create Series**: Allows users to add a new series to their watchlist. The user provides necessary details such as title, genre, and provider, and the controller interacts with the service layer to save the new series to the database. **NOTE:** Please remember to get your UserID by either looking at DB or usign Admin Login

**Update Series**: Enables users to update the details of an existing series. 

**Delete Series**: Allows users to delete a series from their watchlist. 

In the Get Method
```csharp
public async Task<ActionResult<RestDTO<SeriesModel[]>>> Get([FromQuery] RequestDTO<SeriesDTO> input)
```
uses HATEOAS via RestDTO this provides clients with navigational links in the responses, allowing them to discover related resources and actions dynamically. More info about RestDTO below. In the Get method, it uses `_seriesService.GetSeriesAsync();` method for getting series. The Full Implementation is in `ESeriesService.cs`. After sucessful retrieval, the links are created for pagination and filters.

**AdminController**: The adminController only function is retrieving ALL Users. They are able to use SeriesController as well.

### LinkDTO

**Work in progress**
