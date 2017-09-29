using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Text.RegularExpressions;
using System.Web;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using WebUntis.Data;
using Utility;
using System.Threading.Tasks;
using NLog;

namespace WebUntis
{
    public class WebUntisApi
    {
        private static readonly Logger logger = LogManager.GetCurrentClassLogger();

        private readonly string loginCookies;
        private readonly object url;

        public WebUntisApi(string url, string school)
        {
            this.url = url;
            loginCookies = GetLoginCookies(school);
        }

        public async Task<ClassTable> QueryClasses() =>
            await QueryClasses(-1);
        public async Task<ClassTable> QueryClasses(Department filter) =>
            await QueryClasses(filter.Id);
        public async Task<ClassTable> QueryClasses(int departmentFilterId)
        {
            var s1 = await Stage1Object(departmentFilterId);
            CheckStage1(s1);
            return new ClassTable(s1);
        }

        public void CheckStage1(JsonClassesStage1.RootObject stage1)
        {
            if (stage1.filters.Count != 1)
                logger.Error($"New Stage 1 Filter: Stage 1 has new filter(s); New Count: {stage1.filters.Count}; Filter(s): [{stage1.filters.Skip(1).Aggregate("", (a, f) => a + f.typeLabel + ",").TrimEnd(',')}]");
        }

        public async Task<TimeTable> QueryLessons(Class @class, DateTime date, Stage2Filter filter = null)
        {
            return new TimeTable(@class, (await Stage2Object(@class, date, filter)).result);
        }

        public async Task<JsonClassesStage1.RootObject> Stage1Object(int filter_departmentId = -1)
        {
            return JsonConvert.DeserializeObject<JsonClassesStage1.RootObject>(await Stage1String(filter_departmentId));
        }
        public async Task<JsonClassesStage1.RootObject> Stage1Object(Department departmentFilter)
        {
            return JsonConvert.DeserializeObject<JsonClassesStage1.RootObject>(await Stage1String(departmentFilter.Id));
        }
        public async Task<JsonClassesStage2.RootObject> Stage2Object(int elementId, DateTime date, Stage2Filter filter = null)
        {
            return JsonConvert.DeserializeObject<JsonClassesStage2.RootObject>(await Stage2String(elementId, date, filter));
        }
        public async Task<JsonClassesStage2.RootObject> Stage2Object(Class @class, DateTime date, Stage2Filter filter = null)
        {
            return JsonConvert.DeserializeObject<JsonClassesStage2.RootObject>(await Stage2String(@class.Id, date, filter));
        }

        public async Task<string> Stage1String(int filter_departmentId = -1, int formatId = 8)
        {
            var query = new Dictionary<string, string>()
            {
                { "ajaxCommand", "getPageConfig" },
                { "type", "1" },
                { "formatId", formatId.ToString() },
                { "filter.departmentId", filter_departmentId.ToString() }
            };
            return await RequestData(query, loginCookies);
        }
        public class Stage2Filter
        {
            public readonly int klasseId;
            public readonly int klasseOrStudentgroupId;
            public readonly int restypeId;
            public readonly int buildingId;
            public readonly int roomGroupId;
            public readonly int departmentId;

            public Stage2Filter(int klasseId = -1, int klasseOrStudentgroupId = -1, int restypeId = -1, int buildingId = -1, int roomGroupId = -1, int departmentId = -1)
            {
                this.klasseId = klasseId;
                this.klasseOrStudentgroupId = klasseOrStudentgroupId;
                this.restypeId = restypeId;
                this.buildingId = buildingId;
                this.roomGroupId = roomGroupId;
                this.departmentId = departmentId;
            }
        }
        public async Task<string> Stage2String(int elementId, DateTime date, Stage2Filter filter = null, int formatId = 8)
        {
            if (filter == null)
                filter = new Stage2Filter();
            var query = new Dictionary<string, string>()
            {
                { "ajaxCommand",                   "getWeeklyTimetable" },
                { "elementType",                   "1" },
                { "elementId",                     elementId.ToString() },
                { "date",                          date.ToString("yyyyMMdd") },
                { "filter.klasseId",               filter.klasseId.ToString() },
                { "filter.klasseOrStudentgroupId", filter.klasseOrStudentgroupId.ToString() },
                { "filter.restypeId",              filter.restypeId.ToString() },
                { "filter.buildingId",             filter.buildingId.ToString() },
                { "filter.roomGroupId",            filter.roomGroupId.ToString() },
                { "filter.departmentId",           filter.departmentId.ToString() },
                { "formatId",                      formatId.ToString() }
            };
            return await RequestData(query, loginCookies);
        }

        private async Task<string> RequestData(Dictionary<string, string> query, string cookieLogin)
        {
            var data = "";
            foreach (var item in query)
                data += $"{HttpUtility.UrlEncode(item.Key)}={HttpUtility.UrlEncode(item.Value)}&";
            data = data.TrimEnd('&');

            WebRequest request = WebRequest.Create($"https://{url}/WebUntis/Timetable.do");
            request.Method = "POST";
            request.Headers.Add(HttpRequestHeader.Cookie, cookieLogin);
            request.Headers.Add(HttpRequestHeader.Accept, "application/json");
            request.ContentType = "application/x-www-form-urlencoded";


            var dataStream = request.GetRequestStream();
            dataStream.Write(Encoding.UTF8.GetBytes(data), 0, data.Length);

            WebResponse response = await request.GetResponseAsync();
            string text;
            using (StreamReader streamReader = new StreamReader(response.GetResponseStream()))
                text = await streamReader.ReadToEndAsync();

            return text;
        }

        private string GetLoginCookies(string school)
        {
            //data
            var data = $"login_url=%2Flogin.do&school={school}";

            //Create request
            var request = (HttpWebRequest)WebRequest.Create($"https://{url}/WebUntis/j_spring_security_check");
            request.Method = "POST";
            request.ContentType = "application/x-www-form-urlencoded";
            request.AllowAutoRedirect = false;

            //Set data in request
            var dataStream = request.GetRequestStream();
            dataStream.Write(Encoding.UTF8.GetBytes(data), 0, data.Length);


            //Get the response
            WebResponse wr = null;
            try { wr = request.GetResponse(); } catch (WebException e) { wr = e.Response; };

            //Get the cookies
            var cookieHeader = wr.Headers["Set-Cookie"];
            var cookies = "";
            var cookieSet = new HashSet<string>();
            foreach (var singleCookie in cookieHeader.Split(','))
            {
                Match match = Regex.Match(singleCookie, "([^=]+)=([^;]+)");
                if (match.Captures.Count == 0)
                    continue;

                string name = match.Groups[1].ToString().Trim();
                string value = match.Groups[2].ToString().Trim('"', ' ');
                if (!cookieSet.Contains(name))
                {
                    cookies += $"{name}={value};";
                    cookieSet.Add(name);
                }
            }

            return cookies.TrimEnd(';');
        }
    }
}