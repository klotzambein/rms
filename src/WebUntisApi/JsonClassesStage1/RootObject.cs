using System.Collections.Generic;

namespace WebUntis.JsonClassesStage1
{
    public class RootObject
    {
        public string elementTypeLabel { get; set; }
        public bool kioskMode { get; set; }
        public List<Element> elements { get; set; }
        public bool showQuickSelect { get; set; }
        public LabelPattern labelPattern { get; set; }
        public List<Format> formats { get; set; }
        public bool hasDepartmentSelect { get; set; }
        public object departments { get; set; }
        public int selectedDepartmentId { get; set; }
        public long lastImportTimestamp { get; set; }
        public bool showControls { get; set; }
        public int selectedElementId { get; set; }
        public int selectedFormatId { get; set; }
        public bool test { get; set; }
        public int pollingInterval { get; set; }
        public dynamic can { get; set; }
        public object types { get; set; }
        public List<Filter> filters { get; set; }
        public List<string> visibleLayers { get; set; }
    }
}