using CommunityToolkit.Mvvm.ComponentModel;

namespace LiveRunning.ViewModels;

public partial class BaseViewModel : ObservableObject
{
    [ObservableProperty]
    private bool _isBusy;
}