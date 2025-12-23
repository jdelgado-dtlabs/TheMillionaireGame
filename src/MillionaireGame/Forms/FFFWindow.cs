namespace MillionaireGame.Forms;

/// <summary>
/// Window for managing Fastest Finger First rounds
/// </summary>
public partial class FFFWindow : Form
{
    private readonly string _serverUrl;
    
    public FFFWindow(string serverUrl = "http://localhost:5278")
    {
        InitializeComponent();
        _serverUrl = serverUrl;
    }
    
    /// <summary>
    /// Gets the FFF control panel for external access
    /// </summary>
    public FFFControlPanel ControlPanel => fffControlPanel;
    
    private async void FFFWindow_Load(object sender, EventArgs e)
    {
        try
        {
            // Initialize SignalR client
            await fffControlPanel.InitializeClientAsync(_serverUrl);
            
            // Load available questions
            await fffControlPanel.LoadQuestionsAsync();
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Error initializing FFF window: {ex.Message}",
                "Initialization Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }
    
    private void FFFWindow_FormClosing(object sender, FormClosingEventArgs e)
    {
        // Don't actually close, just hide
        if (e.CloseReason == CloseReason.UserClosing)
        {
            e.Cancel = true;
            Hide();
        }
    }
}
