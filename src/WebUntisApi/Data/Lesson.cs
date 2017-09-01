using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Utility;
using static WebUntis.WebUntisUtil;

namespace WebUntis.Data
{
    public class Lesson
    {
        public Lesson(string text, List<LessonInfo> infos, DateTime start, int duration, int priority, CourseCode code, bool isStandard, bool isEvent, int id, int lessonId, int lessonNumber, LessonCode lessonCode, CellState cellState)
        {
            Text = text;
            Infos = infos;
            Start = start;
            Duration = duration;
            Priority = priority;
            Code = code;
            IsStandard = isStandard;
            IsEvent = isEvent;
            Id = id;
            LessonId = lessonId;
            LessonNumber = lessonNumber;
            LessonCode = lessonCode;
            CellState = cellState;
        }

        public Lesson(JsonClassesStage2.Entry legacyCource, IEnumerable<LessonInfo> allInfos)
        {
            Text = "";
            if (legacyCource.lessonText != null) Text += $"lessonText={legacyCource.lessonText}\n";
            if (legacyCource.periodText != null) Text += $"periodText={legacyCource.periodText}\n";
            if (legacyCource.lessonCode != null) Text += $"lessonCode={legacyCource.lessonCode}\n";
            if (legacyCource.cellState != null) Text += $"cellState={legacyCource.cellState}\n";

            Infos = allInfos.Where(i => legacyCource.elements.Exists(e => e.type == i.Type && e.id == i.Id)).ToList();

            Start = new DateTime(legacyCource.date / 10000, (legacyCource.date / 100) % 100, legacyCource.date % 100, legacyCource.startTime / 100, legacyCource.startTime % 100, 0);

            Duration = (legacyCource.endTime / 100 - legacyCource.startTime / 100) * 60 + (legacyCource.endTime % 100 - legacyCource.startTime % 100);

            Priority = legacyCource.priority;

            foreach (var @is in legacyCource.@is)
                switch (@is.Key)
                {
                    case "standard": IsStandard = @is.Value; break;
                    case "additional": IsAdditional = @is.Value; break;
                    case "cancelled": IsCancelled = @is.Value; break;
                    case "substitution": IsSubstitution = @is.Value; break;
                    case "event": IsEvent = @is.Value; break;
                    default:
                        Logger.SendEmail("New Is*", $"Is{@is.Key[0].ToString().ToUpper() + @is.Key.Substring(1)} is not a Variable.");
                        break;
                }

            Id = legacyCource.id;
            LessonId = legacyCource.lessonId;
            LessonNumber = legacyCource.lessonNumber;
            LessonCode = ParseLessonCode(legacyCource.lessonCode);
            Code = ParseCourseCode(legacyCource.code);
            CellState = ParseCellState(legacyCource.cellState);
        }
        public string Text { get; }
        public List<LessonInfo> Infos { get; }
        public DateTime Start { get; }
        public int Duration { get; }
        public int Priority { get; }
        public CourseCode Code { get; }
        public bool IsStandard { get; }
        public bool IsEvent { get; }
        public bool IsCancelled { get; }
        public bool IsAdditional { get; }
        public int Id { get; }
        public int LessonId { get; }
        public int LessonNumber { get; }
        public LessonCode LessonCode { get; }
        public CellState CellState { get; }
        public bool IsSubstitution { get; }
    }
}