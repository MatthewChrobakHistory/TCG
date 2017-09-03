using SFML.Graphics;
using SFML.System;

namespace TCGClient.Graphics.Sfml.Scenes.Objects
{
    public class Button : SceneObject
    {
        public string Caption;
        public Color TextColor;
        public uint FontSize;

        public override void Draw() {
            // Draw the object.
            base.Draw();

            if (this.Name == "cmdProfile") {
                this.Caption = "Hello, " + Data.DataManager.Settings.Username + "!";
                this.FontSize = 24;
            }

            if ((Caption != null)) {
                var text = new Text(Caption, Sfml.GameFont);
                text.Position = new Vector2f(this.Left + (int)(this.Width - (this.Caption.Length * GetRatio(Caption) * this.FontSize)) / 2, this.Top + (this.Height / 2) - (this.FontSize / 2));
                text.Color = TextColor;
                text.CharacterSize = FontSize;
                Sfml.BackBuffer.Draw(text);
            }
        }

        public override void MouseDown(int x, int y) {

            switch (this.Name) {
                case "cmdSendLogin":
                    if (SceneSystem.LoginState == SceneSystem.STATE_LOGIN) {
                        string username = Sfml.Scene.getSceneObject("txtUsername").getStringValue("text");
                        string password = Sfml.Scene.getSceneObject("txtPassword").getStringValue("text");

                        Data.DataManager.Settings.Username = username;
                        if (Data.DataManager.Settings.RememberPassword) {
                            Data.DataManager.Settings.Password = password;
                        } else {
                            Data.DataManager.Settings.Password = "";
                        }
                        
                        Networking.NetworkManager.PacketManager.SendRequestLogin(username, password);
                    }
                    break;
                case "cmdSendRegister":
                    if (SceneSystem.LoginState == SceneSystem.STATE_REGISTER) {
                        string username = Sfml.Scene.getSceneObject("txtUsername").getStringValue("text");
                        string password = Sfml.Scene.getSceneObject("txtPassword").getStringValue("text");
                        string password2 = Sfml.Scene.getSceneObject("txtPassword2").getStringValue("text");

                        if (username.Length > 3) {
                            if (password == password2) {
                                Data.DataManager.Settings.Username = username;
                                Networking.NetworkManager.PacketManager.SendRequestRegister(username, password);
                            }
                        }
                    }
                    break;
                case "cmdBack":
                    if (SceneSystem.LoginState != SceneSystem.STATE_NONE) {
                        this.Visible = false;
                        Sfml.Scene.getSceneObject("cmdLogin").Visible = true;
                        Sfml.Scene.getSceneObject("cmdRegister").Visible = true;
                        Sfml.Scene.getSceneObject("txtUsername").Visible = false;
                        Sfml.Scene.getSceneObject("txtPassword").Visible = false;

                        if (SceneSystem.LoginState == SceneSystem.STATE_LOGIN) {
                            Sfml.Scene.getSceneObject("cmdSendLogin").Visible = false;
                        } else if (SceneSystem.LoginState == SceneSystem.STATE_REGISTER) {
                            Sfml.Scene.getSceneObject("txtPassword2").Visible = false;
                            Sfml.Scene.getSceneObject("cmdSendRegister").Visible = false;
                        }

                        SceneSystem.LoginState = SceneSystem.STATE_NONE;
                    }
                    break;
                case "cmdLogin":
                    if (SceneSystem.LoginState == SceneSystem.STATE_NONE) {
                        this.Visible = false;
                        Sfml.Scene.getSceneObject("cmdRegister").Visible = false;
                        Sfml.Scene.getSceneObject("txtUsername").Visible = true;
                        Sfml.Scene.getSceneObject("txtPassword").Visible = true;
                        Sfml.Scene.getSceneObject("cmdBack").Visible = true;
                        Sfml.Scene.getSceneObject("cmdSendLogin").Visible = true;

                        Sfml.Scene.getSceneObject("txtUsername").setStringValue("text", Data.DataManager.Settings.Username);
                        if (Data.DataManager.Settings.RememberPassword) {
                            Sfml.Scene.getSceneObject("txtPassword").setStringValue("text", Data.DataManager.Settings.Password);
                        } else {
                            Sfml.Scene.getSceneObject("txtPassword").setStringValue("text", "");
                        }

                        SceneSystem.LoginState = SceneSystem.STATE_LOGIN;
                    }
                    break;
                case "cmdRegister":
                    if (SceneSystem.LoginState == SceneSystem.STATE_NONE) {
                        this.Visible = false;
                        Sfml.Scene.getSceneObject("cmdLogin").Visible = false;
                        Sfml.Scene.getSceneObject("txtUsername").Visible = true;
                        Sfml.Scene.getSceneObject("txtPassword").Visible = true;
                        Sfml.Scene.getSceneObject("txtPassword2").Visible = true;
                        Sfml.Scene.getSceneObject("cmdBack").Visible = true;
                        Sfml.Scene.getSceneObject("cmdSendRegister").Visible = true;

                        Sfml.Scene.getSceneObject("txtUsername").setStringValue("text", "");
                        Sfml.Scene.getSceneObject("txtPassword").setStringValue("text", "");
                        Sfml.Scene.getSceneObject("txtPassword2").setStringValue("text", "");

                        SceneSystem.LoginState = SceneSystem.STATE_REGISTER;
                    }
                    break;
                case "cmdMsgOkay":
                    Sfml.Scene.RemoveServerMessage();
                    break;
                case "cmdPlay":
                    Networking.NetworkManager.PacketManager.SendRequestChangeClientState(GameState.Game);
                    break;
                case "cmdCancel":
                    Networking.NetworkManager.PacketManager.SendRequestChangeClientState(GameState.Main);
                    break;
                case "cmdEndTurn":
                    Networking.NetworkManager.PacketManager.SendEndTurn();
                    this.Visible = false;
                    break;
            }
        }
    }
}