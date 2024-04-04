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
    internal class Enemy : GameObject
    {
        public int Life;

        //SECTION: Enemy delay when die and change viewport
        private float delay = 0f;
        private bool delayActive = false;

        public bool IsOnGround
        {
            get { return isOnGround; }
        }
        bool isOnGround;

        public bool IsHitXAxis
        {
            get { return isHitXAxis; }
        }
        private bool isHitXAxis;

        public Rectangle BoundingRectangle
        {
            get
            {
                return new Rectangle((int)Position.X, (int)Position.Y, Viewport.Width, Viewport.Height);
            }
        }

        private float previousBottom;

        public Enemy(Texture2D texture) : base(texture)
        {
        }

        public override void Draw(SpriteBatch spriteBatch)
        {
        }

        public override void Reset()
        {
            base.Reset();
        }

        public override void Update(GameTime gameTime, List<GameObject> gameObjects)
        {
            #region begin deplay feature
            if (Life == 0 && !delayActive)
            {
                delayActive = true;
                delay = 1f; // 1 seconds delay

                Viewport = new Rectangle(0, 672, 32, 32);

                // Generate a random number between 0 and 1
                double randomNumber = Singleton.Instance.random.NextDouble();

                // Check if the random number falls within the desired probability range 25%
                if (randomNumber < 0.25 && Singleton.Instance.life < 3)
                {
                    Singleton.Instance.life += 1;
                    Singleton.Instance.messageLog.AddMessage("Gain Health!", gameTime);
                }
            }

            if (delayActive)
            {
                // Subtract the elapsed time since the last frame from the delay
                delay -= (float)gameTime.ElapsedGameTime.TotalSeconds;

                // Check if the delay has finished
                if (delay <= 0)
                {
                    IsActive = false;
                    delayActive = false; // Reset the delay flag
                }

                // Enemy Move Up to sky
                Position.Y -= 10;

            }
            #endregion

            // check hit player or not and take player's life
            foreach (GameObject s in gameObjects)
            {
                if (this is Enemy)
                {
                    if (IsTouching(s) && (s.Name.Equals("Player")) && !(s as Player).isInvincible)
                    {
                        Singleton.Instance.life -= 1;
                        (s as Player).isInvincible = true;
                        break;
                    }
                }
            }

            HandleCollisions();

            base.Update(gameTime, gameObjects);
        }

        #region Collision
        private void HandleCollisions()
        {
            // Get the enemy's bounding rectangle and find neighboring tiles.
            Rectangle bounds = BoundingRectangle;
            int leftTile = (int)Math.Floor((float)bounds.Left / Tile.Width);
            int rightTile = (int)Math.Ceiling(((float)bounds.Right / Tile.Width)) - 1;
            int topTile = (int)Math.Floor((float)bounds.Top / Tile.Height);
            int bottomTile = (int)Math.Ceiling(((float)bounds.Bottom / Tile.Height)) - 1;

            // Reset flag to search for ground collision.
            isOnGround = false;
            isHitXAxis = false;

            // For each potentially colliding tile,
            for (int y = topTile; y <= bottomTile; ++y)
            {
                for (int x = leftTile; x <= rightTile; ++x)
                {
                    // If this tile is collidable,
                    TileCollision collision = Singleton.Instance.level.GetCollision(x, y);
                    if (collision != TileCollision.Passable)
                    {
                        // Determine collision depth (with direction) and magnitude.
                        Rectangle tileBounds = Singleton.Instance.level.GetBounds(x, y);
                        Vector2 depth = RectangleExtensions.GetIntersectionDepth(bounds, tileBounds);
                        if (depth != Vector2.Zero)
                        {
                            float absDepthX = Math.Abs(depth.X);
                            float absDepthY = Math.Abs(depth.Y);

                            // Resolve the collision along the shallow axis.
                            if (absDepthY < absDepthX || collision == TileCollision.Platform)
                            {
                                // If we crossed the top of a tile, we are on the ground.
                                if (previousBottom <= tileBounds.Top)
                                    isOnGround = true;



                                // Ignore platforms, unless we are on the ground.
                                if (collision == TileCollision.Impassable || IsOnGround)
                                {
                                    isHitXAxis = true;

                                    // Resolve the collision along the Y axis.
                                    Position = new Vector2(Position.X, Position.Y + depth.Y);

                                    // Perform further collisions with the new bounds.
                                    bounds = BoundingRectangle;
                                }
                            }
                            else if (collision == TileCollision.Impassable) // Ignore platforms.
                            {
                                // Resolve the collision along the X axis.
                                Position = new Vector2(Position.X + depth.X, Position.Y);

                                // Perform further collisions with the new bounds.
                                bounds = BoundingRectangle;
                            }
                        }
                    }
                }
            }

            // Save the new bounds bottom.
            previousBottom = bounds.Bottom;
        }
        #endregion
    }
}
