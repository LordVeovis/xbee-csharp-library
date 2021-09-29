using Android.App;
using Android.Content.PM;
using Android.Runtime;
using Android.Views;
using Android.OS;
using Plugin.Permissions;
using Acr.UserDialogs;
using Rg.Plugins.Popup;
using Rg.Plugins.Popup.Services;
using IX15Configurator.Pages;
using Xamarin.Forms;

namespace IX15Configurator.Droid
{
    [Activity(Label = "Digi IX15 Mobile", Icon = "@drawable/icon", Theme = "@style/MainTheme", MainLauncher = false, ScreenOrientation = ScreenOrientation.Portrait, ConfigurationChanges = ConfigChanges.ScreenSize | ConfigChanges.Orientation)]
    public class MainActivity : global::Xamarin.Forms.Platform.Android.FormsAppCompatActivity
    {
        protected override void OnCreate(Bundle savedInstanceState)
        {
            TabLayoutResource = Resource.Layout.Tabbar;
            ToolbarResource = Resource.Layout.Toolbar;

            base.OnCreate(savedInstanceState);

            UserDialogs.Init(this);
            Popup.Init(this);

            Xamarin.Essentials.Platform.Init(this, savedInstanceState);
            global::Xamarin.Forms.Forms.Init(this, savedInstanceState);
            LoadApplication(new IX15ConfiguratorApp());

            AndroidX.AppCompat.Widget.Toolbar toolbar = this.FindViewById<AndroidX.AppCompat.Widget.Toolbar>(Resource.Id.toolbar);
            SetSupportActionBar(toolbar);
        }

        public override bool OnOptionsItemSelected(IMenuItem item)
        {
            // check if the current item id 
            // is equals to the back button id
            if (item.ItemId == 16908332)
            {
                // retrieve the current xamarin forms page instance
                int pageCount = Xamarin.Forms.Application.Current.MainPage.Navigation.NavigationStack.Count;
                Page currentPage = Xamarin.Forms.Application.Current.MainPage.Navigation.NavigationStack[pageCount - 1];
                if (currentPage is CustomContentPage)
                {
                    // check if the page has subscribed to 
                    // the custom back button event
                    if (((CustomContentPage)currentPage)?.CustomBackButtonAction != null)
                    {
                        ((CustomContentPage)currentPage)?.CustomBackButtonAction.Invoke();
                        // and disable the default back button action
                        return false;
                    }
                }

                // if its not subscribed then go ahead 
                // with the default back button action
                return base.OnOptionsItemSelected(item);
            }
            else
            {
                // since its not the back button 
                //click, pass the event to the base
                return base.OnOptionsItemSelected(item);
            }
        }

        public override void OnBackPressed()
        {
            // this is not necessary, but in Android user 
            // has both Nav bar back button and
            // physical back button its safe 
            // to cover the both events

            // If there is a popup page open, close it.
            if (PopupNavigation.Instance.PopupStack.Count > 0 && Popup.SendBackPressed(base.OnBackPressed))
            {
                PopupNavigation.Instance.PopAsync();
                return;
            }

            // retrieve the current xamarin forms page instance
            int pageCount = Xamarin.Forms.Application.Current.MainPage.Navigation.NavigationStack.Count;
            Page currentPage = Xamarin.Forms.Application.Current.MainPage.Navigation.NavigationStack[pageCount - 1];
            if (currentPage is CustomContentPage)
            {
                // check if the page has subscribed to 
                // the custom back button event
                if (((CustomContentPage)currentPage)?.CustomBackButtonAction != null)
                {
                    ((CustomContentPage)currentPage)?.CustomBackButtonAction.Invoke();
                }
                else
                {
                    base.OnBackPressed();
                }
            }
            else
            {
                base.OnBackPressed();
            }
        }

        public override void OnRequestPermissionsResult(int requestCode, string[] permissions, [GeneratedEnum] Android.Content.PM.Permission[] grantResults)
        {
            PermissionsImplementation.Current.OnRequestPermissionsResult(requestCode, permissions, grantResults);
            base.OnRequestPermissionsResult(requestCode, permissions, grantResults);
        }
    }
}