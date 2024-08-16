namespace WatchListV2.DTO
{
    public class UserSeriesDTO
    {
        public string UserName { get; set; } = string.Empty;
        public List<SeriesDTO> Series { get; set; } = new List<SeriesDTO>();
    }
}
