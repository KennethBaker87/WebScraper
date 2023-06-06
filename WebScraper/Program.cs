using HtmlAgilityPack;
using System;
using System.Net.Http;
using MySqlConnector;

namespace WebScraper
{
    internal class Program
    {
        static void Main(String[] args)
        {
            // Send get request to weather.com
            String url = "https://weather.com/weather/today/l/624f0cccc10bececfa4c083056cef743837a76588790f476c9ebea44be35e51f";
            var httpClient = new HttpClient();
            var html = httpClient.GetStringAsync(url).Result;
            var htmlDocument = new HtmlDocument();
            htmlDocument.LoadHtml(html);

            // Get the temperature
            var temperatureElement = htmlDocument.DocumentNode.SelectSingleNode("//span[@class='CurrentConditions--tempValue--MHmYY']");
            var temperature = temperatureElement.InnerText.Trim();
            

            // Get the conditions
            var conditionElement = htmlDocument.DocumentNode.SelectSingleNode("//div[@class='CurrentConditions--phraseValue--mZC_p']");
            var conditions = conditionElement.InnerText.Trim();
            

            // Get the location
            var cityElement = htmlDocument.DocumentNode.SelectSingleNode("//h1[@class='CurrentConditions--location--1YWj_']");
            var city = cityElement.InnerText.Trim();
            

            // Save the scraped data to MySQL Workbench
            string connectionString = "server=localhost;port=3306;database=weatherscraper;uid=root;password=password;";
            MySqlConnection connection = new MySqlConnection(connectionString);
            connection.Open();

            string insertQuery = "INSERT INTO scrapedata (temperature, conditions, city) VALUES (@temperature, @conditions, @city)";
            MySqlCommand command = new MySqlCommand(insertQuery, connection);
            command.Parameters.AddWithValue("@temperature", temperature);
            command.Parameters.AddWithValue("@conditions", conditions);
            command.Parameters.AddWithValue("@city", city);
            command.ExecuteNonQuery();

            connection.Close();
        }
    }
}