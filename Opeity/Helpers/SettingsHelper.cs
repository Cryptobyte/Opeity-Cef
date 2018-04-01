using CefSharp;
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Xml;
using System.Xml.Serialization;

namespace Opeity.Helpers
{
    public class SettingsHelper
    {
        private string _filePath =
            Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "Settings.xml");

        private List<KeyValItem> Settings;

        public class KeyValItem
        {
            public string Key;
            public CefState Value;
            public KeyValItem() { }
        }

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
                    xmlDocument.Save(_filePath);
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
            if (string.IsNullOrEmpty(_filePath)) { return default(T); }

            var objectOut = default(T);

            try
            {
                var xmlDocument = new XmlDocument();
                xmlDocument.Load(_filePath);
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

        public CefState Get(string key)
        {
            foreach (var setting in Settings)
            {
                if (Equals(setting.Key, key))
                {
                    return setting.Value;
                }
            }

            return CefState.Default;
        }

        public CefState GetOrDefault(string key, CefState defaultValue)
        {
            foreach (var setting in Settings)
            {
                if (Equals(setting.Key, key))
                {
                    return setting.Value;
                }
            }

            Settings.Add(new KeyValItem() { Key = key, Value = defaultValue });

            return defaultValue;
        }

        public void Set(string key, CefState value)
        {
            foreach (var setting in Settings)
            {
                if (Equals(setting.Key, key))
                {
                    setting.Value = value;
                    return;
                }
            }

            Settings.Add(new KeyValItem() { Key = key, Value = value });
        }

        public void Save()
        {
            SerializeObject(Settings);
        }

        public SettingsHelper()
        {
            Settings = new List<KeyValItem>();

            if (File.Exists(_filePath))
                Settings = DeSerializeObject<List<KeyValItem>>();

        }
    }
}
