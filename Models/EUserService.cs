using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using WatchListV2.Constants;
using WatchListV2.DTO;
using WatchListV2.Extensions;

namespace WatchListV2.Models
{
    public class EUserService : IUserService
    {
        private readonly UserManager<ApiUsers> _userManager;
        private readonly IConfiguration _configuration;
        private readonly IDistributedCache _cache;
        public EUserService(UserManager<ApiUsers> userManager, IConfiguration configuration, IDistributedCache distributedCache)
        => (_userManager, _configuration, _cache) = (userManager, configuration, distributedCache);
        
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

        public async Task<string> LoginAsync (LoginDTO input)
        {
            var user = await _userManager.FindByNameAsync(input.UserName);
           
            //Checks username and password before continuing 
            if (user == null || !await _userManager.CheckPasswordAsync(user, input.Password))
            {
                throw new Exception("Invalid login attempt");
            }

            //Generating signing credentials when both username and password Match
            var signingCredentials = new SigningCredentials(new SymmetricSecurityKey(
                System.Text.Encoding.UTF8.GetBytes(_configuration["JWT:SigningKey"])), SecurityAlgorithms.HmacSha256);

            //Setting up User claims
            var claims = new List<Claim>();
            claims.Add(new Claim(ClaimTypes.Name, user.UserName));
            claims.AddRange((await _userManager.GetRolesAsync(user))
                .Select(c => new Claim(ClaimTypes.Role, c)));

            //Instantiates JWT object instance
            var jwtObject = new JwtSecurityToken(
                issuer: _configuration["JWT:Issuer"],
                audience: _configuration["JWT:Audience"],
                claims: claims,
                expires: DateTime.Now.AddSeconds(600),
                signingCredentials: signingCredentials
            );

            //return JWT encrypted string
            return new JwtSecurityTokenHandler().WriteToken(jwtObject);
        }
        
    }
}
