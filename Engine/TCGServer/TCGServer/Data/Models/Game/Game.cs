namespace TCGServer.Data.Models.Game
{
    public class Game
    {
        public Player[] Player;
        public int CurrentTurn;

        public Game() {
            Player = new Player[2];

            for (int i = 0; i < 2; i++) {
                Player[i] = new Player();
                Player[i].HeldCards[0] = new Card() {
                    Attack = 1,
                    Cover = "Cavalry",
                    Health = 10,
                    Name = "Cavalry"
                };
            }

            CurrentTurn = RNG.Get(0, 1);
        }
    }
}
