using System;
using System.Collections.Generic;
using NLog;
using System.Threading;
using System.Collections.Concurrent;
using System.Linq;

namespace Server
{
    public class Scheduler : IDisposable
    {
        private static Logger logger = LogManager.GetCurrentClassLogger();

        public bool Running { get; private set; }
        private Thread _schedulerThread;
        private volatile Exception _schedulerThreadException;

        private volatile ConcurrentDictionary<Time, IEnumerable<Action>> _entrys = new ConcurrentDictionary<Time, IEnumerable<Action>>();

        public void Add(DateTime time, Action value) =>
            _entrys.AddOrUpdate(new Time(time), new[] { value }, (k, old) => old.Append(value));
        public void AddMultiple(DateTime time, IEnumerable<Action> value) =>
            _entrys.AddOrUpdate(new Time(time), value, (k, old) => old.Concat(value));


        private void Schdule()
        {
            var next = (IEnumerable<Action>)null;
            var nextTime = (Time)null;
            var nextDist = 0;
            while (true)
            {
                if (next != null && nextDist < nextTime.GetDistance(DateTime.Now))
                    foreach (var a in next) a.Invoke();

                foreach (var e in _entrys)
                {
                    var d = e.Key.GetDistance(DateTime.Now);
                    if (d < nextDist)
                    {
                        nextDist = d;
                        nextTime = e.Key;
                        next = e.Value;
                    }
                }
            }
        }

        public void Start()
        {
            if (Running)
                throw new InvalidOperationException();

            logger.Info("starting ical server");
            _schedulerThread = new Thread(Schdule);
            _schedulerThread.Start();
            Running = true;
        }

        public void Stop()
        {
            if (!Running)
                throw new InvalidOperationException();

            logger.Info("stopping ical server");
            Check();

            _schedulerThread.Abort();
            _schedulerThread = null;
            Running = false;
        }

        public void Check()
        {
            if (Running && (_schedulerThread == null || !_schedulerThread.IsAlive))
            {
                _schedulerThread = null;
                if (_schedulerThreadException != null)
                    throw _schedulerThreadException;
                logger.Error("generic error while running schedulre");
                throw new Exception("generic error while running schedulre");
            }
        }

        public class Time
        {
            public Time(int hours, int minutes)
            {
                Hours = hours;
                Minutes = minutes;
            }
            public Time(DateTime time)
            {
                Hours = time.Hour;
                Minutes = time.Minute;
            }

            public int Hours { get; }
            public int Minutes { get; }

            public int GetDistance(DateTime t) =>
                GetDistance(t.Hour, t.Minute);
            public int GetDistance(int hours, int minutes)
            {
                var t1 = hours * 60 + minutes;
                var t2 = Hours * 60 + Minutes;
                if (t2 < t1)
                    t2 += 24 * 60;
                return t2 - t1;
            }
        }

        public void Dispose()
        {
            if (Running)
                Stop();
        }
    }
}
