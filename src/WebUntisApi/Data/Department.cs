namespace WebUntis.Data
{
    public class Department
    {
        public Department(string name, int id)
        {
            Name = name;
            Id = id;
        }
        public Department(JsonClassesStage1.Element3 legacyDep)
        {
            Name = legacyDep.label;
            Id = legacyDep.id;
        }

        public string Name { get; }
        public int Id { get; }
    }
}