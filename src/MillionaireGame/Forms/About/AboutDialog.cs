using System.Reflection;

namespace MillionaireGame.Forms.About;

public partial class AboutDialog : Form
{
    public AboutDialog()
    {
        InitializeComponent();
        LoadVersionInfo();
    }

    private void LoadVersionInfo()
    {
        var version = Assembly.GetExecutingAssembly().GetName().Version;
        lblVersion.Text = version?.ToString() ?? "1.0.0.0";
    }

    private void btnClose_Click(object sender, EventArgs e)
    {
        Close();
    }
}
