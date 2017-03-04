using System;
using System.IO;
using System.Text;
using Newtonsoft.Json;

namespace ConsoleApp
{
    public class JsonConfigReader : IConfigReader
    {
        private const string ConfigPath = "DdnsClient.json";

        public virtual ConfigRoot Read()
        {
            string filePath = Path.Combine(AppContext.BaseDirectory, ConfigPath);
            string jsonString = File.ReadAllText(filePath, Encoding.UTF8);

            TextReader txtReader = new StringReader(jsonString);
            JsonReader jsonReader = new JsonTextReader(txtReader);
            JsonSerializer serializer = new JsonSerializer();
            ConfigRoot config = serializer.Deserialize<ConfigRoot>(jsonReader);

            return config;
        }
    }
}