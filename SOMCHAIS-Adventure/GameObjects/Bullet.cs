using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using SOMCHAISAdventure.GameObjects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SOMCHAIS_Adventure
{
    class Bullet : GameObject
    {
        public float DistanceMoved;
        public float Speed = 2;
        public enum Axis
        {
            X,
            Y,
        }
        public Axis axisDirection;

        public Rectangle BoundingRectangle
        {
            get
            {
                return new Rectangle((int)Position.X, (int)Position.Y, Viewport.Width, Viewport.Height);
            }
        }

        public Bullet(Texture2D texture) : base(texture)
        {

        }
        public override void Draw(SpriteBatch spriteBatch)
        {
            if (Name.Equals("PlayerBullet"))
            {
                spriteBatch.Draw(_texture, Position, Viewport, Color.White);
            }
            if (Name.Equals("EnemyBullet"))
            {
                spriteBatch.Draw(_texture, Position, Viewport, Color.Red);
            }

            base.Draw(spriteBatch);
        }

        public override void Reset()
        {
            DistanceMoved = 0;
            base.Reset();
        }

        public override void Update(GameTime gameTime, List<GameObject> gameObjects)
        {
            if (axisDirection == Axis.X)
            {
                DistanceMoved += Math.Abs(Velocity.X * gameTime.ElapsedGameTime.Ticks / TimeSpan.TicksPerSecond);
            }
            else if (axisDirection == Axis.Y)
            {
                DistanceMoved += Math.Abs(Velocity.Y * gameTime.ElapsedGameTime.Ticks / TimeSpan.TicksPerSecond);
            }

            Position = Position + Velocity * Speed * gameTime.ElapsedGameTime.Ticks / TimeSpan.TicksPerSecond;

            foreach (GameObject s in gameObjects)
            {
                if (Name.Equals("PlayerBullet"))
                {
                    if (IsTouching(s) && (s is Enemy))
                    {
                        if (s.Name.Equals("ShieldMonster"))
                        {
                            IsActive = false;

                            if ((s as ShieldMonster).MovingDirection == ShieldMonster.Direction.Left && IsTouchingRight(s)) 
                            {
                                (s as Enemy).Life -= 1;
                            }
                            if ((s as ShieldMonster).MovingDirection == ShieldMonster.Direction.Right && IsTouchingLeft(s)) 
                            {
                                (s as Enemy).Life -= 1;
                            }

                        }
                        else if (s.Name.Equals("FirstBoss") && (s as FirstBoss).isInvincible)
                        {
                            IsActive = false;
                        }
                        else
                        {
                            (s as Enemy).Life -= 1;
                            IsActive = false;
                        }
                    }
                }
                if (Name.Equals("EnemyBullet"))
                {
                    if (IsTouching(s) && (s.Name.Equals("Player")) && !(s as Player).isInvincible)
                    {
                        Singleton.Instance.life -= 1;
                        (s as Player).isInvincible = true;
                        break;
                    }
                }

            }

            if (axisDirection == Axis.X)
            {
                if (DistanceMoved >= Singleton.SCREENWIDTH || DistanceMoved <= 0) IsActive = false;
            }
            else
            {
                if (DistanceMoved >= Singleton.SCREENHEIGHT || DistanceMoved <= 0) IsActive = false;
            }


            if (IsBulletHit())
            {
                IsActive = false;
            }

            base.Update(gameTime, gameObjects);
        }


        private bool IsBulletHit()
        {
            bool isHit = false; // Initialize the variable to false

            // Get the bullet's bounding rectangle and find neighboring tiles.
            Rectangle bounds = BoundingRectangle;
            int leftTile = (int)Math.Floor((float)bounds.Left / Tile.Width);
            int rightTile = (int)Math.Ceiling(((float)bounds.Right / Tile.Width)) - 1;
            int topTile = (int)Math.Floor((float)bounds.Top / Tile.Height);
            int bottomTile = (int)Math.Ceiling(((float)bounds.Bottom / Tile.Height)) - 1;

            // For each potentially colliding tile,
            for (int y = topTile; y <= bottomTile; ++y)
            {
                for (int x = leftTile; x <= rightTile; ++x)
                {
                    // If this tile is collidable,
                    TileCollision collision = Singleton.Instance.level.GetCollision(x, y);
                    if (collision == TileCollision.Impassable)
                    {
                        isHit = true;
                        /* Determine collision depth (with direction) and magnitude.
                        Rectangle tileBounds = Singleton.Instance.level.GetBounds(x, y);
                        Vector2 depth = RectangleExtensions.GetIntersectionDepth(bounds, tileBounds);
                        if (depth != Vector2.Zero)
                        {
                            isHit = true; // Set the variable to true when a collision occurs
                            break; // Exit the loop since we've detected a collision
                        }*/
                    }
                }
                if (isHit)
                    break; // Exit the outer loop if a collision has been detected
            }

            return isHit; // Return the boolean variable indicating whether a collision occurred
        }
    }
}
