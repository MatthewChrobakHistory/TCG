namespace TCGClient.Data.Models
{
    public class Player
    {
        public string Name;
        public int Energy;
        public int Life;
        public int Experience;

        public Player() {
            Life = 0;
            Energy = 0;
            Experience = 0;
            Name = "null";
        }
    }
}
