using System.Collections.Generic;

namespace WebUntis.JsonClassesStage2
{
    public class Data
    {
        public bool noDetails { get; set; }
        public List<int> elementIds { get; set; }
        public Dictionary<string, Entry[]> elementPeriods { get; set; }
        public List<object> roomlocks { get; set; }
        public List<Element> elements { get; set; }
    }
}