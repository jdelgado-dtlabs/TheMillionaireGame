using System.Data;
using System.Data.SqlClient;
using MillionaireGame.Core.Database;
using MillionaireGame.Core.Models;
using MillionaireGame.Core.Settings;
using MillionaireGame.Core.Helpers;

namespace MillionaireGame.Forms.QuestionEditor;

/// <summary>
/// Main form for the Question Editor application
/// </summary>
public partial class QuestionEditorMainForm : Form
{
    private readonly SqlSettingsManager _sqlSettings;
    private readonly QuestionRepository _questionRepository;
    private readonly FFFQuestionRepository _fffQuestionRepository;
    private string _connectionString;

    public QuestionEditorMainForm()
    {
        InitializeComponent();
        IconHelper.ApplyToForm(this);
        
        _sqlSettings = new SqlSettingsManager();
        _sqlSettings.LoadSettings();
        _connectionString = _sqlSettings.Settings.GetConnectionString("dbMillionaire");
        
        _questionRepository = new QuestionRepository(_connectionString);
        _fffQuestionRepository = new FFFQuestionRepository(_connectionString);
    }

    private async void QuestionEditorMainForm_Load(object sender, EventArgs e)
    {
        await RefreshQuestions();
        
        // Wire up double-click event handlers
        dgvQuestions.CellDoubleClick += DgvQuestions_CellDoubleClick;
        dgvFFFQuestions.CellDoubleClick += DgvFFFQuestions_CellDoubleClick;
    }

    private async void DgvQuestions_CellDoubleClick(object? sender, DataGridViewCellEventArgs e)
    {
        // Ignore header row clicks
        if (e.RowIndex < 0)
            return;

        var question = dgvQuestions.Rows[e.RowIndex].DataBoundItem as Question;
        if (question != null)
        {
            var editForm = new EditQuestionForm(_connectionString, question);
            if (editForm.ShowDialog() == DialogResult.OK)
            {
                await RefreshQuestions();
            }
        }
    }

    private async void DgvFFFQuestions_CellDoubleClick(object? sender, DataGridViewCellEventArgs e)
    {
        // Ignore header row clicks
        if (e.RowIndex < 0)
            return;

        var question = dgvFFFQuestions.Rows[e.RowIndex].DataBoundItem as FFFQuestion;
        if (question != null)
        {
            var editForm = new EditFFFQuestionForm(_connectionString, question);
            if (editForm.ShowDialog() == DialogResult.OK)
            {
                await RefreshQuestions();
            }
        }
    }

