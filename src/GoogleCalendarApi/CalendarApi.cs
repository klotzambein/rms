using Google.Apis.Auth.OAuth2;
using Google.Apis.Calendar.v3;
using Google.Apis.Calendar.v3.Data;
using Google.Apis.Services;
using Google.Apis.Util.Store;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Google.Apis.Requests;
using Newtonsoft.Json;
using SuccincT.Unions;
using TimeZoneConverter;

namespace GoogleCalendarApi
{
    public class CalendarApi
    {
        public readonly static string[] Scopes = { CalendarService.Scope.Calendar };
        public readonly static string ApplicationName = "Robins Management System";

        public bool Authenticated { get; private set; } = false;
        public UserCredential Credentials { get; private set; }
        public CalendarService Service { get; private set; }

        private CalendarApi() { }
        public static CalendarApi Create(bool authenticate = true) => CreateAsync(authenticate).Result;
        public async static Task<CalendarApi> CreateAsync(bool authenticate = true)
        {
            var api = new CalendarApi();

            if (authenticate)
                await api.Authenticate();

            return api;
        }

        public async Task Authenticate() => await Authenticate(new FileStream("credentials.json", FileMode.Open, FileAccess.Read));
        public async Task Authenticate(Stream credStream)
        {
            if (Authenticated)
                throw new InvalidOperationException("Already authentycated");

            try
            {
                string credPath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
                credPath = Path.Combine(credPath, "RMS", "creds.json");

                GoogleClientSecrets googleClientSecrets = GoogleClientSecrets.Load(credStream);
                ClientSecrets secrets = googleClientSecrets.Secrets;
                Credentials = await GoogleWebAuthorizationBroker.AuthorizeAsync(
                    secrets,
                    Scopes,
                    "user",
                    CancellationToken.None,
                    new FileDataStore(credPath, true));
                if (Credentials == null)
                    throw new AuthorizationException("Credentials null");

                // Create Google Calendar API service.
                Service = new CalendarService(new BaseClientService.Initializer()
                {
                    HttpClientInitializer = Credentials,
                    ApplicationName = ApplicationName,
                });

                Authenticated = true;
            }
            finally
            {
                credStream.Close();
            }
        }

        public async Task InitEnviroment(string cachePath) =>
            await InitEnviroment(
                JsonConvert.DeserializeAnonymousType(
                    File.ReadAllText(cachePath),
                    new { calendarIds = new Dictionary<string, CalendarConfig>() })
                    .calendarIds,
                cachePath);
        public async Task InitEnviroment(Dictionary<string, CalendarConfig> calendarIds, string cachePath)
        {
            var listRes = await Service.CalendarList.List().ExecuteAsync();
            var calendars = new Dictionary<string, CalendarListEntry>();
            foreach (var calId in calendarIds)
            {
                var cal = listRes.Items.FirstOrDefault(c => c.Id == calId.Value.Id);
                if (cal != null)
                    calendars.Add(calId.Key, cal);
                else
                    calendars.Add(calId.Key, await AddCalendar(calId.Value));
            }
            File.WriteAllText(cachePath,
                JsonConvert.SerializeObject(new { calendarIds }));
        }

