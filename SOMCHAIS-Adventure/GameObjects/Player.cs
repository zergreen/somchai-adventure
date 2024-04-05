using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using SOMCHAISAdventure.GameObjects;
using System;
using System.Collections.Generic;

namespace SOMCHAIS_Adventure
{
    class Player : GameObject
    {
        public Keys Left, Right, Fire, Up;
        public Bullet bullet;

        private float jumpVelocity = -8000f;
        private float gravity = 10000f;

        public bool isInvincible = false;
        private float delayInvincible = 0f;
        private bool isVisible = true;
        private float blinkTimer = 0f;

        public bool isHint = false;
        private bool swap = false;

        public int posX, posY;

        public Vector2 PositionHud;

        //give direction to player
        public enum Direction
        {
            Left,
            Right
        }
        public Direction MovingDirection;

        private float previousBottom;

        private SoundEffect deathSound;

        public bool IsOnGround
        {
            get { return isOnGround; }
        }
        bool isOnGround;

        public Rectangle BoundingRectangle
        {
            get
            {
                return new Rectangle((int)Position.X, (int)Position.Y, Viewport.Width, Viewport.Height);
            }
        }

        public Player(Texture2D texture) : base(texture)
        {
        }

        public Player(Texture2D texture, SoundEffect soundEffect) : base(texture, soundEffect)
        {
            deathSound = soundEffect;
        }

        public override void Draw(SpriteBatch spriteBatch)
        {
            
            if (isVisible)
            {
                if (MovingDirection == Direction.Right)
                {
                    spriteBatch.Draw(_texture, Position, Viewport, Color.White);
                }
                else
                {
                    spriteBatch.Draw(_texture, Position, Viewport, Color.White, 0f, Vector2.Zero, 1f, SpriteEffects.FlipHorizontally, 0f);
                }
            }

            if (isHint)
            {
                spriteBatch.Draw(_texture, PositionHud, new Rectangle(32, 912, 32, 32), Color.White); // heart
            }

            base.Draw(spriteBatch);
        }

        public override void Reset()
        {
            Singleton.Instance.life = 3;

            //initial player's side
            MovingDirection = Direction.Right;
            isInvincible = false;

            base.Reset();
        }

        public override void Update(GameTime gameTime, List<GameObject> gameObjects)
        {
            isHint = false;

            PositionHud.X = Position.X + 16;
            PositionHud.Y = Position.Y - 16;

            if (isInvincible)
            {
                delayInvincible += (float)gameTime.ElapsedGameTime.TotalSeconds;
                blinkTimer += (float)gameTime.ElapsedGameTime.TotalSeconds;

                if (blinkTimer >= 0.2f)
                {
                    isVisible = !isVisible;
                    blinkTimer = 0f;
                }

                // delay invicible for 3 second
                if (delayInvincible >= 3)
                {
                    isInvincible = false;
                    delayInvincible = 0f;
                    isVisible = true;

                    deathSound.Play();
                }
            }

            if (Singleton.Instance.CurrentKey.IsKeyDown(Left))
            {
                Velocity.X = -500;
                MovingDirection = Direction.Left;
                bullet.Velocity.X = -100f;
            }
            if (Singleton.Instance.CurrentKey.IsKeyDown(Right))
            {
                Velocity.X = 500;
                MovingDirection = Direction.Right;
                bullet.Velocity.X = 100f;
            }

            if (Singleton.Instance.CurrentKey.IsKeyDown(Up) && isOnGround)
            {
                Velocity.Y = jumpVelocity;
            }

            if (Singleton.Instance.CurrentKey.IsKeyDown(Fire) && Singleton.Instance.CurrentKey != Singleton.Instance.PreviousKey)
            {
                Viewport = new Rectangle(64, 348, 36, 36);
                var newBullet = bullet.Clone() as Bullet;
                newBullet.Position = new Vector2(Rectangle.Width / 2 + Position.X - newBullet.Rectangle.Width / 2, Position.Y);
                newBullet.Reset();
                gameObjects.Add(newBullet);

                SoundEffect.Play();
            }

            if (Singleton.Instance.isCunning && Singleton.Instance.CurrentKey.IsKeyDown(Keys.H) && Singleton.Instance.CurrentKey != Singleton.Instance.PreviousKey)
            {
                swap = !swap;
            }

            // FEATURE: Dig Tile
            if (Singleton.Instance.isDig && Singleton.Instance.CurrentKey.IsKeyDown(Keys.Q) && Singleton.Instance.PreviousKey != Singleton.Instance.CurrentKey)
            {
                Singleton.Instance.level.tiles[posX, posY] = new Tile(null, TileCollision.Passable);
            }

            Velocity.Y += gravity * (float)gameTime.ElapsedGameTime.TotalSeconds;

            // Update player position
            Position += Velocity * (float)gameTime.ElapsedGameTime.TotalSeconds;
            Singleton.Instance.Player.Position = Position;

            Velocity = Vector2.Zero;

            //If the player is now colliding with the level, separate them.
            HandleCollisions();

            // Level Handele In map By Player
            hasPlayerEnterto(gameTime);
            BossLevelHandler();

            base.Update(gameTime, gameObjects);
        }

