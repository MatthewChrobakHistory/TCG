using SFML.Graphics;
using SFML.System;

namespace TCGClient.Graphics.Sfml.Scenes.Objects
{
    public class ColorBox : SceneObject
    {
        public Color Color;

        public override void Draw() {
            var rect = new RectangleShape(new Vector2f(this.Width, this.Height));
            rect.FillColor = Color;
            rect.Position = new Vector2f(this.Left, this.Top);
            Sfml.BackBuffer.Draw(rect);
        }
    }
}
