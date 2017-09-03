using System;
using TCGServer.Data;
using TCGServer.IO;
using TCGServer.Networking;
using TCGServer.Scripting;

namespace TCGServer
{
    public enum ClientState
    {
        Login,
        Main,
        Game,
        Shop,
        Wait,
        Loading,
        Length
    }

    public static class Program
    {
        public static string StartupPath;

        static void Main(string[] args) {
            // Set up the startup path of the application.
            StartupPath = System.AppDomain.CurrentDomain.BaseDirectory;

            // Check the folders and files in the system.
            FolderSystem.Check();

            // Load the game data.
            DataManager.Load();

            // Start the network.
            NetworkManager.Initialize();

            // Initialize the scripting system.
            ScriptManager.Initialize();

            // Set up the Destroy Server event handlers.
            Console.WriteLine("[IMPORTANT INFORMATION] : ");
            Console.WriteLine("------------------------------------------------------------------------------");
            Console.WriteLine("Remember to turn off the server by pressing [CTRL + C] or [CTRL + BREAK].");
            Console.WriteLine("If you do not, all online player's data will NOT be saved.");
            Console.WriteLine("------------------------------------------------------------------------------");
            Console.CancelKeyPress += (s, e) => {
                Destroy();
            };

            //new Form1().Show();

            // Start the gameloop.
            GameLoop();
        }

        private static void GameLoop() {
            int tick = 0, tmr1000 = 0;

            while (true) {
                tick = System.Environment.TickCount;

                if (tmr1000 < tick) {
                    tmr1000 = tick + 1000;
                }
            }
        }

        private static void Destroy() {
            DataManager.Save();
            System.Environment.Exit(1);
        }

        public static void Write(string message) {
            Console.WriteLine(message);
        }
    }
}
