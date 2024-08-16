using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using WatchListV2.Attribute;

namespace WatchListV2.DTO
{
    //Handles incoming request, Mostly used for GET
    public class RequestDTO<T> : IValidatableObject
    {
        [DefaultValue(0)] //for swagger
        public int PageIndex { get; set; } = 0;
        
        [DefaultValue(10)]
        [Range(1, 100)]
        public int PageSize { get; set; } = 10;
        
        [DefaultValue("TitleWatched")]
        [SortColumnValidator(typeof(SeriesDTO))]
        public string? SortColumn { get; set; } = "TitleWatched";

        [DefaultValue("ASC")]
        [SortOrderValidator]
        public string? SortOrder { get; set; } = "ASC";
        [DefaultValue(null)]
        public string? FilterQuery { get; set; } = null; //Filter for looking keywords
        [DefaultValue(null)]
        [Required]
        public string? UserID { get; set; } = null;

        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            var validator = new SortColumnValidatorAttribute(typeof(T));
            var result = validator.GetValidationResult(SortColumn, validationContext);
            return (result != null) ? new[] { result } : Array.Empty<ValidationResult>();
        }
    }
}
