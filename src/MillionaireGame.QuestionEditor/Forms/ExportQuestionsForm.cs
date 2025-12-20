using MillionaireGame.Core.Helpers;

namespace MillionaireGame.QuestionEditor.Forms;

/// <summary>
/// Form for exporting questions to CSV files
/// </summary>
public partial class ExportQuestionsForm : Form
{
    private readonly string _connectionString;

    public ExportQuestionsForm(string connectionString)
    {
        InitializeComponent();
        IconHelper.ApplyToForm(this);
        _connectionString = connectionString;
    }

    private void btnBrowse_Click(object sender, EventArgs e)
    {
        using var saveFileDialog = new SaveFileDialog
        {
            Filter = "CSV Files (*.csv)|*.csv|All Files (*.*)|*.*",
            Title = "Save Questions as CSV",
            DefaultExt = "csv"
        };

        if (saveFileDialog.ShowDialog() == DialogResult.OK)
        {
            txtFilePath.Text = saveFileDialog.FileName;
        }
    }

    private async void btnExport_Click(object sender, EventArgs e)
    {
        if (string.IsNullOrWhiteSpace(txtFilePath.Text))
        {
            MessageBox.Show("Please select a location to save the CSV file.", "Validation Error",
                MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }

        try
        {
            // TODO: Implement CSV export logic
            MessageBox.Show("Export functionality will be implemented soon.", "Info",
                MessageBoxButtons.OK, MessageBoxIcon.Information);
            
            // DialogResult = DialogResult.OK;
            // Close();
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Error exporting questions: {ex.Message}", "Error",
                MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    private void btnCancel_Click(object sender, EventArgs e)
    {
        DialogResult = DialogResult.Cancel;
        Close();
    }
}
