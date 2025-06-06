namespace MediaController;

static class Program
{
    /// <summary>
    ///  The main entry point for the application.
    /// </summary>
    [STAThread]
    static void Main()
    {
        Application.EnableVisualStyles();
        Application.SetCompatibleTextRenderingDefault(false);
        MediaControllerContext context = new MediaControllerContext();

        //SysTray (comment out below and uncomment this section to run as a syst tray app)
        Application.Run(context);

        //Normal winform (comment out above and uncomment this section to run as a normal form app)
        //Application.Run(new MainForm());
    }    
}