using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace Mindsweep.Helpers
{
    public static class RTM
    {
        public const string URI_GET_FROB = "http://www.rememberthemilk.com/services/rest/?method=rtm.auth.getFrob";
        public const string URI_AUTH = "http://www.rememberthemilk.com/services/auth/?perms=delete";
        public const string URI_GET_TOKEN = "http://www.rememberthemilk.com/services/rest/?method=rtm.auth.getToken";

        public const string URI_GET_TIMELINE = "http://www.rememberthemilk.com/services/rest/?method=rtm.timelines.create";

        public const string URI_GETLISTS = "http://www.rememberthemilk.com/services/rest/?method=rtm.lists.getList";
        public const string URI_GETTASKS = "http://www.rememberthemilk.com/services/rest/?method=rtm.tasks.getList";
        public const string URI_SETCOMPLETE = "http://www.rememberthemilk.com/services/rest/?method=rtm.tasks.complete";
        
        public static Uri SignRequest(string url, bool formatJson = false, bool authToken = false)
        {
            Dictionary<string, string> qparams = new Dictionary<string, string>();
            Uri u = new Uri(url);

            if (string.IsNullOrEmpty(u.Query))
                throw new ArgumentException("SignRequest does not handle requests without query paramters.");

            qparams.Add("api_key", App.RtmApiKey);

            if (formatJson)
                qparams.Add("format", "json");

            if (authToken)
                qparams.Add("auth_token", App.ViewModel.Token);

            foreach (Match m in Regex.Matches(u.Query.TrimStart('?'), @"([^=]*)=([^&]*)&?"))
            {
                if (m.Success && m.Groups.Count == 3)
                    qparams.Add(m.Groups[1].Value, m.Groups[2].Value);
            }

            string sortedParamsAndValues = App.RtmSecret;
            foreach (string key in qparams.Keys.OrderBy(k => k))
                sortedParamsAndValues += key + qparams[key];


            url = string.Format("{0}&api_key={1}&api_sig={2}", url, App.RtmApiKey, MD5CryptoServiceProvider.GetMd5String(sortedParamsAndValues));

            if (formatJson)
                url = url + "&format=json";

            if (authToken)
                url = url + "&auth_token=" + App.ViewModel.Token;


            return new Uri(url);
        }

        public static Uri SignJsonRequest(string url)
        {
            return SignRequest(url, true, true);
        }

    }
}
