using System;
using System.Collections.Generic;
using System.Linq;
using IntervalArray;
using WebUnitsApiRipper.Util;

namespace WebUnitsApiRipper.Data
{
    public class Timetable
    {
        public Timetable(DateTime timeStamp, Class activeClass, IntervalTree<DateTime, Lesson> lessons, Dictionary<long, LessonInfo> infos)
        {
            TimeStamp = timeStamp;
            ActiveClass = activeClass;
            Lessons = lessons;
            Infos = infos;
        }

        public Timetable(Class activeClass, JsonClassesStage2.Result result)
        {
            TimeStamp = DateTimeOffset.FromUnixTimeMilliseconds(result.lastImportTimestamp).DateTime;

            ActiveClass = activeClass;

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
        public Dictionary<long, LessonInfo> Infos { get; }


    }
}