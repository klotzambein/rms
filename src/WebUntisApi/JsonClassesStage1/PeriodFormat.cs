using System.Collections.Generic;

namespace WebUntis.JsonClassesStage1
{
    public class PeriodFormat
    {
        public bool strikethroughInfo { get; set; }
        public List<Element2> elements { get; set; }
        public int rows { get; set; }
        public int minHeight { get; set; }
        public int minWidth { get; set; }
        public Position textPosition { get; set; }
        public Position infoPosition { get; set; }
        public Position userPosition { get; set; }
        public Position rescheduleInfoPosition { get; set; }
        public bool showSubstElements { get; set; }
        public bool ignoreIndividualColors { get; set; }
        public int cols { get; set; }
        public Position timePosition { get; set; }
        public bool substituteTextForEmptySubject { get; set; }
    }
}