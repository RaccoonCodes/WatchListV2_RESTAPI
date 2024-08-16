using Azure.Core;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Distributed;
using System.Text.Json;
using System.Linq.Dynamic.Core;
using WatchListV2.DTO;
using WatchListV2.Extensions;
using Microsoft.AspNetCore.Identity;

namespace WatchListV2.Models
{
    public class EAdminSeriesService : IAdminSeriesService
    {
        private readonly ApplicationDbContext _context;
        private readonly IDistributedCache _distributedCache;
        private readonly UserManager<ApiUsers> _userManager;

        public EAdminSeriesService(ApplicationDbContext context, 
            IDistributedCache distributedCache, UserManager<ApiUsers> userManager)
        => (_context, _distributedCache, _userManager) = (context, distributedCache, userManager);

        public async Task<RestDTO<SeriesModel[]>> GetAllSeriesUserAsync(RequestDTO<SeriesDTO> input)
        {
            var query = _context.Series.AsQueryable();

            // Filter by UserID if provided
            if (!string.IsNullOrEmpty(input.UserID))
            {
                query = query.Where(b => b.UserID == input.UserID);
            }

            // Filter by query parameters
            if (!string.IsNullOrEmpty(input.FilterQuery))
            {
                query = query.Where(b => b.TitleWatched.Contains(input.FilterQuery));
            }

            var recordCount = await query.CountAsync();

            var result = Array.Empty<SeriesModel>();

            if (recordCount > 0)
            {
                var cacheKey = $"{input.GetType()}-{JsonSerializer.Serialize(input)}";

                if (!_distributedCache.TryGetValue(cacheKey, out result))
                {
                    result = await query
                        .OrderBy($"{input.SortColumn} {input.SortOrder}")
                        .Skip(input.PageIndex * input.PageSize)
                        .Take(input.PageSize)
                        .ToArrayAsync();

                    _distributedCache.Set(cacheKey, result, new TimeSpan(0, 0, 30));
                }
            }

            return new RestDTO<SeriesModel[]>
            {
                Data = result!,
                PageIndex = input.PageIndex,
                PageSize = input.PageSize,
                RecordCount = recordCount,
                Links = new List<LinkDTO>()
            };
        }

        public async Task<bool> DeleteUserAndSeriesAsync(string userId)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
            {
                return false;
            }

            using var transaction = await _context.Database.BeginTransactionAsync();

            try
            {
                var userSeries = _context.Series.Where(u => u.UserID == userId);
                _context.Series.RemoveRange(userSeries);

                var result = await _userManager.DeleteAsync(user);
                if (!result.Succeeded)
                {
                    await transaction.RollbackAsync();
                    return false;
                }
                await _context.SaveChangesAsync();
                await transaction.CommitAsync();
                return true;
            }
            catch
            {
                await transaction.RollbackAsync();
                return false;
            }
        }

    }
}
