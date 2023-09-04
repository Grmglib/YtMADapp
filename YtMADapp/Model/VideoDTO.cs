using System.Text.Json.Serialization;

namespace YtMADapp.Model
{
    public class VideoDTO
    {
        public string Title { get; set; }
        public string Id { get; set; }
        public string Author { get; set; }

        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
        public TimeSpan Duration { get; set; }

        public string Thumbnail { get; set; }
        public string Url { get; set; }

        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
        public bool IsPlaylist { get; set; }

        public List<StreamDTO> Quality { get; set; }
    }

    public class StreamDTO
    {
        public string Type { get; set; }
        public string Container { get; set; }
        public double Size { get; set; }

        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
        public double Resolution { get; set; }

        public string Bitrate { get; set; }
        public string Url { get; set; }
    }
}