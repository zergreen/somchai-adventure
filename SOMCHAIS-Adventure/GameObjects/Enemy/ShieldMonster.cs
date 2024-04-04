using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using SOMCHAIS_Adventure;
using System.Collections.Generic;

namespace SOMCHAISAdventure.GameObjects
{
    internal class ShieldMonster : Enemy
    {
        public enum Direction
        {
            Left,
            Right
        }
        public Direction MovingDirection;
        public float Speed = 50;
        public float MovedDistance;
        private int MovedRange = 500;
        public ShieldMonster(Texture2D texture) : base(texture)
        {
        }

        public override void Draw(SpriteBatch spriteBatch)
        {
            if (MovingDirection == Direction.Left)
            {
                spriteBatch.Draw(_texture, Position, Viewport, Color.White);
            }
            else
            {
                spriteBatch.Draw(_texture, Position, Viewport, Color.White, 0f, Vector2.Zero, 1f, SpriteEffects.FlipHorizontally, 0f);
            }
            base.Draw(spriteBatch);
        }

        public override void Reset()
        {
            Life = 1;
            MovedDistance = 0;
            MovingDirection = Direction.Left;
            base.Reset();
        }

        public override void Update(GameTime gameTime, List<GameObject> gameObjects)
        {
            if (MovingDirection == Direction.Left)
            {
                Velocity.X = -Speed;
            }
            else
            {
                Velocity.X = Speed;
            }

            float movingThisLoop = Velocity.X * gameTime.ElapsedGameTime.Ticks / TimeSpan.TicksPerSecond;

            MovedDistance += Math.Abs(movingThisLoop);
            float newX = Position.X + movingThisLoop;

            float newY = Position.Y;

            if (MovedDistance >= MovedRange)
            {
                MovingDirection = (MovingDirection == Direction.Left) ? Direction.Right : Direction.Left;
                MovedDistance = 0;
            }
            if (IsHitXAxis)
            {
                MovingDirection = (MovingDirection == Direction.Left) ? Direction.Right : Direction.Left;
                MovedDistance = MovedRange - (int)(MovedDistance);
            }

            Position = new Vector2(newX, newY);

            base.Update(gameTime, gameObjects);
        }
    }
}
