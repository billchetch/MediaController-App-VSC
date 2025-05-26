using System;

namespace MediaController;

public class MediaControllerContext : SysTrayApplicationContext
{
    protected override Form CreateMainForm()
    {
        return new MainForm();
    }

    override protected void InitializeContext(bool asSysTray)
    {
        base.InitializeContext(asSysTray);
        if (NotifyIcon != null)
        {
            String dir = Directory.GetCurrentDirectory();
            String iconFile = "./icon-white.ico";
            var icon = System.Drawing.Icon.ExtractAssociatedIcon(iconFile);
            NotifyIcon.Icon = icon;
            NotifyIcon.Text = "Media Controller";
        }
    }  
}
