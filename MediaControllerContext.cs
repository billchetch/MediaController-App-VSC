using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Text.Json;
using Chetch.Windows;
using Chetch.ChetchXMPP;
using Microsoft.Extensions.Configuration;
using Chetch.Arduino;
using System.Reflection.Metadata.Ecma335;
using System.Text;
using Chetch.Arduino.Devices;
using System.Threading.Tasks;
using Chetch.Arduino.Devices.Infrared;

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

    #region Constants and static methods and fields
    //const String PLEX_MEDIA_PLAYER_PROCESS_NAME = "PlexMediaPlayer";
    const String PLEX_MEDIA_PLAYER_PROCESS_NAME = "notepad";

    static String? PathToMediaPlayer = "C:/Program Files/Plex/Plex Media Player/PlexMediaPlayer.exe";

    static Process? GetMediaPlayerProcess()
    {
        var processes = Process.GetProcessesByName(PLEX_MEDIA_PLAYER_PROCESS_NAME);
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
        return GetMediaPlayerProcess() != null;
    }

    static bool ActivateMediaPlayer()
    {
        bool newProcess = false;
        var process = GetMediaPlayerProcess();

        if (process == null && PathToMediaPlayer != null)
        {
            String cmd = PathToMediaPlayer;
            process = Process.Start(cmd);
            newProcess = true;
            while (process.MainWindowHandle == IntPtr.Zero)
            {
                System.Threading.Thread.Sleep(1000);
            }
        }

        if (process == null)
        {
            throw new Exception("Media Player process failed to start");
        }
        if (process.MainWindowHandle == IntPtr.Zero)
        {
            throw new Exception("Media Player main window handle is zero");
        }

        SwitchToThisWindow(process.MainWindowHandle, true);
        SetForegroundWindow(process.MainWindowHandle);

        MediaPlayerActive = true;

        return !newProcess;
    }

    static bool MediaPlayerActive = false;
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
                if (board != null)
                {
                    if (board.IsReady)
                    {
                        builder.AppendFormat("{0} is ready", board.SID);
                        builder.AppendLine();
                        builder.AppendFormat("Free memory: {0}", board.FreeMemory);
                    }
                    else
                    {
                        builder.AppendFormat("{0} is not ready", board.SID);
                    }
                    builder.AppendLine();
                }
                if (lgCommands != null)
                {
                    builder.AppendFormat("Loaded {0} LG IRCommands", lgCommands.Count);
                    builder.AppendLine();
                }
                if (PLEX_MEDIA_PLAYER_PROCESS_NAME != null)
                {
                    builder.AppendFormat("Media Play Process ({0}): {1}", PLEX_MEDIA_PLAYER_PROCESS_NAME, IsMediaPlayerRunning() ? "Running" : "Not Found");
                    builder.AppendLine();
                }
                if (errors.Count > 0)
                {
                    builder.AppendLine("ERRORS!");
                    foreach (var kv in errors)
                    {
                        builder.AppendFormat("{0} : {1}", kv.Key, kv.Value);
                    }
                }
                else
                {
                    builder.AppendLine("No errors");
                }
                return builder.ToString();
            }
            catch (Exception e)
            {
                return e.Message;
            }
        }
    }
    #endregion

    #region Fields
    Dictionary<String, String> errors = new Dictionary<String, String>();

    ChetchXMPPConnection? cnn = null;

    Dictionary<String, IRData>? lgCommands;

    MainForm? mainForm;

    System.Timers.Timer timer = new System.Timers.Timer();

    ArduinoBoard board = new ArduinoBoard("mc");
    #endregion

    protected override Form CreateMainForm()
    {
        mainForm = new MainForm();

        mainForm.DeviceList.Add("LG Inside");
        mainForm.DeviceList.Add("LG Outside");

        if (lgCommands != null)
        {
            foreach (var kv in lgCommands)
            {
                var ird = kv.Value;
                String s = String.Format("{0} ({1})", kv.Key, ird.Protocol + "," + ird.Address + "," + ird.Command);
                mainForm.CommandList.Add(s);
            }
        }

        mainForm.SendIRCommand += (sender, eargs) =>
        {
            Console.WriteLine("{0} sending {1}", eargs.Device, eargs.CommandString);
            try
            {
                //SendIRCommand();
            }
            catch (Exception e)
            {
                //mainForm.ShowError(e);
                if (Form.ActiveForm == mainForm && mainForm != null)
                {
                    mainForm.UpdateStatus(StatusReport);
                    mainForm.ShowError(e);
                }
            }

        };

        mainForm.Activated += (sender, eargs) =>
        {
            mainForm.UpdateStatus(StatusReport);
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


    #region Init and End
    override protected async void InitializeContext(bool asSysTray)
    {
        if (Config != null && Config.GetSection("PathToMediaPlayer") != null)
        {
            PathToMediaPlayer = Config.GetSection("PathToMediaPlayer").ToString();
        }
        NotifyIconPath = "icon-white.ico";
        NotifyIconText = "Media Controller";

        base.InitializeContext(asSysTray);

        //set up timer
        timer.AutoReset = true;
        timer.Interval = 5000;
        timer.Elapsed += (sender, eargs) =>
        {
            if (Form.ActiveForm == mainForm && mainForm != null)
            {
                mainForm.UpdateStatus(StatusReport);
            }
        };
        timer.Start();

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
            if (Config != null && Config.GetSection("Credentials").Exists())
            {
                var creds = Config.GetSection("Credentials");
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
            }
            else
            {
                throw new Exception("Cannot find XMPP credentials etry in app config");
            }
        }
        catch (Exception e)
        {
            errors["XMPP"] = e.Message;
        }

        //Connect arduino board
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
                        Console.WriteLine("Board is ready");
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
    private void test()
    {
        //SendKeys.SendWait();
    }
    #endregion

    #region Methods
    void SendIRCommand()
    {
        if (board == null)
        {
            throw new Exception("No arduino board");
        }
        if (!board.IsConnected)
        {
            throw new Exception("Arduino board not connected");
        }

    }
    #endregion
}
