using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CommunityToolkit.Maui.Storage;
using CommunityToolkit.Mvvm;
using YtMADapp.Services;

namespace YtMADapp.ViewModel;

[QueryProperty(nameof(Video), nameof(Video))]

public partial class DetailsPageViewModel:BaseViewModel
{
    YoutubeService youtubeService;
    public ObservableCollection<StreamDTO> Streams { get; } = new();

    [ObservableProperty]
    VideoDTO video;
    public DetailsPageViewModel(YoutubeService youtubeService)
    {
        this.youtubeService = youtubeService;
    }
    public async Task VideoInfoAsync()
    {
        if (IsBusy)
            return;
        try
        {
            IsBusy = true;
            var videoDto = await youtubeService.VideoInfo(Video.Url);
            var streams = videoDto.Quality;
            if (streams.Count != 0)
            {
                Streams.Clear();
            }
            foreach (var stream in streams)
            {
                Streams.Add(stream);
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

    [RelayCommand]
    async Task DownloadVideoAsync(StreamDTO stream)
    {
        if (IsBusy)
            return;
        try
        {
            IsBusy = true;
            var path = await FolderPicker.PickAsync(default);
            var videos = await youtubeService.VideoDownload(Video.Url, stream.Container, path.Folder.Path, stream.Resolution, stream.Bitrate);
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
}
