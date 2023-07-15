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
        public static async Task<List<Notification>> GetGames()
        {
            using (var client = new HttpClient())
            {
                var uri = new Uri("https://store-site-backend-static.ak.epicgames.com/freeGamesPromotions");
                var response = await client.GetAsync(uri);
                var json = await response.Content.ReadAsStringAsync();
                var data = JObject.Parse(json)["data"];
                var elements = data["Catalog"]["searchStore"]["elements"];

                var games = new List<Notification>();

                foreach (var element in elements)
                {
                    if (element["price"]["totalPrice"]["discountPrice"].Value<int>() == 0)
                    {
                        var gameTitle = element["title"].ToString();
                        // slug might be pageSlug also or not even there
                        var slug = element["productSlug"].ToString();
                        var url = $"https://store.epicgames.com/en-US/p/{slug}";

                        games.Add(new Notification
                        {
                            Title = gameTitle,
                            Url = !string.IsNullOrEmpty(slug) ? url : "https://store.epicgames.com/en-US/free-games",
                            Description = $"Free on the Epic Game Store:\n" + 
                                          $"{gameTitle}\n"
                        });
                    }
                }

                return games;
            }
        }
    }
}
