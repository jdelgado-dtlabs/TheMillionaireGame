using MillionaireGame.Core.Database;
using MillionaireGame.Core.Models;
using MillionaireGame.Core.Helpers;

namespace MillionaireGame.Forms.QuestionEditor;

/// <summary>
/// Form for adding new regular questions
/// </summary>
public partial class AddQuestionForm : Form
{
    private readonly string _connectionString;
    private readonly QuestionRepository _repository;

    public AddQuestionForm(string connectionString)
    {
        InitializeComponent();
        IconHelper.ApplyToForm(this);
        _connectionString = connectionString;
        _repository = new QuestionRepository(_connectionString);
    }

    private void AddQuestionForm_Load(object sender, EventArgs e)
    {
        // Populate level dropdown (1=Easy, 2=Medium, 3=Hard, 4=Million)
        for (int i = 1; i <= 4; i++)
        {
            cmbLevel.Items.Add(i);
        }
        cmbLevel.SelectedIndex = 0;
    }

    private async void btnSave_Click(object sender, EventArgs e)
    {
        if (!ValidateInput())
            return;

        try
        {
            var question = new Question
            {
                QuestionText = txtQuestion.Text.Trim(),
                AnswerA = txtWrong1.Text.Trim(),
                AnswerB = txtWrong2.Text.Trim(),
                AnswerC = txtCorrectAnswer.Text.Trim(),  // Correct answer as C
                AnswerD = txtWrong3.Text.Trim(),
                CorrectAnswer = "C",  // Always C for simplicity
                Level = (int)(cmbLevel.SelectedItem ?? 1),
                Used = false
            };

            await _repository.AddQuestionAsync(question);
            
            MessageBox.Show("Question added successfully!", "Success",
                MessageBoxButtons.OK, MessageBoxIcon.Information);
            
            DialogResult = DialogResult.OK;
            Close();
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Error adding question: {ex.Message}", "Error",
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
            MessageBox.Show("Please select a level (1=Easy, 2=Medium, 3=Hard, 4=Million).", "Validation Error",
                MessageBoxButtons.OK, MessageBoxIcon.Warning);
            cmbLevel.Focus();
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
