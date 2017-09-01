using System;
using System.Drawing;
using System.Globalization;
using System.Linq;
using WebUntis;

namespace Server
{
    class Program
    {
        static void Main(string[] args)
        {
            var api = new WebUntisApi("stundenplan.hamburg.de", "hh5849");
            var classes = api.QueryClasses();
            var lessons = api.QueryLessons(classes.Find(nameFilter: "12.4"), DateTime.Now.AddDays(3));
            foreach (var l in lessons.Lessons)
                Console.WriteLine(l);

        }
    }
}