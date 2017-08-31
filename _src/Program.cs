using System;
using System.Drawing;
using System.Globalization;
using System.Linq;
using IntervalArray;

namespace WebUnitsApiRipper
{
    class Program
    {
        static void Main(string[] args)
        {
            var ripper = new Ripper();
            var s1 = ripper.Stage1Object(8, 5);
            var s2 = ripper.Stage2Object(169, DateTime.Now, 8);
            var entrys = s2.result.data.elementPeriods.Values.First();
            var groups1 = entrys.GroupBy(e => e.cellState);
        }
    }
}
