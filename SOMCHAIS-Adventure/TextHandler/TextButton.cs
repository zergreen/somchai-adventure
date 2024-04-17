using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace SOMCHAISAdventure
{
    public class TextButton
    {
        public string Text { get; set; }
        public Vector2 Position { get; set; }
        public Rectangle Bounds { get; private set; }
        public bool IsHovering { get; private set; }

        private SpriteFont _font;

        public TextButton(string text, Vector2 position, SpriteFont font)
        {
            Text = text;
            Position = position;
            _font = font;
            UpdateBounds();
        }

        public void Update(Vector2 mousePosition)
        {
            IsHovering = Bounds.Contains(mousePosition.ToPoint());
        }

        public void Draw(SpriteBatch spriteBatch, Color defaultColor, Color hoverColor)
        {
            spriteBatch.DrawString(_font, Text, Position, IsHovering ? hoverColor : defaultColor);
        }

        private void UpdateBounds()
        {
            Bounds = new Rectangle(Position.ToPoint(), _font.MeasureString(Text).ToPoint());
        }
    }
}