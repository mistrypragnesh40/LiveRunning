
using LiveRunning.Services.Interface;
using Microsoft.Maui.Platform;
namespace LiveRunning.Services;

[assembly: Dependency(typeof(LocationService))]
public class LocationService : ILocationService
{
    public Location GetCurrentLocation()
    {
        throw new NotImplementedException();
    }

    public void StartLocationUpdated()
    {
        throw new NotImplementedException();
    }

    public void StopLocationUpdates()
    {
        throw new NotImplementedException();
    }
}