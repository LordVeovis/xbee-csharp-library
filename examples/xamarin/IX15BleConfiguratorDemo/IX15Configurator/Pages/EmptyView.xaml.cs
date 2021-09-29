using IX15Configurator.Utils;

using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace IX15Configurator.Pages
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class EmptyView : ContentView
    {
        /// <summary>
        /// Class constructor. Instantiates a new <c>EmptyView</c> object.
        /// </summary>
        public EmptyView()
        {
            InitializeComponent();
            UpdateFilterLabel();
            AppPreferences.SearchFilterChanged += (s, e) =>
            {
                UpdateFilterLabel();
            };
        }

        /// <summary>
        /// Updates the visibility of the 'filter' label.
        /// </summary>
        private void UpdateFilterLabel()
        {
            filterLabel.IsVisible = !string.IsNullOrEmpty(AppPreferences.GetSearchFilterText());
        }
    }
}