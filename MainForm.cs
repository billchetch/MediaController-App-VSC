using System.ComponentModel;
using Chetch.Arduino.Devices.Infrared;

namespace MediaController;

public partial class MainForm : Form
{

    public class IRSendArgs : EventArgs
    {
        public String? Device;
        public String? Command;
    }

    TextBox? statusBox;
    TextBox? xmppMessaging;
    TextBox? arduinoMessaging;
    ComboBox? devices;
    ComboBox? commands;
    Button? sendButton;

    //event EventHandler<
    public BindingList<String> DeviceList { get; } = new BindingList<String>();

    public BindingList<IRData> CommandList { get; } = new BindingList<IRData>();


    public event EventHandler<IRSendArgs>? SendIRCommand;

    public MainForm()
    {
        InitializeComponent();

        if (devices != null)
        {
            var source = new BindingSource();
            source.DataSource = DeviceList;
            devices.DataSource = source;
        }

        if (commands != null)
        {
            var source = new BindingSource();
            source.DataSource = CommandList;
            commands.DataSource = source;
            commands.ValueMember = "CommandAlias";
            commands.DisplayMember = "Description";
        }

        if (sendButton != null && devices != null && commands != null)
        {
            sendButton.Click += (sender, eargs) =>
            {
                var selectedDevice = devices.SelectedValue;
                var selectedCommandString = commands.SelectedValue;
                if (selectedDevice != null && selectedCommandString != null)
                {
                    var sendArgs = new IRSendArgs();
                    sendArgs.Device = selectedDevice.ToString();
                    sendArgs.Command = selectedCommandString.ToString();
                    SendIRCommand?.Invoke(this, sendArgs);
                }
            };
        }
    }

    public void UpdateStatus(String text)
    {
        if (statusBox != null)
        {
            statusBox.Text = text;
        }
    }

    public void ShowError(Exception e)
    {
        ShowError(e.Message);
    }
    
    public void ShowError(String errorMessage)
    {
        MessageBox.Show(errorMessage, "Error!", MessageBoxButtons.OK, MessageBoxIcon.Error);
    }
}
