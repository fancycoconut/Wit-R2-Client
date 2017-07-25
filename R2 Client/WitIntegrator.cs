using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using System.Xml.Linq;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;

namespace R2_Client
{
    public class WitIntegrator
    {
        private static HttpClient Client = new HttpClient();
        private Dictionary<string, WitResult> resultCache = new Dictionary<string, WitResult>();


        public async Task<WitResult> TranslateCommandAsync(string command)
        {
            WitResult result;
            if (resultCache.ContainsKey(command))
            {
                result = resultCache[command];
            }
            else
            {
                result = await RetrieveResultFromWit(command).ConfigureAwait(false);
                resultCache.Add(command, result);
            }

            return result;
        }

        public async Task<WitResult> RetrieveResultFromWit(string command)
        {
            Client.DefaultRequestHeaders.Accept.Clear();
            Client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            Client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", "<<WIT TOKEN HERE>>");

            WitResult result = new WitResult();
            var response = await Client.GetAsync("https://api.wit.ai/message?q=" + command).ConfigureAwait(false);
            if (response.IsSuccessStatusCode)
            {
                string jsonResult = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
                JObject jsonObject = JObject.Parse(jsonResult);

                result.CallSuccess = true;
                result.Intent = (string)jsonObject["entities"]["intent"].First()["value"];
                result.Confidence = (float)jsonObject["entities"]["intent"].First()["confidence"];
                result.JSONObject = jsonObject;

            }
            else
            {
                result.CallSuccess = false;
            }

            return result;
        }
    }

    public class WitResult
    {
        public bool CallSuccess { get; set; }
        public string Intent { get; set; }
        public float Confidence { get; set; }
        public JObject JSONObject { get; set; }
    }
}
