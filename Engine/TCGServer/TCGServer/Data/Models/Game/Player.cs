namespace TCGServer.Data.Models.Game
{
    public class Player
    {
        public int AccountID;
        public int Life;
        public int Energy;
        public int Experience;
        public bool HasTurn = false;

        public Card[] CardPile = new Card[30];
        public Card[] HeldCards = new Card[7];
        public Card[] FieldCards = new Card[7];

        public Player() {
            AccountID = -1;
            Life = 40;
            Energy = 1;
            Experience = 0;

            for (int i = 0; i < CardPile.Length; i++) {
                CardPile[i] = new Card();
            }
            for (int i = 0; i < HeldCards.Length; i++) {
                HeldCards[i] = new Card();
            }
            for (int i = 0; i < FieldCards.Length; i++) {
                FieldCards[i] = new Card();
            }
        }
    }
}
