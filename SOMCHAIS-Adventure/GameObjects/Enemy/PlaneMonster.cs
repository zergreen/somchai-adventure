using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using SOMCHAIS_Adventure;
using System.Collections.Generic;

namespace SOMCHAISAdventure.GameObjects
{
    internal class PlaneMonster : Enemy
    {
        public Bullet bullet;
        public float fireTimer = 0f;
        public enum Direction
        {
            Left,
            Right
        }
        public Direction MovingDirection;
        public float Speed;
        public float MovedDistance;
        private int MovedRange = 500;

        public PlaneMonster(Texture2D texture) : base(texture)
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
            Life = 4;
            MovedDistance = 0;
            Speed = 250;
            MovingDirection = Direction.Left;
            base.Reset();
        }

        public override void Update(GameTime gameTime, List<GameObject> gameObjects)
        {
            fireTimer += (float)gameTime.ElapsedGameTime.TotalSeconds;

            if (MovingDirection == Direction.Left)
            {
                Velocity.X = -Speed;
            }
            else
            {
                Velocity.X = Speed;
            }

            if (fireTimer >= 2f)
            {
                var newBullet = bullet.Clone() as Bullet;
                newBullet.Position = new Vector2(Rectangle.Width / 2 + Position.X - newBullet.Rectangle.Width / 2, Position.Y);
                newBullet.Reset();
                gameObjects.Add(newBullet);
                fireTimer = 0f;
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
