using System;
using System.Collections.Generic;
using System.Linq;
using IntervalArray;
using WebUnits.Util;

namespace WebUnits.Data
{
    public class ClassTable
    {
        /*public ClassTable(DateTime timeStamp, Class activeClass, IntervalTree<DateTime, Lesson> lessons, Dictionary<long, LessonInfo> infos)
        {
            TimeStamp = timeStamp;
            ActiveClass = activeClass;
            Lessons = lessons;
            Infos = infos;
        }

        public ClassTable(Class activeClass, JsonClassesStage1.RootObject result)
        {
            TimeStamp = DateTimeOffset.FromUnixTimeMilliseconds(result.lastImportTimestamp).DateTime;

            ActiveDepartment = re;

            Infos = result.data.elements
                .Select(e => new LessonInfo(e))
                .ToDictNoDups(e => e.Key);

            Lessons = new IntervalTree<DateTime, Lesson>();
            if (result.data.elementPeriods.ContainsKey(activeClass.Id.ToString()))
            {
                var infos = Infos.Select(e => e.Value);
                var lessons = result.data.elementPeriods[activeClass.Id.ToString()];
                foreach (var l in lessons)
                {
                    var newLesson = new Lesson(l, infos);
                    Lessons.Add(newLesson.GetInterval(), newLesson);
                }
            }
        }
*/
        public DateTime TimeStamp { get; }
        public Department ActiveDepartment { get; }
        public Dictionary<long, Class> Classes { get; }
        public Dictionary<long, Department> Departments { get; }
    }
}