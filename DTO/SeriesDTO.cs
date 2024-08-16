using System.ComponentModel.DataAnnotations;

namespace WatchListV2.DTO
{
    public class SeriesDTO
    {
        [Required]
        public int Id { get; set; }
        [Required]
        public string UserID { get; set; } = string.Empty;
        [Required]
        public string TitleWatched { get; set; } = string.Empty;
        [Required]
        public string SeasonWatched { get; set; } = string.Empty;
        [Required]
        public string ProviderWatched { get; set; } = string.Empty;
        [Required]
        public string Genre { get; set; } = string.Empty;

    }
}
