namespace MillionaireGame.QuestionEditor;

internal static class Program
{
    /// <summary>
    ///  The main entry point for the question editor application.
    /// </summary>
    [STAThread]
    static void Main()
    {
        // Initialize application
        Application.SetHighDpiMode(HighDpiMode.SystemAware);
        Application.EnableVisualStyles();
        Application.SetCompatibleTextRenderingDefault(false);

        // TODO: Create and show Question Editor form when implemented
        MessageBox.Show(
            "Question Editor\n\n" +
            "This component is currently being migrated from VB.NET to C#.\n" +
            "Please use the main game application for now.",
            "Question Editor - Coming Soon",
            MessageBoxButtons.OK,
            MessageBoxIcon.Information);
    }
}
