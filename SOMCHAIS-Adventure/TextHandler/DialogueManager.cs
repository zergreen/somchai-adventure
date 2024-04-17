using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using SOMCHAIS_Adventure;

namespace SOMCHAISAdventure
{
    public class DialogueManager
    {
        private readonly SpriteFont _font;
        private readonly Rectangle _dialogueBox;
        private DialogueEntity _currentEntity;
        private Texture2D _dialogueBoxTexture;

        public DialogueManager(SpriteFont font, Rectangle dialogueBox, Texture2D dialogueBoxTexture)
        {
            _font = font;
            _dialogueBox = dialogueBox;
            _dialogueBoxTexture = dialogueBoxTexture;
        }

        public void SetCurrentEntity(DialogueEntity entity)
        {
            _currentEntity = entity;
        }

        public void Update(GameTime gameTime)
        {
            if (_currentEntity != null)
            {
                var keyState = Keyboard.GetState();
                if (keyState.IsKeyDown(Keys.Enter) && Singleton.Instance.CurrentKey != Singleton.Instance.PreviousKey)
                {
                    _currentEntity.AdvanceDialogue();

                    //Singleton.Instance.messageLog.AddMessage("" + _currentEntity.CurrentLineIndex, gameTime);
                }

                if (_currentEntity.CurrentLineIndex >= _currentEntity.DialogueLines.Length)
                {
                    CloseDialogueBox();
                }
            }
            
        }

        public void Draw(SpriteBatch spriteBatch)
        {
            if (_currentEntity != null)
            {

                spriteBatch.Draw(_dialogueBoxTexture, _dialogueBox, Singleton.Instance.color == Color.White ? Color.Black : Singleton.Instance.color);

                string wrappedDialogueText = WrapText(_currentEntity.GetCurrentLine(), 66);

                spriteBatch.DrawString(_font, wrappedDialogueText, new Vector2(_dialogueBox.X + 10, _dialogueBox.Y + 10), Color.White);
                //spriteBatch.Draw(_game.Content.Load<Texture2D>("DialogueBox"), _dialogueBox, Color.White);
            }
        }

        private string WrapText(string text, int maxCharsPerLine)
        {
            string wrappedText = "";
            int charCount = 0;
            foreach (char c in text)
            {
                if (charCount == maxCharsPerLine)
                {
                    wrappedText += "\n";
                    charCount = 0;
                }

                wrappedText += c;
                charCount++;
            }

            return wrappedText;
        }

        public void CloseDialogueBox()
        {
            _currentEntity = null;
        }
    }
}