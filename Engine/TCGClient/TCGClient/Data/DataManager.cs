using System.IO;
using TCGClient.IO;
using System.Collections.Generic;
using TCGClient;
using TCGClient.Data.Models;

namespace TCGClient.Data
{
    public static class DataManager
    {
        // Public accessors.
        public static Settings Settings { private set; get; }
        public static Game Game = new Game();

        // Data directories
        private static string _data = Program.StartupPath + "data\\";

        public static void Load() {

            // Load the settings.
            Settings = new Settings();
            if (File.Exists(Settings.File)) {
                Settings = Serialization.Deserialize<Settings>(Settings.File, Settings.GetType());
            }
        }

        public static void Save() {
            Serialization.Serialize<Settings>(Settings.File, Settings.GetType(), Settings);
        }

        public static void AddCard(CardType type, string surface, int id, int attack, int health) {
            int x = -1;
            int y = 0;

            y = Card.GetPresetTop((int)type);
            x = Card.IndexToLeft(id);

            System.Console.WriteLine("Adding " + surface);

            Game.Cards.Add(new Card(id, type, surface, attack, health, x, y));
            TCGClient.Graphics.Sfml.Sfml.Scene.AddCard(type, x, y, id);
        }

        public static void RemoveCard(CardType type, int id) {
            System.Console.WriteLine("Removing " + id);
            if (Game.Cards[id].Type == type) {
                Game.Cards.RemoveAt(id);
            }
            TCGClient.Graphics.Sfml.Sfml.Scene.RemoveCard(type, id);
        }
        public static void RemoveAllCards(CardType type) {
            for (int i = 0; i < Game.Cards.Count; i++) {
                if (Game.Cards[i].Type == type) {
                    Game.Cards.RemoveAt(i);
                    i--;
                }
            }
            
        }
    }
}
