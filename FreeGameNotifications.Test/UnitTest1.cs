using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Threading.Tasks;

namespace FreeGameNotifications.Test
{
    [TestClass]
    public class UnitTest1
    {
        [TestMethod]
        public void TestMethod1()
        {
            Task.Run(async () =>
            {
                var games = await EpicGamesWebApi.GetGames();
            }).Wait();
        }
    }
}