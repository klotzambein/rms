using System;
using System.Collections.Generic;
using System.Drawing;
//using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using GoogleCalendarApi;
using Ical.Net;
using Ical.Net.DataTypes;
using Ical.Net.Interfaces.DataTypes;
using ICal;
using Newtonsoft.Json;
using NLog;
using WebUntis;
using WebUntis.Data;

namespace Server
{
    class Program
    {
        static Dictionary<string, WebUntisApi> webUntisDict = new Dictionary<string, WebUntisApi>();
        static void Main(string[] args)
        {
#if DEBUG
            Directory.SetCurrentDirectory(Path.GetDirectoryName(Assembly.GetEntryAssembly().Location));
#endif
            var config = RMSConfig.LoadFile("Config.json");

            var scheduler = new Scheduler();
            foreach (var dt in config.UpdateCycleDT) scheduler.Add(dt, OnUpdateCycle);

            /*var now = DateTime.Now;

            Calendar calendar = new Calendar();
            calendar.Events.Add(new CalendarEvent()
            {
                DtStart = new CalDateTime(now),
                DtEnd = new CalDateTime(now.AddHours(1)),
                Name = "My Event",
            });
            calendar.Events.Add(new CalendarEvent()
            {
                DtStart = new CalDateTime(now.AddHours(25)),
                DtEnd = new CalDateTime(now.AddHours(28)),
                Name = "My Event 2",
            });

            LogManager.GetCurrentClassLogger().Error(new Exception(), "Test");

            ICalCache cache = new ICalCache();
            cache.AddOrUpdate("t1", calendar);
            var api = new ICalServer(8080)
                .UseSpecificPrefixes("http://localhost:\\p/")
                .UseCache(cache);
            api.Start();*/
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

        static void AddTimetableToCalendar(TimeTable timeTable, Calendar calendar, IEnumerable<CourseConfig> courses)
        {
            var now = DateTime.Now;
            var newCal = new Calendar();

            foreach (var e in calendar.Events
                .Where(e => e.End.AsSystemLocal < now))
                newCal.Events.Add(e);

            foreach (var ls in timeTable.Lessons)
            {
                if (ls.Key.End < now)
                    continue;

                foreach (var l in ls.Value)
                {
                    
                    CalendarEvent item = new CalendarEvent()
                    {
                        DtStart = new CalDateTime(ls.Key.Start),
                        DtEnd = new CalDateTime(ls.Key.End),
                        Summary = l.ToString(),
                    };
                    newCal.Events.Add(item);
                }
            }
        }

        static void OnUpdateCycle()
        {

        }
    }
}