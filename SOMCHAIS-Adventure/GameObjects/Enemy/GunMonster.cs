using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using SOMCHAIS_Adventure;
using System.Collections.Generic;

namespace SOMCHAISAdventure.GameObjects
{
    internal class GunMonster : Enemy
    {
        public Bullet bullet;
        public float fireTimer = 0f;
        public enum Direction
        {
            Left,
            Right
        }
        public Direction MovingDirection;

        public GunMonster(Texture2D texture) : base(texture)
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
            Life = 2;
            MovingDirection = Direction.Right;
            base.Reset();
        }

        public override void Update(GameTime gameTime, List<GameObject> gameObjects)
        {
            Vector2 direction = Singleton.Instance.Player.Position - Position;

            direction.Normalize();

            Vector2 rightReference = new Vector2(1, 0);

            float dotProduct = Vector2.Dot(direction, rightReference);

            if (dotProduct > 0)
            {
                MovingDirection = Direction.Right;
                bullet.Velocity.X = 100;
            }
            else if (dotProduct < 0)
            {
                MovingDirection = Direction.Left;
                bullet.Velocity.X = -100;
            }

            fireTimer += (float)gameTime.ElapsedGameTime.TotalSeconds;

            if (fireTimer >= 2f)
            {
                var newBullet = bullet.Clone() as Bullet;
                newBullet.Position = new Vector2(Rectangle.Width / 2 + Position.X - newBullet.Rectangle.Width / 2, Position.Y);
                newBullet.Reset();
                gameObjects.Add(newBullet);
                fireTimer = 0f;
            }

            float newX = Position.X;

            float newY = Position.Y;

            Position = new Vector2(newX, newY);

            base.Update(gameTime, gameObjects);
        }
    }
}
