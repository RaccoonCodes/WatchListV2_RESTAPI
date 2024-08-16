using WatchListV2.DTO;

namespace WatchListV2.Models
{
    public interface IAdminSeriesService
    {
        Task<RestDTO<SeriesModel[]>> GetAllSeriesUserAsync(RequestDTO<SeriesDTO> input);
        Task<bool> DeleteUserAndSeriesAsync(string userId);
    }
}
