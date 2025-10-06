
using CoreLocation;
using LiveRunning.Models;
using LiveRunning.Services;
using LiveRunning.Services.Interface;
using Microsoft.Maui.Platform;
using UIKit;
[assembly: Dependency(typeof(LocationService))]
namespace LiveRunning.Services;

public class LocationService : ILocationService
{
    protected CLLocationManager locMgr;

        protected CLLocation returnedLoc;
        public event EventHandler<LocationUpdatedEventArgs> LocationUpdated = delegate { };

        public CLLocationManager LocMgr
        {
            get { return this.locMgr; }
        }

        public LocationService()
        {
            this.locMgr = new CLLocationManager();
            locMgr.Delegate = new LocationManagerDelegate(this);

            this.locMgr.PausesLocationUpdatesAutomatically = false;

            if (UIDevice.CurrentDevice.CheckSystemVersion(8, 0))
            {
                locMgr.RequestAlwaysAuthorization();
            }

            if (UIDevice.CurrentDevice.CheckSystemVersion(9, 0))
            {
                locMgr.AllowsBackgroundLocationUpdates = true;
            }
        }

        public void StartLocationUpdated()
        {
            if (CLLocationManager.LocationServicesEnabled)
            {
                //set the desired accuracy, in meters
                LocMgr.StartUpdatingLocation();
                LocMgr.DesiredAccuracy = CLLocation.AccurracyBestForNavigation;
            }
        }

        public void HandleLocationChanged(double longitude, double latitude, double speed, double altitude)
        {
            LocationUpdated(this, new LocationUpdatedEventArgs(longitude, latitude, speed, altitude));
        }

        public Location GetCurrentLocation()
        {
            if (!CLLocationManager.LocationServicesEnabled)
            {
                return null; // Location services are disabled
            }

            var loc = LocMgr.Location;
            if (loc?.Coordinate == null)
            {
                return null; // Location data is unavailable
            }

            var res = new Location
            {
                Longitude = loc.Coordinate.Longitude,
                Latitude = loc.Coordinate.Latitude,
                Accuracy = loc.HorizontalAccuracy,
                VerticalAccuracy = loc.VerticalAccuracy,
                Speed = loc.Speed,
                Altitude = loc.Altitude,
                Timestamp = loc.Timestamp.ToDateTime()
            };
            return res;
        }

        public void StopLocationUpdates()
        {
            LocMgr.StopUpdatingLocation();
        }

    }
    
    public class LocationManagerDelegate : CLLocationManagerDelegate
    {
        private readonly LocationService _locationService;
        public LocationManagerDelegate(LocationService locationService)
        {
            _locationService = locationService;
        }

        public override void AuthorizationChanged(CLLocationManager manager, CLAuthorizationStatus status)
        {
            Console.WriteLine("Authorization changed to: {0}", status);
        }

        public override void UpdatedLocation(CLLocationManager manager, CLLocation newLocation, CLLocation oldLocation)
        {
            _locationService.HandleLocationChanged(
                newLocation.Coordinate.Longitude,
                newLocation.Coordinate.Latitude,
                newLocation.Speed,
                newLocation.Altitude);

            
        }
}