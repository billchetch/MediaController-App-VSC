using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Text.Json;
using Chetch.Windows;
using Chetch.ChetchXMPP;
using Chetch.Utilities;
using Chetch.Messaging;
using Microsoft.Extensions.Configuration;
using Chetch.Arduino;
using System.Reflection.Metadata.Ecma335;
using System.Text;
using Chetch.Arduino.Devices;
using System.Threading.Tasks;
using Chetch.Arduino.Devices.Infrared;
using Chetch.Arduino.Devices.Displays;
using XmppDotNet.Xmpp.Jingle;
using Chetch.Messaging;

namespace MediaController;

public class MediaControllerContext : SysTrayApplicationContext
{
    #region Window Externs
    [DllImport("user32.dll")]
    private static extern bool SetForegroundWindow(IntPtr hWnd);

    [DllImport("user32.dll")]
    private static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);

    [DllImport("user32.dll")]
    private static extern void SwitchToThisWindow(IntPtr hWnd, bool fAltTab);

    [DllImport("user32.dll")]
    static extern IntPtr GetForegroundWindow();

    [DllImport("user32.dll")]
    static extern int GetWindowText(IntPtr hWnd, StringBuilder text, int count);
    #endregion

    #region Constants, static methods and fields
    
    const byte OLED_ID = 10;
    const byte LG_INSIDE_ID = 11;
    const byte LG_OUTSIDE_ID = 12;

    static String? PathToMediaPlayer = "C:/Program Files/Plex/Plex Media Player/PlexMediaPlayer.exe";
    static String? MediaPlayerProcessName = "PlexMediaPlayer";

    static Process? MediaPlayerProcess;
    static Object lockMediaPlayerProcess = new object();

    static Dictionary<String, String>? MediaPlayerShortcuts;

    static Process? GetMediaPlayerProcess()
    {
        var processes = Process.GetProcessesByName(MediaPlayerProcessName);
        if (processes.Length >= 1)
        {
            return processes[0];
        }
        else
        {
            return null;
        }
    }

    static bool IsMediaPlayerRunning()
    {
        if (MediaPlayerProcess != null && !MediaPlayerProcess.HasExited)
        {
            return true;
        }
        else
        {
            lock (lockMediaPlayerProcess)
            {
                MediaPlayerProcess = GetMediaPlayerProcess();
            }
            return MediaPlayerProcess != null && !MediaPlayerProcess.HasExited;
        }
    }

    static void StartMediaPlayerProcess()
    {
        if (IsMediaPlayerRunning())
        {
            throw new Exception("Media player already running");
        }
        else
        {
            lock (lockMediaPlayerProcess)
            {
                MediaPlayerProcess = Process.Start(PathToMediaPlayer);
                Thread.Sleep(1000);
            }
            if (MediaPlayerProcess != null)
            {
                SetMediaPlayerAsForeground();
            }
        }
    }

    static void SetMediaPlayerAsForeground()
    {
        if (IsMediaPlayerRunning())
        {
            if (GetForegroundWindow() != MediaPlayerProcess.MainWindowHandle)
            {
                SwitchToThisWindow(MediaPlayerProcess.MainWindowHandle, true);
                SetForegroundWindow(MediaPlayerProcess.MainWindowHandle);
            }
        }
        else
        {
            throw new Exception("Meida Player is not running");
        }
    }
