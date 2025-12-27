using MillionaireGame.Core.Helpers;
using MillionaireGame.Core.Database;
using System.Text;

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

        btnExport.Enabled = false;
        btnCancel.Enabled = false;
        Cursor = Cursors.WaitCursor;

        try
        {
            var repository = new QuestionRepository(_connectionString);
            var questions = await repository.GetAllQuestionsAsync();

            if (questions.Count == 0)
            {
                MessageBox.Show("No questions found in the database.", "Export Warning",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            // Build CSV content
            var csv = new StringBuilder();
            csv.AppendLine("Question,AnswerA,AnswerB,AnswerC,AnswerD,CorrectAnswer,Level,Explanation,ATA_A,ATA_B,ATA_C,ATA_D");

            foreach (var q in questions)
            {
                csv.AppendLine(
                    $"\"{EscapeCsv(q.QuestionText)}\"," +
                    $"\"{EscapeCsv(q.AnswerA)}\"," +
                    $"\"{EscapeCsv(q.AnswerB)}\"," +
                    $"\"{EscapeCsv(q.AnswerC)}\"," +
                    $"\"{EscapeCsv(q.AnswerD)}\"," +
                    $"{q.CorrectAnswer}," +
                    $"{q.Level}," +
                    $"\"{EscapeCsv(q.Explanation)}\"," +
                    $"{q.ATAPercentageA?.ToString() ?? ""}," +
                    $"{q.ATAPercentageB?.ToString() ?? ""}," +
                    $"{q.ATAPercentageC?.ToString() ?? ""}," +
                    $"{q.ATAPercentageD?.ToString() ?? ""}");
            }

            // Write to file
            await File.WriteAllTextAsync(txtFilePath.Text, csv.ToString());

            MessageBox.Show($"Successfully exported {questions.Count} questions to CSV.", "Export Complete",
                MessageBoxButtons.OK, MessageBoxIcon.Information);
            
            DialogResult = DialogResult.OK;
            Close();
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Error exporting questions: {ex.Message}", "Error",
                MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
        finally
        {
            btnExport.Enabled = true;
            btnCancel.Enabled = true;
            Cursor = Cursors.Default;
        }
    }

    private void btnCancel_Click(object sender, EventArgs e)
    {
        DialogResult = DialogResult.Cancel;
        Close();
    }

    private string EscapeCsv(string? text)
    {
        if (string.IsNullOrEmpty(text)) return string.Empty;
        return text.Replace("\"", "\"\"");
    }
}
