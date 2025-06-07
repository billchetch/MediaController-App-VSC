namespace MediaController;

public partial class MainForm : Form
{
    public MainForm()
    {
        InitializeComponent();
    }

    public void UpdateStatus(String text)
    {
        if (statusBox != null)
        {
            statusBox.Text = text;
        }
    }
}
