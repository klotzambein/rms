using System.Collections.Generic;
using System.Linq;

namespace WebUntis.Data
{
    public class Class
    {
        public Class(int id, List<ClassTeacher> teachers, string name, List<string> altNames, List<Department> deps)
        {
            Id = id;
            Teachers = teachers;
            Name = name;
            AltNames = altNames;
            Deps = deps;
        }
        public Class(JsonClassesStage1.Element legacyClass, List<Department> allDeps)
        {
            Id = legacyClass.id;

            Teachers = new List<ClassTeacher>();
            if (legacyClass.classteacher != null)
                Teachers.Add(legacyClass.classteacher);
            if (legacyClass.classteacher2 != null)
                Teachers.Add(legacyClass.classteacher2);

            AltNames = new List<string>();
            Name = null;
            AddName(legacyClass.title);
            AddName(legacyClass.name);
            AddName(legacyClass.longName);
            AddName(legacyClass.alternatename);

            Deps = allDeps.FindAll(d => legacyClass.dids.Contains(d.Id));
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
        public int Id { get; }
        public List<ClassTeacher> Teachers { get; }
        public string Name { get; private set; }
        public List<string> AltNames { get; }
        public List<Department> Deps { get; }

        public override string ToString()
        {
            return $"<{Name}:{string.Join(",", Teachers.Select(t => $"'{(t.name.Length > t.name.Length ? t.name : t.longName)}'"))},{string.Join(",", Deps.Select(d => $"'{d.Name}'"))}>";
        }
    }
}