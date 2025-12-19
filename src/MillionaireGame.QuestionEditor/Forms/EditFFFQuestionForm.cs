using MillionaireGame.Core.Database;
using MillionaireGame.Core.Models;

namespace MillionaireGame.QuestionEditor.Forms;

/// <summary>
/// Form for editing existing FFF questions
/// </summary>
public partial class EditFFFQuestionForm : Form
{
    private readonly string _connectionString;
    private readonly FFFQuestionRepository _repository;
    private readonly FFFQuestion _question;

    public EditFFFQuestionForm(string connectionString, FFFQuestion question)
    {
        InitializeComponent();
        _connectionString = connectionString;
        _repository = new FFFQuestionRepository(_connectionString);
        _question = question;
    }

    private void EditFFFQuestionForm_Load(object sender, EventArgs e)
    {
        // Load question data
        txtQuestion.Text = _question.QuestionText;
        txtAnswer1.Text = _question.Answer1;
        txtAnswer2.Text = _question.Answer2;
        txtAnswer3.Text = _question.Answer3;
        txtAnswer4.Text = _question.Answer4;
        txtCorrectOrder.Text = _question.CorrectOrder;
        chkUsed.Checked = _question.Used;
    }

    private async void btnSave_Click(object sender, EventArgs e)
    {
        if (!ValidateInput())
            return;

        try
        {
            _question.QuestionText = txtQuestion.Text.Trim();
            _question.Answer1 = txtAnswer1.Text.Trim();
            _question.Answer2 = txtAnswer2.Text.Trim();
            _question.Answer3 = txtAnswer3.Text.Trim();
            _question.Answer4 = txtAnswer4.Text.Trim();
            _question.CorrectOrder = txtCorrectOrder.Text.Trim();
            _question.Used = chkUsed.Checked;

            await _repository.UpdateQuestionAsync(_question);
            
            MessageBox.Show("FFF Question updated successfully!", "Success",
                MessageBoxButtons.OK, MessageBoxIcon.Information);
            
            DialogResult = DialogResult.OK;
            Close();
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Error updating FFF question: {ex.Message}", "Error",
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

        if (string.IsNullOrWhiteSpace(txtAnswer1.Text) || string.IsNullOrWhiteSpace(txtAnswer2.Text) ||
            string.IsNullOrWhiteSpace(txtAnswer3.Text) || string.IsNullOrWhiteSpace(txtAnswer4.Text))
        {
            MessageBox.Show("Please enter all four answers.", "Validation Error",
                MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return false;
        }

        if (string.IsNullOrWhiteSpace(txtCorrectOrder.Text))
        {
            MessageBox.Show("Please enter the correct order (e.g., 3,1,4,2).", "Validation Error",
                MessageBoxButtons.OK, MessageBoxIcon.Warning);
            txtCorrectOrder.Focus();
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
