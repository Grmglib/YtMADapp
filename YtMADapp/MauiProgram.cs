using CommunityToolkit.Maui;
using Microsoft.Extensions.Logging;
using YtMADapp.Services;
using YtMADapp.View;

namespace YtMADapp;

public static class MauiProgram
{
    public static MauiApp CreateMauiApp()
    {
        var builder = MauiApp.CreateBuilder();
        builder.UseMauiApp<App>().ConfigureFonts(fonts =>
        {
            fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
            fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
        }).UseMauiCommunityToolkit();
#if DEBUG
        builder.Logging.AddDebug();
#endif
        builder.Services.AddSingleton<MainPage>();
        builder.Services.AddSingleton<YoutubeService>();
        builder.Services.AddSingleton<MainPageViewModel>();
        builder.Services.AddSingleton<DetailsPage>();
        builder.Services.AddSingleton<DetailsPageViewModel>();
        return builder.Build();
    }
}