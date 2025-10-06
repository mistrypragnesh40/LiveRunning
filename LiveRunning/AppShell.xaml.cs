using LiveRunning.View;

namespace LiveRunning;

public partial class AppShell : Shell
{
	public AppShell()
	{
		InitializeComponent();
		
		Routing.RegisterRoute(nameof(RunningDetailView),typeof(RunningDetailView));
	}
}
