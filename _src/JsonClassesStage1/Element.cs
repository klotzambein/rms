using System.Collections.Generic;
using WebUnits.Data;

namespace WebUnits.JsonClassesStage1
{
    public class Element
    {
        public int type { get; set; }
        public int id { get; set; }
        public string name { get; set; }
        public string forename { get; set; }
        public string longName { get; set; }
        public string displayname { get; set; }
        public string externKey { get; set; }
        public List<int> dids { get; set; }
        public object klasseId { get; set; }
        public object klasseOrStudentgroupId { get; set; }
        public string title { get; set; }
        public string alternatename { get; set; }
        public ClassTeacher classteacher { get; set; }
        public ClassTeacher classteacher2 { get; set; }
        public object buildingId { get; set; }
        public object restypeId { get; set; }
        public dynamic can { get; set; }
    }
}