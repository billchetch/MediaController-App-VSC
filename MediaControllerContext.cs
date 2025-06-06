using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Text.Json;
using Chetch.Windows;
using Chetch.ChetchXMPP;
using Microsoft.Extensions.Configuration;
using Chetch.Arduino;

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
    #endregion

    #region Constants and static methods and fields
    //const String PLEX_MEDIA_PLAYER_PROCESS_NAME = "PlexMediaPlayer";
    const String PLEX_MEDIA_PLAYER_PROCESS_NAME = "notepad";
    const String CXMPP_USERNAME = "bbmedia.client@openfire.bb.lan";
    const String CXMPP_PASSWORD = "bbmedia";

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

    #region Fields
    ChetchXMPPConnection cnn = new ChetchXMPPConnection(CXMPP_USERNAME, CXMPP_PASSWORD);

    MainForm? mainForm;

    System.Timers.Timer timer = new System.Timers.Timer();

    ArduinoBoard board = new ArduinoBoard("mc");
    #endregion

    protected override Form CreateMainForm()
    {
        mainForm = new MainForm();
        mainForm.Activated += (sender, eargs) =>
        {
            Console.WriteLine("Activated");
        };
        return mainForm;
    }

    #region Init and End
    override protected void InitializeContext(bool asSysTray)
    {
        if(Config != null && Config.GetSection("PathToMediaPlayer") != null)
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

        };
        timer.Start();

        //Connect client
        try
        {
            cnn.ConnectAsync();
        }
        catch (Exception e)
        {
            Console.WriteLine(e.Message);
        }

        //Connect arduino board
        if (Config != null && Config.GetSection("Arduino").Exists())
        {
            var cnnConfig = Config.GetSection("Arduino").GetSection("Connection");
            try
            {
                var path2device = cnnConfig["PathToDevice"];
                if (path2device == null) throw new Exception("No path to device found");
                int baudRate = System.Convert.ToInt32(cnnConfig["BaudRate"]);
                board.Connection = new ArduinoSerialConnection(path2device.ToString(), baudRate);

                board.Ready += (sender, ready) =>
                {

                };

                //Handle errors either thrown or generated from the board by just logging them
                board.ErrorReceived += (sender, errorArgs) =>
                {

                };
                board.ExceptionThrown += (sender, errorArgs) =>
                {

                };

                //Now begin
                board.Begin();
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }
        }
    }

    protected override void ExitThreadCore()
    {
        try
        {
            cnn.DisconnectAsync();
        }
        catch { };

        base.ExitThreadCore();
    }
    #endregion

    #region Command handling
    private void test()
    {
        //SendKeys.SendWait();
    }
    #endregion
}
