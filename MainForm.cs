using System.ComponentModel;
using System.Security.Cryptography.X509Certificates;
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

    TextBox? errorsBox;

    TextBox? xmppMessaging;
    TextBox? arduinoMessaging;
    ComboBox? devices;
    ComboBox? commands;
    Button? sendIRButton;


    ComboBox? shortcuts;
    Button? sendShortcutButton;

    //event EventHandler<
    public BindingList<String> DeviceList { get; } = new BindingList<String>();

    public BindingList<IRData> CommandList { get; } = new BindingList<IRData>();

    public BindingList<KeyValuePair<String, String>> ShortcutsList { get; } = new BindingList<KeyValuePair<String, String>>();


    public event EventHandler<IRSendArgs>? SendIRCommand;

    public event EventHandler<String> SendKeysCommand;

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

        if (shortcuts != null)
        {
            var source = new BindingSource();
            source.DataSource = ShortcutsList;
            shortcuts.DataSource = source;
            shortcuts.ValueMember = "Key";
            shortcuts.DisplayMember = "Value";
        }

        if (sendIRButton != null && devices != null && commands != null)
        {
            sendIRButton.Click += (sender, eargs) =>
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

        if (sendShortcutButton != null && shortcuts != null)
        {
            sendShortcutButton.Click += (sender, eargs) =>
            {
                var keys2send = shortcuts.SelectedValue;
                if (keys2send != null)
                {
                    SendKeysCommand?.Invoke(this, keys2send.ToString());
                }
            };
        }
    }

    public void UpdateStatus(String status)
    {
        if (statusBox != null)
        {
            statusBox.Text = status;
        }
    }

    public void UpdateErrors(String errors)
    {
        if (errorsBox != null)
        {
            errorsBox.Text = errors;
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
