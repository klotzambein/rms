using System;
using System.Drawing;
using System.Globalization;
using System.Linq;
using IntervalArray;
using WebUnits.Data;
using WebUnits.Util;

namespace WebUnits
{
    class Program
    {
        static void Main(string[] args)
        {
            var ripper = new Ripper();
            var s1 = ripper.Stage1Object();
            var deps = s1.filters[0].elements.Select(e => new Department(e))
                                             .ToList();
            var classes = s1.elements.Select(e => new Class(e, deps))
                                     .FilterAdvanced(teacherFilter: "Hob");

            var @class = classes.First();
            var s2 = ripper.Stage2Object(@class, DateTime.Now);
            var t = new Timetable(@class, s2.result);
        }
    }
}