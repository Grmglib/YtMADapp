using Microsoft.Maui.Controls;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace YtMADapp.Model
{
    class PlaylistDTO
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
