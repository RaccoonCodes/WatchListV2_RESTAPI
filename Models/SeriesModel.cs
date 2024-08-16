using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace WatchListV2.Models
{
    [Table("Series")]
    public class SeriesModel
    {
        public int SeriesID { get; set; }
        public string UserID { get; set; } = string.Empty;

        [Required(ErrorMessage = "Please enter Title")]
        public string TitleWatched { get; set; } = string.Empty;

        [Required(ErrorMessage = "Please enter Season")]
        [RegularExpression(@"^\d+$", ErrorMessage = "Season must contain only whole numbers")]
        public string SeasonWatched { get; set; } = string.Empty;

        [Required(ErrorMessage = "Please enter Provider")]
        public string ProviderWatched { get; set; } = string.Empty;

        [Required(ErrorMessage = "Please enter the Genre for the series")]
        public string Genre { get; set; } = string.Empty;
        public ApiUsers? ApiUsers { get; set; }
        public byte[]? RowVersion { get; set; } //for concurrency

    }
}
