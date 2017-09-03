using TCGClient.Graphics.Sfml.Scenes;
using TCGClient.Data.Models;
using SFML.Graphics;
using System.Collections.Generic;
using System.IO;
using SFML.System;

namespace TCGClient.Graphics.Sfml
{
    public class Sfml : IGraphics
    {
        public static RenderWindow BackBuffer { private set; get; }
        public static SceneSystem Scene { private set; get; }
        public static Font GameFont { private set; get; }
        private List<GraphicalSurface>[] _surface;
        private Color _bgColor;
        private IInput _input;

        #region Core SFML
        public void Initialize() {
            LoadSurfaces();

            LoadFont();

            BackBuffer = new RenderWindow(new BackBuffer(Program.Window,
                Program.Window.getTrueWidth(),
                Program.Window.getTrueHeight(),
                0, 0).GetHandle());
            _bgColor = new Color(25, 25, 25);

            _input = new Input();
            BackBuffer.MouseButtonPressed += _input.MouseDown;
            BackBuffer.MouseButtonReleased += _input.MouseUp;
            BackBuffer.MouseMoved += _input.MouseMove;
            BackBuffer.KeyPressed += _input.KeyPress;
            BackBuffer.KeyReleased += _input.KeyRelease;
            BackBuffer.SetKeyRepeatEnabled(true);
            Card.BlankID = GetSurfaceIndex("Cover", SurfaceType.Card);

            Scene = new SceneSystem();
        }
        public void Destroy() {

        }
        public void Draw() {
            BackBuffer.DispatchEvents();
            BackBuffer.Clear(_bgColor);
            DrawGame();
            Scene.Draw();
            BackBuffer.Display();
        }
        public void ServerMessage(string message) {
            Scene.ServerMessage(message);
        }
        public void EnableTurn() {
            Scene.EnableTurn();
        }

        private void LoadSurfaces() {
            _surface = new List<GraphicalSurface>[(int)SurfaceType.Length];

            for (int i = 0; i < (int)SurfaceType.Length; i++) {
                _surface[i] = new List<GraphicalSurface>();
            }

            foreach (string file in Directory.GetFiles(GraphicsManager.CardsPath, "*.png", SearchOption.AllDirectories)) {
                _surface[(int)SurfaceType.Card].Add(new GraphicalSurface(file));
            }
        }
        private void LoadFont() {
            if (System.IO.File.Exists(Program.StartupPath + "data\\fonts\\" + Data.DataManager.Settings.Font + ".ttf")) {
                GameFont = new Font(Program.StartupPath + "data\\fonts\\" + Data.DataManager.Settings.Font + ".ttf");
            } else if (System.IO.File.Exists(Program.StartupPath + "data\\fonts\\" + Data.DataManager.Settings.Font + ".otf")) {
                GameFont = new Font(Program.StartupPath + "data\\fonts\\" + Data.DataManager.Settings.Font + ".otf");
            }
        }

        public GraphicalSurface GetSurface(string tagName, SurfaceType type) {
            for (int i = 0; i < _surface[(int)type].Count; i++) {
                if (_surface[(int)type][i].tag.ToLower() == tagName.ToLower()) {
                    return _surface[(int)type][i];
                }
            }
            return null;
        }
        public int GetSurfaceIndex(string tagName, SurfaceType type) {
            for (int i = 0; i < _surface[(int)type].Count; i++) {
                if (_surface[(int)type][i].tag.ToLower() == tagName.ToLower()) {
                    return i;
                }
            }
            return -1;
        }
        #endregion

        private void DrawGame() {
            var game = Data.DataManager.Game;

            if (Program.State == (int)GameState.Game) {
                for (int i = 0; i < (int)CardType.Length; i++) {
                    var spaceSprite = new RectangleShape(new Vector2f(Card.CardWidth, Card.CardHeight));
                    spaceSprite.FillColor = default(Color);
                    int top = Card.GetPresetTop(i);
                    int left = 0;

                    switch (i) {
                        case (int)CardType.EnemyField:
                            spaceSprite.FillColor = new Color(255, 0, 0);
                            break;
                        case (int)CardType.EnemyHeld:
                            spaceSprite.FillColor = new Color(255, 0, 0);
                            break;
                        case (int)CardType.MyField:
                            spaceSprite.FillColor = new Color(0, 255, 0);
                            break;
                        case (int)CardType.MyHeld:
                            spaceSprite.FillColor = new Color(0, 255, 0);
                            break;
                    }

                    for (int x = 0; x < 7; x++) {
                        left = Card.IndexToLeft(x);

                        spaceSprite.Position = new Vector2f(left, top);
                        BackBuffer.Draw(spaceSprite);
                    }
                }


                foreach (var card in game.Cards) {

                    // Exit out early.
                    if (card.CardID < 0) {
                        break;
                    }
                    if (card.SurfaceID < 0) {
                        card.SurfaceID = GetSurfaceIndex(card.SurfaceName, SurfaceType.Card);
                    }

                    if (card.SurfaceID >= 0) {
                        int x = card.X;
                        int y = card.Y;
                        int id = card.SurfaceID;

                        if (card.Type == CardType.EnemyDeck || card.Type == CardType.MyDeck) {
                            id = Card.BlankID;
                        }

                        var surface = _surface[(int)SurfaceType.Card][id].sprite;
                        surface.Position = new Vector2f(x, y);
                        surface.Scale = new Vector2f(Card.CardWidth / surface.Texture.Size.X, Card.CardHeight / surface.Texture.Size.Y);
                        BackBuffer.Draw(surface);
                    }
                }
            }
        }
    }
}
