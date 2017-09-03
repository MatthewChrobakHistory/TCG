using TCGServer.IO;
namespace TCGServer.Data.Models
{
    public class Account
    {
        public static string Folder = Program.StartupPath + "data\\accounts\\";

        public string Username;
        public string DisplayName;
        public string Password;

        public int Money;
        public int WinCount;
        public int LossCount;
        public byte State;

        public GameMetaData Game;

        public void Create(string username, string password) {
            this.Username = username;
            this.DisplayName = username;
            this.Password = Encryption.Encrypt(password);

            this.Money = 0;
            this.WinCount = 0;
            this.LossCount = 0;

            this.Game = new GameMetaData();

            this.Save();
        }

        public static bool FileExists(string username) {
            if (FolderSystem.FileExists(Folder + username + ".xml")) {
                return true;
            }
            return false;
        }
        public static bool VerifyAccount(string username, string password) {
            if (FolderSystem.FileExists(Folder + username + ".xml")) {
                Account player = new Account();
                player = Serialization.Deserialize<Account>(Folder + username + ".xml", player.GetType());

                if (player.Password == Encryption.Encrypt(password)) {
                    return true;
                }
            }
            return false;
        }
        // Hack: Fix the player loading system.
        public static Account Load(string username) {
            var player = new Account();
            player = Serialization.Deserialize<Account>(Folder + username + ".xml", player.GetType());
            return player;
        }

        public void Save() {
            Serialization.Serialize<Account>(Folder + Username + ".xml", this.GetType(), this);
        }

        
    }

    public class GameMetaData
    {
        // Which one of the games in the game list is the one we're in?
        public int GameID;
        // Player 1 or 2.
        public int PlayerID;

        public GameMetaData() {
            GameID = -1;
            PlayerID = -1;
        }
    }
}
