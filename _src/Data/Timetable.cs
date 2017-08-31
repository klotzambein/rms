using System;
using System.Collections.Generic;
using System.Linq;
using IntervalArray;
using WebUnitsApiRipper.Util;

namespace WebUnitsApiRipper.Data
{
    public class Timetable
    {
        public Timetable(DateTime timeStamp, Class activeClass, IntervalTree<DateTime, Lesson> lessons, Dictionary<int, Department> departments, Dictionary<long, LessonInfo> infos)
        {
            TimeStamp = timeStamp;
            ActiveClass = activeClass;
            Lessons = lessons;
            Departments = departments;
            Infos = infos;
        }

        public Timetable(Class activeClass, List<Department> departments, JsonClassesStage2.Result result)
        {
            TimeStamp = DateTimeOffset.FromUnixTimeMilliseconds(result.lastImportTimestamp).DateTime;

            ActiveClass = activeClass;

            Departments = departments.ToDictNoDups(d => d.Id);

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

        public DateTime TimeStamp { get; }
        public Class ActiveClass { get; }
        public IntervalTree<DateTime, Lesson> Lessons { get; }
        public Dictionary<int, Department> Departments { get; }
        public Dictionary<long, LessonInfo> Infos { get; }
    }
}