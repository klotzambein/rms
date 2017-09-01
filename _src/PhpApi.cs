using System;
using System.IO;
using System.Net;
using System.Text;
using Newtonsoft.Json;

namespace WebUnits
{
    public class PhpApi
    {
        private static Random random = new Random();


        private static int id()
        {
            var id = random.Next();
            return id;
        }

        private static dynamic request(dynamic json, string server, string school, string session = null)
        {
            var url = (session != null) ?
                $"https://{server}/WebUntis/jsonrpc.do;jsessionid={session}?school={school}" :
                $"https://{server}/WebUntis/jsonrpc.do?school={school}";

            var data = JsonConvert.SerializeObject(json);

            var request = (HttpWebRequest)WebRequest.Create(url);
            request.AllowAutoRedirect = false;
            request.Method = "POST";
            request.Headers.Add(HttpRequestHeader.ContentType, "application/json");

            var dataStream = request.GetRequestStream();
            dataStream.Write(Encoding.UTF8.GetBytes(data), 0, data.Length);

            WebResponse response = null;
            try { response = request.GetResponse(); } catch (WebException e) { response = e.Response; };
            using (var reader = new StreamReader(response.GetResponseStream()))
            {
                return (dynamic)JsonConvert.DeserializeObject(reader.ReadToEnd());
            }
        }

        public static void auth(string server, string school, string user = "#anonymous#", string pass = "")
        {
            var json = new
            {
                id = id(),
                method = "authenticate",
                @params = new
                {
                    user = user,
                    password = pass,
                    client = "web"
                },
                jsonrpc = "2.0"
            };

            var result = request(json, server, school);

            if (result.HasProperty("error"))
            {
                throw new Exception(result.error.ToString());
            }

            var sessionid = result.result.sessionId;
            var klasseid = result.result.klasseId;
            var type = result.result.personType;
            var studentid = result.result.personId;

            //return result;
        }

        /*public static void logout()
        {
            var json = new
            {
                id = id(),
                method = "logout",
                @params = new
                {
                },
                jsonrpc = "2.0"
            };

            return request(json);
        }

        public static void getTimegrid()
        {
            var json = new
            {
                id = id(),
                method = "getTimegridUnits",
                @params = new
                {
                },
                jsonrpc = "2.0"
            };

            return request(json);
        }

        public static void getTimetable()
        {
            var json = new
            {
                id = id(),
                method = "getTimetable",
                @params = new
                {
                    id = studentid,
                    type = type
                },
                jsonrpc = "2.0"
            };

            return request(json);
        }

        public static void getSubjects()
        {
            var json = new
            {
                id = id(),
                method = "getSubjects",
                @params = new
                {
                },
                jsonrpc = "2.0"
            };

            return request(json);
        }

        public static void getTeachers()
        {
            var json = new
            {
                id = id(),
                method = "getTeachers",
                @params = new
                {
                },
                jsonrpc = "2.0"
            };

            return request(json);
        }

        public static void getKlassen()
        {
            var json = new
            {
                id = id(),
                method = "getKlassen",
                @params = new
                {
                },
                jsonrpc = "2.0"
            };

            return request(json);
        }

        public static void getRooms()
        {
            var json = new
            {
                id = id(),
                method = "getRooms",
                @params = new
                {
                },
                jsonrpc = "2.0"
            };

            return request(json);
        }//*/
    }
}
// 176 54970606
// Dienstag 3c Pausenhalle