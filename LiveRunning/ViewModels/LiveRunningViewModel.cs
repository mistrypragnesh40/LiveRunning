using System.Diagnostics;
using System.Timers;
using System.Windows.Input;
using Timer = System.Timers.Timer;
using LiveRunning.Models;
using LiveRunning.Services.Interface;
using LiveRunning.View;

namespace LiveRunning.ViewModels;

public class LiveRunningViewModel : BaseViewModel
{
    private bool _isLocked;
    public Stopwatch StopWatch = new Stopwatch();
    public Timer Timer = new Timer();
    private string _defaultTimer = "00:00:00";
    private int _remainToStartSecond = 10;
    public System.Threading.CancellationTokenSource cts;
    public LiveRunningModel LiveRunning { get; set; } = new LiveRunningModel();
    private ILocationService _locationService;
    private LiveRunningView _pageUIRef;

    public LiveRunningViewModel(ILocationService locationService)
    {
        Timer.Elapsed += _timer_Elapsed;
        Timer.Interval = 1000;
        LiveRunning.WaitingTimer = "";
    }


    private async Task<bool> CheckForLocationPermission()
    {
        var status = await Permissions.CheckStatusAsync<Permissions.LocationAlways>();
        if (status == PermissionStatus.Granted)
        {
            return true;
        }
        else
        {
            status = await Permissions.RequestAsync<Permissions.LocationAlways>();
            if (status == PermissionStatus.Granted)
            {
                return true;
            }

            return false;
        }
    }


    public async Task SetInitialLocation(LiveRunningView pageUIRef, ILocationService locationService)
    {
        _locationService = locationService;
        _pageUIRef = pageUIRef;
      
            try
            {
                bool isPermissionGranted = false;
                isPermissionGranted = await CheckForLocationPermission();

                if (!isPermissionGranted)
                {
                    App.Current!.Dispatcher.Dispatch(async () =>
                    {
                        await App.Current.MainPage.DisplayAlert("Heads Up", "Please allow location permission",
                            "Okay");
                    });
                    return;
                }

                var currentLocation = await Geolocation.GetLocationAsync();
                if (currentLocation != null)
                {
                    App.Current!.Dispatcher.Dispatch(() =>
                    {
                        LiveRunning.IsStartLiveRunningButtonVisible = true;
                        _pageUIRef.SetIntialOrEndingLocation(currentLocation, isStarted: true, true);
                        IsBusy = false;
                    });
                }
            }
            catch (Exception ex)
            {
                await App.Current.MainPage.DisplayAlert("Heads Up", ex.Message, "Okay");
            }
    }

    private void _timer_Elapsed(object sender, ElapsedEventArgs e)
    {
        if (!StopWatch.IsRunning)
            return;

        TimeSpan elapsed = StopWatch.Elapsed;

        if (!LiveRunning.IsLiveRunningStarted)
        {
            if (LiveRunning.IsWaitingPanelVisible && elapsed.TotalSeconds > 10)
            {
                StopWatch.Restart();
                LiveRunning.IsWaitingPanelVisible = false;
                LiveRunning.IsLiveRunningStarted = true;

                Task.Run(async () => { await TextToSpeech.SpeakAsync("Workout Started"); });
            }
            else
            {
                LiveRunning.WaitingTimer = (_remainToStartSecond - elapsed.TotalSeconds).ToString("0");
            }
        }
        else
        {
            LiveRunning.RunningTime = elapsed;
            LiveRunning.DisplayTimer = $"{elapsed.Hours:00}:{elapsed.Minutes:00}:{elapsed.Seconds:00}";
            GetCurrentLocationAsync();
        }
    }

