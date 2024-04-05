using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using SOMCHAIS_Adventure;
using System.Collections.Generic;

namespace SOMCHAISAdventure.GameObjects
{
    internal class FirstBoss : Enemy
    {
        public Bullet bullet;
        public float fireTimer = 0f;

        private float delayInvincible = 0f;
        public bool isInvincible = true;

        public enum Direction
        {
            Left,
            Right
        }
        public Direction MovingDirection;

        public float Speed;
        public float MovedDistance;

        private Vector2 PositionHud;

        public FirstBoss(Texture2D texture) : base(texture)
        {
        }

        public override void Draw(SpriteBatch spriteBatch)
        {
            spriteBatch.DrawString(Singleton.Instance.font, String.Format("{0}", Life), PositionHud, Color.Red);

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
            Life = 10;
            MovedDistance = 0;
            Speed = 250;
            MovingDirection = Direction.Left;
            base.Reset();
        }

        public override void Update(GameTime gameTime, List<GameObject> gameObjects)
        {
            PositionHud.X = Position.X + 16;
            PositionHud.Y = Position.Y - 16;

            if (Life <= 0)
            {
                IsActive = false;

                Singleton.Instance.isBoss1Dead = true;
            }

            delayInvincible += (float)gameTime.ElapsedGameTime.TotalSeconds;

            // delay invicible for 10 second
            if (delayInvincible >= 10)
            {
                delayInvincible = 0f;
                isInvincible = !isInvincible;
                if (isInvincible)
                {
                    Viewport = new Rectangle(0, 608, 47, 48);
                }
                else
                {
                    Viewport = new Rectangle(0, 348, 32, 36);
                }
            }

            // Calculate the direction vector from the enemy to the player
            Vector2 direction = Singleton.Instance.Player.Position - Position;

            // Normalize the direction vector to get a unit vector
            direction.Normalize();

            // Reference vector pointing to the right (assuming right is (1,0))
            Vector2 rightReference = new Vector2(1, 0);

            // Calculate the dot product of direction and rightReference
            float dotProduct = Vector2.Dot(direction, rightReference);

            // Determine the side
            if (dotProduct > 0)
            {
                // Player is on the right side of the enemy
                MovingDirection = Direction.Right;
            }
            else if (dotProduct < 0)
            {
                // Player is on the left side of the enemy
                MovingDirection = Direction.Left;
            }

            // Set the velocity based on the direction and speed
            Velocity = direction * Speed;
            bullet.Velocity = direction * 100;

            fireTimer += (float)gameTime.ElapsedGameTime.TotalSeconds;

            if (fireTimer >= 2f)
            {
                var newBullet = bullet.Clone() as Bullet;
                newBullet.Position = new Vector2(Rectangle.Width / 2 + Position.X - newBullet.Rectangle.Width / 2, Position.Y);
                newBullet.Reset();
                gameObjects.Add(newBullet);
                fireTimer = 0f;
            }

            // Update the position based on the velocity and elapsed time
            Position += Velocity * (float)gameTime.ElapsedGameTime.TotalSeconds;

            base.Update(gameTime, gameObjects);
        }
    }
}
