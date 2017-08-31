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
using WebUnits.Data;

namespace WebUnits
{
    public class WebUnitsApi
    {
        private readonly string loginCookies;

        public WebUnitsApi()
        {
            loginCookies = GetLoginCookies();
        }

        public string Stage1String(int filter_departmentId = -1, int formatId = 8)
        {
            var query = new Dictionary<string, string>()
            {
                { "ajaxCommand", "getPageConfig" },
                { "type", "1" },
                { "formatId", formatId.ToString() },
                { "filter.departmentId", filter_departmentId.ToString() }
            };
            return RequestData(query, loginCookies);
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
        public string Stage2String(int elementId, DateTime date, Stage2Filter filter = null, int formatId = 8)
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
            return RequestData(query, loginCookies);
        }
        public JsonClassesStage1.RootObject Stage1Object(int filter_departmentId = -1)
        {
            return JsonConvert.DeserializeObject<JsonClassesStage1.RootObject>(Stage1String(filter_departmentId));
        }
        public JsonClassesStage1.RootObject Stage1Object(Department departmentFilter)
        {
            return JsonConvert.DeserializeObject<JsonClassesStage1.RootObject>(Stage1String(departmentFilter.Id));
        }
        public JsonClassesStage2.RootObject Stage2Object(int elementId, DateTime date, Stage2Filter filter = null)
        {
            return JsonConvert.DeserializeObject<JsonClassesStage2.RootObject>(Stage2String(elementId, date, filter));
        }
        public JsonClassesStage2.RootObject Stage2Object(Class @class, DateTime date, Stage2Filter filter = null)
        {
            return JsonConvert.DeserializeObject<JsonClassesStage2.RootObject>(Stage2String(@class.Id, date, filter));
        }

        private static string RequestData(Dictionary<string, string> query, string cookieLogin)
        {
            var data = "";
            foreach (var item in query)
                data += $"{HttpUtility.UrlEncode(item.Key)}={HttpUtility.UrlEncode(item.Value)}&";
            data = data.TrimEnd('&');

            WebRequest request = WebRequest.Create("https://stundenplan.hamburg.de/WebUntis/Timetable.do");
            request.Method = "POST";
            request.Headers.Add(HttpRequestHeader.Cookie, cookieLogin);
            request.Headers.Add(HttpRequestHeader.Accept, "application/json");
            request.ContentType = "application/x-www-form-urlencoded";


            var dataStream = request.GetRequestStream();
            dataStream.Write(Encoding.UTF8.GetBytes(data), 0, data.Length);

            string text = new StreamReader(request.GetResponse().GetResponseStream()).ReadToEnd();
            File.WriteAllText("tmp.json", text);
            return text;
        }

        private static string GetLoginCookies(string school = "hh5849")
        {
            //data
            var data = $"login_url=%2Flogin.do&school={school}";

            //Create request
            var request = (HttpWebRequest)WebRequest.Create("https://stundenplan.hamburg.de/WebUntis/j_spring_security_check");
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