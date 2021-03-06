﻿using SFML.Graphics;
using SFML.System;

namespace TCGClient.Graphics.Sfml.Scenes.Objects
{
    public class Label : SceneObject
    {
        public string Caption;
        public Color TextColor;
        public uint FontSize;

        public override void Draw() {
            if (Caption != null) {
                var text = new Text(Caption, Sfml.GameFont);
                text.Position = new Vector2f(this.Left + (int)(this.Width - (this.Caption.Length * GetRatio(Caption) * this.FontSize)) / 2, this.Top + (this.Height / 2) - (this.FontSize / 2));
                text.Color = this.TextColor;
                text.CharacterSize = this.FontSize;
                Sfml.BackBuffer.Draw(text);
            }
        }

        public override void MouseDown(int x, int y) {
            Sfml.Scene.Reload();
        }
    }
}
