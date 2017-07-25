using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;

namespace R2_Client
{
    public class CommandInterpreter
    {
        private static HttpClient Client = new HttpClient();
        Dictionary<string, Func<WitResult, Task>> commands = new Dictionary<string, Func<WitResult, Task>>();

        public CommandInterpreter()
        {
            InitialiseCommands();
        }

        public void InitialiseCommands()
        {
            commands.Add("greeting", Greeting);
            commands.Add("small_talk", SmallTalk);
            commands.Add("get_weather", GetWeather);
            commands.Add("clear", ClearConsole);
        }

        public async Task ProcessCommandAsync(string command)
        {
            try
            {
                if (commands.ContainsKey(command))
                {
                    await commands[command](null).ConfigureAwait(false);
                }
                else
                {
                    WitIntegrator wit = new WitIntegrator();
                    WitResult result = await wit.TranslateCommandAsync(command).ConfigureAwait(false);
                    if (result.CallSuccess)
                    {
                        if (result.Confidence > 0.50)
                        {
                            await commands[result.Intent](result).ConfigureAwait(false);
                        }
                        else
                        {
                            Console.WriteLine("R2:  I'm not too sure what you meant, can you be more specific?");
                        }
                    }
                    else
                    {
                        Console.WriteLine("R2:  I'm running into problems can you try again?");
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.Print(ex.Message);
                Console.WriteLine("R2:  I'm running into problems can you try again?");
            }            
        }

        public async Task Greeting(WitResult result)
        {
            string[] greetings = new string[]
            {
                "Hi, how do you do?",
                "I'm R2-D2, a bot made to help you with your day to day tasks. How may I be of service",
                "Howdy!",
                "Sup",
                "Hi, how are you?",
                "How may I be of service?"
            };

            Random rnd = new Random();
            int rndIndex = rnd.Next(greetings.Count() - 1);

            Console.WriteLine("R2:  " + greetings[rndIndex]);
        }

        public async Task SmallTalk(WitResult result)
        {
            string[] phrases = new string[]
            {
                "I'm in the middle of organising files.",
                "Not too bad, what are you up to?"
            };

            Random rnd = new Random();
            int rndIndex = rnd.Next(phrases.Count() - 1);

            Console.WriteLine("R2:  " + phrases[rndIndex]);
        }

        public async Task ClearConsole(WitResult result)
        {
            Console.Clear();
        }

        public async Task GetWeather(WitResult result)
        {
            Client.DefaultRequestHeaders.Accept.Clear();
            Client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

            bool dateTimeSpecified = result.JSONObject["entities"]["datetime"] != null;
            bool locationSpecified = result.JSONObject["entities"]["location"] != null;

            string location = "Auckland";

            if (locationSpecified)
            {
                location = (string)result.JSONObject["entities"]["location"].First()["value"];
                float locationConfidence = (float)result.JSONObject["entities"]["location"].First()["confidence"];

                Console.WriteLine($"R2:  Let's see... {location}...");
            }
            else
            {
                Console.WriteLine("R2:  Let me check the weather for you...");
            }

            string url = "https://query.yahooapis.com/v1/public/yql?q=";
            string query = $"select * from weather.forecast where woeid in (select woeid from geo.places(1) where text=\"{ location }\")";
            var response = await Client.GetAsync(url + query + "&format=json").ConfigureAwait(false);
            if (response.IsSuccessStatusCode)
            {
                string jsonResult = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
                JObject jsonObject = JObject.Parse(jsonResult);

                Console.WriteLine("R2:  I've pulled back some information from Yahoo Weather. Have a look:");
                string forecast = (string)jsonObject["query"]["results"]["channel"]["item"]["condition"]["text"];
                float tempFahrenheit = (float)jsonObject["query"]["results"]["channel"]["item"]["condition"]["temp"];

                Console.WriteLine($"R2:  {forecast}");
                Console.WriteLine($"R2:  Temperature: {tempFahrenheit}F/{(tempFahrenheit - 32) * 5 / 9}C");
            }
        }
    }
}
