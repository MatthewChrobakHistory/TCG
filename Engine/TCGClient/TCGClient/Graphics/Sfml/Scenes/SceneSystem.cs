using System.Collections.Generic;
using System.IO;
using System.Linq;

using TCGClient.Graphics.Sfml.Scenes.Objects;
using TCGClient.Data;

namespace TCGClient.Graphics.Sfml.Scenes
{
    public class SceneSystem
    {
        public static byte LoginState;
        public const byte STATE_NONE = 0;
        public const byte STATE_LOGIN = 1;
        public const byte STATE_REGISTER = 2;

        public List<GraphicalSurface> Surface { private set; get; }
        private SceneObject[][] _UIObject;
        private List<Card> _gameCards = new List<Card>();
        private SceneObject _curFocus;
        public bool IsMouseDown { private set; get; }
        public int DragX = 0;
        public int DragY = 0;

        #region You shouldn't have to touch this.
        public SceneSystem() {
            LoadSurfaces();
            LoadGuiObjects();
        }

        public void Reload() {
            SceneObject.ResetZOrder();
            LoadGuiObjects();
            LoginState = STATE_NONE;
        }

        public void Draw() {

            if (_UIObject != null) {
                if (_UIObject[Program.State] != null) {
                    foreach (var obj in _UIObject[Program.State]) {
                        if (obj.Visible) {
                            obj.Draw();
                        }
                    }
                }
            }

            if (IsMouseDown) {
                return;
            }
            foreach (var card in Data.DataManager.Game.Cards) {
                foreach (var uicard in _gameCards) {
                    if (card.CardID == uicard.ID && card.Type == uicard.CardType) {
                        if (card.X != uicard.Left) {
                            card.X = uicard.Left;
                            System.Console.WriteLine(card.CardID + " had the wrong x");
                        }
                        if (card.Y != uicard.Top) {
                            card.Y = uicard.Top;
                            System.Console.WriteLine(card.CardID + "had the wrong y");
                        }
                    }
                }
            }
        }

        private void LoadSurfaces() {
            _UIObject = new SceneObject[(int)GameState.Length][];
            Surface = new List<GraphicalSurface>();

            foreach (string file in Directory.GetFiles(GraphicsManager.GuiPath, "*.png")) {
                Surface.Add(new GraphicalSurface(file));
            }
            
            // UI elements are also used by the card system. Ensure that we get all the graphics.
            foreach (string file in Directory.GetFiles(GraphicsManager.CardsPath, "*.png", SearchOption.AllDirectories)) {

            }
        }

        private void DisposeSurfaces() {
            Surface = null;
        }

        public GraphicalSurface getSurface(string tagName) {
            if (Surface == null) {
                LoadSurfaces();
            }
            foreach (var surface in Surface) {
                if (surface.tag.ToLower() == tagName.ToLower()) {
                    return surface;
                }
            }
            return null;
        }

        public SceneObject getSceneObject(string name) {

            if (_UIObject != null) {
                if (_UIObject[Program.State] != null) {
                    foreach (var obj in _UIObject[Program.State]) {
                        if (obj.Name == name) {
                            return obj;
                        }
                    }
                }
            }
            return null;
        }

        // GUI Events
        public void MouseUp(int x, int y) {
            IsMouseDown = false;
            DragX = 0;
            DragY = 0;

            // _curFocus is set when we MouseDown. 
            // Just make sure we're mousing up on the same object.
            if (_curFocus != null) {
                if (_curFocus.isCard()) {
                    if (DataManager.Game.myTurn) {
                        _curFocus.MouseUp(x, y);
                    }
                } else {
                    if (x >= _curFocus.Left && x <= _curFocus.Left + _curFocus.Width) {
                        if (y >= _curFocus.Top && y <= _curFocus.Top + _curFocus.Height) {
                            _curFocus.MouseUp(x, y);
                        }
                    }
                }
            }
        }

        public void MouseDown(int x, int y) {
            IsMouseDown = true;
            DragX = x;
            DragY = y;
            if (_UIObject != null) {
                if (Program.State == (byte)GameState.Game) {
                    if (DataManager.Game.myTurn) {
                        for (int i = 0; i < _gameCards.Count; i++) {
                            var card = _gameCards[i];
                            if (x >= card.Left && x <= card.Left + card.Width) {
                                if (y >= card.Top && y <= card.Top + card.Height) {
                                    if (_curFocus != null) {
                                        _curFocus.HasFocus = false;
                                    }
                                    _curFocus = card;
                                    card.HasFocus = true;
                                    card.MouseDown(x - card.Left, y - card.Top);
                                    return;
                                }
                            }
                        }
                    }
                }
                if (_UIObject[Program.State] != null) {
                    for (int z = SceneObject.getHighZ(); z >= 0; z--) {
                        foreach (var obj in _UIObject[Program.State]) {
                            if (obj.Z == z && obj.Visible) {
                                if (x >= obj.Left && x <= obj.Left + obj.Width) {
                                    if (y >= obj.Top && y <= obj.Top + obj.Height) {
                                        if (_curFocus != null) {
                                            _curFocus.HasFocus = false;
                                        }
                                        _curFocus = obj;
                                        _curFocus.HasFocus = true;
                                        obj.MouseDown(x - obj.Left, y - obj.Top);
                                        return;
                                    }
                                }
                            }
                        }
                    }
                }
            }

            _curFocus = null;
        }

