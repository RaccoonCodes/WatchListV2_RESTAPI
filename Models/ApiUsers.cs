using Microsoft.AspNetCore.Identity;

namespace WatchListV2.Models
{
    //using IdentityUser Properties with one-to-many relationship
    public class ApiUsers : IdentityUser
    {
        public ICollection<SeriesModel> Series { get; set; } = new List<SeriesModel>();
    }
}
