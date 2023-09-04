using CommunityToolkit.Maui.Alerts;
using CommunityToolkit.Maui.Storage;
using YtMADapp.Services;

namespace YtMADapp.ViewModel;

[QueryProperty(nameof(Video), nameof(Video))]
public partial class DetailsPageViewModel : BaseViewModel
{
    private YoutubeService youtubeService;
    public ObservableCollection<StreamDTO> Streams { get; } = new();
    public ObservableCollection<StreamDTO> Audios { get; } = new();
    public ObservableCollection<StreamDTO> Videos { get; } = new();
    public ObservableCollection<StreamDTO> Mixed { get; } = new();

    public double Progress { get; set; }

    [ObservableProperty]
    private VideoDTO video;

    public DetailsPageViewModel(YoutubeService youtubeService)
    {
        this.youtubeService = youtubeService;
    }

    #region Video Info

    //Execute on details page startup to retrieve video streams and display download options
    public async Task VideoInfoAsync()
    {
        if (IsBusy)
            return;
        try
        {
            if (Video.IsPlaylist)
            {
                IsBusy = true;
                var videos = await youtubeService.PlaylistInfo(Video.Url);
                List<StreamDTO> Qualities = new List<StreamDTO>();
                foreach (var video in videos)
                {
                    if (video.Quality.Count > Qualities.Count)
                    {
                        Qualities.Clear();
                        Qualities.AddRange(video.Quality);
                    }
                }
                if (Qualities.Count != 0)
                {
                    Audios.Clear();
                    Videos.Clear();
                    Mixed.Clear();
                    Streams.Clear();
                }
                foreach (var stream in Qualities)
                {
                    switch (stream.Type)
                    {
                        case "Audio":
                            Audios.Add(stream);
                            break;

                        case "Video":
                            Videos.Add(stream);
                            break;

                        case "Mixed":
                            Mixed.Add(stream);
                            break;
                    }
                }
            }
            else
            {
                IsBusy = true;
                var videoDto = await youtubeService.VideoInfo(Video.Url);
                var streams = videoDto.Quality;
                if (streams.Count != 0)
                {
                    Audios.Clear();
                    Videos.Clear();
                    Mixed.Clear();
                    Streams.Clear();
                }
                foreach (var stream in streams)
                {
                    Streams.Add(stream);
                    switch (stream.Type)
                    {
                        case "Audio":
                            Audios.Add(stream);
                            break;

                        case "Video":
                            Videos.Add(stream);
                            break;

                        case "Mixed":
                            Mixed.Add(stream);
                            break;
                    }
                }
            }
        }
        catch (Exception ex)
        {
            Debug.Write(ex);
            await Shell.Current.DisplayAlert("Error", $"Unable to get list:{ex.Message}", "OK");
        }
        finally
        {
            IsBusy = false;
        }
    }

    #endregion Video Info

    #region Download Command

    [RelayCommand]
    private async Task DownloadVideoAsync(StreamDTO stream)
    {
        var progressHandler = new Progress<double>(p => Progress = p);
        if (IsBusy)
            return;
        try
        {
            if (Video.IsPlaylist)
            {
                IsBusy = true;
                Progress = 0;
                var path = await FolderPicker.PickAsync(default);
                await youtubeService.PlaylistDownload(Video.Url, stream.Container, path.Folder.Path, stream.Bitrate, stream.Resolution, progressHandler);
                var toast = Toast.Make("Track Downloaded", CommunityToolkit.Maui.Core.ToastDuration.Short, 30);
                toast.Show();
            }
            else
            {
                IsBusy = true;
                Progress = 0;
                var path = await FolderPicker.PickAsync(default);
                var videos = await youtubeService.VideoDownload(Video.Url, stream.Container, path.Folder.Path, stream.Resolution, stream.Bitrate, progressHandler);
                var toast = Toast.Make("Track Downloaded", CommunityToolkit.Maui.Core.ToastDuration.Short, 30);
                toast.Show();
            }
        }
        catch (Exception ex)
        {
            Debug.Write(ex);
            await Shell.Current.DisplayAlert("Error", $"Unable to get list:{ex.Message}", "OK");
        }
        finally
        {
            IsBusy = false;
        }
    }

    #endregion Download Command
}