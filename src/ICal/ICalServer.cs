using System;
using System.Collections.Specialized;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using Ical.Net.Serialization;
using Ical.Net.Serialization.iCalendar.Serializers;
using NLog;

namespace ICal
{
    public class ICalServer : IDisposable
    {
        //private const string MIME_TYPE = "text/calendar";/*
        private const string MIME_TYPE = "inline; text/text"; //*/

        private const string EXT = ".ics";/*
        private const string EXT = ".txt"; //*/

        private static Logger logger = LogManager.GetCurrentClassLogger();

        public int Port { get; }

        public ICalCache Cache { get; private set; } = null;
        public bool UsingCache { get { return Cache != null; } }

        public Encoding CalendarEncoding { get; private set; } = Encoding.ASCII;

        public string[] Prefixes { get; private set; }
        public bool Running { get; private set; }

        private Thread _listnerThread;
        private volatile Exception _listnerThreadException;

        public ICalServer(int port)
        {
            if (port <= 0)
                throw new ArgumentOutOfRangeException(nameof(port));
            Port = port;
            Prefixes = new[] { $"http://*:{port}/" };
        }

        public ICalServer UseCache(ICalCache cache)
        {
            if (Running)
                throw new InvalidOperationException();

            Cache = cache ?? throw new ArgumentNullException(nameof(cache));

            return this;
        }

        /// <summary>
        /// Set the prefixes to specific prefixes;
        /// </summary>
        /// <param name="prefixes">use \p ("\\p") for port</param>
        /// <returns></returns>
        public ICalServer UseSpecificPrefixes(params string[] prefixes)
        {
            if (Running)
                throw new InvalidOperationException();

            if (prefixes.Length == 0)
                throw new ArgumentException("prefixes.Length == 0", nameof(prefixes));

            Prefixes = prefixes.Select(p => p.Replace("\\p", Port.ToString())).ToArray();
            return this;
        }

        public ICalServer UseEncoding(Encoding encoding)
        {
            if (Running)
                throw new InvalidOperationException();

            CalendarEncoding = encoding ?? throw new ArgumentNullException(nameof(encoding));
            return this;
        }

        private HttpListener _listner;
        private void Listen()
        {
            try
            {
                _listner = new HttpListener();

                foreach (var prefix in Prefixes)
                    _listner.Prefixes.Add(prefix);

                _listner.Start();

                while (true)
                {
                    try
                    {
                        Process(_listner.GetContext());
                    }
                    catch (Exception ex)
                    {
                        logger.Warn(ex, "exeption while handeling request");
                    }
                }
            }
            catch (Exception ex)
            {
                logger.Error(ex, "error while listening for ical requests");
                _listnerThreadException = ex;
            }
        }

        private void Process(HttpListenerContext context)
        {
            try
            {
                logger.Debug("got request '{0}'", context.Request.RawUrl);

                if (!context.Request.Url.AbsolutePath.EndsWith(EXT))
                {
                    logger.Debug("request '{0}' has wrong extension: redirecting", context.Request.RawUrl);
                    try
                    {
                        context.Response.Redirect(RemoveExtension(context.Request.Url.AbsolutePath) + EXT);
                    }
                    catch (Exception ex)
                    {
                        logger.Warn(ex, "error while redirecting request {0}", context.Request.RawUrl);
                        context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;
                    }
                    return;
                }

                var calRec = ParsePath(context.Request.Url.AbsolutePath);
                calRec = ApplyFilters(calRec, context.Request.QueryString);

                var buffer = calRec.GetCalendarBytes(CalendarEncoding);
                if (buffer == null)
                    context.Response.StatusCode = (int)HttpStatusCode.NotFound;
                else
                    try
                    {

                        //Adding permanent http response headers
                        context.Response.ContentType = MIME_TYPE;
                        context.Response.ContentLength64 = buffer.Length;
                        context.Response.ContentEncoding = CalendarEncoding;

                        context.Response.AddHeader("Date", DateTime.Now.ToString("r"));

                        context.Response.OutputStream.Write(buffer, 0, buffer.Length);

                        context.Response.StatusCode = (int)HttpStatusCode.OK;
                        context.Response.OutputStream.Flush();
                    }
                    catch (Exception ex)
                    {
                        logger.Warn(ex, "error while sending response to request {0}", context.Request.RawUrl);
                        context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;
                    }
            }
            finally
            {
                context.Response.OutputStream.Close();
            }
        }

        private static string RemoveExtension(string absPath)
        {
            var match = Regex.Match(absPath, @"^(.*)\.[^.]*$");
            if (match.Success)
                return match.Groups[1].Value;
            else
                return absPath;
        }

        private CalendarResource ParsePath(string path)
        { //TODO
            path = RemoveExtension(path.Trim('/'));
            return new CalendarResource(Cache.GetCalendarProvider(path)/*, new Debug_ICalendarProvider()*/);
        }

        private CalendarResource ApplyFilters(CalendarResource resource, NameValueCollection query)
        { //TODO
            foreach (var q in query.AllKeys)
                switch (q.ToLower())
                {
                    case "s":
                    case "start":
                        resource.Filter(Filter.StartTime, time: ParseTime(query[q]));
                        break;
                    case "e":
                    case "end":
                        resource.Filter(Filter.EndTime, time: ParseTime(query[q]));
                        break;
                    case "n":
                    case "number":
                    case "entrys":
                        resource.Filter(Filter.Entrys, number: int.Parse(query[q].Trim()));
                        break;
                    default:
                        logger.Warn("unknown query: {0}, value: {1}", q, query[q]);
                        break;
                }

            return resource;
        }
        private DateTime ParseTime(string time)
        {
            if (DateTime.TryParseExact(time, new[] { "dd-MM-yyyy HH:mm", "dd-MM-yyyy" }, CultureInfo.InvariantCulture, DateTimeStyles.AllowLeadingWhite | DateTimeStyles.AllowTrailingWhite, out var result))
                return result;
            logger.Warn("cant parse date {0}", time);
            throw new FormatException($"cant parse date {time}");
        }

        public void Start()
        {
            if (Running)
                throw new InvalidOperationException();

            logger.Info("starting ical server");
            _listnerThread = new Thread(Listen);
            _listnerThread.Start();
            Running = true;
        }

        public void Stop()
        {
            if (!Running)
                throw new InvalidOperationException();

            logger.Info("stopping ical server");
            Check();

            _listnerThread.Abort();
            _listner.Stop();
            _listnerThread = null;
            _listner = null;
            Running = false;
        }

        public void Check()
        {
            if (Running && (_listnerThread == null || !_listnerThread.IsAlive))
            {
                if (_listner != null && _listner.IsListening)
                    _listner.Stop();
                _listner = null;
                _listnerThread = null;
                if (_listnerThreadException != null)
                    throw _listnerThreadException;
                logger.Error("generic error while listening");
                throw new Exception("generic error while listening");
            }
        }


        public void Dispose()
        {
            if (Running)
                Stop();
        }
    }
}