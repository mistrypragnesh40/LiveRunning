using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LiveRunning.Models;
using LiveRunning.ViewModels;
using Microsoft.Maui.Controls.Maps;
using Microsoft.Maui.Maps;

namespace LiveRunning.View;



[QueryProperty(nameof(LiveRunning), "LiveRunning")]  
public partial class RunningDetailView : ContentPage
{
    public LiveRunningModel LiveRunning { get; set; }
  
    
    private RunningDetailViewModel _viewModel;

    public RunningDetailView(RunningDetailViewModel viewModel)
    {
        InitializeComponent();
        this.BindingContext = _viewModel = viewModel;
    }


    override protected async void OnAppearing()
    {
        base.OnAppearing();

        if (LiveRunning != null)
        {
            _viewModel.LiveRunning = LiveRunning;
            SetMapData(LiveRunning.LocationInfos);
        }
    }
    
    public void SetMapData(List<Location> runningData)
    {
        if (map.MapElements?.Count > 0) return;
        try
        {
            var startLocation = runningData.FirstOrDefault();
            var endLocation = runningData.LastOrDefault();

            AddPolyline(LiveRunning.LocationInfos);
            AddCircle(startLocation.Latitude, startLocation.Longitude, Color.FromArgb("#61d52a"));
            AddCircle(endLocation.Latitude, endLocation.Longitude, Color.FromArgb("#D0695F"));

            double centerLatitude = (startLocation.Latitude + endLocation.Latitude) / 2;
            double centerLongitude = (startLocation.Longitude + endLocation.Longitude) / 2;
            Location center = new Location(centerLatitude, centerLongitude);

            double mapZoomPercentage = (LiveRunning.TotalMiles * 32) / 100;
            // Move the map to the region defined by the circle
            MapSpan mapSpan = MapSpan.FromCenterAndRadius(center, Distance.FromMiles(mapZoomPercentage));
            map.MoveToRegion(mapSpan);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error setting map data: {ex.Message}");
        }
    }

    void AddCircle(double lat, double lon, Color color)
    {

        map.MapElements.Add(new Circle
        {
            Center = new Location(lat, lon),
            Radius = new Distance(8),
            StrokeColor = Colors.White,
            StrokeWidth = 3,
            FillColor = color,
        });
    }


    private void AddPolyline(List<Location> runningData)
    {
        var polyline = new Polyline
        {
            StrokeWidth = 10
        };

        double minSpeed = double.MaxValue;
        double maxSpeed = double.MinValue;
        double prevSpeedRange = 0;

        foreach (var location in runningData)
        {
            double speedMps = location.Speed ?? 0; // Speed in meters per second
            double speedMph = speedMps * 2.23694; // Conversion from meters per second to miles per hour

            if (speedMph < minSpeed)
                minSpeed = speedMph;

            if (speedMph > maxSpeed)
                maxSpeed = speedMph;

            double currentSpeedRange = CalculateSpeedRange(minSpeed, maxSpeed, speedMph);

            if (Math.Abs(prevSpeedRange - currentSpeedRange) > 0.3) // Adjust the threshold as needed
            {
                if (polyline.Geopath.Count > 1)
                {
                    polyline.StrokeColor = GetPolylineColor(prevSpeedRange);
                    map.MapElements.Add(polyline);
                    polyline = new Polyline
                    {
                        StrokeWidth = 10
                    };
                }
            }

            polyline.Geopath.Add(new Location(location.Latitude, location.Longitude));
            prevSpeedRange = currentSpeedRange;
        }

        if (polyline.Geopath.Count > 1)
        {
            polyline.StrokeColor = GetPolylineColor(prevSpeedRange);
            map.MapElements.Add(polyline);
        }
    }

    private double CalculateSpeedRange(double minSpeed, double maxSpeed, double speed)
    {
        double speedRange = maxSpeed - minSpeed;
        double level1Threshold = minSpeed + (speedRange * 0.25);
        double level2Threshold = minSpeed + (speedRange * 0.5);
        double level3Threshold = minSpeed + (speedRange * 0.75);

        if (speed < level1Threshold)
            return 1;
        else if (speed < level2Threshold)
            return 2;
        else if (speed < level3Threshold)
            return 3;
        else
            return 4;
    }

    private Color GetPolylineColor(double speedRange)
    {
        switch (speedRange)
        {
            case 1:
                return Color.FromArgb("#61d52a");
            case 2:
                return Color.FromArgb("#feef0e");
            case 3:
                return Color.FromArgb("#ff9003");
            case 4:
                return Color.FromArgb("#fc2123");
            default:
                return Colors.White;
        }
    }
}