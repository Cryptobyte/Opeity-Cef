using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using CefSharp;

namespace Opeity
{
    public class LifespanHandler : ILifeSpanHandler
    {
        public bool OnBeforePopup(IWebBrowser browserControl, IBrowser browser, IFrame frame, string targetUrl, string targetFrameName,
            WindowOpenDisposition targetDisposition, bool userGesture, IPopupFeatures popupFeatures, IWindowInfo windowInfo,
            IBrowserSettings browserSettings, ref bool noJavascriptAccess, out IWebBrowser newBrowser)
        {
            newBrowser = null;

            if (MainWindow._forceSingleWindow)
            {
                browserControl.Load(targetUrl);
                return true;
            }

            var newWin = Process.Start(new ProcessStartInfo()
            {
                FileName = System.Reflection.Assembly.GetExecutingAssembly().Location,
                Arguments = $"-o \"{targetUrl}\""
            });

            return true;
        }

        public void OnAfterCreated(IWebBrowser browserControl, IBrowser browser)
        {
            
        }

        public bool DoClose(IWebBrowser browserControl, IBrowser browser)
        {
            return false;
        }

        public void OnBeforeClose(IWebBrowser browserControl, IBrowser browser)
        {
            
        }
    }
}
