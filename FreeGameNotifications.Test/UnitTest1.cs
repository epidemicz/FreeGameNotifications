using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Threading.Tasks;

namespace FreeGameNotifications.Test
{
    [TestClass]
    public class UnitTest1
    {
        [TestMethod]
        public async void TestMethod1()
        {
            var games = await EpicGamesWebApi.GetGames();
            Assert.IsNotNull(games);   
        }
    }
}