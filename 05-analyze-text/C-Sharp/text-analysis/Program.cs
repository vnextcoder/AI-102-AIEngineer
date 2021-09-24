using System;
using System.IO;
using System.Text;
using Microsoft.Extensions.Configuration;

// Import namespaces
// import namespaces
using Azure;
using Azure.AI.TextAnalytics;

namespace text_analysis
{
    class Program
    {
        static void Main(string[] args)
        {
            try
            {
                // Get config settings from AppSettings
                IConfigurationBuilder builder = new ConfigurationBuilder().AddJsonFile("appsettings.json");
                IConfigurationRoot configuration = builder.Build();
                string cogSvcEndpoint = configuration["CognitiveServicesEndpoint"];
                string cogSvcKey = configuration["CognitiveServiceKey"];

                // Set console encoding to unicode
                Console.InputEncoding = Encoding.Unicode;
                Console.OutputEncoding = Encoding.Unicode;

                // Create client using endpoint and key
                // Create client using endpoint and key
                AzureKeyCredential credentials = new AzureKeyCredential(cogSvcKey);
                Uri endpoint = new Uri(cogSvcEndpoint);
                TextAnalyticsClient CogClient = new TextAnalyticsClient(endpoint, credentials);

                // Analyze each text file in the reviews folder
                var folderPath = Path.GetFullPath("./reviews");
                DirectoryInfo folder = new DirectoryInfo(folderPath);
                foreach (var file in folder.GetFiles("*.txt"))
                {
                    // Read the file contents
                    Console.WriteLine("\n-------------\n" + file.Name);
                    StreamReader sr = file.OpenText();
                    var text = sr.ReadToEnd();
                    sr.Close();
                    Console.WriteLine("\n" + text);

                    // Get language
                    DetectedLanguage detectedLanguage = CogClient.DetectLanguage(text);
                    Console.WriteLine($"\nLanguage: {detectedLanguage.Name}");

                    // Get sentiment
                    DocumentSentiment sentimentAnalysis = CogClient.AnalyzeSentiment(text);
                    Console.WriteLine($"\nSentiment: {sentimentAnalysis.Sentiment}");

                    // Get key phrases
                    // Get key phrases
                    KeyPhraseCollection phrases = CogClient.ExtractKeyPhrases(text);
                    if (phrases.Count > 0)
                    {
                        Console.WriteLine("\nKey Phrases:");
                        foreach (string phrase in phrases)
                        {
                            Console.WriteLine($"\t{phrase}");
                        }
                    }

                    // Get entities
                    // Get entities
                    CategorizedEntityCollection entities = CogClient.RecognizeEntities(text);
                    if (entities.Count > 0)
                    {
                        Console.WriteLine("\nEntities:");
                        foreach (CategorizedEntity entity in entities)
                        {
                            Console.WriteLine($"\t{entity.Text} ({entity.Category})");
                        }
                    }

                    // Get linked entities
                    // Get entities
                    LinkedEntityCollection linkedEntities = CogClient.RecognizeLinkedEntities(text);
                    if (linkedEntities.Count > 0)
                    {
                        Console.WriteLine("\nLinked Entities:");
                        foreach (LinkedEntity linkedentity in linkedEntities)
                        {
                            Console.WriteLine($"\t{linkedentity.Name} ({linkedentity.Url})");
                        }
                    }

                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }



    }
}