    private void GetCurrentLocationAsync()
    {
        if (_isLocked || cts.IsCancellationRequested)
            return;

        try
        {
            Task.Run(async () =>
            {
                Location location;
                if (DeviceInfo.Platform == DevicePlatform.iOS)
                {
                    location = _locationService.GetCurrentLocation();
                }
                else
                {
                    var request = new GeolocationRequest(GeolocationAccuracy.Best, TimeSpan.FromSeconds(10));
                    location = await Geolocation.GetLocationAsync(request, cts.Token);
                }

                Application.Current!.Dispatcher.Dispatch(() =>
                {
                    if (location != null)
                    {
                        if (LiveRunning.LocationInfos.Count == 0)
                            _pageUIRef.SetIntialOrEndingLocation(location,
                                isStarted: LiveRunning.LocationInfos.Count == 0);
                        else
                            _pageUIRef.StartNavigating(location);
                    }

                    _isLocked = false;
                });
            });
        }
        catch
        {
            // Handle the exception appropriately
        }
        finally
        {
        }
    }

    #region Commands

    public ICommand StartLiveRunningImmediatelyCommand => new Command(async () =>
    {
        if (cts == null)
            cts = new CancellationTokenSource();

        StopWatch.Restart();
        LiveRunning.IsWaitingPanelVisible = false;
        LiveRunning.IsLiveRunningStarted = true;
    });

    public ICommand StartLiveRunningCommand => new Command(async () =>
    {
        if (cts == null)
            cts = new CancellationTokenSource();


        bool isLocationPermissionGranted = false;
        isLocationPermissionGranted = await CheckForLocationPermission();
        if (!isLocationPermissionGranted)
        {
            await App.Current.MainPage.DisplayAlert("Heads Up", "Please Allow Location Permission to continue", "Okay");
            return;
        }

        if (DeviceInfo.Platform == DevicePlatform.iOS)
        {
            try
            {
                _locationService.StartLocationUpdated();
            }
            catch (Exception ex)
            {
            }
        }


        StopWatch.Start();
        Timer.Start();
        LiveRunning.RunningDate = DateTime.Now;
        LiveRunning.IsStartLiveRunningButtonVisible = false;
        LiveRunning.IsWaitingPanelVisible = true;
        LiveRunning.WaitingTimer = _remainToStartSecond.ToString();
        //}
    });

    public ICommand PauseRunningCommand => new Command(() =>
    {
        if (LiveRunning.IsWorkoutPaused)
        {
            // resume
            try
            {
                if (cts == null)
                    cts = new CancellationTokenSource();


                StopWatch.Start();
                Timer.Start();
                LiveRunning.IsWorkoutResumed = true;
                LiveRunning.IsWorkoutPaused = false;

                if (DeviceInfo.Platform == DevicePlatform.iOS)
                    _locationService.StartLocationUpdated();
            }
            catch
            {
            }
            finally
            {
            }

            return;
        }

        try
        {
            StopWatch.Stop();
            Timer.Stop();
            LiveRunning.IsWorkoutPaused = true;
            LiveRunning.IsWorkoutResumed = false;

            if (DeviceInfo.Platform == DevicePlatform.iOS)
                _locationService.StopLocationUpdates();
        }
        catch
        {
        }
        finally
        {
        }
    });

    public ICommand StopWorkOutCommand => new Command(async () =>
    {
        if (LiveRunning.IsLiveRunningStarted)
        {
            if (!LiveRunning.IsWorkoutPaused)
            {
                StopWatch.Stop();
                Timer.Stop();
                LiveRunning.IsWorkoutPaused = true;
                LiveRunning.IsWorkoutResumed = false;

                if (DeviceInfo.Platform == DevicePlatform.iOS)
                    _locationService.StopLocationUpdates();
            }

            var res = await App.Current.MainPage.DisplayAlert("Heads Up", "Do you want to end workout?", "Yes", "No");

            if (res)
            {
            }
            else
            {
                PauseRunningCommand.Execute(null);
            }

            var navParams = new Dictionary<string, object>
            {
                { "LiveRunning", LiveRunning }
            };

            await Shell.Current.GoToAsync(nameof(RunningDetailView), navParams);
        }
    });

    #endregion
}