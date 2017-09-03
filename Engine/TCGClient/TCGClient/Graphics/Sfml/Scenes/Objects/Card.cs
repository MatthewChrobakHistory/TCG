using System.Linq;
using TCGClient.Data.Models;

namespace TCGClient.Graphics.Sfml.Scenes.Objects
{
    public class Card : SceneObject
    {
        public CardType CardType = CardType.Length;
        public int ID = -1;
        private int xOffset = 0;
        private int yOffset = 0;
        private int originalX = 0;
        private int originalY = 0;

        public override void Draw() {
            // base.Draw();
        }

        public override void MouseUp(int x, int y) {

            if (CardType == Data.Models.CardType.EnemyHeld || CardType == Data.Models.CardType.EnemyField || CardType == Data.Models.CardType.EnemyDeck) {
                return;
            }

            var game = Data.DataManager.Game;
            int id = getMyDataID();

            if (id > -1) {
                int height = (int)Data.Models.Card.CardHeight;
                int width = (int)Data.Models.Card.CardWidth;

                for (int i = 0; i < (int)CardType.Length; i++) {
                    int pTop = Data.Models.Card.GetPresetTop(i);
                    if (pTop <= y && y <= pTop + height) {
                        for (int cardnum = 0; cardnum < 7; cardnum++) {
                            int cardLeft = Data.Models.Card.IndexToLeft(cardnum);

                            if (cardLeft <= x && x <= cardLeft + width) {
                                foreach (var card in game.Cards) {
                                    if (card.X == cardLeft && card.Y == pTop) {
                                        this.Left = originalX;
                                        this.Top = originalY;
                                        game.Cards[id].X = this.Left;
                                        game.Cards[id].Y = this.Top;
                                        return;
                                    }
                                }
                                this.Left = cardLeft;
                                game.Cards[id].X = cardLeft;
                                this.Top = pTop;
                                game.Cards[id].Y = pTop;
                                this.ID = cardnum;
                                game.Cards[id].CardID = cardnum;
                                return;
                            }
                        }
                    }
                }

                this.Left = originalX;
                this.Top = originalY;
                game.Cards[id].X = this.Left;
                game.Cards[id].Y = this.Top;
            }
        }

        public override void MouseMove(int x, int y) {
            var game = Data.DataManager.Game;
            if (Sfml.Scene.IsMouseDown) {
            int id = getMyDataID();
            
            if (id > -1) {
                
                    if (CardType == Data.Models.CardType.EnemyHeld || CardType == Data.Models.CardType.EnemyField || CardType == Data.Models.CardType.EnemyDeck) {
                        return;
                    }
                    game.Cards[id].X = x - xOffset;
                    game.Cards[id].Y = y - yOffset;
                }
            }
        }

        public override void MouseDown(int x, int y) {

            if (CardType == Data.Models.CardType.EnemyHeld || CardType == Data.Models.CardType.EnemyField || CardType == Data.Models.CardType.EnemyDeck) {
                return;
            }

            this.xOffset = x;
            this.yOffset = y;
            this.originalX = this.Left;
            this.originalY = this.Top;
            base.MouseDown(x, y);
        }

        public override bool isCard() {
            return true;
        }

        private int getMyDataID() {
            var game = Data.DataManager.Game;

            for (int i = 0; i < game.Cards.Count; i++) {
                if (game.Cards[i].CardID == this.ID && game.Cards[i].Type == this.CardType) {
                    return i;
                }
            }

            return -1;
        }
    }
}