    private async Task RefreshQuestions()
    {
        try
        {
            // Load regular questions
            var questions = await _questionRepository.GetAllQuestionsAsync();
            dgvQuestions.DataSource = questions;
            
            // Configure columns with proper sizing
            // Fixed width columns (sized to header)
            if (dgvQuestions.Columns["Id"] != null)
            {
                dgvQuestions.Columns["Id"].AutoSizeMode = DataGridViewAutoSizeColumnMode.ColumnHeader;
            }
            if (dgvQuestions.Columns["Level"] != null)
            {
                dgvQuestions.Columns["Level"].AutoSizeMode = DataGridViewAutoSizeColumnMode.ColumnHeader;
            }
            if (dgvQuestions.Columns["DifficultyType"] != null)
            {
                dgvQuestions.Columns["DifficultyType"].AutoSizeMode = DataGridViewAutoSizeColumnMode.ColumnHeader;
                dgvQuestions.Columns["DifficultyType"].HeaderText = "Difficulty";
            }
            if (dgvQuestions.Columns["CorrectAnswer"] != null)
            {
                dgvQuestions.Columns["CorrectAnswer"].AutoSizeMode = DataGridViewAutoSizeColumnMode.ColumnHeader;
                dgvQuestions.Columns["CorrectAnswer"].HeaderText = "Correct";
            }
            if (dgvQuestions.Columns["Used"] != null)
            {
                dgvQuestions.Columns["Used"].AutoSizeMode = DataGridViewAutoSizeColumnMode.ColumnHeader;
            }
            
            // Auto-fill columns (expand evenly)
            if (dgvQuestions.Columns["QuestionText"] != null)
            {
                dgvQuestions.Columns["QuestionText"].AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;
                dgvQuestions.Columns["QuestionText"].HeaderText = "Question";
            }
            if (dgvQuestions.Columns["AnswerA"] != null)
            {
                dgvQuestions.Columns["AnswerA"].AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;
                dgvQuestions.Columns["AnswerA"].HeaderText = "Answer A";
            }
            if (dgvQuestions.Columns["AnswerB"] != null)
            {
                dgvQuestions.Columns["AnswerB"].AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;
                dgvQuestions.Columns["AnswerB"].HeaderText = "Answer B";
            }
            if (dgvQuestions.Columns["AnswerC"] != null)
            {
                dgvQuestions.Columns["AnswerC"].AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;
                dgvQuestions.Columns["AnswerC"].HeaderText = "Answer C";
            }
            if (dgvQuestions.Columns["AnswerD"] != null)
            {
                dgvQuestions.Columns["AnswerD"].AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;
                dgvQuestions.Columns["AnswerD"].HeaderText = "Answer D";
            }
            
            // Hide unwanted columns
            if (dgvQuestions.Columns["Answer1"] != null) dgvQuestions.Columns["Answer1"].Visible = false;
            if (dgvQuestions.Columns["Answer2"] != null) dgvQuestions.Columns["Answer2"].Visible = false;
            if (dgvQuestions.Columns["Answer3"] != null) dgvQuestions.Columns["Answer3"].Visible = false;
            if (dgvQuestions.Columns["Answer4"] != null) dgvQuestions.Columns["Answer4"].Visible = false;
            if (dgvQuestions.Columns["Note"] != null) dgvQuestions.Columns["Note"].Visible = false;
            if (dgvQuestions.Columns["Explanation"] != null) dgvQuestions.Columns["Explanation"].Visible = false;
            if (dgvQuestions.Columns["LevelRange"] != null) dgvQuestions.Columns["LevelRange"].Visible = false;
            if (dgvQuestions.Columns["ATAPercentageA"] != null) dgvQuestions.Columns["ATAPercentageA"].Visible = false;
            if (dgvQuestions.Columns["ATAPercentageB"] != null) dgvQuestions.Columns["ATAPercentageB"].Visible = false;
            if (dgvQuestions.Columns["ATAPercentageC"] != null) dgvQuestions.Columns["ATAPercentageC"].Visible = false;
            if (dgvQuestions.Columns["ATAPercentageD"] != null) dgvQuestions.Columns["ATAPercentageD"].Visible = false;

            lblTotalQuestions.Text = $"Total Questions: {questions.Count}";

            // Load FFF questions
            var fffQuestions = await _fffQuestionRepository.GetAllQuestionsAsync();
            dgvFFFQuestions.DataSource = fffQuestions;
            
            // Configure FFF columns
            if (dgvFFFQuestions.Columns["Id"] != null)
            {
                dgvFFFQuestions.Columns["Id"].AutoSizeMode = DataGridViewAutoSizeColumnMode.ColumnHeader;
            }
            if (dgvFFFQuestions.Columns["QuestionText"] != null)
            {
                dgvFFFQuestions.Columns["QuestionText"].AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;
                dgvFFFQuestions.Columns["QuestionText"].HeaderText = "Question";
            }
            if (dgvFFFQuestions.Columns["AnswerA"] != null)
            {
                dgvFFFQuestions.Columns["AnswerA"].AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;
                dgvFFFQuestions.Columns["AnswerA"].HeaderText = "A";
            }
            if (dgvFFFQuestions.Columns["AnswerB"] != null)
            {
                dgvFFFQuestions.Columns["AnswerB"].AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;
                dgvFFFQuestions.Columns["AnswerB"].HeaderText = "B";
            }
            if (dgvFFFQuestions.Columns["AnswerC"] != null)
            {
                dgvFFFQuestions.Columns["AnswerC"].AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;
                dgvFFFQuestions.Columns["AnswerC"].HeaderText = "C";
            }
            if (dgvFFFQuestions.Columns["AnswerD"] != null)
            {
                dgvFFFQuestions.Columns["AnswerD"].AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;
                dgvFFFQuestions.Columns["AnswerD"].HeaderText = "D";
            }
            if (dgvFFFQuestions.Columns["CorrectOrder"] != null)
            {
                dgvFFFQuestions.Columns["CorrectOrder"].AutoSizeMode = DataGridViewAutoSizeColumnMode.ColumnHeader;
                dgvFFFQuestions.Columns["CorrectOrder"].HeaderText = "Order";
            }
            if (dgvFFFQuestions.Columns["Used"] != null)
            {
                dgvFFFQuestions.Columns["Used"].AutoSizeMode = DataGridViewAutoSizeColumnMode.ColumnHeader;
            }
            
            // Hide unwanted FFF columns
            if (dgvFFFQuestions.Columns["Answer1"] != null) dgvFFFQuestions.Columns["Answer1"].Visible = false;
            if (dgvFFFQuestions.Columns["Answer2"] != null) dgvFFFQuestions.Columns["Answer2"].Visible = false;
            if (dgvFFFQuestions.Columns["Answer3"] != null) dgvFFFQuestions.Columns["Answer3"].Visible = false;
            if (dgvFFFQuestions.Columns["Answer4"] != null) dgvFFFQuestions.Columns["Answer4"].Visible = false;

            lblTotalFFFQuestions.Text = $"Total FFF Questions: {fffQuestions.Count}";
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Error loading questions: {ex.Message}\n\nStack Trace:\n{ex.StackTrace}", "Error",
                MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    private void btnAdd_Click(object sender, EventArgs e)
    {
        var addForm = new AddQuestionForm(_connectionString);
        if (addForm.ShowDialog() == DialogResult.OK)
        {
            _ = RefreshQuestions();
        }
    }

    private async void btnEdit_Click(object sender, EventArgs e)
    {
        if (tabControl.SelectedTab == tabRegularQuestions)
        {
            if (dgvQuestions.SelectedRows.Count == 0)
            {
                MessageBox.Show("Please select a question to edit.", "No Selection",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            var question = dgvQuestions.SelectedRows[0].DataBoundItem as Question;
            if (question != null)
            {
                var editForm = new EditQuestionForm(_connectionString, question);
                if (editForm.ShowDialog() == DialogResult.OK)
                {
                    await RefreshQuestions();
                }
            }
        }
        else
        {
            if (dgvFFFQuestions.SelectedRows.Count == 0)
            {
                MessageBox.Show("Please select an FFF question to edit.", "No Selection",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            var question = dgvFFFQuestions.SelectedRows[0].DataBoundItem as FFFQuestion;
            if (question != null)
            {
                var editForm = new EditFFFQuestionForm(_connectionString, question);
                if (editForm.ShowDialog() == DialogResult.OK)
                {
                    await RefreshQuestions();
                }
            }
        }
    }

    private async void btnDelete_Click(object sender, EventArgs e)
    {
        if (tabControl.SelectedTab == tabRegularQuestions)
        {
            if (dgvQuestions.SelectedRows.Count == 0)
            {
                MessageBox.Show("Please select a question to delete.", "No Selection",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            var question = dgvQuestions.SelectedRows[0].DataBoundItem as Question;
            if (question != null)
            {
                var result = MessageBox.Show(
                    $"Are you sure you want to delete this question?\n\n{question.QuestionText}",
                    "Confirm Delete",
                    MessageBoxButtons.YesNo,
                    MessageBoxIcon.Warning);

                if (result == DialogResult.Yes)
                {
                    try
                    {
                        await _questionRepository.DeleteQuestionAsync(question.Id);
                        await RefreshQuestions();
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Error deleting question: {ex.Message}", "Error",
                            MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
            }
        }
        else
        {
            if (dgvFFFQuestions.SelectedRows.Count == 0)
            {
                MessageBox.Show("Please select an FFF question to delete.", "No Selection",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            var question = dgvFFFQuestions.SelectedRows[0].DataBoundItem as FFFQuestion;
            if (question != null)
            {
                var result = MessageBox.Show(
                    $"Are you sure you want to delete this FFF question?\n\n{question.QuestionText}",
                    "Confirm Delete",
                    MessageBoxButtons.YesNo,
                    MessageBoxIcon.Warning);

                if (result == DialogResult.Yes)
                {
                    try
                    {
                        await _fffQuestionRepository.DeleteQuestionAsync(question.Id);
                        await RefreshQuestions();
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Error deleting question: {ex.Message}", "Error",
                            MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
            }
        }
    }

    private async void btnResetUsed_Click(object sender, EventArgs e)
    {
        var result = MessageBox.Show(
            "This will mark all questions as unused. Are you sure?",
            "Reset All Questions",
            MessageBoxButtons.YesNo,
            MessageBoxIcon.Question);

        if (result == DialogResult.Yes)
        {
            try
            {
                await _questionRepository.ResetAllQuestionsAsync();
                await _fffQuestionRepository.ResetAllQuestionsAsync();
                await RefreshQuestions();
                MessageBox.Show("All questions have been reset to unused.", "Success",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error resetting questions: {ex.Message}", "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }

    private void btnImport_Click(object sender, EventArgs e)
    {
        var importForm = new ImportQuestionsForm(_connectionString);
        if (importForm.ShowDialog() == DialogResult.OK)
        {
            _ = RefreshQuestions();
        }
    }

    private void btnExport_Click(object sender, EventArgs e)
    {
        var exportForm = new ExportQuestionsForm(_connectionString);
        exportForm.ShowDialog();
    }

    private async void btnRefresh_Click(object sender, EventArgs e)
    {
        await RefreshQuestions();
    }
}
