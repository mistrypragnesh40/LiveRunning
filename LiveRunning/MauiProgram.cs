using LiveRunning.Services;
using LiveRunning.Services.Interface;
using LiveRunning.View;
using LiveRunning.ViewModels;
using Microsoft.Extensions.Logging;

namespace LiveRunning;

public static class MauiProgram
{
	public static MauiApp CreateMauiApp()
	{
		var builder = MauiApp.CreateBuilder();
		builder
			.UseMauiApp<App>()
#if IOS || ANDROID
			.UseMauiMaps()
			#endif
			.ConfigureFonts(fonts =>
			{
				fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
				fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
			});

#if DEBUG
		builder.Logging.AddDebug();
#endif

		
		builder.Services.AddSingleton<ILocationService>(sp =>
		{
#if IOS
			return new LocationService();   // your iOS implementation
#else
			return null;
#endif
		});
		builder.Services.AddTransient(typeof(LiveRunningViewModel));
		builder.Services.AddTransient(typeof(LiveRunningView));
		
		builder.Services.AddTransient(typeof(RunningDetailViewModel));
		builder.Services.AddTransient(typeof(RunningDetailView));

		

		return builder.Build();
	}
}
