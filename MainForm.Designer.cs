namespace MediaController;

partial class MainForm
{
    TextBox statusBox; 
    TextBox lastCommandReceived;
    TextBox lastCommandSent;
    
    /// <summary>
    ///  Required designer variable.
    /// </summary>
    private System.ComponentModel.IContainer components = null;

    /// <summary>
    ///  Clean up any resources being used.
    /// </summary>
    /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
    protected override void Dispose(bool disposing)
    {
        if (disposing && (components != null))
        {
            components.Dispose();
        }
        base.Dispose(disposing);
    }

    #region Windows Form Designer generated code

    /// <summary>
    ///  Required method for Designer support - do not modify
    ///  the contents of this method with the code editor.
    /// </summary>
    private void InitializeComponent()
    {
        this.components = new System.ComponentModel.Container();
        this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
        this.ClientSize = new System.Drawing.Size(800, 450);
        this.Text = "Media Controller";

        statusBox = new TextBox();
        statusBox.Location = new Point(30, 50);
        statusBox.Width = 600;
        statusBox.Height = 80;
        statusBox.Visible = true;
        statusBox.Text = "--";
        statusBox.ReadOnly = true;
        statusBox.Multiline = true;
        statusBox.Enabled = false;
        Controls.Add(statusBox);

        lastCommandReceived = new TextBox();
        lastCommandReceived.Location = new Point(30, 80);
        lastCommandReceived.Width = 150;
        lastCommandReceived.Visible = true;
        lastCommandReceived.Text = "No commands received";
        lastCommandReceived.ReadOnly = true;
        lastCommandReceived.Enabled = false;
        Controls.Add(lastCommandReceived);

        lastCommandSent = new TextBox();
        lastCommandSent.Location = new Point(30, 110);
        lastCommandSent.Width = 150;
        lastCommandSent.Visible = true;
        lastCommandSent.Text = "No commands sent";
        lastCommandSent.ReadOnly = true;
        lastCommandSent.Enabled = false;
        Controls.Add(lastCommandSent);

    }

    #endregion
}
