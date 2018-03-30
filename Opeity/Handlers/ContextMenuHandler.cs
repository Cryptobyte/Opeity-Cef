using CefSharp;
using CefSharp.Wpf;
using System.Diagnostics;
using System.Windows;

namespace Opeity.Handlers
{
    public class ContextMenuHandler : IContextMenuHandler
    {
        public void OnBeforeContextMenu(IWebBrowser browserControl, IBrowser browser, IFrame frame, IContextMenuParams parameters, IMenuModel model)
        {
            if (model.Count > 0)
            {
                model.AddSeparator();
            }

            if (!string.IsNullOrEmpty(parameters.UnfilteredLinkUrl))
                model.AddItem((CefMenuCommand)26501, "Copy Link Address");

            if (!string.IsNullOrEmpty(parameters.LinkUrl))
                model.AddItem((CefMenuCommand)26502, "Open Link in New Window");

            if (!string.IsNullOrEmpty(parameters.UnfilteredLinkUrl) || !string.IsNullOrEmpty(parameters.LinkUrl))
                model.AddSeparator();

            var dev = model.AddSubMenu((CefMenuCommand)26503, "Developer Tools");
                dev.AddItem((CefMenuCommand)26504, "Inspector");
        }

        public bool OnContextMenuCommand(IWebBrowser browserControl, IBrowser browser, IFrame frame, IContextMenuParams parameters, CefMenuCommand commandId, CefEventFlags eventFlags)
        {
            switch (commandId)
            {
                case (CefMenuCommand)26501:
                    Clipboard.SetText(parameters.UnfilteredLinkUrl);
                    return true;

                case (CefMenuCommand)26502:
                    Process.Start(new ProcessStartInfo()
                    {
                        FileName = System.Reflection.Assembly.GetExecutingAssembly().Location,
                        Arguments = $"-o \"{parameters.LinkUrl}\""
                    });

                    return true;

                case (CefMenuCommand)26504:
                    if (browser.GetHost().HasDevTools)
                        browser.GetHost().CloseDevTools();
                    
                    if (!browser.GetHost().HasDevTools)
                        browser.GetHost().ShowDevTools();

                    return true;

                default: return false;
            }
        }

        public void OnContextMenuDismissed(IWebBrowser browserControl, IBrowser browser, IFrame frame)
        {
            var chromiumWebBrowser = (ChromiumWebBrowser)browserControl;

            chromiumWebBrowser.Dispatcher.Invoke(() =>
            {
                chromiumWebBrowser.ContextMenu = null;
            });
        }

        public bool RunContextMenu(IWebBrowser browserControl, IBrowser browser, IFrame frame, IContextMenuParams parameters, IMenuModel model, IRunContextMenuCallback callback)
        {
            return false;
        }
    }
}
