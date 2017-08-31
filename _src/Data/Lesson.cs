using System;
using System.Collections.Generic;
using System.IO;
using WebUnitsApiRipper.Util;

namespace WebUnitsApiRipper.Data
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

        public Lesson(JsonClassesStage2.Entry legacyCource, List<LessonInfo> allInfos)
        {
            Text = "";
            if (legacyCource.lessonText != null) Text += $"lessonText={legacyCource.lessonText}\n";
            if (legacyCource.periodText != null) Text += $"periodText={legacyCource.periodText}\n";
            if (legacyCource.lessonCode != null) Text += $"lessonCode={legacyCource.lessonCode}\n";
            if (legacyCource.cellState != null) Text += $"cellState={legacyCource.cellState}\n";

            Infos = allInfos.FindAll(i => legacyCource.elements.Exists(e => e.type == i.Type && e.id == i.Id));

            Start = new DateTime(legacyCource.date / 10000, (legacyCource.date / 100) % 100, legacyCource.date % 100, legacyCource.startTime / 100, legacyCource.startTime % 100, 0);

            Duration = (legacyCource.endTime / 100 - legacyCource.startTime / 100) * 60 + (legacyCource.endTime % 100 - legacyCource.startTime % 100);

            Priority = legacyCource.priority;

            foreach (var @is in legacyCource.@is)
                switch (@is.Key)
                {
                    case "standars": IsStandard = @is.Value; break;
                    case "event": IsEvent = @is.Value; break;
                    default:
                        SendEmail.SendNote("New Is*", $"Is{@is.Key[0].ToString().ToUpper() + @is.Key.Substring(1)} is not a Variable.\n\n{File.ReadAllText("tmp.txt")}");
                        break;
                }

            Id = legacyCource.id;
            LessonId = legacyCource.lessonId;
            LessonNumber = legacyCource.lessonNumber;
            LessonCode = ParseUtil.ParseLessonCode(legacyCource.lessonCode);
            Code = ParseUtil.ParseCourseCode(legacyCource.code);
            CellState = ParseUtil.ParseCellState(legacyCource.cellState);
        }
        public string Text { get; }
        public List<LessonInfo> Infos { get; }
        public DateTime Start { get; }
        public int Duration { get; }
        public int Priority { get; }
        public CourseCode Code { get; }
        public bool IsStandard { get; }
        public bool IsEvent { get; }
        public int Id { get; }
        public int LessonId { get; }
        public int LessonNumber { get; }
        public LessonCode LessonCode { get; }
        public CellState CellState { get; }
    }
}