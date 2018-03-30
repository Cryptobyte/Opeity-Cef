// Copyright © 2010-2017 The CefSharp Authors. All rights reserved.
//
// Use of this source code is governed by a BSD-style license that can be found in the LICENSE file.

using System;
using System.Collections.ObjectModel;
using System.Windows.Shell;
using System.Windows.Threading;
using CefSharp;

namespace Opeity
{
    public class DownloadHandler : IDownloadHandler
    {
        public event EventHandler<DownloadItem> OnBeforeDownloadFired;

        public event EventHandler<DownloadItem> OnDownloadUpdatedFired;

        public ObservableCollection<DownloadItem> FilesDownloading = new ObservableCollection<DownloadItem>();

        public void OnBeforeDownload(IBrowser browser, DownloadItem downloadItem, IBeforeDownloadCallback callback)
        {
            OnBeforeDownloadFired?.Invoke(this, downloadItem);

            if (callback.IsDisposed)
                return;
            
            using (callback)
            {
                callback.Continue(downloadItem.SuggestedFileName, showDialog: true);
            }
        }

        public void OnDownloadUpdated(IBrowser browser, DownloadItem downloadItem, IDownloadItemCallback callback)
        {
            if (downloadItem.IsInProgress)
            {
                if (!FilesDownloading.Contains(downloadItem))
                    FilesDownloading.Add(downloadItem);

                OnDownloadUpdatedFired?.Invoke(this, downloadItem);
                return;
            }

            if (downloadItem.IsCancelled)
            {
                OnDownloadCanceled(browser, downloadItem);
                return;
            }
            
            OnDownloadComplete(browser, downloadItem);
        }

        public void OnDownloadComplete(IBrowser browser, DownloadItem downloadItem)
        {
            FilesDownloading.Remove(downloadItem);
        }

        public void OnDownloadCanceled(IBrowser browser, DownloadItem downloadItem)
        {
            FilesDownloading.Remove(downloadItem);
        }
    }
}
