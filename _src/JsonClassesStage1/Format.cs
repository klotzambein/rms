namespace WebUnits.JsonClassesStage1
{
    public class Format
    {
        public string name { get; set; }
        public int id { get; set; }
        public int type { get; set; }
        public int endTime { get; set; }
        public int startTime { get; set; }
        public bool timegridDays { get; set; }
        public bool connectConsecutive { get; set; }
        public bool showBreakSupervisions { get; set; }
        public bool showRoomlocks { get; set; }
        public int days { get; set; }
        public int rowHeads { get; set; }
        public int colHeads { get; set; }
        public bool showLegend { get; set; }
        public bool hideDetails { get; set; }
        public bool renderOnServer { get; set; }
        public bool renderRealTime { get; set; }
        public string periodConnectType { get; set; }
        public bool showHorizontalGridLines { get; set; }
        public bool hideEmptyColumns { get; set; }
        public object elementInfo { get; set; }
        public PeriodFormat periodFormat { get; set; }
        public string longName { get; set; }
        public int colHeads0 { get; set; }
        public int maxSlices { get; set; }
    }
}