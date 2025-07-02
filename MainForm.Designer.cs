namespace MediaController;

partial class MainForm
{
    
    
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
        this.ClientSize = new System.Drawing.Size(450, 500);
        this.Text = "Media Controller";

        int xPos = 35;
        int yPos = 15;
        int yPad1 = 0;
        int yPad2 = 12;

        #region Status Box
        var lbl = new Label();
        lbl.Font = new Font(Label.DefaultFont, FontStyle.Bold);
        lbl.Text = "Status";
        lbl.AutoSize = true;
        lbl.Location = new Point(xPos, yPos);
        Controls.Add(lbl);

        statusBox = new TextBox();
        statusBox.Location = new Point(xPos, lbl.Location.Y + lbl.Height + yPad1);
        statusBox.Width = this.ClientSize.Width - 2*xPos;
        statusBox.Height = 80;
        statusBox.Visible = true;
        statusBox.Text = "--";
        //statusBox.BorderStyle = BorderStyle.FixedSingle;
        statusBox.ReadOnly = true;
        statusBox.Multiline = true;
        statusBox.Enabled = false;
        statusBox.ScrollBars = ScrollBars.Vertical;
        Controls.Add(statusBox);
        #endregion

        #region Errors Box
        lbl = new Label();
        lbl.Font = new Font(Label.DefaultFont, FontStyle.Bold);
        lbl.Text = "Errors";
        lbl.AutoSize = true;
        lbl.Location = new Point(xPos, statusBox.Location.Y + statusBox.Height + yPad2);
        Controls.Add(lbl);

        errorsBox = new TextBox();
        errorsBox.Location = new Point(xPos, lbl.Location.Y + lbl.Height + yPad1);
        errorsBox.Width = statusBox.Width;
        errorsBox.Height = 40;
        errorsBox.Visible = true;
        errorsBox.Text = "--";
        //statusBox.BorderStyle = BorderStyle.FixedSingle;
        errorsBox.ReadOnly = true;
        errorsBox.Multiline = true;
        errorsBox.Enabled = false;
        errorsBox.ScrollBars = ScrollBars.Vertical;
        Controls.Add(errorsBox);
        #endregion

        #region Client Messaging
        lbl = new Label();
        lbl.Font = new Font(Label.DefaultFont, FontStyle.Bold);
        lbl.Text = "Client Messaging";
        lbl.AutoSize = true;
        lbl.Location = new Point(xPos, errorsBox.Location.Y + errorsBox.Height + yPad2);
        Controls.Add(lbl);

        clientMessaging = new TextBox();
        clientMessaging.Location = new Point(xPos, lbl.Location.Y + lbl.Height + yPad1);
        clientMessaging.Width = errorsBox.Width;
        clientMessaging.Height = 40;
        clientMessaging.Multiline = true;
        clientMessaging.Visible = true;
        clientMessaging.Text = "No commands received or sent";
        clientMessaging.ReadOnly = true;
        clientMessaging.Enabled = false;
        Controls.Add(clientMessaging);
        #endregion

        #region Arduino Messaging
        lbl = new Label();
        lbl.Font = new Font(Label.DefaultFont, FontStyle.Bold);
        lbl.AutoSize = true;
        lbl.Text = "Arduino Messaging";
        lbl.Location = new Point(xPos, clientMessaging.Location.Y + clientMessaging.Height + yPad2);
        Controls.Add(lbl);

        arduinoMessaging = new TextBox();
        arduinoMessaging.Location = new Point(xPos, lbl.Location.Y + lbl.Height + yPad1);
        arduinoMessaging.Width = clientMessaging.Width;
        arduinoMessaging.Height = 40;
        arduinoMessaging.Multiline = true;
        arduinoMessaging.Visible = true;
        arduinoMessaging.Text = "No commands received or sent";
        arduinoMessaging.ReadOnly = true;
        arduinoMessaging.Enabled = false;
        Controls.Add(arduinoMessaging);
        #endregion

        lbl = new Label();
        lbl.Font = new Font(Label.DefaultFont, FontStyle.Bold);
        lbl.AutoSize = true;
        lbl.Text = "Send IR Command";
        lbl.Location = new Point(xPos, arduinoMessaging.Location.Y + arduinoMessaging.Height + yPad2);
        Controls.Add(lbl);

        devices = new ComboBox();
        devices.Location = new Point(xPos, lbl.Location.Y + lbl.Height + 4);
        devices.Width = 100;
        devices.Enabled = true;
        devices.DropDownStyle = ComboBoxStyle.DropDownList;
        Controls.Add(devices);

        commands = new ComboBox();
        commands.Location = new Point(devices.Location.X + devices.Width + 8, devices.Location.Y);
        commands.Width = 200;
        commands.Enabled = true;
        commands.DropDownStyle = ComboBoxStyle.DropDownList;
        Controls.Add(commands);

        sendIRButton = new Button();
        sendIRButton.Location = new Point(commands.Location.X + commands.Width + 8, commands.Location.Y);
        sendIRButton.Text = "Send";
        sendIRButton.Enabled = true;
        Controls.Add(sendIRButton);

        lbl = new Label();
        lbl.Font = new Font(Label.DefaultFont, FontStyle.Bold);
        lbl.AutoSize = true;
        lbl.Text = "Media Player Shortcuts";
        lbl.Location = new Point(xPos, devices.Location.Y + devices.Height + yPad2);
        Controls.Add(lbl);

        shortcuts = new ComboBox();
        shortcuts.Location = new Point(xPos, lbl.Location.Y + lbl.Height + 4);
        shortcuts.Width = 200;
        shortcuts.Enabled = true;
        shortcuts.DropDownStyle = ComboBoxStyle.DropDownList;
        Controls.Add(shortcuts);
        
        sendShortcutButton = new Button();
        sendShortcutButton.Location = new Point(shortcuts.Location.X + shortcuts.Width + 8, shortcuts.Location.Y);
        sendShortcutButton.Text = "Send";
        sendShortcutButton.Enabled = true;
        Controls.Add(sendShortcutButton);
    }

    #endregion
}
