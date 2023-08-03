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
using Playnite.SDK;
using static System.Net.WebRequestMethods;

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
                        var isCodeRedemptionOnly = element["isCodeRedemptionOnly"].ToString().ToUpper() == "TRUE";

                        // skipping code redemption only
                        if (isCodeRedemptionOnly)
                        {
                            continue;
                        }

                        var url = GetGameUrl(element);

                        games.Add(new Notification
                        {
                            Title = gameTitle,
                            Url = url,
                            Description = $"Free on the Epic Game Store:\n" +
                                          $"{gameTitle}\n"
                        });
                    }
                }

                return games;
            }
        }

        private static string GetGameUrl(JToken element)
        {
            // the url of the game isn't consistent
            // it seems like it's in different spots for each game
            var slug = string.Empty;

            try
            {
                slug = element["productSlug"].ToString();

                if (string.IsNullOrWhiteSpace(slug))
                {
                    slug = element["catalogNs"]["mappings"][0]["pageSlug"].ToString();
                }

                return $"https://store.epicgames.com/en-US/p/{slug}";
            }
            catch
            {
                slug = string.Empty;
            }

            // couldn't resolve the real game page
            return "https://store.epicgames.com/en-US/free-games";
        }
    }
}
