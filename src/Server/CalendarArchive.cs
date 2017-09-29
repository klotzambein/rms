using System.IO;
using PathTools = System.IO.Path;
using System.IO.Compression;
using System;
using System.Collections.Concurrent;
using Ical.Net;
using Ical.Net.Serialization;
using Ical.Net.Serialization.iCalendar.Serializers;
using System.Text;
using System.Linq;
using System.Text.RegularExpressions;
using NLog;

public class CalendarArchive : IDisposable
{
    private static readonly Logger logger = LogManager.GetCurrentClassLogger();

    private readonly ConcurrentDictionary<string, Calendar> _calendars = new ConcurrentDictionary<string, Calendar>();
    private object _fileLock = new object();

    public string Path { get; }
    public Encoding CalendarEncoding { get; }
    public bool Saved { get; private set; } = false;
    public DateTime _lastSaved;

    public CalendarArchive(string path, Encoding calendarEncoding)
    {
        if (!Directory.Exists(PathTools.GetDirectoryName(path)))
            throw new System.ArgumentException("path does not exist", nameof(path));
        Path = path;
        CalendarEncoding = calendarEncoding ?? throw new ArgumentNullException(nameof(calendarEncoding));

        if (File.Exists(path))
            Load();
    }

    public void AddOrUpdate(string id, Calendar cal)
    {
        if (disposedValue)
            throw new InvalidOperationException("already disposed");

        if (!Regex.IsMatch(id, "^[A-Za-z0-9_-]+$"))
            throw new ArgumentException("does not comply with ^[A-Za-z0-9_-]+$", nameof(id));
        if (cal == null)
            throw new ArgumentNullException(nameof(cal));

        lock (_fileLock)
        {
            _calendars.AddOrUpdate(id.ToUpper(), cal, (oldId, old) => cal);
            Saved = false;
        }
    }

    private void Load()
    {
        lock (_fileLock)
        {
            _calendars.Clear();
            var serializer = new CalendarSerializer(new SerializationContext());
            using (var file = ZipFile.OpenRead(Path))
            {
                foreach (var c in file.Entries)
                {
                    if (!c.Name.EndsWith(".ics"))
                        continue;
                    using (var f = c.Open())
                    {
                        AddOrUpdate(c.Name.Substring(0, c.Name.Length - 4), (Calendar)serializer.Deserialize(f, CalendarEncoding));
                    }
                }
            }
            _lastSaved = DateTime.Now;
            Saved = true;
        }
    }

    public void Save()
    {
        logger.Info("Saving Calendars to: {0}", Path);
        lock (_fileLock)
        {
            File.Copy(Path, PathTools.ChangeExtension(Path, ".ics-backup"), true);

            var serializer = new CalendarSerializer(new SerializationContext());
            using (var file = ZipFile.Open(Path, ZipArchiveMode.Create))
            {
                foreach (var c in _calendars)
                {
                    var f = file.CreateEntry(c.Key.ToLower());
                    using (var s = f.Open())
                    {
                        serializer.Serialize(c.Value, s, CalendarEncoding);
                    }
                }
            }
            _lastSaved = DateTime.Now;
            Saved = true;
        }
    }

    public void Save(TimeSpan dirtyTimeout)
    {
        if ((DateTime.Now - _lastSaved) > dirtyTimeout)
            Save();
    }

    #region IDisposable Support
    private bool disposedValue = false; // To detect redundant calls

    protected virtual void Dispose(bool disposing)
    {
        if (!disposedValue)
        {
            if (disposing)
            {
                // TODO: dispose managed state (managed objects).
            }

            Save();

            disposedValue = true;
        }
    }

    // TODO: override a finalizer only if Dispose(bool disposing) above has code to free unmanaged resources.
    ~CalendarArchive()
    {
        // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
        Dispose(false);
    }

    // This code added to correctly implement the disposable pattern.
    public void Dispose()
    {
        // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
        Dispose(true);
        // TODO: uncomment the following line if the finalizer is overridden above.
        GC.SuppressFinalize(this);
    }
    #endregion
}