namespace WatchListV2.DTO
{
    //Class containing data and links dent to the client
    //This serves more of outgoing and responding request
    public class RestDTO<T>
    {
        public List<LinkDTO> Links { get; set; } = new List<LinkDTO>();
        public T Data { get; set; } = default!;
        public int? PageIndex { get; set; }
        public int? PageSize { get; set; }
        public int? RecordCount { get; set; }
    }
}
