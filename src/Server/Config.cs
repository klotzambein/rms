using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using NLog;

namespace Server
{
    public class RMSConfig
    {
        private static readonly Logger logger = LogManager.GetCurrentClassLogger();

        public static RMSConfig Load(string json) =>
            JsonConvert.DeserializeObject<RMSConfig>(json);
        public static RMSConfig LoadFile(string path) =>
            JsonConvert.DeserializeObject<RMSConfig>(File.ReadAllText(path));
        public static bool TryLoadFile(string path, out RMSConfig config)
        {
            try
            {
                config = JsonConvert.DeserializeObject<RMSConfig>(File.ReadAllText(path));
                return true;
            }
            catch (Exception ex)
            {
                logger.Warn(ex, "error loading config file");
                config = null;
                return false;
            }

        }

        public Dictionary<string, CalendarProviderConfig> Calendars { get; set; }
        public int Server_port { get; set; } = 8080;
        [JsonConverter(typeof(StringSingleArrayConverter))]
        public List<string> Server_prefixes { get; set; } = null;
        public string Loging_email { get; set; } = "none";
        public IReadOnlyList<string> UpdateCycle
        {
            get => UpdateCycleDT?.Select(t => t.ToString("HH:mm"))?.ToList();
            set => UpdateCycleDT = value.Select(t => DateTime.ParseExact(t, "HH:mm", CultureInfo.InvariantCulture)).ToList();
        }
        [JsonIgnore]
        public IReadOnlyList<DateTime> UpdateCycleDT { get; set; } = new DateTime[0];
        public string Encoding
        {
            get => EncodingEnc.EncodingName;
            set
            {
                try { EncodingEnc = System.Text.Encoding.GetEncoding(value); }
                catch (Exception ex) { logger.Warn(ex, "error while parsing encoding"); }
            }
        }
        [JsonIgnore]
        public Encoding EncodingEnc { get; set; } = System.Text.Encoding.ASCII;
    }

    internal class StringSingleArrayConverter : JsonConverter
    {
        public override bool CanConvert(Type objectType) =>
            typeof(ICollection<string>).IsAssignableFrom(objectType) || objectType.IsAssignableFrom(typeof(List<string>));

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            ICollection<string> strings;

            if (typeof(ICollection<string>).IsAssignableFrom(objectType))
                strings = (ICollection<string>)Activator.CreateInstance(objectType);
            else
                strings = new List<string>();

            string s;
            if (reader.TokenType == JsonToken.StartArray)
                while ((s = reader.ReadAsString()) != null)
                    strings.Add(s);
            else if (reader.ValueType == typeof(string))
                strings.Add((string)reader.Value);
            else
                throw new JsonReaderException("Can not convert to string[]");

            return strings;
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            var strings = (IEnumerable<string>)value;
            if (strings.Count() == 1)
                writer.WriteValue(strings.First());
            else
            {
                writer.WriteStartArray();
                foreach (var s in strings)
                    writer.WriteValue(s);
                writer.WriteEndArray();
            }
        }
    }

    public class CalendarProviderConfig
    {
        [JsonRequired]
        public string Source
        {
            get => $"{SourceSchool}@{SourceDomain}";
            set => (SourceSchool, SourceDomain) = (value.Split("@")[0], value.Split("@")[1]);
        }

        [JsonIgnore]
        public string SourceSchool { get; set; }

        [JsonIgnore]
        public string SourceDomain { get; set; }

        public string ClassFilter { get; set; } = null;

        public string TeacherFilter { get; set; } = null;

        [JsonRequired]
        public List<CourseConfig> Courses { get; set; }

        [JsonRequired]
        [JsonConverter(typeof(StringEnumConverter))]
        public UpdateFreq Update { get; set; }

        [JsonRequired]
        public bool Accumulate { get; set; }
    }

    public enum UpdateFreq
    {
        OnDemand,
        UpdateCycle,
        Daily,
    }

    public class CourseConfig
    {
        [JsonRequired]
        public string Filter { get; set; }

        [JsonConverter(typeof(StringEnumConverter))]
        public WarnMethod Warn { get; set; } = WarnMethod.NotFound;

        public string Color { get; set; } = "default";
    }

    public enum WarnMethod
    {
        Never = 0b00,
        NotFound = 0b01,
        /*Ambiguous = 0b10,
        NotFoundOrAmbiguous = 0b11,*/
    }
}