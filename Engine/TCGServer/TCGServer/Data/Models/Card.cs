namespace TCGServer.Data.Models
{
    public class Card
    {
        public static string Folder = Program.StartupPath + "data\\cards\\";
        public string Name;
        public int Health;
        public int Attack;
        public string Cover;
        public string[] Script;

        public Card() {
            this.Name = "";
            this.Health = 1;
            this.Attack = 1;
            this.Cover = "";
            this.Script = new string[1];
        }
    }
}
