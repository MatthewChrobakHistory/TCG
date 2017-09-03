using System.Collections.Generic;

namespace TCGClient.Data.Models
{
    public class Game
    {
        public Player[] Player;
        public List<Card> Cards;
        public bool myTurn = false;

        public Game() {
            Player = new Player[2];

            for (int i = 0; i < 2; i++) {
                Player[i] = new Player();
            }

            Cards = new List<Card>();
        }

        public void Clear() {
            for (int i = 0; i < 2; i++) {
                Player[i] = new Player();
            }
        }
    }
}
