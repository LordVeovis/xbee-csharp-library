using IX15Configurator.ViewModels;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace IX15Configurator.Pages
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class AboutPage : CustomContentPage
    {
        // Constants.
        private const int LOGO_GRID_HEIGHT_VERTICAL = 120;
        private const int LOGO_GRID_HEIGHT_HORIZONTAL = 70;

        private const int LOGO_HEIGHT_VERTICAL = 100;
        private const int LOGO_HEIGHT_HORIZONTAL = 50;

        // Variables.
        private double width;
        private double height;

        /// <summary>
        /// Class constructor. Instantiates a new <c>AboutPage</c> object.
        /// </summary>
        public AboutPage()
        {
            InitializeComponent();

            BindingContext = new AboutPageViewModel();

            // Register the back button action.
            if (EnableBackButtonOverride)
            {
                CustomBackButtonAction = () =>
                {
                    Device.BeginInvokeOnMainThread(async () =>
                    {
                        await Navigation.PopAsync(true);
                    });
                };
            }

            OnSizeAllocated(width, height);
        }

        protected override void OnSizeAllocated(double width, double height)
        {
            base.OnSizeAllocated(width, height);
            if (width != this.width || height != this.height)
            {
                this.width = width;
                this.height = height;
                if (width > height)
                {
                    LogoGrid.MinimumHeightRequest = LOGO_GRID_HEIGHT_HORIZONTAL;
                    LogoImage.HeightRequest = LOGO_HEIGHT_HORIZONTAL;
                }
                else
                {
                    LogoGrid.MinimumHeightRequest = LOGO_GRID_HEIGHT_VERTICAL;
                    LogoImage.HeightRequest = LOGO_HEIGHT_VERTICAL;
                }
            }
        }
    }
}