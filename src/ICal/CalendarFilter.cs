using System;

namespace ICal
{
    public class CalendarFilter
    {
        public CalendarFilter(Filter type, DateTime? time = null, string name = null, int? number = null)
        {
            Type = type;

            switch (type)
            {
                case Filter.StartTime:
                case Filter.EndTime:
                    Time = time ?? throw new ArgumentNullException(nameof(time)); break;
                case Filter.Entrys:
                    Number = number ?? throw new ArgumentNullException(nameof(number)); break;
                //case Filters.Something:
                //    Name = string.IsNullOrWhiteSpace(name) ? throw new ArgumentNullException(nameof(name)) : name.Trim(); break;
                default: throw new NotImplementedException();
            }
        }

        public Filter Type { get; }
        public DateTime? Time { get; }
        public string Name { get; }
        public int? Number { get; }
    }
}