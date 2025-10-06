using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LiveRunning.Models;
using LiveRunning.Services.Interface;
using LiveRunning.ViewModels;
using Microsoft.Maui.Controls.Maps;
using Microsoft.Maui.Maps;

namespace LiveRunning.View;

public partial class LiveRunningView : ContentPage
{
    private LiveRunningViewModel _viewModel;
    private Location _lastLocation;
    private Polyline _polyLine;
    private ILocationService _locationService;
    private bool _isInitialLocationSet = false;

    public LiveRunningView(LiveRunningViewModel viewModel, ILocationService locationService)
    {
        InitializeComponent();
        _locationService = locationService;
        this.BindingContext = viewModel;
        _viewModel = viewModel;
    }


    protected override void OnNavigatedTo(NavigatedToEventArgs args)
    {
        base.OnNavigatedTo(args);
        if (!_isInitialLocationSet)
        {
            _viewModel.SetInitialLocation(this, _locationService);
            _isInitialLocationSet = true;
        }
    }


    public void SetIntialOrEndingLocation(Location location, bool isStarted, bool isJustMapUpdate = false)
    {
        if (isStarted)
            map.MoveToRegion(MapSpan.FromCenterAndRadius(new Location(location.Latitude, location.Longitude),
                Distance.FromMiles(0.3)));

        if (isJustMapUpdate)
            return;

        if (_viewModel == null && this.BindingContext != null)
            _viewModel = this.BindingContext as LiveRunningViewModel;

        string mapRunningPointID = isStarted ? MapRunningPoint.Start.ToString() : MapRunningPoint.End.ToString();

        if (map.MapElements.Any(f => f.ClassId == mapRunningPointID)) return;

        Circle circle = new Circle
        {
            Center = new Location(location.Latitude, location.Longitude),
            StrokeColor = Colors.White,
            Radius = new Distance(25),
            ClassId = mapRunningPointID,
            StrokeWidth = 5,
            FillColor = isStarted ? Color.FromArgb("#61d52a") : Color.FromArgb("#D0695F"),
        };

        try
        {
            map.MapElements.Add(circle);

            //Add Starting Point in LocationInfos
            if (isStarted)
                _viewModel.LiveRunning.LocationInfos.Add(location);
        }
        catch (Exception ex)
        {
        }
    }

    public void StartNavigating(Location location)
    {
        const double movementThresholdInMiles = 0.0001;
        if (_lastLocation != null && _lastLocation.Latitude == location.Latitude &&
            _lastLocation.Longitude == location.Longitude)
            return;

        if (_lastLocation != null)
        {
            double distanceInMiles = Location.CalculateDistance(location, _lastLocation, DistanceUnits.Miles);

            if (distanceInMiles < movementThresholdInMiles)
                return; // Ignore insignificant movement
        }

        var liveRunning = _viewModel.LiveRunning;

        if (_polyLine == null)
        {
            if (map.MapElements.Any(f => f.ClassId == MapRunningPoint.Running.ToString()))
                _polyLine =
                    map.MapElements.FirstOrDefault(map =>
                        map.ClassId == MapRunningPoint.Running.ToString()) as Polyline;
            else
            {
                _polyLine = new Polyline
                {
                    StrokeColor = Color.FromArgb("40A2FD"),
                    StrokeWidth = 8,
                };
                map.MapElements.Add(_polyLine);
            }
        }

        _polyLine.Geopath.Add(new Location(location.Latitude, location.Longitude));
        liveRunning.LocationInfos.Add(location);

        if (_lastLocation != null)
        {
            double distanceInMiles = Location.CalculateDistance(location, _lastLocation, DistanceUnits.Miles);

            liveRunning.TotalMiles += distanceInMiles;
            liveRunning.TotalMilesStr = liveRunning.TotalMiles.ToString("0.00");

            double durationInSeconds = (location.Timestamp - _lastLocation.Timestamp).TotalSeconds;
            liveRunning.CaloriesBurned +=
                CalculateCaloriesBurned(153, "m", 29, 60.0, durationInSeconds,
                    location.Speed ?? 0);
            liveRunning.CaloriesBurnedStr = Math.Round(liveRunning.CaloriesBurned, 2).ToString();
            double totalMinutes = liveRunning.RunningTime.TotalMinutes;

            if (liveRunning.TotalMiles > 0)
            {
                liveRunning.AveragePace = totalMinutes / liveRunning.TotalMiles;
                ;
                liveRunning.AveragePaceStr = TimeSpan.FromMinutes(liveRunning.AveragePace).ToString("mm':'ss");

                double totalHours = totalMinutes / 60;
                liveRunning.AverageSpeed = liveRunning.TotalMiles / totalHours;
                liveRunning.AverageSpeedStr = liveRunning.AverageSpeed.ToString("0.00");
            }
        }

        map.MoveToRegion(MapSpan.FromCenterAndRadius(new Location(location.Latitude, location.Longitude),
            Distance.FromMiles(0.3)));

        _lastLocation = location;
    }

    public static double CalculateCaloriesBurned(int heightInCm, string gender, int age, double weightInKg,
        double seconds, double speed)
    {
        double rate = 0.0;


        // Convert speed from m/s to mph
        speed = speed * 2.23694;

        // Default MET
        double met = 6.0;

        // Try to find matching MET from the speed table
        var matchedMet = GetSpeedMets
            .Where(x => x.Speed <= speed)
            .OrderByDescending(x => x.Speed)
            .Select(x => x.Met)
            .FirstOrDefault();

        if (matchedMet > 0)
            met = matchedMet;


        // Mifflin-St Jeor BMR formula
        if (gender == "f" || gender == "female")
        {
            rate = ((9.99 * weightInKg) + (6.25 * heightInCm) - (4.92 * age) - 161) / 1000 * met;
        }
        else
        {
            rate = ((9.99 * weightInKg) + (6.25 * heightInCm) - (4.92 * age) + 5) / 1000 * met;
        }

        // Adjust for duration in minutes
        rate *= seconds / 60.0;

        return rate; //
    }

    public static readonly List<SpeedMet> GetSpeedMets = new List<SpeedMet>()
    {
        new SpeedMet { Speed = 2.0, Met = 2.0, Description = "Very slow walk" },
        new SpeedMet { Speed = 2.5, Met = 2.8, Description = "Slow walk" },
        new SpeedMet { Speed = 3.0, Met = 3.0, Description = "Normal walk" },
        new SpeedMet { Speed = 3.5, Met = 3.5, Description = "Brisk walk" },
        new SpeedMet { Speed = 4.0, Met = 8.0, Description = "Very brisk walk or light jog" },
        new SpeedMet { Speed = 4.5, Met = 8.3, Description = "Jogging" },
        new SpeedMet { Speed = 5.0, Met = 9.0, Description = "Steady jogging" },
        new SpeedMet { Speed = 5.5, Met = 9.8, Description = "Faster jogging" },
        new SpeedMet { Speed = 6.0, Met = 10.5, Description = "Moderate run" },
        new SpeedMet { Speed = 6.7, Met = 11.0, Description = "Brisk run" },
        new SpeedMet { Speed = 7.5, Met = 11.8, Description = "Fast run" },
        new SpeedMet { Speed = 8.5, Met = 12.8, Description = "Very fast run" },
        new SpeedMet { Speed = 9.5, Met = 14.5, Description = "Hard effort run" },
        new SpeedMet { Speed = 10.5, Met = 16.0, Description = "Intense run" },
        new SpeedMet { Speed = 12.0, Met = 19.0, Description = "Very intense run" },
        new SpeedMet { Speed = 14.0, Met = 23.0, Description = "Sprint effort" }
    };
}