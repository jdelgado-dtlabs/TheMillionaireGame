using System.Reflection;
using System.Diagnostics;
using MillionaireGame.Core.Helpers;

namespace MillionaireGame.Forms.About;

public partial class AboutDialog : Form
{
    public AboutDialog()
    {
        InitializeComponent();
        IconHelper.ApplyToForm(this);
        LoadLogo();
        LoadVersionInfo();
    }

    private void LoadLogo()
    {
        try
        {
            var assembly = Assembly.GetExecutingAssembly();
            var resourceName = "MillionaireGame.lib.image.logo.png";
            using var stream = assembly.GetManifestResourceStream(resourceName);
            if (stream != null)
            {
                picLogo.Image = Image.FromStream(stream);
            }
        }
        catch (Exception ex)
        {
            if (Program.DebugMode)
            {
                Console.WriteLine($"[About] Failed to load logo: {ex.Message}");
            }
        }
    }

    private void LoadVersionInfo()
    {
        // Version format: Major.Minor-YYMM
        // TODO: Update this when implementing proper version control
        lblVersion.Text = "0.1-2512";
        
        // Get .NET runtime version dynamically
        var dotnetVersion = Environment.Version;
        lblBuildInfo.Text = $"Built in: C# using .NET {dotnetVersion.Major}.{dotnetVersion.Minor}";
    }

    private void lnkAuthor_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
    {
        try
        {
            Process.Start(new ProcessStartInfo
            {
                FileName = "https://github.com/jdelgado-dtlabs",
                UseShellExecute = true
            });
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Unable to open link: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    private void lnkOriginalAuthor_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
    {
        try
        {
            Process.Start(new ProcessStartInfo
            {
                FileName = "https://github.com/Macronair",
                UseShellExecute = true
            });
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Unable to open link: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    private void btnClose_Click(object sender, EventArgs e)
    {
        Close();
    }
}
