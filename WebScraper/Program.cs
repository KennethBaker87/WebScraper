using HtmlAgilityPack;
using System;
using System.Net.Http;
using MySqlConnector;
using System.Collections.Generic;

namespace WebScraper
{
    internal class Program
    {
        static void Main(String[] args)
        {
            
            List<string> urls = new List<string>()
            {
                "https://weather.com/weather/today/l/624f0cccc10bececfa4c083056cef743837a76588790f476c9ebea44be35e51f",
                "https://weather.com/weather/today/l/5cdcfcd9b1f0eb2558ee5fe36d77904cb5590b965b327809ccbbf65621f61e01",
                "https://weather.com/weather/today/l/96f2f84af9a5f5d452eb0574d4e4d8a840c71b05e22264ebdc0056433a642c84"
                // Add more URLs as needed
            };
                // Establish database connection
            string connectionString = "server=localhost;port=3306;database=weatherscraper;uid=root;password=password;";
            MySqlConnection connection = new MySqlConnection(connectionString);
            connection.Open();

            // Iterate over the URLs
            foreach (string url in urls)
            {
                // Send get request to weather.com
                var httpClient = new HttpClient();
                var html = httpClient.GetStringAsync(url).Result;
                var htmlDocument = new HtmlDocument();
                htmlDocument.LoadHtml(html);

                // Get the temperature
                var temperatureElement = htmlDocument.DocumentNode.SelectSingleNode("//span[@class='CurrentConditions--tempValue--MHmYY']");
                var temperature = temperatureElement.InnerText.Trim();
                Console.WriteLine("Temperature: " + temperature);

                // Get the conditions
                var conditionElement = htmlDocument.DocumentNode.SelectSingleNode("//div[@class='CurrentConditions--phraseValue--mZC_p']");
                var conditions = conditionElement.InnerText.Trim();
                Console.WriteLine("Conditions: " + conditions);

                // Get the location
                var cityElement = htmlDocument.DocumentNode.SelectSingleNode("//h1[@class='CurrentConditions--location--1YWj_']");
                var city = cityElement.InnerText.Trim();

                // Check if the location already exists in the database
                string selectQuery = "SELECT COUNT(*) FROM scrapedata WHERE city = @city";
                MySqlCommand selectCommand = new MySqlCommand(selectQuery, connection);
                selectCommand.Parameters.AddWithValue("@city", city);
                int count = Convert.ToInt32(selectCommand.ExecuteScalar());

                // If the location already exists, skip to the next URL
                if (count > 0)
                {
                    Console.WriteLine("Location already exists. Skipping...");
                    continue;
                }

                // Save the scraped data to MySQL Workbench
                string insertQuery = "INSERT INTO scrapedata (temperature, conditions, city) VALUES (@temperature, @conditions, @city)";
                MySqlCommand command = new MySqlCommand(insertQuery, connection);
                command.Parameters.AddWithValue("@temperature", temperature);
                command.Parameters.AddWithValue("@conditions", conditions);
                command.Parameters.AddWithValue("@city", city);
                command.ExecuteNonQuery();
            }

           connection.Close();
            
        }
    }
}