#endregion

    #region Properties
    public String StatusReport
    {
        get
        {
            try
            {
                StringBuilder builder = new StringBuilder();
                if (cnn != null)
                {
                    builder.AppendFormat("{0}: {1}", cnn.Username, cnn.CurrentState.ToString());
                    builder.AppendLine();
                }
                else
                {
                    builder.AppendLine("XMPP not configured");
                }

                if (btsc != null)
                {
                    builder.AppendFormat("Bluetooth on {0} connected: {1}", btsc.PortName, btsc.IsConnected);
                    builder.AppendLine();
                }
                else
                {
                    builder.AppendLine("Bluetooth not configured");
                }
                if (board != null)
                {
                    if (board.IsReady)
                    {
                        builder.AppendFormat("Arduino Board {0} is ready", board.SID);
                        builder.AppendLine();
                        builder.AppendFormat("Free memory: {0}", board.FreeMemory);
                    }
                    else
                    {
                        builder.AppendFormat("Arduino Board {0} is not ready", board.SID);
                    }
                    builder.AppendLine();
                }
                if (lgCommands != null)
                {
                    builder.AppendFormat("Loaded {0} LG IRCommands", lgCommands.Count);
                    builder.AppendLine();
                }
                if (MediaPlayerProcessName != null)
                {
                    builder.AppendFormat("Media Player Process ({0}): {1}", MediaPlayerProcessName, IsMediaPlayerRunning() ? "Running" : "Not Found");
                    builder.AppendLine();
                }
                
                return builder.ToString();
            }
            catch (Exception e)
            {
                return e.Message;
            }
        }
    }

    public String ErrorReport
    {
        get
        {
            StringBuilder builder = new StringBuilder();
            if (errors.Count > 0)
            {
                foreach (var kv in errors)
                {
                    builder.AppendFormat("{0} : {1} ", kv.Key, kv.Value);
                    builder.AppendLine();
                }
            }
            else
            {
                builder.AppendLine("No errors");
            }
            return builder.ToString();
        }
    }


    public Chetch.Messaging.Message? LastClientMessageSent;

    public Chetch.Messaging.Message? LastClientMessageReceived;

    public String ClientMessagingReport
    {
        get
        {
            StringBuilder builder = new StringBuilder();
            String dstr;
            String desc;
            if (LastClientMessageSent != null)
            {
                dstr = LastClientMessageSent.Created.ToString("yyyyMMddHHmmss");
                desc = LastClientMessageSent.Type + " to " + LastClientMessageSent.Target;
                builder.AppendFormat("Last sent on {0}: {1}", dstr, desc);
            } else {
                builder.Append("No client messages sent");
            }
            builder.AppendLine();
            if (LastClientMessageReceived != null)
            {
                dstr = LastClientMessageReceived.Created.ToString("yyyyMMddHHmmss");
                desc = LastClientMessageReceived.Type + " from " + LastClientMessageReceived.Sender;
                builder.AppendFormat("Last received on {0}: {1}", dstr, desc);
            } else {
                builder.Append("No client messages received");
            }
            return builder.ToString();
        }
    }

    public ArduinoMessage? LastArduinoMessageSent;

    public ArduinoMessage? LastArduinoMessageReceived;

    public String ArduinoMessagingReport
    {
        get
        {
            StringBuilder builder = new StringBuilder();
            String dstr;
            String desc;
            if (LastArduinoMessageSent != null)
            {
                dstr = LastArduinoMessageSent.Created.ToString("yyyyMMddHHmmss");
                desc = LastArduinoMessageSent.Type + " to " + LastArduinoMessageSent.Target;
                builder.AppendFormat("Last sent on {0}: {1}", dstr, desc);
            } else {
                builder.Append("No arduino messages sent");
            }
            builder.AppendLine();
            if (LastArduinoMessageReceived != null)
            {
                dstr = LastArduinoMessageReceived.Created.ToString("yyyyMMddHHmmss");
                desc = LastArduinoMessageReceived.Type + " from " + LastArduinoMessageReceived.Sender;
                builder.AppendFormat("Last received on {0}: {1}", dstr, desc);
            } else {
                builder.Append("No arduino messages received");
            }
            return builder.ToString();
        }
    }

    protected bool IsMainFormActive => Form.ActiveForm == mainForm && mainForm != null;
    #endregion

    #region Fields
    Dictionary<String, String> errors = new Dictionary<String, String>();

    ChetchXMPPConnection? cnn = null;

    BluetoothSerialConnection? btsc = null;

    Dictionary<String, IRData>? lgCommands;
    Dictionary<String, String>? lgCommandSequences;

    MainForm? mainForm;

    System.Timers.Timer timer = new System.Timers.Timer();

    OLEDTextDisplay oled = new OLEDTextDisplay(OLED_ID, "oled");
    IRTransmitter lgInside = new IRTransmitter(LG_INSIDE_ID, "lgin", "LG Inside");
    IRTransmitter lgOutside = new IRTransmitter(LG_OUTSIDE_ID, "lgin", "LG Outside");

    ArduinoBoard board = new ArduinoBoard("mc");
    #endregion

    #region Init and End
    override protected async void InitializeContext(bool asSysTray)
    {
        NotifyIconPath = "icon-white.ico";
        NotifyIconText = "Media Controller";

        base.InitializeContext(asSysTray);

        //Set up timer
        timer.AutoReset = true;
        timer.Interval = 5000;
        timer.Elapsed += (sender, eargs) =>
        {
            if (IsMainFormActive && mainForm != null)
            {
                mainForm.UpdateStatus(StatusReport);
                mainForm.UpdateErrors(ErrorReport);
                mainForm.UpdateClientMessaging(ClientMessagingReport);
                mainForm.UpdateArduinoMessaging(ArduinoMessagingReport);
            }
        };
        timer.Start();

        //Configure Media Player settings
        try
        {
            if (Config != null && Config.GetSection("MediaPlayer").Exists())
            {
                var mps = Config.GetSection("MediaPlayer");
                PathToMediaPlayer = mps["PathToMediaPlayer"];
                MediaPlayerProcessName = mps["MediaPlayerProcessName"];

                MediaPlayerShortcuts = mps.GetSection("Shortcuts").Get<Dictionary<String, String>>();
            }
            else
            {
                //use default values
            }

            //In case it's already running when this fires up
            MediaPlayerProcess = GetMediaPlayerProcess();
        }
        catch (Exception e)
        {
            errors["Media Player"] = e.Message;
        }
        
        //Load IR Commands
        try
        {
            if (Config != null && Config.GetSection("IRCommands").Exists())
            {
                var irc = Config.GetSection("IRCommands");
                var uri = irc["URI"];
                if (uri == null)
                {
                    throw new Exception("No URI found for IR Commands");
                }

                String deviceName = "LG Home Theater";
                uri = String.Format(uri, deviceName);
                //Console.WriteLine("Getting commands for device {0} using URI: {1}", deviceName, uri);
                lgCommands = await getIRCommands(deviceName, uri);
                lgCommandSequences = irc.GetSection("Sequences").Get<Dictionary<String, String>>();
            }
            else
            {
                throw new Exception("Cannot find IRCommands entry in app config");
            }
        }
        catch (Exception e)
        {
            //Status update failed to get IR Commands
            errors["IRCommands"] = e.Message;
        }

        //Connect XMPP client
        try
        {
            if (Config != null && Config.GetSection("XMPP").Exists())
            {
                var creds = Config.GetSection("XMPP");
                var un = creds["Username"];
                var pw = creds["Password"];
                if (un == null || pw == null) throw new Exception("Username and/or password cannot be null");

                cnn = new ChetchXMPPConnection(un.ToString(), pw.ToString());
                try
                {
                    _ = cnn.ConnectAsync();
                }
                catch (Exception e)
                {
                    Console.WriteLine(e.Message);
                }
                cnn.MessageReceived += (sender, eargs) =>
                {
                    LastClientMessageReceived = eargs.Message;
                    var response = new Chetch.Messaging.Message();
                    if (handleClientMessage(eargs.Message, response))
                    {
                        cnn.SendMessageAsync(response);
                        LastClientMessageSent = response;
                    }

                    if (IsMainFormActive)
                    {
                        mainForm.UpdateClientMessaging(ClientMessagingReport);
                    }
                };
            }
            else
            {
                //Config does not contain xmpp
            }
        }
        catch (Exception e)
        {
            errors["XMPP"] = e.Message;
        }

        //Connect Bluetooth client
        try
        {
            if (Config != null && Config.GetSection("Bluetooth").Exists())
            {
                var bconfig = Config.GetSection("Bluetooth");
                var devicePath = bconfig["PathToDevice"];
                if (String.IsNullOrEmpty(devicePath))
                {
                    throw new Exception("No path to device supplied");
                }

                btsc = new BluetoothSerialConnection(devicePath);
                Frame btInFrame = new Frame(Frame.FrameSchema.MEDIUM_SIMPLE_CHECKSUM, MessageEncoding.JSON);
                Frame btOutFrame = new Frame(Frame.FrameSchema.MEDIUM_SIMPLE_CHECKSUM, MessageEncoding.JSON);
                btInFrame.FrameComplete += (sender, payload) =>
                {
                    try
                    {
                        var json = Chetch.Utilities.Convert.ToString(payload);
                        var message = Chetch.Messaging.Message.Deserialize(json);
                        LastClientMessageReceived = message;
                        var response = new Chetch.Messaging.Message();
                        if (handleClientMessage(message, response))
                        {
                            json = response.Serialize();
                            btOutFrame.Payload = Chetch.Utilities.Convert.ToBytes(json);
                            btsc.SendData(btOutFrame.GetBytes().ToArray());
                            LastClientMessageSent = response;
                        }

                        if (IsMainFormActive)
                        {
                            mainForm.UpdateClientMessaging(ClientMessagingReport);
                        }
                    }
                    catch (Exception e)
                    {
                        errors["Bluetooth Comms"] = e.Message;
                    }
                };

                btsc.DataReceived += (sender, data) =>
                {
                    try
                    {
                        btInFrame.Add(data);
                    }
                    catch (Exception e)
                    {
                        errors["Bluetooth Comms"] = e.Message;
                    }
                };


                try
                {
                    btsc.Connect();
                }
                catch (Exception e)
                {
                    Console.WriteLine(e.Message);
                }
            }
            else
            {
                //Config does not contain bluetooth
            }
        }
        catch (Exception e)
        {
            errors["Bluetooth"] = e.Message;
        }

        //Connect Arduino board
        try
        {
            if (Config != null && Config.GetSection("Arduino").Exists())
            {
                var cnnConfig = Config.GetSection("Arduino").GetSection("Connection");
                try
                {
                    //Add devices

                    //Connection
                    var path2device = cnnConfig["PathToDevice"];
                    if (path2device == null) throw new Exception("No path to device found");
                    int baudRate = System.Convert.ToInt32(cnnConfig["BaudRate"]);
                    board.Connection = new ArduinoSerialConnection(path2device.ToString(), baudRate);

                    //Event handlers
                    board.Ready += (sender, ready) =>
                    {
                        if (IsMainFormActive)
                        {
                            mainForm.UpdateStatus(StatusReport);
                        }
                    };

                    //Handle errors either thrown or generated from the board by just logging them
                    board.ErrorReceived += (sender, errorArgs) =>
                    {
                        errors["Arduino"] = errorArgs.ErrorMessage.ToString();
                    };
                    board.ExceptionThrown += (sender, errorArgs) =>
                    {
                        errors["Arduino"] = errorArgs.GetException().Message;
                    };

                    board.MessageReceived += (sender, message) =>
                    {
                        LastArduinoMessageReceived = message;
                        if (IsMainFormActive)
                        {
                            mainForm.UpdateArduinoMessaging(ArduinoMessagingReport);
                        }
                    };

                    board.MessageSent += (sender, message) =>
                    {
                        LastArduinoMessageSent = message;
                        if (IsMainFormActive)
                        {
                            mainForm.UpdateArduinoMessaging(ArduinoMessagingReport);
                        }
                    };

                    //Now begin
                    board.Begin();
                }
                catch (Exception e)
                {
                    errors["Arduino"] = e.Message;
                }
            }
            else
            {
                throw new Exception("Cannot find Arduino setion in app config");
            }
        }
        catch (Exception e)
        {
            errors["Arduino"] = e.Message;
        }
    }

    protected override void ExitThreadCore()
    {
        try
        {
            cnn?.DisconnectAsync();

            board?.End();

            Thread.Sleep(1000);
        }
        catch { }
        ;

        base.ExitThreadCore();
    }
    #endregion

    #region Message handling
    private bool handleClientMessage(Chetch.Messaging.Message message, Chetch.Messaging.Message response)
    {
        return false;
    }
    #endregion

    #region Methods
    protected override Form CreateMainForm()
    {
        mainForm = new MainForm();

        mainForm.DeviceList.Add(lgInside.Name);
        mainForm.DeviceList.Add(lgOutside.Name);

        if (lgCommands != null)
        {
            foreach (var ird in lgCommands.Values)
            {
                mainForm.CommandList.Add(ird);
            }
        }
        if (lgCommandSequences != null)
        {
            foreach (var kv in lgCommandSequences)
            {
                var ird = new IRData(0, 0, 0);
                ird.CommandAlias = kv.Key;
                mainForm.CommandList.Add(ird);
            }
        }

        foreach (var kv in MediaPlayerShortcuts)
            mainForm.ShortcutsList.Add(kv);

        mainForm.SendIRCommand += (sender, eargs) =>
        {
            Console.WriteLine("{0} sending {1}", eargs.Device, eargs.Command);
            try
            {
                SendIRCommand(eargs.Device, eargs.Command);
            }
            catch (Exception e)
            {
                //mainForm.ShowError(e);
                if (IsMainFormActive)
                {
                    mainForm.UpdateErrors(ErrorReport);
                    mainForm.ShowError(e);
                }
            }
        };

        mainForm.SendKeysCommand += (Senders, keys2send) =>
        {
            Console.WriteLine("Sending {0} media player", keys2send);
            try
            {
                SendKeysToMediaPlayer(keys2send);
            }
            catch (Exception e)
            {
                //mainForm.ShowError(e);
                if (IsMainFormActive)
                {
                    mainForm.UpdateErrors(ErrorReport);
                    mainForm.ShowError(e);
                }
            }
        };

        mainForm.Activated += (sender, eargs) =>
        {
            mainForm.UpdateStatus(StatusReport);
            mainForm.UpdateErrors(ErrorReport);
        };

        return mainForm;
    }

    async Task<Dictionary<String, IRData>> getIRCommands(String deviceName, String uri)
    {
        //TODO: from here should be a func
        String filename = String.Format("ircommands.{0}.json", deviceName.ToLower().Replace(" ", "-"));
        String json = String.Empty;

        if (File.Exists(filename))
        {
            json = File.ReadAllText(filename);
        }
        else
        {
            await Task.Run(async () =>
            {
                HttpClient client = new HttpClient();
                var response = await client.GetAsync(uri);
                json = await response.Content.ReadAsStringAsync();

                if (!String.IsNullOrEmpty(json))
                {
                    File.WriteAllText(filename, json);
                }
            });
        }

        if (String.IsNullOrEmpty(json))
        {
            throw new Exception(String.Format("No commands found for device: {0}", deviceName));
        }
        var result = JsonSerializer.Deserialize<List<IRData>>(json);
        if (result == null) throw new Exception("No result from json deserialize");

        return result.ToDictionary(x => x.CommandAlias, x => x);
    }

    void SendIRCommand(String device, String command)
    {
        if (board == null)
        {
            throw new Exception("No arduino board");
        }

        IRTransmitter? transmitter = null;
        if (device == lgInside.Name)
        {
            transmitter = lgInside;
        }
        else if (device == lgOutside.Name)
        {
            transmitter = lgOutside;
        }

        if (transmitter == null)
        {
            throw new Exception(String.Format("Cannot find device {0}", device));
        }

        if (lgCommands.ContainsKey(command))
        {
            transmitter.Transmit(lgCommands[command]);
        }
        else if (lgCommandSequences.ContainsKey(command))
        {
            var sequenceOfCommands = lgCommandSequences[command].Split(",");
            List<IRData> sequence = new List<IRData>();
            foreach (var cmd in sequenceOfCommands)
            {
                if (lgCommands.ContainsKey(cmd))
                {
                    sequence.Add(lgCommands[cmd]);
                }
            }
            if (sequence.Count > 0)
            {
                transmitter.TransmitAsync(sequence, 1000);
            }
            else
            {
                throw new Exception(String.Format("{0} is not a valid sequence", lgCommandSequences[command]));
            }
        }
        else
        {
            throw new Exception(String.Format("Cannot find command {0} for device {1}", command, device));
        }

    }

    void SendKeysToMediaPlayer(String keys)
    {
        if (IsMediaPlayerRunning())
        {
            SetMediaPlayerAsForeground(); //ensure in foreground
        }
        else
        {
            StartMediaPlayerProcess();
        }

        SendKeys.Send(keys);
    }
    #endregion
}