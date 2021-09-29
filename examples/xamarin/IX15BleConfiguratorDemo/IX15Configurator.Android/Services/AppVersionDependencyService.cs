using Android.Content.PM;
using IX15Configurator.Droid.Services;
using IX15Configurator.Services;

[assembly: Xamarin.Forms.Dependency(typeof(AppVersionDependencyService))]
namespace IX15Configurator.Droid.Services
{
    class AppVersionDependencyService : IAppVersionDependencyService
    {
        public string GetName()
        {
            var context = global::Android.App.Application.Context;

            PackageManager manager = context.PackageManager;
            PackageInfo info = manager.GetPackageInfo(context.PackageName, 0);

            return info.ApplicationInfo.NonLocalizedLabel.ToString();
        }

        public string GetVersion()
        {
            var context = global::Android.App.Application.Context;

            PackageManager manager = context.PackageManager;
            PackageInfo info = manager.GetPackageInfo(context.PackageName, 0);

            return info.VersionName;
        }

        public string GetBuild()
        {
            var context = global::Android.App.Application.Context;

            PackageManager manager = context.PackageManager;
            PackageInfo info = manager.GetPackageInfo(context.PackageName, 0);

            return info.VersionCode.ToString();
        }
    }
}