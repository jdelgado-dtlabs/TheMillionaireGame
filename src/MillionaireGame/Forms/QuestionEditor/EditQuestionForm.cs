using MillionaireGame.Core.Database;
using MillionaireGame.Core.Models;
using MillionaireGame.Core.Helpers;

namespace MillionaireGame.Forms.QuestionEditor;

/// <summary>
/// Form for editing existing regular questions
/// </summary>
public partial class EditQuestionForm : Form
{
    private readonly string _connectionString;
    private readonly QuestionRepository _repository;
    private readonly Question _question;

    public EditQuestionForm(string connectionString, Question question)
    {
        InitializeComponent();
        IconHelper.ApplyToForm(this);
        _connectionString = connectionString;
        _repository = new QuestionRepository(_connectionString);
        _question = question;
    }

    private void EditQuestionForm_Load(object sender, EventArgs e)
    {
        // Populate level dropdown
        for (int i = 1; i <= 15; i++)
        {
            cmbLevel.Items.Add(i);
        }

        // Populate difficulty dropdown
        cmbDifficultyType.Items.AddRange(new object[] { "Easy", "Medium", "Hard" });

        // Load question data
        txtQuestion.Text = _question.QuestionText;
        
        // Determine which answer is correct and load accordingly
        if (_question.CorrectAnswer == "A")
        {
            txtCorrectAnswer.Text = _question.AnswerA;
            txtWrong1.Text = _question.AnswerB;
            txtWrong2.Text = _question.AnswerC;
            txtWrong3.Text = _question.AnswerD;
        }
        else if (_question.CorrectAnswer == "B")
        {
            txtCorrectAnswer.Text = _question.AnswerB;
            txtWrong1.Text = _question.AnswerA;
            txtWrong2.Text = _question.AnswerC;
            txtWrong3.Text = _question.AnswerD;
        }
        else if (_question.CorrectAnswer == "C")
        {
            txtCorrectAnswer.Text = _question.AnswerC;
            txtWrong1.Text = _question.AnswerA;
            txtWrong2.Text = _question.AnswerB;
            txtWrong3.Text = _question.AnswerD;
        }
        else // D
        {
            txtCorrectAnswer.Text = _question.AnswerD;
            txtWrong1.Text = _question.AnswerA;
            txtWrong2.Text = _question.AnswerB;
            txtWrong3.Text = _question.AnswerC;
        }
        
        cmbLevel.SelectedItem = _question.Level;
        cmbDifficultyType.SelectedItem = _question.DifficultyType.ToString();
        chkUsed.Checked = _question.Used;
    }

    private async void btnSave_Click(object sender, EventArgs e)
    {
        if (!ValidateInput())
            return;

        try
        {
            _question.QuestionText = txtQuestion.Text.Trim();
            
            // Keep the same correct answer position, just update the values
            _question.AnswerA = txtWrong1.Text.Trim();
            _question.AnswerB = txtWrong2.Text.Trim();
            _question.AnswerC = txtCorrectAnswer.Text.Trim();
            _question.AnswerD = txtWrong3.Text.Trim();
            _question.CorrectAnswer = "C";  // Always C for simplicity
            
            _question.Level = (int)(cmbLevel.SelectedItem ?? 1);
            _question.DifficultyType = (DifficultyType)Enum.Parse(typeof(DifficultyType), cmbDifficultyType.SelectedItem!.ToString() ?? "Specific");
            _question.Used = chkUsed.Checked;

            await _repository.UpdateQuestionAsync(_question);
            
            MessageBox.Show("Question updated successfully!", "Success",
                MessageBoxButtons.OK, MessageBoxIcon.Information);
            
            DialogResult = DialogResult.OK;
            Close();
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Error updating question: {ex.Message}", "Error",
                MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    private bool ValidateInput()
    {
        if (string.IsNullOrWhiteSpace(txtQuestion.Text))
        {
            MessageBox.Show("Please enter a question.", "Validation Error",
                MessageBoxButtons.OK, MessageBoxIcon.Warning);
            txtQuestion.Focus();
            return false;
        }

        if (string.IsNullOrWhiteSpace(txtCorrectAnswer.Text))
        {
            MessageBox.Show("Please enter the correct answer.", "Validation Error",
                MessageBoxButtons.OK, MessageBoxIcon.Warning);
            txtCorrectAnswer.Focus();
            return false;
        }

        if (string.IsNullOrWhiteSpace(txtWrong1.Text))
        {
            MessageBox.Show("Please enter wrong answer 1.", "Validation Error",
                MessageBoxButtons.OK, MessageBoxIcon.Warning);
            txtWrong1.Focus();
            return false;
        }

        if (string.IsNullOrWhiteSpace(txtWrong2.Text))
        {
            MessageBox.Show("Please enter wrong answer 2.", "Validation Error",
                MessageBoxButtons.OK, MessageBoxIcon.Warning);
            txtWrong2.Focus();
            return false;
        }

        if (string.IsNullOrWhiteSpace(txtWrong3.Text))
        {
            MessageBox.Show("Please enter wrong answer 3.", "Validation Error",
                MessageBoxButtons.OK, MessageBoxIcon.Warning);
            txtWrong3.Focus();
            return false;
        }

        if (cmbLevel.SelectedItem == null)
        {
            MessageBox.Show("Please select a level.", "Validation Error",
                MessageBoxButtons.OK, MessageBoxIcon.Warning);
            cmbLevel.Focus();
            return false;
        }

        if (cmbDifficultyType.SelectedItem == null)
        {
            MessageBox.Show("Please select a difficulty type.", "Validation Error",
                MessageBoxButtons.OK, MessageBoxIcon.Warning);
            cmbDifficultyType.Focus();
            return false;
        }

        return true;
    }

    private void btnCancel_Click(object sender, EventArgs e)
    {
        DialogResult = DialogResult.Cancel;
        Close();
    }
}
