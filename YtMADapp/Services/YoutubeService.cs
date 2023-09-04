using YoutubeExplode;
using YoutubeExplode.Common;
using YoutubeExplode.Search;
using YoutubeExplode.Videos.Streams;

namespace YtMADapp.Services
{
    public class YoutubeService
    {
        #region Search Video

        //Execute a search on youtube with the provided query, returns 30 first results as a videoDTO list, set by researchResults value.
        public async Task<List<VideoDTO>> VideoSearch(string query)
        {
            int researchResults = 30;
            var youtube = new YoutubeClient();
            List<VideoDTO> videoList = new List<VideoDTO>();
            try
            {
                var results = await youtube.Search.GetResultsAsync(query).CollectAsync(researchResults);
                foreach (var result in results)
                {
                    // Use pattern matching to handle different results (videos, playlists, channels)
                    switch (result)
                    {
                        case VideoSearchResult video:
                            {
                                Thumbnail thumbnail = null;
                                int area = 0;
                                var thumbList = video.Thumbnails;
                                foreach (var thumb in thumbList)
                                {
                                    if (thumb.Resolution.Area > area)
                                    {
                                        area = thumb.Resolution.Area;
                                        thumbnail = thumb;
                                    }
                                }
                                var videodto = new VideoDTO()
                                {
                                    Title = video.Title,
                                    Id = video.Id,
                                    Author = video.Author.ChannelTitle,
                                    Duration = video.Duration.GetValueOrDefault(),
                                    Thumbnail = thumbnail.Url,
                                    Url = video.Url,
                                    IsPlaylist = false
                                };
                                videoList.Add(videodto);
                                break;
                            }
                        case PlaylistSearchResult playlist:
                            {
                                Thumbnail thumbnail = null;
                                int area = 0;
                                var thumbList = playlist.Thumbnails;
                                foreach (var thumb in thumbList)
                                {
                                    if (thumb.Resolution.Area > area)
                                    {
                                        area = thumb.Resolution.Area;
                                        thumbnail = thumb;
                                    }
                                }
                                var videodto = new VideoDTO()
                                {
                                    Title = playlist.Title,
                                    Id = playlist.Id,
                                    Author = playlist.Author.ChannelTitle,
                                    Thumbnail = thumbnail.Url,
                                    Url = playlist.Url,
                                    IsPlaylist = true
                                };
                                videoList.Add(videodto);
                                break;
                            }
                        case ChannelSearchResult channel:
                            {
                                break;
                            }
                    }
                }
                return videoList;
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        #endregion Search Video

        #region Video Info

        //Retrieves a more detailed videoDTO including all streams available
        public async Task<VideoDTO> VideoInfo(string url)
        {
            List<StreamDTO> streamList = new List<StreamDTO>();
            int area = 0;
            Thumbnail thumbnail = null;
            var youtube = new YoutubeClient();

            // You can specify either the video URL or its ID
            var videoUrl = url;
            try
            {
                var video = await youtube.Videos.GetAsync(videoUrl);
                var manifests = await youtube.Videos.Streams.GetManifestAsync(videoUrl);
                var audioStreams = manifests.GetAudioOnlyStreams();
                var videoStreams = manifests.GetVideoOnlyStreams();
                var mixedStreams = manifests.GetMuxedStreams();
                var thumbList = video.Thumbnails;

                foreach (var thumb in thumbList)
                {
                    if (thumb.Resolution.Area > area)
                    {
                        area = thumb.Resolution.Area;
                        thumbnail = thumb;
                    }
                }

                foreach (var stream in audioStreams)
                {
                    var streamDto = new StreamDTO()
                    {
                        Type = "Audio",
                        Container = stream.Container.ToString(),
                        Size = Math.Round(stream.Size.MegaBytes, 2),
                        Bitrate = stream.Bitrate.ToString(),
                        Resolution = 0,
                        Url = stream.Url
                    };
                    streamList.Add(streamDto);
                }
                foreach (var stream in videoStreams)
                {
                    var streamDto = new StreamDTO()
                    {
                        Type = "Video",
                        Container = stream.Container.ToString(),
                        Size = Math.Round(stream.Size.MegaBytes, 2),
                        Resolution = stream.VideoResolution.Height,
                        Bitrate = stream.Bitrate.ToString(),
                        Url = stream.Url
                    };
                    if (streamDto.Container != "webm")
                    {
                        streamList.Add(streamDto);
                    }
                }
                foreach (var stream in mixedStreams)
                {
                    var streamDto = new StreamDTO()
                    {
                        Type = "Mixed",
                        Container = stream.Container.ToString(),
                        Size = Math.Round(stream.Size.MegaBytes, 2),
                        Resolution = stream.VideoResolution.Height,
                        Bitrate = stream.Bitrate.ToString(),
                        Url = stream.Url
                    };
                    streamList.Add(streamDto);
                }
                var streamSorted = streamList.OrderBy(StreamDTO => StreamDTO.Size).ToList();
                var videoDto = new VideoDTO()
                {
                    Title = video.Title,
                    Id = video.Id.Value,
                    Author = video.Author.ChannelTitle,
                    Duration = video.Duration.Value,
                    Thumbnail = thumbnail.Url,
                    Url = video.Url,
                    Quality = streamSorted
                };

                return videoDto;
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        #endregion Video Info

        #region Playlist Info

        /*Retrieves a more detailed videoDTO including all streams available of the first 10 videos on a playlist, set by researchResults value.
          This limitation is necessary since retrieving all information of all videos in playlist that can have up to 5000 videos would take too
          much time*/

        public async Task<List<VideoDTO>> PlaylistInfo(string url)
        {
            int researchResults = 10;
            List<VideoDTO> videoList = new List<VideoDTO>();
            List<string> UrlList = new List<string>();
            var youtube = new YoutubeClient();
            try
            {
                var videos = await youtube.Playlists.GetVideosAsync(url).CollectAsync(researchResults);
                foreach (var video in videos)
                {
                    UrlList.Add(video.Url);
                }
                foreach (var videoUrl in UrlList)
                {
                    videoList.Add(await VideoInfo(videoUrl));
                }
                return videoList;
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        #endregion Playlist Info

        #region Playlist Download

        //Execute a download of all videos on the provided playlist url by matching stream quality selected and using max quality when there is no match
        public async Task PlaylistDownload(string url, string container, string filePath, string bitrate, double resolution, Progress<double> progress)
        {
            var youtube = new YoutubeClient();
            try
            {
                var videos = await youtube.Playlists.GetVideosAsync(url);
                foreach (var video in videos)
                {
                    var streamManifest = await youtube.Videos.Streams.GetManifestAsync(video.Url);
                    string fileName = string.Concat(video.Title.Split(Path.GetInvalidFileNameChars()));
                    if (streamManifest.Streams.Any(f => f.Container.Name == container) & streamManifest.Streams.Any(f => f.Bitrate.ToString() == bitrate) & resolution != 0)
                    {
                        var streamInfo = streamManifest.GetMuxedStreams().Where(f => f.Bitrate.ToString() == bitrate).First();
                        var stream = await youtube.Videos.Streams.GetAsync(streamInfo);
                        await youtube.Videos.Streams.DownloadAsync(streamInfo, $"{filePath}/{fileName}.{streamInfo.Container}", progress);
                    }
                    else if (resolution != 0)
                    {
                        var streamInfo = streamManifest.GetMuxedStreams().TryGetWithHighestVideoQuality();
                        var stream = await youtube.Videos.Streams.GetAsync(streamInfo);
                        await youtube.Videos.Streams.DownloadAsync(streamInfo, $"{filePath}/{fileName}.{streamInfo.Container}", progress);
                    }
                    else
                    {
                        var streamInfo = streamManifest.GetAudioOnlyStreams().TryGetWithHighestBitrate();
                        var stream = await youtube.Videos.Streams.GetAsync(streamInfo);
                        await youtube.Videos.Streams.DownloadAsync(streamInfo, $"{filePath}/{fileName}.{streamInfo.Container}", progress);
                    }
                }
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        #endregion Playlist Download

        #region Video Download

        //Execute a download a video with the provided url by matching stream quality
        public async Task<string> VideoDownload(string url, string container, string filePath, double? resolution, string? bitrate, Progress<double> progress)
        {
            var youtube = new YoutubeClient();
            try
            {
                var video = await youtube.Videos.GetAsync(url);
                var streamManifest = await youtube.Videos.Streams.GetManifestAsync(url);
                string fileName = string.Concat(video.Title.Split(Path.GetInvalidFileNameChars()));
                if (resolution != 0)
                {
                    var streamInfo = streamManifest.GetVideoStreams().Where(s => s.Container.Name == container).Where(s => s.VideoResolution.Height == resolution).First();
                    await youtube.Videos.Streams.DownloadAsync(streamInfo, $"{filePath}/{fileName}.{streamInfo.Container}", progress);
                    return "Video Downloaded";
                }
                else if (bitrate != null)
                {
                    var streamInfo = streamManifest.Streams.Where(s => s.Container.Name == container).Where(s => s.Bitrate.ToString() == bitrate).First();
                    await youtube.Videos.Streams.DownloadAsync(streamInfo, $"{filePath}/{fileName}.{streamInfo.Container}");
                    return "Track Downloaded";
                }
                else
                {
                    throw new Exception("Quality not selected");
                }
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        #endregion Video Download

        #region Stream Download

        public static async Task<Stream> StreamDownload(string url, string container, double? resolution, string? bitrate)
        {
            var youtube = new YoutubeClient();
            try
            {
                var streamManifest = await youtube.Videos.Streams.GetManifestAsync(url);
                if (resolution != null)
                {
                    var streamInfo = streamManifest.GetVideoStreams().Where(s => s.Container.Name == container).Where(s => s.VideoResolution.Height == resolution).First();
                    var stream = await youtube.Videos.Streams.GetAsync(streamInfo);
                    return stream;
                }
                else if (bitrate != null)
                {
                    var streamInfo = streamManifest.Streams.Where(s => s.Container.Name == container).Where(s => s.Bitrate.ToString() == bitrate).First();
                    var stream = await youtube.Videos.Streams.GetAsync(streamInfo);
                    return stream;
                }
                else
                {
                    throw new Exception("Quality not selected");
                }
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        #endregion Stream Download
    }
}