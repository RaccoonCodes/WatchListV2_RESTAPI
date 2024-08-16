using WatchListV2.DTO;
using Microsoft.Extensions.Caching.Distributed;
using WatchListV2.Extensions;
using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using System.Linq.Dynamic.Core;

namespace WatchListV2.Models
{
    public class ESeriesService : ISeriesService
    {
        private readonly ApplicationDbContext _context;
        private readonly IDistributedCache _distributedCache;

        public ESeriesService(ApplicationDbContext context, IDistributedCache distributedCache)
        => (_context, _distributedCache) = (context, distributedCache);

        public async Task<RestDTO<SeriesModel>> CreateSeriesAsync(SeriesDTO input)
        {
            var newSeries = new SeriesModel
            {
                UserID = input.UserID,
                TitleWatched = input.TitleWatched,
                SeasonWatched = input.SeasonWatched,
                ProviderWatched = input.ProviderWatched,
                Genre = input.Genre
            };

            _context.Series.Add(newSeries);
            await _context.SaveChangesAsync();

            return new RestDTO<SeriesModel>
            {
                Data = newSeries,
                Links = new List<LinkDTO>()
            };
        }
        public async Task<RestDTO<SeriesModel[]>> GetSeriesAsync(RequestDTO<SeriesDTO> input)
        {
            if (string.IsNullOrEmpty(input.UserID))
            {
                return new RestDTO<SeriesModel[]>
                {
                    Data = Array.Empty<SeriesModel>(),
                    PageIndex = input.PageIndex,
                    PageSize = input.PageSize,
                    RecordCount = 0,
                    Links = new List<LinkDTO>()
                    
                };
            }
            var query = _context.Series.AsQueryable().Where(b => b.UserID == input.UserID);

            if (!await query.AnyAsync())
            {
                return new RestDTO<SeriesModel[]>
                {
                    Data = Array.Empty<SeriesModel>(),
                    PageIndex = input.PageIndex,
                    PageSize = input.PageSize,
                    RecordCount = 0,
                    Links = new List<LinkDTO>()
                };
            }
            if (!string.IsNullOrEmpty(input.FilterQuery))
            {
                query = query.Where(b => b.TitleWatched.Contains(input.FilterQuery));
            }

            var recordCount = await query.CountAsync();

            if (recordCount == 0)
            {
                return new RestDTO<SeriesModel[]>
                {
                    Data = Array.Empty<SeriesModel>(),
                    PageIndex = input.PageIndex,
                    PageSize = input.PageSize,
                    RecordCount = recordCount,
                    Links = new List<LinkDTO>()
                };
            }

            SeriesModel[]? result = null;

            var cacheKey = $"{input.GetType()}-{JsonSerializer.Serialize(input)}";

            if (!_distributedCache.TryGetValue<SeriesModel[]>(cacheKey, out result))
            {
                query = query.OrderBy($"{input.SortColumn} {input.SortOrder}")
                             .Skip(input.PageIndex * input.PageSize).Take(input.PageSize);

                result = await query.ToArrayAsync();
                _distributedCache.Set(cacheKey, result, new TimeSpan(0, 0, 30));
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

        public async Task<RestDTO<SeriesModel?>> UpdateSeriesAsync(SeriesDTO model)
        {
            var series = await _context.Series
                .FirstOrDefaultAsync(b => b.UserID == model.UserID && b.SeriesID == model.Id);

            if (series == null)
            {
                return new RestDTO<SeriesModel?>
                {
                    Data = null,
                    Links = new List<LinkDTO>()
                };
            }

            series.TitleWatched = model.TitleWatched ?? series.TitleWatched;
            series.Genre = model.Genre ?? series.Genre;
            series.ProviderWatched = model.ProviderWatched ?? series.ProviderWatched;
            series.SeasonWatched = model.SeasonWatched ?? series.SeasonWatched;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!SeriesExists(series.SeriesID))
                {
                    return new RestDTO<SeriesModel?>
                    {
                        Data = null,
                        Links = new List<LinkDTO>()
                    };
                }
                else
                {
                    return new RestDTO<SeriesModel?>
                    {
                        Data = null,
                        Links = new List<LinkDTO>()
                    };
                }
            }

            return new RestDTO<SeriesModel?>
            {
                Data = series,
                Links = new List<LinkDTO>()
                
            };
        }
        public async Task<RestDTO<SeriesModel?>> DeleteSeriesAsync(int id)
        {
            var series = await _context.Series.FirstOrDefaultAsync(b => b.SeriesID == id);
            if (series == null)
            {
                return new RestDTO<SeriesModel?>
                {
                    Data = null,
                    Links = new List<LinkDTO>()
                };
            }

            _context.Series.Remove(series);
            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!SeriesExists(series.SeriesID))
                {
                    return new RestDTO<SeriesModel?>
                    {
                        Data = null,
                        Links = new List<LinkDTO>()
                    };
                }
                else
                {
                    throw;
                }
            }

            return new RestDTO<SeriesModel?>
            {
                Data = series,
                Links = new List<LinkDTO>()
            };
        }

        private bool SeriesExists(int seriesID)
        {
            return _context.Series.Any(e => e.SeriesID == seriesID);
        }
    }
}
