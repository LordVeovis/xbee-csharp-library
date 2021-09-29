using Android.Content;
using Android.Locations;
using IX15Configurator.Droid.Services;
using IX15Configurator.Services;

[assembly: Xamarin.Forms.Dependency(typeof(GPSDependencyService))]
namespace IX15Configurator.Droid.Services
{
    class GPSDependencyService : IGPSDependencyService
    {
        public bool IsGPSEnabled()
        {
            LocationManager locationManager = (LocationManager)Android.App.Application.Context.GetSystemService(Context.LocationService);
            return locationManager.IsProviderEnabled(LocationManager.GpsProvider);
        }
    }
}