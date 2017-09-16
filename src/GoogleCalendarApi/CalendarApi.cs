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
        public readonly string ApplicationName;

        public bool Authenticated { get; private set; } = false;
        public UserCredential Credentials { get; private set; }
        public CalendarService Service { get; private set; }
        public bool Initiated { get; private set; } = false;
        public Dictionary<string, CalendarConfig> Calendars { get; private set; }

        private CalendarApi(string applicationName) { ApplicationName = applicationName; }
        public static CalendarApi Create(string name = "Robins Management System", bool authenticate = true) => CreateAsync(name, authenticate).Result;
        public async static Task<CalendarApi> CreateAsync(string name = "Robins Management System", bool authenticate = true)
        {
            var api = new CalendarApi(name);

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
        public async Task InitEnviroment(string cachePath, Dictionary<string, CalendarConfig> defaultCalendarIds)
        {
            if (File.Exists(cachePath))
                await InitEnviroment(cachePath);
            else
                await InitEnviroment(defaultCalendarIds, cachePath);
        }
        public async Task InitEnviroment(Dictionary<string, CalendarConfig> calendarIds, string cachePath)
        {
            if (Initiated)
                throw new InvalidOperationException("Already initiaded");
            if (!Authenticated)
                throw new InvalidOperationException("Not authenticated");
            var listRes = await Service.CalendarList.List().ExecuteAsync();
            var calendars = new Dictionary<string, CalendarConfig>();
            foreach (var calId in calendarIds)
            {
                var cal = listRes.Items.FirstOrDefault(c => c.Id == calId.Value.Id);
                if (cal != null)
                    calendars.Add(calId.Key, new CalendarConfig(cal));
                else
                    calendars.Add(calId.Key, new CalendarConfig(await AddCalendar(calId.Value)));
            }
            File.WriteAllText(cachePath,
                JsonConvert.SerializeObject(new { calendarIds }));
            Calendars = calendars;
            Initiated = true;
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
            if (config.NotificationSettings != null)
                calListEntry.NotificationSettings = new CalendarListEntry.NotificationSettingsData() { Notifications = config.NotificationSettings };

            var insertReq = Service.CalendarList.Insert(calListEntry);
            if (config.Colors.HasValue)
                insertReq.ColorRgbFormat = !config.Colors.Value.IsLeft;

            return await insertReq.ExecuteAsync();
        }

        public async Task<Event> AddEvent(EventConfig eventConfig, string calendarIndex, bool sendNotifications = false) => await AddEvent(eventConfig, Calendars[calendarIndex].Id, sendNotifications);
        public async Task<Event> AddEvent(EventConfig eventConfig, string calendarId, bool sendNotifications = false/*, int? maxAttendees = null, bool supportsAttachments = false*/)
        {
            var body = new Event();
            body.Start = new EventDateTime()
            {
                DateTime = eventConfig.Start,
                TimeZone = TZConvert.WindowsToIana(eventConfig.TimeZone.Id)
            };
            body.End = new EventDateTime()
            {
                DateTime = eventConfig.End,
                TimeZone = TZConvert.WindowsToIana(eventConfig.TimeZone.Id)
            };
            body.Summary = eventConfig.Summary;
            body.ColorId = eventConfig.ColorId?.ToString();
            body.Description = eventConfig.Description;
            body.Location = eventConfig.Location;
            if (eventConfig.Status.HasValue)
                body.Status = eventConfig.Status.Value.ToString();
            if (eventConfig.Visibility.HasValue)
                body.Visibility = eventConfig.Visibility.Value.ToString().TrimStart('@');
            if (eventConfig.IsTransparent.HasValue)
                body.Transparency = eventConfig.IsTransparent.Value ? "transparent" : "opaque";

            var request = Service.Events.Insert(body, calendarId);
            request.SendNotifications = sendNotifications;
            //request.MaxAttendees = maxAttendees;
            //request.SupportsAttachments = supportsAttachments;

            return await request.ExecuteAsync();
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

        public override string ToString() => ApplicationName;
    }

    public class CalendarConfig
    {
        public string Id;
        public string Summary;
        public string Description;
        public string Location;
        public TimeZoneInfo TimeZone;
        /// <summary> Either<id, Tuple<foreground, background>> </summary>
        public Either<int, Tuple<string, string>>? Colors;
        public bool? Hidden;
        public bool? Selected;
        public AccessRole? AccessRole;
        public IList<EventReminder> DefaultReminders;
        public IList<CalendarNotification> NotificationSettings;
        public CalendarConfig(CalendarListEntry calEntry)
        {
            Id = calEntry.Id;
            Summary = calEntry.Summary;
            Description = calEntry.Description;
            Location = calEntry.Location;
            TimeZone = TZConvert.GetTimeZoneInfo(calEntry.TimeZone); //TODO: Test for null case
            if (!string.IsNullOrWhiteSpace(calEntry.ColorId))
                Colors = new Either<int, Tuple<string, string>>(int.Parse(calEntry.ColorId));
            else if (!string.IsNullOrWhiteSpace(calEntry.ForegroundColor + calEntry.BackgroundColor))
                Colors = new Either<int, Tuple<string, string>>(Tuple.Create(calEntry.ForegroundColor, calEntry.BackgroundColor));
            Hidden = calEntry.Hidden;
            Selected = calEntry.Selected;
            if (!string.IsNullOrWhiteSpace(calEntry.AccessRole))
                AccessRole = (AccessRole)Enum.Parse(typeof(AccessRole), calEntry.AccessRole, true);
            DefaultReminders = calEntry.DefaultReminders;
            NotificationSettings = calEntry.NotificationSettings.Notifications;
        }

        public CalendarConfig(string id = null, string summary = null, string description = null, string location = null, TimeZoneInfo timeZone = null, string summaryOverride = null, Either<int, Tuple<string, string>>? colors = null, bool? hidden = null, bool? selected = null, AccessRole? accessRole = null, List<EventReminder> defaultReminders = null, List<CalendarNotification> notificationSettings = null)
        {
            Id = id;
            Summary = summary;
            Description = description;
            Location = location;
            TimeZone = timeZone;
            Colors = colors;
            Hidden = hidden;
            Selected = selected;
            AccessRole = accessRole;
            DefaultReminders = defaultReminders;
            NotificationSettings = notificationSettings;
        }
    }
    public class EventConfig
    {
        //TODO: Completet Arguments
        // attachments[].fileUrl attendees[].email reminders.overrides[].method reminders.overrides[].minutes ----- Optional Properties ------ anyoneCanAddSelf attendees[] attendees[].additionalGuests attendees[].comment attendees[].displayName attendees[].optional attendees[].responseStatus extendedProperties.private extendedProperties.shared gadget.display gadget.height gadget.iconLink gadget.link gadget.preferences gadget.title gadget.type gadget.width guestsCanInviteOthers guestsCanSeeOtherGuests originalStartTime.date originalStartTime.dateTime originalStartTime.timeZone recurrence[] reminders.overrides[] reminders.useDefault source.title source.url
        public DateTime End { get; private set; }
        public DateTime Start { get; private set; }
        public TimeZoneInfo TimeZone { get; private set; }
        public int? ColorId { get; private set; }
        public string Description { get; private set; }
        public string Summary { get; private set; }
        public string Location { get; private set; }
        public EventStatus? Status { get; private set; }
        public EventVisibility? Visibility { get; private set; }
        public bool? IsTransparent { get; private set; }
        public EventConfig(DateTime end, DateTime start, string summary, TimeZoneInfo timeZone = null, int? colorId = null, string description = null, string location = null, EventStatus? status = null, EventVisibility? visibility = null, bool? isTransparent = null)
        {
            if (string.IsNullOrWhiteSpace(summary))
                throw new ArgumentException($"{nameof(summary)} IsNullOrWhiteSpace", nameof(summary));
            End = end;
            Start = start;
            TimeZone = timeZone;
            ColorId = colorId;
            Description = description;
            Summary = summary;
            Location = location;
            Status = status;
            Visibility = visibility;
            IsTransparent = isTransparent;
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
    public enum EventStatus
    {
        ///<summary>The event is confirmed. This is the default status.</summary>
        confirmed,
        ///<summary>The event is tentatively confirmed.</summary>
        tentative,
        ///<summary>The event is cancelled.</summary>
        cancelled,
    }
    public enum EventVisibility
    {

        ///<summar>Uses the default visibility for events on the calendar. This is the default value.</summary>
        @default,
        ///<summar>The event is public and event details are visible to all readers of the calendar. </summary>
        @public,
        ///<summar>The event is private and only event attendees may view event details.</summary>
        @private,
        ///<summar>The event is private. This value is provided for compatibility reasons.</summary>
        confidential,

    }
}