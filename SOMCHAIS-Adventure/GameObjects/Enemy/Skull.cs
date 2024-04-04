using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SOMCHAIS_Adventure
{
    internal class Skull : Enemy
    {
        public enum Direction
        {
            Left,
            Right
        }
        public Direction MovingDirection;
        public float Speed;
        public float MovedDistance;
        private int MovedRange = 500;

        public Skull(Texture2D texture) : base(texture)
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
            Life = 3;
            MovedDistance = 0;
            Speed = 100;
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

            Position = new Vector2(newX, newY);;

            base.Update(gameTime, gameObjects);
        }

    }
}