        public void MouseMove(int x, int y) {
            if (_UIObject != null) {


                if (Program.State == (byte)GameState.Game) {
                    if (DataManager.Game.myTurn) {
                        for (int i = 0; i < _gameCards.Count; i++) {
                            if (_curFocus != null) {
                                if (_curFocus.isCard()) {
                                    _curFocus.MouseMove(x, y);
                                }
                            }
                        }
                    }
                } 
                
                if (_UIObject[Program.State] != null) {
                    for (int z = SceneObject.getHighZ(); z >= 0; z--) {
                        foreach (var obj in _UIObject[Program.State]) {
                            if (obj.Z == z && obj.Visible) {
                                if (x >= obj.Left && x <= obj.Left + obj.Width) {
                                    if (y >= obj.Top && y <= obj.Top + obj.Height) {
                                        obj.MouseMove(x - obj.Left, y - obj.Top);
                                        return;
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }

        public void KeyDown(string key) {
            if (_curFocus != null) {
                _curFocus.KeyDown(key);
            }
        }

        public void EnableTurn() {
            foreach (var obj in _UIObject[(int)GameState.Game]) {
                if (obj.Name == "cmdEndTurn") {
                    obj.Visible = true;
                    return;
                }
            }
        }

        #endregion

        private void LoadGuiObjects() {
            #region Login
            _UIObject[(int)GameState.Login] = new SceneObject[] { 
                new Image() {
                    Name = "imgBackground",
                    Width = Program.Window.getTrueWidth(),
                    Height = Program.Window.getTrueHeight(),
                    Surface = getSurface("MenuBackground")
                },
                new Image() {
                    Name = "imgBackdrop",
                    Width = 350,
                    Height = 300,
                    Surface = getSurface("WhiteTrans"),
                    Left = 50,
                    Top = 250
                },
                new Label() {
                    Width = 100,
                    Height = 25,
                    FontSize = 18,
                    Caption = "Reload",
                    TextColor = SFML.Graphics.Color.White,
                },
                new Button() {
                    Name = "cmdLogin",
                    Width = 250,
                    Height = 25,
                    Caption = "Login",
                    FontSize = 18,
                    TextColor = SFML.Graphics.Color.Black,
                    Surface = getSurface("WhiteTrans"),
                    Left = 100,
                    Top = 300
                },
                new Button() {
                    Name = "cmdRegister",
                    Width = 250,
                    Height = 25,
                    Caption = "Register",
                    FontSize = 18,
                    TextColor = SFML.Graphics.Color.Black,
                    Surface = getSurface("WhiteTrans"),
                    Left = 100,
                    Top = 375
                },
                new Button() {
                    Name = "cmdBack",
                    Width = 100,
                    Height = 25,
                    Caption = "Back",
                    FontSize = 18,
                    TextColor = SFML.Graphics.Color.Black,
                    Surface = getSurface("WhiteTrans"),
                    Left = 100,
                    Top = 450,
                    Visible = false
                },
                new Button() {
                    Name = "cmdSendLogin",
                    Width = 100,
                    Height = 25,
                    Caption = "Login",
                    FontSize = 18,
                    TextColor = SFML.Graphics.Color.Black,
                    Surface = getSurface("WhiteTrans"),
                    Left = 250,
                    Top = 450,
                    Visible = false
                },
                new Button() {
                    Name = "cmdSendRegister",
                    Width = 100,
                    Height = 25,
                    Caption = "Register",
                    FontSize = 18,
                    TextColor = SFML.Graphics.Color.Black,
                    Surface = getSurface("WhiteTrans"),
                    Left = 250,
                    Top = 450,
                    Visible = false
                },
                new Textbox() {
                    Name = "txtUsername",
                    Width = 250,
                    Height = 25,
                    FontSize = 18,
                    TextColor = SFML.Graphics.Color.Black,
                    Surface = getSurface("WhiteTrans"),
                    Left = 100,
                    Top = 300,
                    MaxLength = 12,
                    Visible = false
                },
                new Textbox() {
                    Name = "txtPassword",
                    Width = 250,
                    Height = 25,
                    FontSize = 18,
                    TextColor = SFML.Graphics.Color.Black,
                    Surface = getSurface("WhiteTrans"),
                    Left = 100,
                    Top = 340,
                    MaxLength = 12,
                    Visible = false,
                    PasswordChar = '*'
                },
                new Textbox() {
                    Name = "txtPassword2",
                    Width = 250,
                    Height = 25,
                    FontSize = 18,
                    TextColor = SFML.Graphics.Color.Black,
                    Surface = getSurface("WhiteTrans"),
                    Left = 100,
                    Top = 380,
                    MaxLength = 12,
                    Visible = false,
                    PasswordChar = '*'
                }
            };
            #endregion
            #region Menu
            _UIObject[(int)GameState.Main] = new SceneObject[] {
                new Button() {
                    Name = "cmdPlay",
                    Width = 250,
                    Left = (Program.Window.getTrueWidth() / 2) -125,
                    Height = 40,
                    Caption = "Play",
                    TextColor = SFML.Graphics.Color.Black,
                    FontSize = 24,
                    Surface = getSurface("GoldTrans")
                },
                new Button() {
                    Name = "cmdProfile",
                    Width = 300,
                    Left = (Program.Window.getTrueWidth() - 300),
                    Height = 40,
                    TextColor = SFML.Graphics.Color.Black,
                    FontSize = 24,
                    Surface = getSurface("WhiteTrans")
                }
            };
            #endregion
            #region Waiting
            _UIObject[(int)GameState.Wait] = new SceneObject[] {
                new Label() {
                    Name = "lblWaiting",
                    TextColor = SFML.Graphics.Color.White,
                    FontSize = 24,
                    Caption = "Waiting for a game...",
                    Top = Program.Window.getTrueHeight() / 2 - 12,
                    Height = 25,
                    Width = 300,
                    Left = Program.Window.getTrueWidth() / 2 - 150
                },
                new Button() {
                    Name = "cmdCancel",
                    TextColor = SFML.Graphics.Color.White,
                    FontSize = 18,
                    Caption = "Cancel",
                    Top = Program.Window.getTrueHeight() / 2 + 10,
                    Width = 100,
                    Height = 30,
                    Left = Program.Window.getTrueWidth() / 2 - 50,
                    Surface = getSurface("GoldTrans")
                }
            };
            #endregion
            #region Game
            _UIObject[(int)GameState.Game] = new SceneObject[] {
                new Button() {
                    Name = "cmdEndTurn",
                    Width = 100,
                    Height = 50,
                    TextColor = SFML.Graphics.Color.White,
                    FontSize = 18,
                    Left = Program.Window.getTrueWidth() / 2 - 50,
                    Top = Program.Window.getTrueHeight() / 2 - 25,
                    Caption = "End Turn",
                    Surface = getSurface("WhiteTrans"),
                    Visible = false
                }
            };
            #endregion
            DisposeSurfaces();
        }

        public void ServerMessage(string message) {
            if (_UIObject[Program.State] == null) {
                return;
            }
            var msgBox = new ColorBox() {
                Name = "msgBox",
                Width = Program.Window.getTrueWidth(),
                Height = Program.Window.getTrueHeight(),
                Color = new SFML.Graphics.Color(12, 12, 12)
            };
            var msg = new Label() {
                Name = "lblMsg",
                Width = Program.Window.getTrueWidth(),
                Top = Program.Window.getTrueHeight() / 3,
                Height = 50,
                FontSize = 18,
                TextColor = SFML.Graphics.Color.White,
                Caption = message
            };
            var okButton = new Button() {
                Name = "cmdMsgOkay",
                Width = 100,
                Height = 25,
                TextColor = SFML.Graphics.Color.White,
                FontSize = 18,
                Caption = "Okay",
                Left = Program.Window.getTrueWidth() / 2 - 50,
                Top = Program.Window.getTrueHeight() / 2 - 12
            };

            System.Array.Resize(ref _UIObject[Program.State], _UIObject[Program.State].Length + 3);
            _UIObject[Program.State][_UIObject[Program.State].Length - 3] = msgBox;
            _UIObject[Program.State][_UIObject[Program.State].Length - 2] = msg;
            _UIObject[Program.State][_UIObject[Program.State].Length - 1] = okButton;
        }
        public void RemoveServerMessage() {
            if (_UIObject[Program.State] == null) {
                return;
            }
            System.Array.Resize(ref _UIObject[Program.State], _UIObject[Program.State].Length - 3);
        }

        public void AddCard(TCGClient.Data.Models.CardType type, int x, int y, int id) {
            _gameCards.Add(new Card() {
                CardType = type,
                Left = x,
                Top = y,
                Width = (int)Data.Models.Card.CardWidth,
                Height = (int)Data.Models.Card.CardHeight,
                ID = id
            });
        }

        public void RemoveCard(TCGClient.Data.Models.CardType type, int id) {
            if (_gameCards[id].CardType == type) {
                _gameCards.RemoveAt(id);
            }
        }

        public void RemoveAllCards(TCGClient.Data.Models.CardType type) {
            for (int i = 0; i < _gameCards.Count; i++) {
                if (_gameCards[i].CardType == type) {
                    _gameCards.RemoveAt(i);
                    i--;
                }
            }
        }

        public void ModifyCard(TCGClient.Data.Models.CardType searchType, int id, TCGClient.Data.Models.CardType newType) {
            for (int i = 0; i < _gameCards.Count; i++) {
                if (_gameCards[i].CardType == searchType && _gameCards[i].ID == id) {
                    _gameCards[i].CardType = newType;
                    return;
                }
            }
        }
    }
}
