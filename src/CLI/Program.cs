using System;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;
using WebUntis;

namespace CLI
{
    class Program
    {
        static void Main(string[] args)
        {
            if (args.Length > 0 && (args[0] == "timetable" || args[0] == "t"))
                Timetable(args.Skip(1).ToArray());
            else if (args.Length > 0 && (args[0] == "classes" || args[0] == "c"))
                Classes(args.Skip(1).ToArray());
            else
                Console.Error.WriteLine(@"
Command usage: 
    'webuntis timetable|t' => display weekly timetable
    'webuntis classes|c' => display all classes");
        }

        private static void Classes(string[] args)
        {
            string teacherFilter = null;
            string classNameFilter = null;
            string serverUrl = "stundenplan.hamburg.de";
            string school = "hh5849";

            for (int i = 0; i < args.Length; i++)
            {
                switch (args[i])
                {
                    case "--teacher":
                    case "-t":
                        teacherFilter = args[++i];
                        break;
                    case "--class":
                    case "-c":
                        classNameFilter = args[++i];
                        break;
                    case "--url":
                    case "-u":
                        serverUrl = args[++i];
                        if (Regex.Match(serverUrl, "[^.:/?&@_-]+(\\.[^.:/?&@_-]+)+").Success)
                        {
                            Console.Error.WriteLine("Error: Url has to be in format [^.:/?&@_-]+(\\.[^.:/?&@_-]+)+ without protocol, path or query.");
                            goto help;
                        }
                        break;
                    case "--school":
                    case "-s":
                        school = args[++i];
                        break;
                    default:
                        Console.Error.WriteLine("Error parsing Arguments");
                        goto help;
                    case "--help":
                    case "-h":
                    case "-?":
                    help: WriteClassesHelp(); return;
                }
            }

            var api = new WebUntisApi(serverUrl, school);
            var classes = api.QueryClasses();

            var selectedClasses = classes
                .FindAll(teacherFilter, classNameFilter);

            if (selectedClasses.Any())
                foreach (var c in selectedClasses)
                    Console.WriteLine(c);
            else
                Console.WriteLine("No classes match filter or no classes at all");
        }

        private static void WriteClassesHelp()
        {
            Console.Error.WriteLine(@"
Help:
    --teacher -s: Filter string for class teacher
    --class -r: Filter string for class
    --url -d: Server url without protocol, path or query; Default: stundenplan.hamburg.de
    --school -f: School id; Default: hh5849
    --help -h -?: Display this help");
        }

        static void Timetable(string[] args)
        {
            string teacherFilter = null;
            string classNameFilter = null;
            string serverUrl = "stundenplan.hamburg.de";
            string school = "hh5849";
            DateTime date = DateTime.Now;

            for (int i = 0; i < args.Length; i++)
            {
                switch (args[i])
                {
                    case "--teacher":
                    case "-t":
                        teacherFilter = args[++i];
                        break;
                    case "--class":
                    case "-c":
                        classNameFilter = args[++i];
                        break;
                    case "--url":
                    case "-u":
                        serverUrl = args[++i];
                        if (Regex.Match(serverUrl, "[^.:/?&@_-]+(\\.[^.:/?&@_-]+)+").Success)
                        {
                            Console.Error.WriteLine("Error: Url has to be in format [^.:/?&@_-]+(\\.[^.:/?&@_-]+)+ without protocol, path or query.");
                            goto help;
                        }
                        break;
                    case "--school":
                    case "-s":
                        school = args[++i];
                        break;
                    case "--date":
                    case "-d":
                        if (!DateTime.TryParseExact(args[++i], "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.None, out date))
                        {
                            Console.Error.WriteLine("Error: Date has to be in format yyyy-mm-dd");
                            goto help;
                        }
                        break;
                    default:
                        Console.Error.WriteLine("Error parsing Arguments");
                        goto help;
                    case "--help":
                    case "-h":
                    case "-?":
                    help: WriteTimeTableHelp(); return;
                }
            }

            if (classNameFilter == null && teacherFilter == null)
            {
                Console.Error.WriteLine("Error: Either --teacher or --class have to be specified");
                WriteTimeTableHelp();
                return;
            }

            var api = new WebUntisApi(serverUrl, school);
            var classes = api.QueryClasses();

            var selectedClasses = classes
                .FindAll(teacherFilter, classNameFilter);

            if (selectedClasses.Count() == 0)
            {
                Console.Error.WriteLine("Error: No class fits filter"); return;
            }
            else if (selectedClasses.Count() == 1)
                Console.WriteLine($"Found class: {selectedClasses.First()}");
            else
                Console.WriteLine($"Multiple classes fit filter: [{string.Join(",", selectedClasses)}], Selecting {selectedClasses.First()}");

            var lessons = api.QueryLessons(selectedClasses.First(), date);

            foreach (var l in lessons.Lessons)
                Console.WriteLine(l);
        }

        private static void WriteTimeTableHelp()
        {
            Console.Error.WriteLine(@"
Help:
    --teacher -s: Filter string for class teacher
    --class -r: Filter string for class
    --url -d: Server url without protocol, path or query; Default: stundenplan.hamburg.de
    --school -f: School id; Default: hh5849
    --date -e: Date in week, format: yy-mm-dd; Default current date
    --help -h -?: Display this help
    
    Either --teacher or --class have to be specified.");
        }
    }
}
