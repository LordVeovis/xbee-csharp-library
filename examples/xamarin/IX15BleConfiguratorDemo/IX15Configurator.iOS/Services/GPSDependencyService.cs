using CoreLocation;
using IX15Configurator.iOS.Services;
using IX15Configurator.Services;

[assembly: Xamarin.Forms.Dependency(typeof(GPSDependencyService))]
namespace IX15Configurator.iOS.Services
{
    class GPSDependencyService : IGPSDependencyService
    {
        public bool IsGPSEnabled()
        {
            if (CLLocationManager.Status == CLAuthorizationStatus.Denied)
            {
                return false;
            }
            else
            {
                return true;
            }
        }
    }
}