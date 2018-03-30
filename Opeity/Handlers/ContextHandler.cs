using CefSharp;

namespace Opeity.Handlers
{
    public class RequestContextHandler : IRequestContextHandler
    {
        private ICookieManager customCookieManager;

        public ICookieManager GetCookieManager()
        {
            if (customCookieManager == null)
                customCookieManager = new CookieManager(null, persistSessionCookies: false, callback: null);

            return customCookieManager;
        }

        public bool OnBeforePluginLoad(string mimeType, string url, bool isMainFrame, string topOriginUrl, WebPluginInfo pluginInfo, ref PluginPolicy pluginPolicy)
        {
            return false;
        }
    }
}
