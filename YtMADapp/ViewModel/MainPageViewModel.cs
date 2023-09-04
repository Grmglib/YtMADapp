using YtMADapp.Services;
using YtMADapp.View;

namespace YtMADapp.ViewModel
{
    public partial class MainPageViewModel : BaseViewModel
    {
        public ObservableCollection<VideoDTO> Videos { get; } = new();
        public string Pesquisa { get; set; }
        public string VideoUrl { get; set; }

        private YoutubeService youtubeService;

        public MainPageViewModel(YoutubeService youtubeService)
        {
            Title = "Youtube MAD";
            this.youtubeService = youtubeService;
        }

        #region SearchVideo Command
        [RelayCommand]
        private async Task SearchVideoAsync()
        {
            if (IsBusy)
                return;
            try
            {
                IsBusy = true;
                var videos = await youtubeService.VideoSearch(Pesquisa);
                if (videos.Count != 0)
                {
                    Videos.Clear();
                }
                foreach (var video in videos)
                {
                    Videos.Add(video);
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

#endregion

        #region GoToDetails Command

        [RelayCommand]
        private async Task GoToDetailsAsync(VideoDTO video)
        {
            if (video is null)
                return;

            await Shell.Current.GoToAsync($"{nameof(DetailsPage)}", true,
                new Dictionary<string, object>
                {
                    {"Video", video}
                });
        }

        #endregion
    }
}