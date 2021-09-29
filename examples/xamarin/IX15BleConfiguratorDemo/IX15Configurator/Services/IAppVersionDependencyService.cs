namespace IX15Configurator.Services
{
    public interface IAppVersionDependencyService
    {
        /// <summary>
        /// Returns the name (title) of the application.
        /// </summary>
        /// <returns>The name (title) of the application.</returns>
        string GetName();

        /// <summary>
        /// Returns the version of the application.
        /// </summary>
        /// <returns>The version of the application.</returns>
        string GetVersion();

        /// <summary>
        /// Returns the build ID of the application.
        /// </summary>
        /// <returns>The build ID of the application.</returns>
        string GetBuild();
    }
}
