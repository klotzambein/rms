using System;
using System.Collections.Generic;
using IntervalArray;

namespace WebUnitsApiRipper.Data
{
    public class Timetable
    {
        public Timetable(DateTime timeStamp, Class activeClass, IntervalTree<DateTime, Lesson> lessons, Dictionary<int, Department> departments, Dictionary<int, LessonInfo> infos)
        {
            TimeStamp = timeStamp;
            ActiveClass = activeClass;
            Lessons = lessons;
            Departments = departments;
            Infos = infos;
        }

        public Timetable(Class activeClass, List<Department> departments, JsonClassesStage2.Result result)
        {
            ActiveClass = activeClass;
            Departments = departments;
            Lessons = lessons;
            Infos = infos;
        }

        public DateTime TimeStamp { get; }
        public Class ActiveClass { get; }
        public IntervalTree<DateTime, Lesson> Lessons { get; }
        public Dictionary<int, Department> Departments { get; }
        public Dictionary<int, LessonInfo> Infos { get; }
    }
}