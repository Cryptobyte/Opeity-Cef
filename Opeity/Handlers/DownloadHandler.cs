using System;
using CefSharp;

namespace Opeity.Handlers
{
    public class DownloadHandler : IDownloadHandler
    {
        public event EventHandler<DownloadProgress> OnDownloadItemUpdated;

        public class DownloadProgress : EventArgs
        {
            public DownloadItem Item { get; private set; }

            private DownloadProgress() { }

            public DownloadProgress(DownloadItem item)
            {
                Item = item;
            }
        }

        public void OnBeforeDownload(IBrowser browser, DownloadItem downloadItem, IBeforeDownloadCallback callback)
        {
            if (callback.IsDisposed)
                return;
            
            using (callback)
            {
                callback.Continue(downloadItem.SuggestedFileName, showDialog: true);
            }
        }

        public void OnDownloadUpdated(IBrowser browser, DownloadItem downloadItem, IDownloadItemCallback callback)
        {
            OnDownloadItemUpdated?.Invoke(this, new DownloadProgress(downloadItem));
        }
    }
}
