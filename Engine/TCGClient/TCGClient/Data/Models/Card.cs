namespace TCGClient.Data.Models
{
    public enum CardType
    {
        MyDeck,
        MyHeld,
        MyField,
        EnemyDeck,
        EnemyHeld,
        EnemyField,
        Length
    }

    public class Card
    {
        private const int HorizontalMidWeight = 14;
        private const int VerticalMidWeight = 120;
        public const float CardWidth = 125f;
        public const float CardHeight = 175f;
        public static int BlankID;

        public int X;
        public int Y;
        public int CardID;
        public int SurfaceID;
        public int Attack;
        public int Health;
        public string SurfaceName { private set; get; }
        public CardType Type;

        public Card(int id, CardType type, string surface, int attack, int health, int x, int y) {
            this.CardID = id;
            this.Type = type;
            this.SurfaceName = surface;
            this.Attack = attack;
            this.Health = health;
            this.X = x;
            this.Y = y;
            this.SurfaceID = -1;
        }

        public static int GetPresetTop(int type) {
            switch (type) {
                case (int)CardType.EnemyField:
                    return Card.VerticalMidWeight;
                case (int)CardType.EnemyHeld:
                    return 0 - ((int)Card.CardHeight / 2);
                case (int)CardType.MyField:
                    return Program.Window.getTrueHeight() - Card.VerticalMidWeight - (int)Card.CardHeight;
                case (int)CardType.MyHeld:
                    return Program.Window.getTrueHeight() - (int)Card.CardHeight / 2;
                default:
                    return -2 * (int)Card.CardHeight;
            }
        }

        public static int IndexToLeft(int num) {
            return num * ((int)Card.CardWidth + Card.HorizontalMidWeight);
        }
    }
}
