namespace MediaController;

public partial class MainForm : Form
{
    public MainForm()
    {
        InitializeComponent();
    }

    void updateStatus(String text)
    {
        if (statusBox != null)
        {
            statusBox.Text = text;
        }
    }
}
