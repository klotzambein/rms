using System;
using System.Drawing;
using System.Globalization;
using System.Linq;
using IntervalArray;
using WebUnitsApiRipper.Data;

namespace WebUnitsApiRipper
{
    class Program
    {
        static void Main(string[] args)
        {
            var ripper = new Ripper();
            var s1 = ripper.Stage1Object(8, -1);
            var deps = s1.filters[0].elements.Select(e => new Department(e))
                                             .ToList();
            var c = new Class(s1.elements.Find(e => e.name == "12.4"), deps);

            var s2 = ripper.Stage2Object(c.Id, DateTime.Now, 8);
            var t = new Timetable(c, s2.result);
        }
    }
}