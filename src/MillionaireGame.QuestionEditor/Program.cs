using MillionaireGame.QuestionEditor.Forms;

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

        // Launch Question Editor
        Application.Run(new QuestionEditorMainForm());
    }
}
