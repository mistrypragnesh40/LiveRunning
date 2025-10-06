using CommunityToolkit.Mvvm.ComponentModel;

namespace LiveRunning.Models;

public partial class LiveRunningModel : ObservableObject
{
    public double TotalMiles { get; set; }
    public double AveragePace { get; set; }
    public double AverageSpeed { get; set; }
    [ObservableProperty] private bool _isLiveRunningStarted;
    [ObservableProperty] private bool _isWaitingPanelVisible;
    [ObservableProperty] private string _waitingTimer;
    [ObservableProperty] private string _displayTimer;
    [ObservableProperty] private TimeSpan _runningTime;

    [ObservableProperty] private bool _isStartLiveRunningButtonVisible;
    [ObservableProperty] private bool _elevationGain;
    [ObservableProperty] private bool _elevationLoss;
    [ObservableProperty] private string _totalMilesStr = "0:00";
    [ObservableProperty] private string _averagePaceStr = "00:00";
    [ObservableProperty] private string _averageSpeedStr = "00:00";

    [ObservableProperty] private bool _isWorkoutPaused ;
    [ObservableProperty] private bool _isWorkoutResumed ;
  
        public DateTime RunningDate { get; set; }
    
    
    public double CaloriesBurned { get; set; }
    [ObservableProperty] private string _caloriesBurnedStr = "0.00";


    public List<Location> LocationInfos { get; set; } = new List<Location>();
}