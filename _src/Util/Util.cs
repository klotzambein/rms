using System;
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Linq;
using IntervalArray;
using WebUnits.Data;

namespace WebUnits.Util
{
    public static class ParseUtil
    {
        public static CourseCode ParseCourseCode(int code)
        {
            if (!Enum.IsDefined(typeof(CourseCode), code))
            {
                Logger.SendEmail("New CourseCode", $"The Code {code} is not in the {nameof(CourseCode)} enum.");
                return CourseCode.Unknown;
            }
            return (CourseCode)code;
        }

        public static LessonCode ParseLessonCode(string code)
        {
            switch (code)
            {
                case "UNTIS_ADDITIONAL": return LessonCode.UNTIS_ADDITIONAL;
                case "UNTIS_LESSON": return LessonCode.UNTIS_LESSON;
                default:
                    Logger.SendEmail("New LessonCode", $"The Code {code} is not in the {nameof(LessonCode)} enum.");
                    return LessonCode.Unknown;
            }
        }

        public static CellState ParseCellState(string state)
        {
            switch (state)
            {
                case "ADDITIONAL": return CellState.ADDITIONAL;
                case "STANDARD": return CellState.STANDARD;
                case "CANCEL": return CellState.CANCEL;
                case "SUBSTITUTION": return CellState.SUBSTITUTION;
                default:
                    Logger.SendEmail("New CellState", $"The State {state} is not in the {nameof(CellState)} enum.");
                    return CellState.Unknown;
            }
        }

        public static Color ParseColor(string color)
        {
            int argb = Int32.Parse(color.Replace("#", ""), NumberStyles.HexNumber);
            return Color.FromArgb(argb);
        }
    }

    public static class ExtensionUtil
    {
        public static Dictionary<TKey, TValue> ToDictNoDups<TKey, TValue>(this IEnumerable<TValue> ienum, Func<TValue, TKey> keySelector)
        {
            var dict = new Dictionary<TKey, TValue>();
            foreach (var item in ienum)
            {
                var key = keySelector(item);
                if (!dict.ContainsKey(key))
                    dict.Add(key, item);
            }
            return dict;
        }

        public static IEnumerable<Class> FilterAdvanced(this IEnumerable<Class> classes, string teacherFilter = null, string nameFilter = null, List<Department> departmentFilter = null)
        {
            if (departmentFilter != null)
            {
                var depIds = departmentFilter.Select(d => d.Id);
                classes = classes
                    .Where(c => c.Deps
                        .Select(d => d.Id)
                        .Intersect(depIds)
                        .Any());
            }

            var sortedClasses = classes
                .Select(c => Tuple.Create(c, 0));

            if (!string.IsNullOrEmpty(teacherFilter))
            {
                sortedClasses = sortedClasses
                    .Select(c =>
                        Tuple.Create(c.Item1, c.Item2,
                            (c.Item1.Teachers.Count == 0) ?
                                int.MaxValue :
                                c.Item1.Teachers.Min(t =>
                                    Math.Min(
                                        t.longName.MatchWith(teacherFilter),
                                        t.name.MatchWith(teacherFilter)))))
                    .Where(t => t.Item3 != int.MaxValue)
                    .Select(t => Tuple.Create(t.Item1, t.Item2 + t.Item3));
            }

            if (!string.IsNullOrEmpty(nameFilter))
            {
                sortedClasses = sortedClasses
                    .Select(c =>
                        Tuple.Create(c.Item1, c.Item2,
                            Math.Min(
                                c.Item1.Name.MatchWith(nameFilter),
                                (c.Item1.AltNames.Count > 0) ?
                                    c.Item1.AltNames.Min(t => t.MatchWith(nameFilter)) :
                                    int.MaxValue)))
                    .Where(t => t.Item3 != int.MaxValue)
                    .Select(t => Tuple.Create(t.Item1, t.Item2 + t.Item3));
            }

            return sortedClasses
                .OrderBy(t => t.Item2)
                .Select(t => t.Item1);
        }

        public static int MatchWith(this string str, string filter)
        {
            var strUpper = str.ToUpper();
            var filterUpper = filter.ToUpper();
            var score = 0;
            var scoreLocal = 0;
            var j = 0;
            for (int i = 0; i < str.Length; i++)
            {
                if (j < filter.Length && strUpper[i] == filterUpper[j])
                {
                    score += scoreLocal * scoreLocal * (str[i] == filter[j] ? 1 : 2);
                    scoreLocal = 0;
                    j++;
                }
                else
                {
                    scoreLocal++;
                }
            }
            if (scoreLocal == str.Length || j != filter.Length)
                return int.MaxValue;
            return score + scoreLocal;

        }

        public static Interval<DateTime> GetInterval(this Lesson lesson) => new Interval<DateTime>(lesson.Start, lesson.Start.AddMinutes(lesson.Duration));
    }
}