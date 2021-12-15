namespace OneClickDesktop.Overseer
{
    /// <summary>
    /// Zamiennik statycznej konfiguracji
    /// [TODO][CONFIG] Wynieść do konfiguracji!
    /// </summary>
    public static class Configuration
    {
        public const string AppId = "Overseer";//To zmienne od instancji modułu oczywiście. Do testów ustawione na jakąś wartość.
        public const int ModelUpdateWait = 10000; // Ile czekamy między prośbami o model
    }
}