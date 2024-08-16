using WatchListV2.DTO;

namespace WatchListV2.Models
{
    public interface ISeriesService
    {
        Task<RestDTO<SeriesModel>> CreateSeriesAsync(SeriesDTO input);
        Task<RestDTO<SeriesModel[]>> GetSeriesAsync(RequestDTO<SeriesDTO> input);
        Task<RestDTO<SeriesModel?>> UpdateSeriesAsync(SeriesDTO model);
        Task<RestDTO<SeriesModel?>> DeleteSeriesAsync(int id);
    }
}
