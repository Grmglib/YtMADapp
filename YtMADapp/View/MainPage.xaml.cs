using YtMADapp.Services;

namespace YtMADapp.View;

public partial class MainPage : ContentPage
{
    private YoutubeService youtubeService;

    public MainPage(MainPageViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }
}