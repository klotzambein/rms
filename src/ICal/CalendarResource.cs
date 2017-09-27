using System;
using System.Collections.Generic;
using System.Text;
using Ical.Net.Serialization;
using Ical.Net.Serialization.iCalendar.Serializers;

namespace ICal
{
    public class CalendarResource
    {
        public ICalendarProvider[] Providers { get; }

        public List<CalendarFilter> Filters { get; set; } = new List<CalendarFilter>();

        public CalendarResource(params ICalendarProvider[] providers)
        {
            Providers = providers;
        }

        public byte[] GetCalendarBytes(Encoding encoding)
        {
            for (int i = 0; i < Providers.Length; i++)
            {
                var prov = Providers[i];
                if (prov.UseBytes)
                {
                    var bytes = prov.GetCalendarBytes(Filters);
                    if (bytes != null)
                    {
                        return bytes;
                    }
                }
                else
                {
                    var cal = prov.GetCalendar(Filters);
                    if (cal != null)
                    {
                        var serializer = new CalendarSerializer(new SerializationContext());
                        return encoding.GetBytes(serializer.SerializeToString(cal));
                    }
                }
            }
            return null;
        }

        public CalendarResource Filter(Filter filter, DateTime? time = null, string name = null, int? number = null)
        {
            Filters.Add(new CalendarFilter(filter, time, name, number));
            return this;
        }
    }

    public enum Filter
    {
        StartTime,
        EndTime,
        Entrys,
    }
}