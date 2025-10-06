using CommunityToolkit.Mvvm.ComponentModel;
using LiveRunning.Models;
using LiveRunning.View;

namespace LiveRunning.ViewModels;

public partial class RunningDetailViewModel: BaseViewModel
{
    [ObservableProperty]
    private LiveRunningModel  _liveRunning;
    
    private RunningDetailView _pageUIRef;
    public RunningDetailViewModel()
    {
        
    }

   
}