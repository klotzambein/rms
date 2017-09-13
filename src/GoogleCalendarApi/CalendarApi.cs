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


namespace GoogleCalendarApi
{
    public class CalendarApi
    {
        public readonly static string[] Scopes = { CalendarService.Scope.Calendar };
        public readonly static string ApplicationName = "Robins Management System";

        public bool Authenticated { get; private set; } = false;
        public UserCredential Credentials { get; private set; }

        private CalendarApi() { }
        public async static CalendarApi Create()
        {

        }

        public void Authenticate() => Authenticate(new FileStream("credentials.json", FileMode.Open, FileAccess.Read));
        public async void Authenticate(Stream credStream)
        {
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
                Authenticated = true;
            }
            finally
            {
                credStream.Close();
            }
        }

        public static void Run()
        {
            UserCredential credential;

            using (var stream = new FileStream("credentials.json", FileMode.Open, FileAccess.Read))
            {

                Console.WriteLine("Credential file saved to: " + credPath);
            }

            // Create Google Calendar API service.
            var service = new CalendarService(new BaseClientService.Initializer()
            {
                HttpClientInitializer = credential,
                ApplicationName = ApplicationName,
            });

            // Define parameters of request.
            EventsResource.ListRequest request = service.Events.List("primary");
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
}

/*
{
 "summary": "RMS_Untis",
 "description": "Robins Management System  Web Untis Calendar",
 "location": "Germany",
 "timeZone": "Europe/Berlin"
}
{
 "kind": "calendar#calendar",
 "etag": "\"W8S50vTLOjlc76YTigzSZVQwSEE/7WTmvnXfYWN8batjkduWaAEYDLo\"",
 "id": "jmnh7rjh9ba3cepqanjf1q1q7c@group.calendar.google.com",
 "summary": "RMS_Untis",
 "description": "Robins Management System  Web Untis Calendar",
 "location": "Germany",
 "timeZone": "Europe/Berlin"
}
{
 "id": "jmnh7rjh9ba3cepqanjf1q1q7c@group.calendar.google.com",
 "selected": true,
 "summaryOverride": "Override"
}
{
 "kind": "calendar#calendarListEntry",
 "etag": "\"1504878686378000\"",
 "id": "jmnh7rjh9ba3cepqanjf1q1q7c@group.calendar.google.com",
 "summary": "RMS_Untis",
 "description": "Robins Management System  Web Untis Calendar",
 "location": "Germany",
 "timeZone": "Europe/Berlin",
 "summaryOverride": "Override",
 "colorId": "17",
 "backgroundColor": "#9a9cff",
 "foregroundColor": "#000000",
 "selected": true,
 "accessRole": "owner",
 "defaultReminders": [
 ]
}
 */
