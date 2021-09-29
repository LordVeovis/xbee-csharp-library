using IX15Configurator.Pages;
using Xamarin.Forms;

namespace IX15Configurator
{
    public partial class IX15ConfiguratorApp : Application
    {
        public IX15ConfiguratorApp()
        {
            InitializeComponent();

            var mainPage = new DeviceListPage();
            MainPage = new NavigationPage(mainPage);

            ((NavigationPage)MainPage).BackgroundColor = Color.White;
            ((NavigationPage)MainPage).BarBackgroundColor = Color.FromHex("#3577B6");
            ((NavigationPage)MainPage).BarTextColor = Color.White;
        }

        protected override void OnStart()
        {
            // Handle when your app starts
        }

        protected override void OnSleep()
        {
            // Handle when your app sleeps
        }

        protected override void OnResume()
        {
            // Handle when your app resumes
        }
    }
}
