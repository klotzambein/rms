using System;
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
using System.IO;
using IntervalArray;
using WebUnitsApiRipper.Data;

namespace WebUnitsApiRipper.Util
{
    public static class ParseUtil
    {
        public static CourseCode ParseCourseCode(int code)
        {
            if (!Enum.IsDefined(typeof(CourseCode), code))
            {
                SendEmail.SendNote("New CourseCode", $"The Code {code} is not in the {nameof(CourseCode)} enum.\n\n{File.ReadAllText("tmp.txt")}");
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
                    SendEmail.SendNote("New LessonCode", $"The Code {code} is not in the {nameof(LessonCode)} enum.\n\n{File.ReadAllText("tmp.txt")}");
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
                    SendEmail.SendNote("New CellState", $"The State {state} is not in the {nameof(CellState)} enum.\n\n{File.ReadAllText("tmp.txt")}");
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

        public static Interval<DateTime> GetInterval(this Lesson lesson) => new Interval<DateTime>(lesson.Start, lesson.Start.AddMinutes(lesson.Duration));
    }
}