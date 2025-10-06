namespace LiveRunning.Services.Interface;

public interface ILocationService
{
    Location GetCurrentLocation();
    void StartLocationUpdated();
    void StopLocationUpdates();
}