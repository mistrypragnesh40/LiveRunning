namespace LiveRunning.Models;

public class LocationUpdatedEventArgs
{
    Location location;

    public LocationUpdatedEventArgs(double lon,
        double lat,
        double speed,
        double alt)
    {
        var loc = new Location();
        loc.Longitude = lon;
        loc.Latitude = lat;
        loc.Speed = speed;
        loc.Altitude = alt;

        this.location = loc;
    }

    public Location Location
    {
        get { return location; }
    } 
}