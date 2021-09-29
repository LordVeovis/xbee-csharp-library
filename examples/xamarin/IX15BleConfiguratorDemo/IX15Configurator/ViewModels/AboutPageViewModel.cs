using IX15Configurator.Services;
using System.IO;
using System.Reflection;
using Xamarin.Forms;

namespace IX15Configurator.ViewModels
{
    class AboutPageViewModel : ViewModelBase
    {
        // Constants.
        private const string DIGI_URL = "http://www.digi.com";
        private const string TWITTER_URL = "https://twitter.com/digidotcom";
        private const string FACEBOOK_URL = "https://www.facebook.com/digi.international/";
        private const string LINKEDIN_URL = "https://www.linkedin.com/company/digi-international";
        private const string YOUTUBE_URL = "https://www.youtube.com/user/Digidotcom";
        private const string GITHUB_URL = "https://github.com/digidotcom";

        private const string FOLDER_LICENSES = "Resources.about.licenses";

        private const string FILE_AUTHORS = "Resources.about.authors.txt";

        private const string VERSION_LABEL = "Version: {0} - Build ID: {1}";

        // Variables.
        private string authors = "";
        private string licenses = "";

        IAppVersionDependencyService versionInterface;

        // Properties.
        /// <summary>
        /// List of open source projects used.
        /// </summary>
        public string Authors
        {
            get { return authors; }
            private set
            {
                authors = value;
                RaisePropertyChangedEvent("Authors");
            }
        }
        /// <summary>
        /// List of open source licenses with their contents.
        /// </summary>
        public string Licenses
        {
            get { return licenses; }
            private set
            {
                licenses = value;
                RaisePropertyChangedEvent("Licenses");
            }
        }
        /// <summary>
        /// Application name.
        /// </summary>
        public string AppName
        {
            get
            {
                if (versionInterface != null)
                    return versionInterface.GetName();
                return "";
            }
        }
        /// <summary>
        /// Application version.
        /// </summary>
        public string AppVersion
        {
            get
            {
                if (versionInterface != null)
                    return string.Format(VERSION_LABEL, versionInterface.GetVersion(), versionInterface.GetBuild());
                return "";
            }
        }
        /// <summary>
        /// URL of Digi's official site.
        /// </summary>
        public string URLDigi => DIGI_URL;
        /// <summary>
        /// URL of Digi's Twitter account.
        /// </summary>
        public string URLTwitter => TWITTER_URL;
        /// <summary>
        /// URL of Digi's Facebook account.
        /// </summary>
        public string URLFacebook => FACEBOOK_URL;
        /// <summary>
        /// URL of Digi's Linkedin account.
        /// </summary>
        public string URLLinkedin => LINKEDIN_URL;
        /// <summary>
        /// URL of Digi's Youtube account.
        /// </summary>
        public string URLYoutube => YOUTUBE_URL;
        /// <summary>
        /// URL of Digi's Github account.
        /// </summary>
        public string URLGithub => GITHUB_URL;

        /// <summary>
        /// Class constructor. Instantiates a new <c>AboutPageViewModel</c> 
        /// object.
        /// </summary>
        public AboutPageViewModel()
        {
            versionInterface = DependencyService.Get<IAppVersionDependencyService>(DependencyFetchTarget.NewInstance);

            GenerateAuthorsText();
            GenerateLicensesText();
        }

        /// <summary>
        /// Generates the text corresponding to the authors and sets it 
        /// to the <c>Authors</c> property.
        /// </summary>
        private void GenerateAuthorsText()
        {
            // Get the file from an assembly included with the project.
            string[] assemblyItems = Assembly.GetExecutingAssembly().GetManifestResourceNames();
            foreach (string fileName in assemblyItems)
            {
                if (fileName.Contains(FILE_AUTHORS))
                {
                    using (var streamReader = new StreamReader(Assembly.GetExecutingAssembly().GetManifestResourceStream(fileName)))
                    {
                        Authors = streamReader.ReadToEnd();
                    }
                    break;
                }
            }
        }

        /// <summary>
        /// Generates the text corresponding to the licenses and sets it 
        /// to the <c>Licenses</c> property.
        /// </summary>
        private void GenerateLicensesText()
        {
            string readLicenses = "";

            // Get the file from an assembly included with the project.
            string[] assemblyItems = Assembly.GetExecutingAssembly().GetManifestResourceNames();
            foreach (string fileName in assemblyItems)
            {
                if (fileName.Contains(FOLDER_LICENSES))
                {
                    using (var streamReader = new StreamReader(Assembly.GetExecutingAssembly().GetManifestResourceStream(fileName)))
                    {
                        readLicenses += "\r\n_______________________________________\r\n\r\n";
                        readLicenses += streamReader.ReadToEnd();
                    }
                }
            }
            Licenses = readLicenses;
        }
    }
}
