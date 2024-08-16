using Microsoft.AspNetCore.Identity;
using System.Security.Claims;
using WatchListV2.DTO;

namespace WatchListV2.Models
{
    public interface IUserService
    {
        Task<IdentityResult> RegisterAsync(RegisterDTO input);
        Task<string> LoginAsync (LoginDTO input);
    }
}
