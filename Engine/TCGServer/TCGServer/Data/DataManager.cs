using System.IO;
using TCGServer.IO;
using System.Collections.Generic;
using TCGServer;
using TCGServer.Data.Models;
using TCGServer.Data.Models.Game;

namespace TCGServer.Data
{
    public static class DataManager
    {
        public static List<Account> User;
        public static List<Card> Cards;
        public static List<Game> Game;

        // Data directories
        private static string _data = Program.StartupPath + "data\\";

        public static void Load() {
            User = new List<Account>();
            Game = new List<Game>();
            Cards = new List<Card>();

            var cardType = new Card().GetType();
            foreach (var file in Directory.GetFiles(Card.Folder)) {
                Cards.Add(Serialization.Deserialize<Card>(file, cardType));
            }
        }

        public static void Save() {


            foreach (var card in Cards) {
                Serialization.Serialize<Card>(Card.Folder + card.Name + ".xml", card.GetType(), card);
            }
        }

        public static void SaveUserAccount(int index, bool logout = false) {
            if (index <= User.Count) {
                if (User[index] != null) {
                    if (User[index].Username != null) {
                        User[index].Save();
                    }
                }
            }

            if (logout) {
                User[index] = new Account();
            }
        }

        public static void CreateGame(int player1, int player2) {
            var game = new Game();
            int startLife = 40;

            // Player 1
            var player = game.Player[0];
            player.AccountID = player1;
            player.Life = startLife;
            player.Energy = 1;

            var userGame = User[player1].Game;
            userGame.GameID = Game.Count;
            userGame.PlayerID = 0;

            // Player 2
            player = game.Player[1];
            player.AccountID = player2;
            player.Life = startLife;
            player.Energy = 1;

            userGame = User[player2].Game;
            userGame.GameID = Game.Count;
            userGame.PlayerID = 1;

            Game.Add(game);
        }
    }
}
