using System.Configuration;
using System.Windows.Forms;
using CapsLockSharpPrototype.Properties;

namespace CapsLockSharpPrototype.Helper
{
    public class TrayIcon
    {
        public static void RefleshIcon(NotifyIcon ni)
        {
            var showIconConfigString = ConfigurationManager.AppSettings.Get("showIcon");
            var showIcon = showIconConfigString == null ? true : bool.Parse(showIconConfigString);
            var capslock = Control.IsKeyLocked(Keys.CapsLock);
            ni.Icon = capslock ? Resources.logo_32 : Resources.logo_32_disable;
            ni.Visible = showIcon;
            Logger.Info("Currently, Caps is " + (capslock ? "" : "not ") + "locked");
        }
    }
}
