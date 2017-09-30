using System;
using System.Collections.Generic;
using System.Drawing;
//using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using GoogleCalendarApi;
using Ical.Net;
using Ical.Net.DataTypes;
using Ical.Net.Interfaces.DataTypes;
using Ical.Net.Interfaces.General;
using ICal;
using Newtonsoft.Json;
using NLog;
using WebUntis;
using WebUntis.Data;

namespace Server
{
    class Program
    {
        private static readonly Logger logger = LogManager.GetCurrentClassLogger();
        static Dictionary<string, WebUntisApi> webUntisDict = new Dictionary<string, WebUntisApi>();
        static void Main(string[] args)
        {
#if DEBUG
            Directory.SetCurrentDirectory(Path.GetDirectoryName(Assembly.GetEntryAssembly().Location));
#endif
            var config = RMSConfig.LoadFile("Config.json");

            var scheduler = new Scheduler();
            foreach (var dt in config.UpdateCycleDT) scheduler.Add(dt, OnUpdateCycle);

            var archive = new CalendarArchive("cals.zip", config.EncodingEnc);

            var server = new ICalServer(config.Server_port)
                .UseEncoding(config.EncodingEnc);
            if (config.Server_prefixes != null)
                server.UseSpecificPrefixes(config.Server_prefixes.ToArray());
            server.Start();
        }

        static WebUntisApi GetWebUntisApi(string sourceSchool, string sourceDomain)
        {
            string source = $"{sourceSchool}@{sourceDomain.ToLower()}";

            WebUntisApi api;

            if (webUntisDict.TryGetValue(source, out api))
                return api;

            api = new WebUntisApi(sourceDomain, sourceSchool);
            webUntisDict.Add(source, api);
            return api;
        }

        static Calendar CalendarAddTimeTable(Calendar calendar, TimeTable timeTable, IEnumerable<CourseConfig> courses)
        {
            var now = DateTime.Now;
            var newCal = new Calendar();

            //Add old events
            foreach (var e in calendar.Events
                .Where(e => e.End.AsSystemLocal < now))
                newCal.Events.Add(e);

            //add new eventa
            //Filter and select lessons
            var lessons = timeTable.Lessons
                .Where(ls => ls.Key.End < now)
                .SelectMany(ls => ls.Value.Select(l => (time: ls.Key, lesson: l)))
                .ToList();

            //Select lessons for each course
            var cs = courses
                .Select(c =>
                    (course: c,
                    lesson: lessons
                        .Select(l =>
                            (time: l.time,
                            lesson: l.lesson,
                            score: l.lesson
                                .Score(c.Filter)))
                        .Where(l => l.score != int.MaxValue)))
                .ToList();

            //Warn empty courses
            foreach (var emptyC in cs.Where(c => !c.lesson.Any() && c.course.Warn == WarnMethod.NotFound))
                logger.Warn("Course '{0}' does not have any lessons and the warning flag is not disabled.", emptyC.course.Filter);

            //Actualy add courses
            foreach (var l in cs.SelectMany(c => c.lesson.Select(l => (lesson: l.lesson, time: l.time, course: c.course))))
            {
                CalendarEvent item = new CalendarEvent()
                {
                    DtStart = new CalDateTime(l.time.Start),
                    DtEnd = new CalDateTime(l.time.End),
                    Summary = l.lesson.ToString(),
                };

                if (Regex.IsMatch(l.course.Color, "^#[A-Fa-f0-9]{3}([A-Fa-f0-9]{3})?$"))
                    item.AddProperty("COLOR", l.course.Color);
                else if (l.course.Color != "default")
                    logger.Warn("{0} is not a valid color value", l.course.Color);

                newCal.Events.Add(item);
            }

            return newCal;
        }
        static Calendar CalendarFromTimeTable(TimeTable timeTable, IEnumerable<CourseConfig> courses)
        {
            var now = DateTime.Now;
            var newCal = new Calendar();

            //select lessons
            var lessons = timeTable.Lessons
                .SelectMany(ls => ls.Value.Select(l => (time: ls.Key, lesson: l)))
                .ToList();

            //Select lessons for each course
            var cs = courses
                .Select(c =>
                    (course: c,
                    lesson: lessons
                        .Select(l =>
                            (time: l.time,
                            lesson: l.lesson,
                            score: l.lesson
                                .Score(c.Filter)))
                        .Where(l => l.score != int.MaxValue)))
                .ToList();

            //Warn empty courses
            foreach (var emptyC in cs.Where(c => !c.lesson.Any() && c.course.Warn == WarnMethod.NotFound))
                logger.Warn("Course '{0}' does not have any lessons and the warning flag is not disabled.", emptyC.course.Filter);

            //Actualy add courses
            foreach (var l in cs.SelectMany(c => c.lesson.Select(l => (lesson: l.lesson, time: l.time, course: c.course))))
            {
                CalendarEvent item = new CalendarEvent()
                {
                    DtStart = new CalDateTime(l.time.Start),
                    DtEnd = new CalDateTime(l.time.End),
                    Summary = l.lesson.ToString(),
                };

                if (Regex.IsMatch(l.course.Color, "^#[A-Fa-f0-9]{3}([A-Fa-f0-9]{3})?$"))
                    item.AddProperty("COLOR", l.course.Color);
                else if (l.course.Color != "default")
                    logger.Warn("{0} is not a valid color value", l.course.Color);

                newCal.Events.Add(item);
            }

            return newCal;
        }

        static void OnUpdateCycle()
        {
            
        }
    }
}