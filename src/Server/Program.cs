using System;
using System.Drawing;
//using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using GoogleCalendarApi;
using Ical.Net;
using Ical.Net.DataTypes;
using ICal;

namespace Server
{
    class Program
    {
        static void Main(string[] args)
        {
#if DEBUG
            Directory.SetCurrentDirectory(Path.GetDirectoryName(Assembly.GetEntryAssembly().Location));
#endif

            var now = DateTime.Now;
            
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


            ICalCache cache = new ICalCache();
            cache.AddOrUpdate("t1", calendar);
            var api = new ICalServer(8080)
                .UseSpecificPrefixes("http://localhost:\\p/")
                .UseCache(cache);
            api.Start();
        }
    }
}