using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;
using CommunityToolkit.Maui.Alerts;
using CommunityToolkit.Maui.Storage;
using CommunityToolkit.Mvvm;
using YtMADapp.Services;

namespace YtMADapp.ViewModel;

[QueryProperty(nameof(Video), nameof(Video))]

public partial class DetailsPageViewModel:BaseViewModel
{
    YoutubeService youtubeService;
    public ObservableCollection<StreamDTO> Streams { get; } = new();
    public ObservableCollection<StreamDTO> Audios { get; } = new();
    public ObservableCollection<StreamDTO> Videos { get; } = new();
    public ObservableCollection<StreamDTO> Mixed { get; } = new();

    public double Progress { get; set; }

    [ObservableProperty]
    VideoDTO video;
    public DetailsPageViewModel(YoutubeService youtubeService)
    {
        this.youtubeService = youtubeService;
    }

    public void OnProgressChanged()
    {
        while(IsBusy)
        {
            
        }
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
                Audios.Clear();
                Videos.Clear();
                Mixed.Clear();
                Streams.Clear();
            }
            foreach (var stream in streams)
            {
                Streams.Add(stream);
                switch(stream.Type)
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
        var progressHandler = new Progress<double>(p => Progress = p);
        if (IsBusy)
            return;
        try
        {
            IsBusy = true;
            Progress = 0;
            var path = await FolderPicker.PickAsync(default);
            var videos = await youtubeService.VideoDownload(Video.Url, stream.Container, path.Folder.Path, stream.Resolution, stream.Bitrate, progressHandler);
            var toast = Toast.Make("Track Downloaded", CommunityToolkit.Maui.Core.ToastDuration.Short, 30);
            toast.Show();
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
