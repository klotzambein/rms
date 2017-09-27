using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Ical.Net;
using Ical.Net.Serialization;
using Ical.Net.Serialization.iCalendar.Serializers;

namespace ICal
{
    public class ICalCache
    {
        public Encoding CalendarEncoding { get; set; } = Encoding.ASCII;

        private volatile ConcurrentDictionary<string, byte[]> _dict = new ConcurrentDictionary<string, byte[]>();

        public void AddOrUpdate(string id, Calendar cal)
        {
            var serializer = new CalendarSerializer(new SerializationContext());
            var bytes = CalendarEncoding.GetBytes(serializer.SerializeToString(cal));
            _dict.AddOrUpdate(id.ToUpper(), bytes, (oldId, old) => bytes);
        }
        public byte[] Request(string id) => _dict.TryGetValue(id.ToUpper(), out var result) ? result : null;
        public byte[] Invalidate(string id) => _dict.TryRemove(id.ToUpper(), out var removed) ? removed : null;

        public ICalendarProvider GetCalendarProvider(string id) => new NoFilterCalendarProvider(Request(id));

    }

    public class NoFilterCalendarProvider : ICalendarProvider
    {
        private readonly Calendar cal = null;
        private readonly byte[] bytes = null;

        public NoFilterCalendarProvider(byte[] bytes)
        {
            this.bytes = bytes;
            UseBytes = true;
        }
        public NoFilterCalendarProvider(Calendar cal)
        {
            this.cal = cal;
            UseBytes = false;
        }

        public bool UseBytes { get; }

        public Calendar GetCalendar(IEnumerable<CalendarFilter> filters) => (filters == null || filters.Count() == 0) ? cal : null;
        public byte[] GetCalendarBytes(IEnumerable<CalendarFilter> filters) => (filters == null || filters.Count() == 0) ? bytes : null;
    }
}