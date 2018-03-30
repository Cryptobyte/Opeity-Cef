using CefSharp;
using Opeity.Helpers;
using System;
using System.ComponentModel;
using System.IO;
using System.Runtime.CompilerServices;

namespace Opeity.Objects
{
    public class DownloadItemEx : INotifyPropertyChanged
    {
        public int _Id = 0;
        public string _File = string.Empty;
        public string _Source = string.Empty;
        public string _Destination = string.Empty;
        public string _ReadableSpeed = string.Empty;

        public long _Speed = 0;
        public int _Progress = 0;

        public int Id
        {
            get => _Id;
            set
            {
                if (value != _Id)
                {
                    _Id = value;
                    NotifyPropertyChanged();
                }
            }
        }

        public string File {
            get => _File;
            set {
                if (value != _File)
                {
                    _File = value;
                    NotifyPropertyChanged();
                }
            }
        }

        public string Source {
            get => _Source;
            set {
                if (value != _Source)
                {
                    _Source = value;
                    NotifyPropertyChanged();
                }
            }
        }

        public string Destination {
            get => _Destination;
            set {
                if (value != _Destination)
                {
                    _Destination = value;
                    NotifyPropertyChanged();
                }
            }
        }

        public long Speed
        {
            get => _Speed;
            set
            {
                if (value != _Speed)
                {
                    _Speed = value;
                    NotifyPropertyChanged();
                }
            }
        }

        public string ReadableSpeed
        {
            get => _ReadableSpeed;
            set
            {
                if (value != _ReadableSpeed)
                {
                    _ReadableSpeed = value;
                    NotifyPropertyChanged();
                }
            }
        }

        public int Progress
        {
            get => _Progress;
            set 
            {
                if (value != _Progress)
                {
                    _Progress = value;
                    NotifyPropertyChanged();
                }
            }
        }

        public void UpdateReadableSpeed()
        {
            ReadableSpeed = FileSizeHelper.GetBytesReadable(Speed);
        }

        private void NotifyPropertyChanged([CallerMemberName] String propertyName = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public DownloadItemEx(DownloadItem item)
        {
            Id = item.Id;
            File = Path.GetFileName(item.FullPath);
            Source = item.Url;
            Destination = item.FullPath;
            Speed = item.CurrentSpeed;
            Progress = item.PercentComplete;
        }

        public event PropertyChangedEventHandler PropertyChanged;
    }
}
