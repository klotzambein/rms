using System.Collections.Generic;

namespace WebUntis.JsonClassesStage1
{
    public class Filter
    {
        public int type { get; set; }
        public string typeLabel { get; set; }
        public LabelPattern labelPattern { get; set; }
        public List<Element3> elements { get; set; }
        public string property { get; set; }
        public int selected { get; set; }
        public string selectedKlasseOrStudentgroupId { get; set; }
    }
}