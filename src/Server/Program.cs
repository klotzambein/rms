using System;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using GoogleCalendarApi;

namespace Server
{
    class Program
    {
        static void Main(string[] args)
        {
#if DEBUG
            Directory.SetCurrentDirectory(Path.GetDirectoryName(Assembly.GetEntryAssembly().Location));
#endif
            var api = CalendarApi.Create();
        }
    }
}