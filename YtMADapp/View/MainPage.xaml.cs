using YtMADapp.Services;

namespace YtMADapp.View;

public partial class MainPage : ContentPage
{
    YoutubeService youtubeService;
    public MainPage(MainPageViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }
}

