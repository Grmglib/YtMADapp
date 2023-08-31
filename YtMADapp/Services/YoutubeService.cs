using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using YoutubeExplode;
using YoutubeExplode.Common;
using YoutubeExplode.Search;
using YtMADapp.Model;

namespace YtMADapp.Services
{
    public class YoutubeService
    {
        #region Search Video
        public async Task<List<VideoDTO>> VideoSearch(string query)
        {
            var youtube = new YoutubeClient();
            List<VideoDTO> videoList = new List<VideoDTO>();
            try
            {
                await foreach (var result in youtube.Search.GetResultsAsync(query))
                {
                    if (videoList.Count < 20)
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
                                        Duration = video.Duration.Value,
                                        Thumbnail = thumbnail.Url,
                                        Url = video.Url
                                    };
                                    videoList.Add(videodto);
                                    break;
                                }
                            case PlaylistSearchResult playlist:
                                {
                                    //var id = playlist.Id;
                                    //var title = playlist.Title;
                                    //videoList.Add(title);
                                    break;
                                }
                            case ChannelSearchResult channel:
                                {
                                    //var id = channel.Id;
                                    //var title = channel.Title;
                                    //videoList.Add(title);
                                    break;
                                }
                        }
                    }
                    else { break; }

                }

                return videoList;
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        #endregion
        #region Video Info
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
                    if(streamDto.Container != "webm")
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
        #endregion
        #region Video Download
        public async Task<string> VideoDownload(string url, string container, string filePath, double? resolution, string? bitrate, Progress<double> progress)
        {
            var youtube = new YoutubeClient();
            try
            {
                var video = await youtube.Videos.GetAsync(url);
                var streamManifest = await youtube.Videos.Streams.GetManifestAsync(url);
                string fileName = string.Concat(video.Title.Split(Path.GetInvalidFileNameChars()));
                if (resolution != null)
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
        #endregion
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
        #endregion
    }
}
