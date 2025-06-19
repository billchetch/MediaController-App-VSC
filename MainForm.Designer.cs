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

        var lbl = new Label();
        lbl.Font = new Font(Label.DefaultFont, FontStyle.Bold);
        lbl.Text = "Status";
        lbl.AutoSize = true;
        lbl.Location = new Point(xPos, yPos);
        Controls.Add(lbl);

        statusBox = new TextBox();
        statusBox.Location = new Point(xPos, lbl.Location.Y + lbl.Height + yPad1);
        statusBox.Width = this.ClientSize.Width - 2*xPos;
        statusBox.Height = 100;
        statusBox.Visible = true;
        statusBox.Text = "--";
        //statusBox.BorderStyle = BorderStyle.FixedSingle;
        statusBox.ReadOnly = true;
        statusBox.Multiline = true;
        statusBox.Enabled = false;
        Controls.Add(statusBox);


        lbl = new Label();
        lbl.Font = new Font(Label.DefaultFont, FontStyle.Bold);
        lbl.Text = "XMPP Messaging";
        lbl.AutoSize = true;
        lbl.Location = new Point(xPos, statusBox.Location.Y + statusBox.Height + yPad2);
        Controls.Add(lbl);

        xmppMessaging = new TextBox();
        xmppMessaging.Location = new Point(xPos, lbl.Location.Y + lbl.Height + yPad1);
        xmppMessaging.Width = statusBox.Width;
        xmppMessaging.Height = 40;
        xmppMessaging.Multiline = true;
        xmppMessaging.Visible = true;
        xmppMessaging.Text = "No commands received or sent";
        xmppMessaging.ReadOnly = true;
        xmppMessaging.Enabled = false;
        Controls.Add(xmppMessaging);

        lbl = new Label();
        lbl.Font = new Font(Label.DefaultFont, FontStyle.Bold);
        lbl.AutoSize = true;
        lbl.Text = "Arduino Messaging";
        lbl.Location = new Point(xPos, xmppMessaging.Location.Y + xmppMessaging.Height + yPad2);
        Controls.Add(lbl);

        arduinoMessaging = new TextBox();
        arduinoMessaging.Location = new Point(xPos, lbl.Location.Y + lbl.Height + yPad1);
        arduinoMessaging.Width = xmppMessaging.Width;
        arduinoMessaging.Height = 40;
        arduinoMessaging.Multiline = true;
        arduinoMessaging.Visible = true;
        arduinoMessaging.Text = "No commands received or sent";
        arduinoMessaging.ReadOnly = true;
        arduinoMessaging.Enabled = false;
        Controls.Add(arduinoMessaging);


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
