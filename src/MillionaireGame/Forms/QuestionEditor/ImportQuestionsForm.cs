using MillionaireGame.Core.Helpers;
using MillionaireGame.Core.Models;
using MillionaireGame.Core.Database;

namespace MillionaireGame.Forms.QuestionEditor;

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
            Title = "Select CSV File to Import",
            RestoreDirectory = true
        };

        // Run on separate thread to avoid modal deadlock that can freeze the UI
        DialogResult result = DialogResult.Cancel;
        var thread = new System.Threading.Thread(() =>
        {
            result = openFileDialog.ShowDialog();
        });
        thread.SetApartmentState(System.Threading.ApartmentState.STA);
        thread.Start();
        
        // Keep UI responsive while waiting
        while (thread.IsAlive)
        {
            Application.DoEvents();
            System.Threading.Thread.Sleep(10);
        }

        if (result == DialogResult.OK)
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

        btnImport.Enabled = false;
        btnCancel.Enabled = false;
        Cursor = Cursors.WaitCursor;

        try
        {
            var questions = await ParseCsvFileAsync(txtFilePath.Text);
            
            if (questions.Count == 0)
            {
                MessageBox.Show("No valid questions found in the CSV file.", "Import Warning",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            var repository = new QuestionRepository(_connectionString);
            int successCount = 0;
            int errorCount = 0;
            var errors = new List<string>();

            foreach (var question in questions)
            {
                try
                {
                    await repository.AddQuestionAsync(question);
                    successCount++;
                }
                catch (Exception ex)
                {
                    errorCount++;
                    errors.Add($"Line {questions.IndexOf(question) + 2}: {ex.Message}");
                }
            }

            var message = $"Import completed!\n\nSuccessfully imported: {successCount} questions";
            if (errorCount > 0)
            {
                message += $"\nFailed to import: {errorCount} questions";
                if (errors.Count <= 5)
                {
                    message += "\n\nErrors:\n" + string.Join("\n", errors);
                }
                else
                {
                    message += "\n\nShowing first 5 errors:\n" + string.Join("\n", errors.Take(5));
                }
            }

            MessageBox.Show(message, "Import Complete",
                MessageBoxButtons.OK, errorCount > 0 ? MessageBoxIcon.Warning : MessageBoxIcon.Information);
            
            if (successCount > 0)
            {
                DialogResult = DialogResult.OK;
                Close();
            }
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Error importing questions: {ex.Message}", "Error",
                MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
        finally
        {
            btnImport.Enabled = true;
            btnCancel.Enabled = true;
            Cursor = Cursors.Default;
        }
    }

    private void btnCancel_Click(object sender, EventArgs e)
    {
        DialogResult = DialogResult.Cancel;
        Close();
    }

    private async Task<List<Question>> ParseCsvFileAsync(string filePath)
    {
        var questions = new List<Question>();
        var lines = await File.ReadAllLinesAsync(filePath);

        // Skip header row
        for (int i = 1; i < lines.Length; i++)
        {
            try
            {
                var question = ParseCsvLine(lines[i]);
                if (question != null)
                {
                    questions.Add(question);
                }
            }
            catch (Exception ex)
            {
                // Log but continue processing other lines
                System.Diagnostics.Debug.WriteLine($"Error parsing line {i + 1}: {ex.Message}");
            }
        }

        return questions;
    }

    private Question? ParseCsvLine(string line)
    {
        if (string.IsNullOrWhiteSpace(line)) return null;

        var fields = SplitCsvLine(line);
        if (fields.Length < 7) return null; // Minimum: Question, A, B, C, D, Correct, Level

        var question = new Question
        {
            QuestionText = fields[0].Trim(),
            AnswerA = fields[1].Trim(),
            AnswerB = fields[2].Trim(),
            AnswerC = fields[3].Trim(),
            AnswerD = fields[4].Trim(),
            CorrectAnswer = fields[5].Trim().ToUpper(),
            Level = int.TryParse(fields[6], out var level) ? level : 1,
            DifficultyType = DifficultyType.Specific,
            Explanation = fields.Length > 7 ? fields[7].Trim() : string.Empty
        };

        // Validate
        if (string.IsNullOrWhiteSpace(question.QuestionText) ||
            !new[] { "A", "B", "C", "D" }.Contains(question.CorrectAnswer))
        {
            return null;
        }

        return question;
    }

    private string[] SplitCsvLine(string line)
    {
        var fields = new List<string>();
        bool inQuotes = false;
        var currentField = new System.Text.StringBuilder();

        for (int i = 0; i < line.Length; i++)
        {
            char c = line[i];

            if (c == '"')
            {
                inQuotes = !inQuotes;
            }
            else if (c == ',' && !inQuotes)
            {
                fields.Add(currentField.ToString());
                currentField.Clear();
            }
            else
            {
                currentField.Append(c);
            }
        }

        fields.Add(currentField.ToString());
        return fields.ToArray();
    }
}
