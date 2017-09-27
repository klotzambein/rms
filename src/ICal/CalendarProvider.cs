using System;
using System.Collections.Generic;
using Ical.Net;
using Ical.Net.DataTypes;

namespace ICal
{
    public interface ICalendarProvider
    {
        Calendar GetCalendar(IEnumerable<CalendarFilter> filters);
        bool UseBytes { get; }
        byte[] GetCalendarBytes(IEnumerable<CalendarFilter> filters);
    }

    internal class Debug_ICalendarProvider : ICalendarProvider
    {
        public bool UseBytes => false;
        public byte[] GetCalendarBytes(IEnumerable<CalendarFilter> filters) => throw new NotSupportedException();

        public Calendar GetCalendar(IEnumerable<CalendarFilter> filters)
        {
            var now = DateTime.Now;
            var later = now.AddHours(1);

            Calendar calendar = new Calendar();
            calendar.Events.Add(new CalendarEvent()
            {
                DtStart = new CalDateTime(now),
                DtEnd = new CalDateTime(later),
                Name = "My Event",
            });

            return calendar;
        }

    }
}