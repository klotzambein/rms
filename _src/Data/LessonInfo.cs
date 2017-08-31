using System.Collections.Generic;
using System.Drawing;
using WebUnits.Util;

namespace WebUnits.Data
{
    public class LessonInfo
    {
        public LessonInfo(string name, List<string> altNames, List<Color> colors, int id, int type)
        {
            Name = name;
            AltNames = altNames;
            Colors = colors;
            Id = id;
            Type = type;
        }
        public LessonInfo(JsonClassesStage2.Element legacyCourceInfo)
        {
            AltNames = new List<string>();
            AddName(legacyCourceInfo.name);
            AddName(legacyCourceInfo.longName);
            AddName(legacyCourceInfo.alternatename);
            AddName(legacyCourceInfo.displayname);

            Colors = new List<Color>();
            if (!string.IsNullOrEmpty(legacyCourceInfo.backColor)) Colors.Add(ParseUtil.ParseColor(legacyCourceInfo.backColor));
            if (!string.IsNullOrEmpty(legacyCourceInfo.foreColor)) Colors.Add(ParseUtil.ParseColor(legacyCourceInfo.foreColor));

            Id = legacyCourceInfo.id;
            Type = legacyCourceInfo.type;
        }

        private void AddName(string name)
        {
            if (string.IsNullOrEmpty(name))
                return;
            name = name.Trim();
            if (Name == null)
                Name = name;
            if (Name == name || AltNames.Contains(name))
                return;
            AltNames.Add(name);
        }
        public string Name { get; private set; }
        public List<string> AltNames { get; }
        public List<Color> Colors { get; }
        public int Id { get; }
        public int Type { get; }
        public long Key { get => (long)Id + (long)Type << 32; }
    }
}