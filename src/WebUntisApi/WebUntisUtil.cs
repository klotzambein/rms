using System;
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Linq;
using NLog;
using Utility;
using Utility.IntervalArray;
using WebUntis.Data;

namespace WebUntis
{
    internal static class WebUntisUtil
    {
        private static readonly Logger logger = LogManager.GetCurrentClassLogger();

        public static CourseCode ParseCourseCode(int code)
        {
            if (!Enum.IsDefined(typeof(CourseCode), code))
            {
                logger.Error($"New CourseCode: The Code {code} is not in the {nameof(CourseCode)} enum.");
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
                    logger.Error($"New LessonCode: The Code {code} is not in the {nameof(LessonCode)} enum.");
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
                    logger.Error($"New CellState: The State {state} is not in the {nameof(CellState)} enum.");
                    return CellState.Unknown;
            }
        }

        public static Color ParseColor(string color)
        {
            int argb = Int32.Parse(color.Replace("#", ""), NumberStyles.HexNumber);
            return Color.FromArgb(argb);
        }

        public static string GetPath(string fileName)
        {
            return Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "RMS", fileName);
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
                .Select(c => (@class: c, score: 0));

            if (!string.IsNullOrEmpty(teacherFilter))
            {
                sortedClasses = sortedClasses
                    .Select(c =>
                            (c.Item1, c.Item2,
                            (c.Item1.Teachers.Count == 0) ?
                                int.MaxValue :
                                c.Item1.Teachers.Min(t =>
                                    Math.Min(
                                        t.longName.MatchWith(teacherFilter),
                                        t.name.MatchWith(teacherFilter)))))
                    .Where(t => t.Item3 != int.MaxValue)
                    .Select(t => (t.Item1, t.Item2 + t.Item3));
            }

            if (!string.IsNullOrEmpty(nameFilter))
            {
                sortedClasses = sortedClasses
                    .Select(c =>
                        (@class: c.@class, score: c.score,
                            newScore: Math.Min(
                                c.Item1.Name.MatchWith(nameFilter),
                                (c.Item1.AltNames.Count > 0) ?
                                    c.Item1.AltNames.Min(t => t.MatchWith(nameFilter)) :
                                    int.MaxValue)))
                    .Where(t => t.newScore != int.MaxValue)
                    .Select(t => (t.@class, t.score + t.newScore));
            }

            return sortedClasses
                .OrderBy(t => t.score)
                .Select(t => t.@class);
        }

        public static int Score(this Lesson l, string lessonFilter)
        {
            return new[] {
                l.LessonText.MatchWith(lessonFilter),
                l.PeriodText.MatchWith(lessonFilter) }
                .Concat(l.Infos.Select(i =>
                    Math.Min(
                        i.Name.MatchWith(lessonFilter),
                        i.AltNames.Select(n =>
                            n.MatchWith(lessonFilter)).Min())))
                .Min();
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