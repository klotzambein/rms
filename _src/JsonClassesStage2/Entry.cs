using System.Collections.Generic;
using Newtonsoft.Json;


namespace WebUnits.JsonClassesStage2
{
    public class Entry
    {
        public int id { get; set; }
        public int lessonId { get; set; }
        public int lessonNumber { get; set; }
        public string lessonCode { get; set; }
        public string lessonText { get; set; }
        public string periodText { get; set; }
        public bool hasPeriodText { get; set; }
        public int date { get; set; }
        public int startTime { get; set; }
        public int endTime { get; set; }
        public List<dynamic> elements { get; set; }
        public int code { get; set; }
        public string cellState { get; set; }
        public int priority { get; set; }
        [JsonProperty("is")]
        public Dictionary<string, bool> @is { get; set; }
    }
}