namespace WatchListV2.DTO
{
    //Class that will host descriptive link
    public class LinkDTO
    {
        public string Href { get; private set; } //URLs
        public string Rel { get; private set; } //Relationship
        public string Type { get; private set; } //Type being send

        public LinkDTO(string href, string rel, string type)
        {
            Href = href;
            Rel = rel;
            Type = type;
        }
    }
}