        #region Collision
        private void HandleCollisions()
        {
            // Get the player's bounding rectangle and find neighboring tiles.
            Rectangle bounds = BoundingRectangle;
            //Console.WriteLine("bounds: "+bounds);
            int leftTile = (int)Math.Floor((float)bounds.Left / Tile.Width);
            int rightTile = (int)Math.Ceiling(((float)bounds.Right / Tile.Width)) - 1;
            int topTile = (int)Math.Floor((float)bounds.Top / Tile.Height);
            int bottomTile = (int)Math.Ceiling(((float)bounds.Bottom / Tile.Height)) - 1;

            // Reset flag to search for ground collision.
            isOnGround = false;

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
                                    // !Keep x,y tile index when walk on tile
                                    //Console.WriteLine("X: {0} | Y: {1} ", x, y);
                                    posX = x;
                                    posY = y;

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

        #region Level Handele In map By Player
        public void Level2WarpPoint()
        {
            // #1 Start
            if (Vector2.Distance(Position, new Vector2(40, 418)) < 20)
            {
                Position = new Vector2(983, 2370);
            }

            // #2
            if (Vector2.Distance(Position, new Vector2(806, 2146)) < 20)
            {
                Position = new Vector2(760, 1697);
            }

            // #3
            if (Vector2.Distance(Position, new Vector2(1120, 1570)) < 20)
            {
                Position = new Vector2(2087, 1697);
            }

            // #4 Goal
            if (Vector2.Distance(Position, new Vector2(2445, 1570)) < 20)
            {
                Position = new Vector2(2480, 418);
            }

            // #5
            if (Vector2.Distance(Position, new Vector2(2445, 1697)) < 20)
            {
                Position = new Vector2(2126, 2274);
            }

            // #6
            if (Vector2.Distance(Position, new Vector2(1163, 2274)) < 20)
            {
                Position = new Vector2(2126, 2274);
            }

            // #7
            if (Vector2.Distance(Position, new Vector2(2126, 2145)) < 20)
            {
                Position = new Vector2(760, 1697);
            }

            // #8
            if (Vector2.Distance(Position, new Vector2(1170, 1697)) < 20)
            {
                Position = new Vector2(2126, 2274);
            }

            // #9
            if (Vector2.Distance(Position, new Vector2(2485, 2274)) < 20)
            {
                Position = new Vector2(806, 2274);
            }

            // #10
            if (Vector2.Distance(Position, new Vector2(760, 1569)) < 20)
            {
                Position = new Vector2(806, 2274);
            }


            // #11
            if (Vector2.Distance(Position, new Vector2(1170, 1697)) < 20)
            {
                Position = new Vector2(2126, 2274);
            }
        }

        public void hasPlayerEnterto(GameTime gameTime)
        {
            switch (Singleton.Instance.levelIndex)
            {
                case 0:
                    //1400,500 (DANGER AREA ON LEVEL1)
                    if ((Vector2.Distance(Position, new Vector2(1400, 700)) < 50) || ((Vector2.Distance(Position, new Vector2(2600, 630)) < 100)) || ((Vector2.Distance(Position, new Vector2(2761, 800)) < 100)))
                    {
                        Singleton.Instance.life = 0;
                    }

                    if (Singleton.Instance.levelIndex == 0)
                    {
                        if (swap && Vector2.Distance(Position, new Vector2(3120, 387)) < 5)
                        {
                            Singleton.Instance.messageLog.AddMessage("[GUIDE] Kill All Enemy to Clear This level", gameTime);
                        }
                    }

                    break;
                case 1:
                    // FEATURE: Warp
                    if (Vector2.Distance(Position, new Vector2(800, 222)) < 20)
                    {
                        Position = new Vector2(977, 862);
                    }

                    // FEATURE: Unlock Secret Door
                    if (Singleton.Instance.isBoss1Dead && Vector2.Distance(Position, new Vector2(3048, 797)) < 10)
                    {
                        {
                            Singleton.Instance.level.tiles[44, 25] = new Tile(null, TileCollision.Passable); //key

                            Singleton.Instance.level.tiles[76, 25] = new Tile(null, TileCollision.Passable); //door

                            Singleton.Instance.messageLog.AddMessage("[EVENT] Unlock Secret Door" +
                                "" +
                                "", gameTime);
                        }
                    }

                    // ACQUIRED: Dig Skill Lv. 1 -> But Open Secret Door Before
                    if (Singleton.Instance.isBoss1Dead && Vector2.Distance(Position, new Vector2(398, 830)) < 5)
                    {

                        {
                            Singleton.Instance.isDig = true;
                            Singleton.Instance.level.tiles[10, 26] = new Tile(null, TileCollision.Passable); // dig skill pos
                            Singleton.Instance.messageLog.AddMessage("Acquired: Dig Skill", gameTime);
                            Singleton.Instance.messageLog.AddMessage("[SHORTCUT] Press Q For Dig", gameTime);
                        }
                    }

                    // Lock Gate
                    {
                        if (Vector2.Distance(Position, new Vector2(1440, 835)) < 4)
                        {
                            Singleton.Instance.messageLog.AddMessage("[Lock] GATE to Prajim", gameTime);
                        }
                        if (Vector2.Distance(Position, new Vector2(1200, 835)) < 4)
                        {
                            Singleton.Instance.messageLog.AddMessage("[Lock] GATE to Burpha", gameTime);
                        }
                        if (Vector2.Distance(Position, new Vector2(968, 835)) < 4)
                        {
                            Singleton.Instance.messageLog.AddMessage("[Lock] GATE to Akanay", gameTime);
                        }
                        if (Vector2.Distance(Position, new Vector2(720, 835)) < 4)
                        {
                            Singleton.Instance.messageLog.AddMessage("[Lock] GATE to Payup", gameTime);
                        }
                    }

                    // Restrict Area Cannot Enter
                    {
                        if (!Singleton.Instance.isSkillDefected && Vector2.Distance(Position, new Vector2(1000, 2000)) < 1000)
                        {
                            Singleton.Instance.messageLog.AddMessage("[DANGER] Not Allow to this Area", gameTime);
                            Singleton.Instance.messageLog.AddMessage("[GUIDE] Acquire Indefect Skill On Level 2,first", gameTime);
                            Singleton.Instance.life = 0;
                        }
                    }

                    {
                        //ACQUIRED: Indefected Skill can pass Gas Area.
                        if (!Singleton.Instance.isBoss1Dead && Vector2.Distance(Position, new Vector2(1800, 803)) < 5)
                        {
                            Singleton.Instance.messageLog.AddMessage("[GUIDE] Find The Key on This Level", gameTime);
                            isHint = true;
                        }

                        //ACQUIRED: Indefected Skill can pass Gas Area.
                        if (Vector2.Distance(Position, new Vector2(520, 867)) < 5)
                        {
                            isHint = true;
                        }

                        if (swap && Vector2.Distance(Position, new Vector2(520, 867)) < 5)
                        {
                            Singleton.Instance.messageLog.AddMessage("[GUIDE] Try To Jump in Wall", gameTime);
                        }
                    }

                    {
                        //ACQUIRED: Cunning Skill.
                        if (!Singleton.Instance.isBoss1Dead && Vector2.Distance(Position, new Vector2(2968, 799)) < 5)
                        {
                            Singleton.Instance.messageLog.AddMessage("[HINT] Clear Boss First", gameTime);
                        }
                    }

                    // Check Game Win
                    {
                        if (Vector2.Distance(Position, new Vector2(1150, 1600)) < 10)
                        {
                            Singleton.Instance.messageLog.AddMessage("[END] You are true S O M C H A I...", gameTime);
                            Singleton.Instance.CurrentGameState = Singleton.GameState.GameWin;
                        }
                    }

                    break;
                case 2:
                    Level2WarpPoint();

                    {
                        //ACQUIRED: Indefected Skill can pass Gas Area.
                        if (Vector2.Distance(Position, new Vector2(1487, 1055)) < 5)
                        {
                            Singleton.Instance.messageLog.AddMessage("[ACQUIRED] Indefected Skill", gameTime);
                            Singleton.Instance.isSkillDefected = true;
                            Singleton.Instance.level.tiles[37, 33] = new Tile(null, TileCollision.Passable);
                        }
                    }

                    {
                        if (swap && Vector2.Distance(Position, new Vector2(2480, 415)) < 10)
                        {
                            Singleton.Instance.messageLog.AddMessage("[HINT] TRY TO DIG", gameTime);
                        }
                        if (Vector2.Distance(Position, new Vector2(2480, 415)) < 5)
                        {
                            isHint = true;
                        }
                    }

                    break;
                case 3:
                    {
                        //ACQUIRED: Cunning Skill.
                        if (Vector2.Distance(Position, new Vector2(3000, 0)) < 10)
                        {
                            Singleton.Instance.messageLog.AddMessage("[ACQUIRED] Cunning Skill", gameTime);
                            Singleton.Instance.messageLog.AddMessage("[SHORTCUT] Press Enter to toggle activate", gameTime);
                            Singleton.Instance.isCunning = true;
                            Singleton.Instance.level.tiles[75, 0] = new Tile(null, TileCollision.Passable);
                        }
                    }

                    break;
                case 4:
                    // ACQUIRED: End Game When See God****
                    if (Vector2.Distance(Position, new Vector2(880, 2018)) < 5)
                    {
                        Singleton.Instance.CurrentGameState = Singleton.GameState.GameWin;
                    }

                    // UNCLOCK: Skill Door On Level4 After kill boss
                    if (Singleton.Instance.isColorSight == false && Singleton.Instance.isBoss1Dead)
                    {
                        Singleton.Instance.level.tiles[68, 35] = new Tile(null, TileCollision.Passable);

                        Singleton.Instance.messageLog.AddMessage("DOOR is Open", gameTime);

                        Singleton.Instance.messageLog.AddMessage("[ACQUIRED] Eye Of Color", gameTime);
                        Singleton.Instance.messageLog.AddMessage("[SHORTCUT] Press Tab to toggle activate", gameTime);

                        Singleton.Instance.isColorSight = true;

                        Singleton.Instance.messageLog.AddMessage("[CLEAR] Is Boss Game Dead", gameTime);
                    }

                    break;
                default: break;
            }
        }

        public void BossLevelHandler()
        {
            if (Singleton.Instance.isBoss1Dead)
            {
                if (Singleton.Instance.levelIndex == 1)
                {
                    Singleton.Instance.level.tiles[75, 25] = new Tile(null, TileCollision.Passable); //door
                }
            }
        }
        #endregion
    }
}
