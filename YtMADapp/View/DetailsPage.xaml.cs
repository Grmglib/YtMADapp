using YtMADapp.Services;

namespace YtMADapp.View;

public partial class DetailsPage : ContentPage
{
    private YoutubeService youtubeService;

    public DetailsPage(DetailsPageViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
        if (BindingContext is DetailsPageViewModel viewModel)
        {
            viewModel.VideoInfoAsync();
        }
    }

    protected override void OnNavigatedTo(NavigatedToEventArgs args)
    {
        base.OnNavigatedTo(args);
    }
}