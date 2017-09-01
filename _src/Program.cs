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
            var api = new WebUnitsApi("stundenplan.hamburg.de", "hh5849");
            var classes = api.QueryClasses();
            var lessons = api.QueryLessons(classes.Find("mey", "12"), DateTime.Now.AddDays(3));
        }
    }
}