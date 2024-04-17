using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;

namespace SOMCHAISAdventure;

public class MessageLog
{
    private readonly List<string> messages;
    private readonly List<float> messageTimes; // New list to store message times
    private readonly int maxMessages;
    private readonly Vector2 position;
    private readonly SpriteFont font;
    private float displayDuration = 3f; // Duration in seconds to display each message

    //private float animationSpeed = 0.5f; // Speed at which the message moves up (pixels per second)
    private float animationDuration = 1f; // Duration for which the message should animate (seconds)

    public MessageLog(int maxMessages, Vector2 position, SpriteFont font)
    {
        this.maxMessages = maxMessages;
        this.position = position;
        this.font = font;
        messages = new List<string>();
        messageTimes = new List<float>(); // Initialize the message times list
    }

    public void AddMessage(string message, GameTime gameTime)
    {
        messages.Add(message);
        messageTimes.Add((float)gameTime.TotalGameTime.TotalSeconds); // Store the current game time

        if (messages.Count > maxMessages)
        {
            messages.RemoveAt(0);
            messageTimes.RemoveAt(0); // Remove the corresponding message time
        }
    }

    public void Draw(SpriteBatch spriteBatch, GameTime gameTime)
    {
        float totalSeconds = (float)gameTime.TotalGameTime.TotalSeconds;

        for (int i = 0; i < messages.Count; i++)
        {
            float elapsedTime = totalSeconds - messageTimes[i];
            if (elapsedTime <= displayDuration)
            {
                // Calculate the vertical position based on the animation
                float animationProgress = Math.Min(elapsedTime, animationDuration) / animationDuration;
                float verticalOffset = -font.LineSpacing * (1 - animationProgress);

                Vector2 messagePosition = position + new Vector2(0, i * font.LineSpacing + verticalOffset);
                Color messageColor = Color.Lerp(Color.Transparent, Color.White, animationProgress);

                spriteBatch.DrawString(font, messages[i], messagePosition, messageColor);
            }
        }
    }

   #if false //old version not display animation
        private readonly List<string> messages;
        private readonly int maxMessages;
        private readonly Vector2 position;
        private readonly SpriteFont font;

        public void AddMessage(string message)
        {
            messages.Add(message);
            if (messages.Count > maxMessages)
                messages.RemoveAt(0);
        }

        public void Draw(SpriteBatch spriteBatch)
        {
            for (int i = 0; i < messages.Count; i++)
            {
                Vector2 messagePosition = position + new Vector2(0, i * font.LineSpacing);
                spriteBatch.DrawString(font, messages[i], messagePosition, Color.White);
            }
        }
     #endif
}