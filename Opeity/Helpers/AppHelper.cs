using IWshRuntimeLibrary;
using Opeity.Properties;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Windows.Forms;
using System.Xml;
using System.Xml.Serialization;
using File = System.IO.File;

namespace Opeity.Helpers
{
    public class AppHelper
    {
        public class OpeityApp
        {
            public string Name;
            public string Address;
            public string RootPath;
            public string CachePath;
            public OpeityApp() { }
        }
        
        public static string AppDirectory =
            Path.Combine(Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location), "Apps");

        private List<OpeityApp> _apps;

        private string _appConfig =
            Path.Combine(AppDirectory, "Apps.xml");

        /// <summary>
        /// Serializes an object.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="serializableObject"></param>
        public void SerializeObject<T>(T serializableObject)
        {
            if (serializableObject == null) { return; }

            try
            {
                var xmlDocument = new XmlDocument();
                var serializer = new XmlSerializer(serializableObject.GetType());
                using (var stream = new MemoryStream())
                {
                    serializer.Serialize(stream, serializableObject);
                    stream.Position = 0;
                    xmlDocument.Load(stream);
                    xmlDocument.Save(_appConfig);
                    stream.Close();
                    stream.Dispose();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }


        /// <summary>
        /// Deserializes an xml file into an object list
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public T DeSerializeObject<T>()
        {
            if (string.IsNullOrEmpty(_appConfig)) { return default(T); }

            var objectOut = default(T);

            try
            {
                var xmlDocument = new XmlDocument();
                xmlDocument.Load(_appConfig);
                var xmlString = xmlDocument.OuterXml;

                using (var read = new StringReader(xmlString))
                {
                    var outType = typeof(T);

                    var serializer = new XmlSerializer(outType);
                    using (var reader = new XmlTextReader(read))
                    {
                        objectOut = (T)serializer.Deserialize(reader);
                        reader.Close();
                    }

                    read.Close();
                    read.Dispose();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }

            return objectOut;
        }

        public void Make(string title, string address)
        {
            var sfd = new SaveFileDialog()
            {
                Title = Resources.MainWindow_C_BTN_MakeApp_Click_Create_App_Shortcut,
                AddExtension = true,
                CheckPathExists = true,
                FileName = $"{title}.lnk",
                Filter = Resources.AppHelper_Make_Shortcut____lnk____lnk,
                InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.Desktop),
                RestoreDirectory = true,
            };

            if (sfd.ShowDialog() == DialogResult.OK)
            {
                var app = new OpeityApp()
                {
                    Name = title,
                    Address = address,
                    RootPath = Path.Combine(AppDirectory, title),
                    CachePath = Path.Combine(AppDirectory, title, "Cache")
                };

                if (!Directory.Exists(app.RootPath))
                    Directory.CreateDirectory(app.RootPath);

                if (!Directory.Exists(app.CachePath))
                    Directory.CreateDirectory(app.CachePath);

                using (var client = new WebClient())
                {
                    client.DownloadFile(
                        $"https://api.statvoo.com/favicon/?url={address}",
                        Path.Combine(app.RootPath, $"{app.Name}.ico")
                    );
                }

                var shell = new WshShell();
                var shortCutLinkFilePath = sfd.FileName;
                var windowsApplicationShortcut = (IWshShortcut)shell.CreateShortcut(shortCutLinkFilePath);

                windowsApplicationShortcut.Description = app.Name;

                windowsApplicationShortcut.WorkingDirectory =
                    Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location);

                windowsApplicationShortcut.TargetPath =
                    System.Reflection.Assembly.GetExecutingAssembly().Location;

                windowsApplicationShortcut.Arguments = $"-app -o \"{app.Address}\"";
                windowsApplicationShortcut.IconLocation = Path.Combine(app.RootPath, $"{app.Name}.ico");
                windowsApplicationShortcut.Save();

                _apps.Add(app);
                SerializeObject(_apps);
            }
        }

        public OpeityApp Find(string address)
        {
            foreach (var app in _apps)
            {
                if (Equals(app.Address, address))
                    return app;

            }

            return null;
        }

        public AppHelper()
        {
            _apps = new List<OpeityApp>();

            if (!Directory.Exists(AppDirectory))
                Directory.CreateDirectory(AppDirectory);

            if (File.Exists(_appConfig))
                _apps = DeSerializeObject<List<OpeityApp>>();

        }
    }
}
