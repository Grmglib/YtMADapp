using System.Text.Json.Serialization;

namespace YtMADapp.Model
{
    internal class PlaylistDTO
    {
        public string Title { get; set; }
        public string Id { get; set; }
        public string Author { get; set; }
        public string Thumbnail { get; set; }
        public string Url { get; set; }

        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
        public List<VideoDTO> Videos { get; set; }
    }
}