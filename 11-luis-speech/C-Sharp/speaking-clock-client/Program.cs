using System;
using Newtonsoft.Json.Linq;
using Microsoft.Extensions.Configuration;
using System.Threading.Tasks;

// Import namespaces
// Import namespaces
using Microsoft.CognitiveServices.Speech;
using Microsoft.CognitiveServices.Speech.Audio;
using Microsoft.CognitiveServices.Speech.Intent;
using System.Media;

namespace speaking_clock_client
{
    class Program
    {

        static async Task Main(string[] args)
        {
            try
            {
                // Get config settings from AppSettings
                IConfigurationBuilder builder = new ConfigurationBuilder().AddJsonFile("appsettings.json");
                IConfigurationRoot configuration = builder.Build();
                string luAppId = configuration["LuAppID"];
                string predictionRegion = configuration["LuPredictionRegion"];
                string predictionKey = configuration["LuPredictionKey"];

                // Configure speech service and get intent recognizer
                SpeechConfig speechConfig = SpeechConfig.FromSubscription(predictionKey, predictionRegion);
                speechConfig.SpeechRecognitionLanguage = "hi-HI";
                AudioConfig audioConfig = AudioConfig.FromDefaultMicrophoneInput();
                IntentRecognizer recognizer = new IntentRecognizer(speechConfig, audioConfig);

                // Get the model from the AppID and add the intents we want to use
                // Get the model from the AppID and add the intents we want to use
                var model = LanguageUnderstandingModel.FromAppId(luAppId);
                recognizer.AddIntent(model, "GetTime", "time");
                recognizer.AddIntent(model, "GetDate", "date");
                recognizer.AddIntent(model, "GetDay", "day");
                recognizer.AddIntent(model, "None", "none");

                

                // Process speech input
                // Process speech input
                string intent = "";
                var stopRecognition = new TaskCompletionSource<int>();
                Console.WriteLine("Hello, what can I do for you?");
                IntentRecognitionResult result = null;
                recognizer.Recognizing += (s, e) =>
                {
                    Console.WriteLine($"RECOGNIZING: Text={e.Result.Text}");
                };

                recognizer.Recognized += (s, e) =>
                {
                    if (e.Result.Reason == ResultReason.RecognizedIntent)
                    {
                        Console.WriteLine($"RECOGNIZED: Text={e.Result.Text}");
                        result = e.Result;
                        stopRecognition.TrySetResult(0);

                    }
                    else if (e.Result.Reason == ResultReason.NoMatch)
                    {
                        Console.WriteLine($"NOMATCH: Speech could not be recognized.");
                    }
                };

                recognizer.Canceled += (s, e) =>
                {
                    Console.WriteLine($"CANCELED: Reason={e.Reason}");

                    if (e.Reason == CancellationReason.Error)
                    {
                        Console.WriteLine($"CANCELED: ErrorCode={e.ErrorCode}");
                        Console.WriteLine($"CANCELED: ErrorDetails={e.ErrorDetails}");
                        Console.WriteLine($"CANCELED: Did you update the speech key and location/region info?");
                    }

                    stopRecognition.TrySetResult(0);
                };

                recognizer.SessionStopped += (s, e) =>
                {
                    Console.WriteLine("\n    Session stopped event.");
                    stopRecognition.TrySetResult(0);
                };

                await recognizer.StartContinuousRecognitionAsync();

                // Waits for completion. Use Task.WaitAny to keep the task rooted.
                Task.WaitAny(new[] { stopRecognition.Task });
                await recognizer.StopContinuousRecognitionAsync();
                // make the following call at some point to stop recognition.
                // await recognizer.StopContinuousRecognitionAsync();
                //IntentRecognitionResult result = await recognizer.RecognizeOnceAsync().ConfigureAwait(false);

                if (result.Reason == ResultReason.RecognizedIntent)
                {
                    // Intent was identified
                    intent = result.IntentId;
                    Console.WriteLine($"Query: {result.Text}");
                    Console.WriteLine($"Intent Id: {intent}.");
                    string jsonResponse = result.Properties.GetProperty(PropertyId.LanguageUnderstandingServiceResponse_JsonResult);
                    Console.WriteLine($"JSON Response:\n{jsonResponse}\n");

                    // Get the first entity (if any)
                    // Get the first entity (if any)
                    JObject jsonResults = JObject.Parse(jsonResponse);
                    string entityType = "";
                    string entityValue = "";
                    if (jsonResults["entities"].HasValues)
                    {
                        JArray entities = new JArray(jsonResults["entities"][0]);
                        entityType = entities[0]["type"].ToString();
                        entityValue = entities[0]["entity"].ToString();
                        Console.WriteLine(entityType + ": " + entityValue);
                    }

                    // Apply the appropriate action
                    //Apply the appropriate action
                    switch (intent)
                    {
                        case "time":
                            var location = "local";
                            // Check for entities
                            if (entityType == "Location")
                            {
                                location = entityValue;
                            }
                            // Get the time for the specified location
                            var getTimeTask = Task.Run(() => GetTime(location));
                            string timeResponse = await getTimeTask;
                            Console.WriteLine(timeResponse);
                            var synthesizer = new SpeechSynthesizer(speechConfig, audioConfig);
                            await synthesizer.SpeakTextAsync(timeResponse);
                            break;
                        case "day":
                            var date = DateTime.Today.ToShortDateString();
                            // Check for entities
                            if (entityType == "Date")
                            {
                                date = entityValue;
                            }
                            // Get the day for the specified date
                            var getDayTask = Task.Run(() => GetDay(date));
                            string dayResponse = await getDayTask;
                            Console.WriteLine(dayResponse);
                            break;
                        case "date":
                            var day = DateTime.Today.DayOfWeek.ToString();
                            // Check for entities
                            if (entityType == "Weekday")
                            {
                                day = entityValue;
                            }

                            var getDateTask = Task.Run(() => GetDate(day));
                            string dateResponse = await getDateTask;
                            Console.WriteLine(dateResponse);
                            break;
                        default:
                            // Some other intent (for example, "None") was predicted
                            Console.WriteLine("You said " + result.Text.ToLower());
                            if (result.Text.ToLower().Replace(".", "") == "stop")
                            {
                                intent = result.Text;
                            }
                            else
                            {
                                Console.WriteLine("Try asking me for the time, the day, or the date.");
                            }
                            break;
                    }

                }
                else if (result.Reason == ResultReason.RecognizedSpeech)
                {
                    // Speech was recognized, but no intent was identified.
                    intent = result.Text;
                    Console.Write($"I don't know what {intent} means.");
                }
                else if (result.Reason == ResultReason.NoMatch)
                {
                    // Speech wasn't recognized
                    Console.WriteLine($"Sorry. I didn't understand that.");
                }
                else if (result.Reason == ResultReason.Canceled)
                {
                    // Something went wrong
                    var cancellation = CancellationDetails.FromResult(result);
                    Console.WriteLine($"CANCELED: Reason={cancellation.Reason}");
                    if (cancellation.Reason == CancellationReason.Error)
                    {
                        Console.WriteLine($"CANCELED: ErrorCode={cancellation.ErrorCode}");
                        Console.WriteLine($"CANCELED: ErrorDetails={cancellation.ErrorDetails}");
                    }
                }

            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }


        static string GetTime(string location)
        {
            var timeString = "";
            var time = DateTime.Now;

            /* Note: To keep things simple, we'll ignore daylight savings time and support only a few cities.
               In a real app, you'd likely use a web service API (or write  more complex code!)
               Hopefully this simplified example is enough to get the the idea that you
               use LU to determine the intent and entitites, then implement the appropriate logic */

            switch (location.ToLower())
            {
                case "local":
                    timeString = time.Hour.ToString() + ":" + time.Minute.ToString("D2");
                    break;
                case "london":
                    time = DateTime.UtcNow;
                    timeString = time.Hour.ToString() + ":" + time.Minute.ToString("D2");
                    break;
                case "paris":
                    time = DateTime.UtcNow.AddHours(1);
                    timeString = time.Hour.ToString() + ":" + time.Minute.ToString("D2");
                    break;
                case "sydney":
                    time = DateTime.UtcNow.AddHours(11);
                    timeString = time.Hour.ToString() + ":" + time.Minute.ToString("D2");
                    break;
                case "new york":
                    time = DateTime.UtcNow.AddHours(-5);
                    timeString = time.Hour.ToString() + ":" + time.Minute.ToString("D2");
                    break;
                case "nairobi":
                    time = DateTime.UtcNow.AddHours(3);
                    timeString = time.Hour.ToString() + ":" + time.Minute.ToString("D2");
                    break;
                case "tokyo":
                    time = DateTime.UtcNow.AddHours(9);
                    timeString = time.Hour.ToString() + ":" + time.Minute.ToString("D2");
                    break;
                case "delhi":
                    time = DateTime.UtcNow.AddHours(5.5);
                    timeString = time.Hour.ToString() + ":" + time.Minute.ToString("D2");
                    break;
                default:
                    timeString = "I don't know what time it is in " + location;
                    break;
            }

            return timeString;
        }

        static string GetDate(string day)
        {
            string date_string = "I can only determine dates for today or named days of the week.";

            // To keep things simple, assume the named day is in the current week (Sunday to Saturday)
            DayOfWeek weekDay;
            if (Enum.TryParse(day, true, out weekDay))
            {
                int weekDayNum = (int)weekDay;
                int todayNum = (int)DateTime.Today.DayOfWeek;
                int offset = weekDayNum - todayNum;
                date_string = DateTime.Today.AddDays(offset).ToShortDateString();
            }
            return date_string;

        }

        static string GetDay(string date)
        {
            // Note: To keep things simple, dates must be entered in US format (MM/DD/YYYY)
            string day_string = "Enter a date in MM/DD/YYYY format.";
            DateTime dateTime;
            if (DateTime.TryParse(date, out dateTime))
            {
                day_string = dateTime.DayOfWeek.ToString();
            }

            return day_string;
        }
    }
}