        public async Task<CalendarListEntry> AddCalendar(CalendarConfig config)
        {
            var cal = new Calendar();
            if (!string.IsNullOrWhiteSpace(config.Description))
                cal.Description = config.Description;
            if (!string.IsNullOrWhiteSpace(config.Summary))
                cal.Summary = config.Summary;
            if (!string.IsNullOrWhiteSpace(config.Location))
                cal.Location = config.Location;
            if (config.TimeZone != null)
                cal.TimeZone = TZConvert.WindowsToIana(config.TimeZone.Id);
            cal = await Service.Calendars.Insert(cal).ExecuteAsync();

            var calListEntry = new CalendarListEntry();
            if (config.AccessRole.HasValue)
                calListEntry.AccessRole = config.AccessRole.ToString();
            if (config.Colors.HasValue)
                if (config.Colors.Value.IsLeft)
                    calListEntry.ColorId = config.Colors.Value.Left.ToString();
                else
                {
                    calListEntry.ForegroundColor = config.Colors.Value.Right.Item1;
                    calListEntry.BackgroundColor = config.Colors.Value.Right.Item2;
                }
            if (config.DefaultReminders != null)
                calListEntry.DefaultReminders = config.DefaultReminders;
            if (config.Hidden.HasValue)
                calListEntry.Hidden = config.Hidden.Value;
            if (config.Selected.HasValue)
                calListEntry.Selected = config.Selected.Value;
            if (!string.IsNullOrWhiteSpace(config.SummaryOverride))
                calListEntry.SummaryOverride = config.SummaryOverride;
            if (config.NotificationSettings != null)
                calListEntry.NotificationSettings = new CalendarListEntry.NotificationSettingsData() { Notifications = config.NotificationSettings };

            var insertReq = Service.CalendarList.Insert(calListEntry);
            if (config.Colors.HasValue)
                insertReq.ColorRgbFormat = !config.Colors.Value.IsLeft;

            return await insertReq.ExecuteAsync();
        }

        //TODO: Remove
        public void Run()
        {
            // Define parameters of request.
            EventsResource.ListRequest request = Service.Events.List("primary");
            request.TimeMin = DateTime.Now;
            request.ShowDeleted = false;
            request.SingleEvents = true;
            request.MaxResults = 10;
            request.OrderBy = EventsResource.ListRequest.OrderByEnum.StartTime;

            // List events.
            Events events = request.Execute();
            Console.WriteLine("Upcoming events:");
            if (events.Items != null && events.Items.Count > 0)
            {
                foreach (var eventItem in events.Items)
                {
                    string when = eventItem.Start.DateTime.ToString();
                    if (String.IsNullOrEmpty(when))
                    {
                        when = eventItem.Start.Date;
                    }
                    Console.WriteLine("{0} ({1})", eventItem.Summary, when);
                }
            }
            else
            {
                Console.WriteLine("No upcoming events found.");
            }
            Console.Read();
        }
    }

    public class CalendarConfig
    {
        public string Id;
        public string Summary;
        public string Description;
        public string Location;
        public TimeZoneInfo TimeZone;
        public string SummaryOverride;
        /// <summary> Either<id, Tuple<foreground, background>> </summary>
        public Either<int, Tuple<string, string>>? Colors;
        public bool? Hidden;
        public bool? Selected;
        public AccessRole? AccessRole;
        public List<EventReminder> DefaultReminders;
        public List<CalendarNotification> NotificationSettings;
        public CalendarConfig(string id = null, string summary = null, string description = null, string location = null, TimeZoneInfo timeZone = null, string summaryOverride = null, Either<int, Tuple<string, string>>? colors = null, bool? hidden = null, bool? selected = null, AccessRole? accessRole = null, List<EventReminder> defaultReminders = null, List<CalendarNotification> notificationSettings = null)
        {
            Id = id;
            Summary = summary;
            Description = description;
            Location = location;
            TimeZone = timeZone;
            SummaryOverride = summaryOverride;
            Colors = colors;
            Hidden = hidden;
            Selected = selected;
            AccessRole = accessRole;
            DefaultReminders = defaultReminders;
            NotificationSettings = notificationSettings;
        }
    }
    public enum AccessRole
    {
        /// <summary>Provides read access to free/busy information.</summary>    
        freeBusyReader,
        /// <summary>Provides read access to the calendar. Private events will appear to users with reader access, but event details will be hidden.</summary>
        reader,
        /// <summary>Provides read and write access to the calendar. Private events will appear to users with writer access, and event details will be visible.</summary>
        writer,
        /// <summary>Provides ownership of the calendar. This role has all of the permissions of the writer role with the additional ability to see and manipulate ACLs.</summary>
        owner,
    }
}