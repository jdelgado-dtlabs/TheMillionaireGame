using MillionaireGame.Core.Helpers;

namespace MillionaireGame.QuestionEditor.Forms;

/// <summary>
/// Form for importing questions from CSV files
/// </summary>
public partial class ImportQuestionsForm : Form
{
    private readonly string _connectionString;

    public ImportQuestionsForm(string connectionString)
    {
        InitializeComponent();
        IconHelper.ApplyToForm(this);
        _connectionString = connectionString;
    }

    private void btnBrowse_Click(object sender, EventArgs e)
    {
        using var openFileDialog = new OpenFileDialog
        {
            Filter = "CSV Files (*.csv)|*.csv|All Files (*.*)|*.*",
            Title = "Select CSV File to Import"
        };

        if (openFileDialog.ShowDialog() == DialogResult.OK)
        {
            txtFilePath.Text = openFileDialog.FileName;
        }
    }

    private async void btnImport_Click(object sender, EventArgs e)
    {
        if (string.IsNullOrWhiteSpace(txtFilePath.Text))
        {
            MessageBox.Show("Please select a CSV file to import.", "Validation Error",
                MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }

        if (!File.Exists(txtFilePath.Text))
        {
            MessageBox.Show("The selected file does not exist.", "File Error",
                MessageBoxButtons.OK, MessageBoxIcon.Error);
            return;
        }

        try
        {
            // TODO: Implement CSV import logic
            MessageBox.Show("Import functionality will be implemented soon.", "Info",
                MessageBoxButtons.OK, MessageBoxIcon.Information);
            
            // DialogResult = DialogResult.OK;
            // Close();
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Error importing questions: {ex.Message}", "Error",
                MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    private void btnCancel_Click(object sender, EventArgs e)
    {
        DialogResult = DialogResult.Cancel;
        Close();
    }
}
