namespace MillionaireGame.FFFGuest;

internal static class Program
{
    /// <summary>
    ///  The main entry point for the Fastest Finger First guest application.
    /// </summary>
    [STAThread]
    static void Main()
    {
        // Initialize application
        Application.SetHighDpiMode(HighDpiMode.SystemAware);
        Application.EnableVisualStyles();
        Application.SetCompatibleTextRenderingDefault(false);

        // TODO: Create and show FFF Guest form when implemented
        MessageBox.Show(
            "Fastest Finger First - Guest Application\n\n" +
            "This component is currently being migrated from VB.NET to C#.\n" +
            "Please use the main game application for now.",
            "FFF Guest - Coming Soon",
            MessageBoxButtons.OK,
            MessageBoxIcon.Information);
    }
}
