using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls.Primitives;
using System.Windows.Documents;
using Newtonsoft.Json.Linq;

namespace FreeGameNotifications
{
    public class EpicGamesWebApi
    {
        public static async Task<List<string>> GetGames()
        {
            using (var client = new HttpClient())
            {
                var uri = new Uri("https://store-site-backend-static.ak.epicgames.com/freeGamesPromotions");
                var response = await client.GetAsync(uri);
                var json = await response.Content.ReadAsStringAsync();
                var data = JObject.Parse(json)["data"];
                var elements = data["Catalog"]["searchStore"]["elements"];

                var games = new List<string>();

                foreach (var element in elements)
                {
                    if (element["price"]["totalPrice"]["discountPrice"].Value<int>() == 0)
                    {
                        var gameTitle = element["title"].ToString();
                        //var pageSlug = element["offerMappings"].First?["pageSlug"];
                        var pageSlug = element["productSlug"];
                        var url = $"https://store.epicgames.com/en-US/p/{pageSlug}";

                        var message = "Free on the Epic Game Store:\n" +
                                      $"{gameTitle}\n" +
                                      $"{url}";

                        games.Add(message);

                        //Console.WriteLine(element);
                    }
                }

                return games;
            }
        }
    }
}
