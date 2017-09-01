using System;
using System.Collections.Generic;
using System.Linq;
using IntervalArray;
using WebUnits.Util;

namespace WebUnits.Data
{
    public class ClassTable
    {
        public ClassTable(DateTime timeStamp, Department activeDepartment, Dictionary<int, Department> departments, Dictionary<int, Class> classes)
        {
            TimeStamp = timeStamp;
            ActiveDepartment = activeDepartment;
            Departments = departments;
            Classes = classes;
        }

        public ClassTable(JsonClassesStage1.RootObject result)
        {
            TimeStamp = DateTimeOffset.FromUnixTimeMilliseconds(result.lastImportTimestamp).DateTime;

            var depList = result.filters[0].elements
                .Select(e => new Department(e))
                .ToList();

            if (result.filters[0].selected >= 0)
                ActiveDepartment = Departments[result.filters[0].selected];

            Departments = depList
                .ToDictNoDups(d => d.Id);

            Classes = result.elements
                .Select(e => new Class(e, depList))
                .ToDictNoDups(c => c.Id);
        }

        public IEnumerable<Class> FindAll(string teacherFilter = null, string nameFilter = null, List<Department> departmentFilter = null)
        {
            return Classes
                .Select(d => d.Value)
                .FilterAdvanced(teacherFilter, nameFilter, departmentFilter);
        }

        public Class Find(string teacherFilter = null, string nameFilter = null, List<Department> departmentFilter = null)
        {
            return FindAll(teacherFilter, nameFilter, departmentFilter).FirstOrDefault();
        }

        public DateTime TimeStamp { get; }
        public Department ActiveDepartment { get; }
        public Dictionary<int, Department> Departments { get; }
        public Dictionary<int, Class> Classes { get; }
    }
}