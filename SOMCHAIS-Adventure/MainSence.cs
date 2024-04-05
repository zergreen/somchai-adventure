﻿using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;
using SOMCHAISAdventure;
using SOMCHAISAdventure.GameObjects;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Vector2 = Microsoft.Xna.Framework.Vector2;

namespace SOMCHAIS_Adventure
{
    public class MainSence : Game
    {
        private GraphicsDeviceManager _graphics;
        private SpriteBatch _spriteBatch;

        private List<GameObject> _gameObjects;
        private int _numObject;

        SpriteFont _font;

        // Meta-level game state.
        private int levelIndex = -1;

        private const int numberOfLevels = 5;

        public Texture2D _bg, _howtoplay, _howtoplay2, _howtoplay3, _howtoplay4, _endgame, _overlay, _map;
        public Texture2D gameSprite;

        public int screenWidth = Singleton.SCREENWIDTH;
        public int screenHeight = Singleton.SCREENHEIGHT;

        // Green Define HUD
        TimeSpan currentTime;
        bool isMap = false, debug = false;

        // Camera properties
        private Vector2 cameraPosition;
        private Vector2 targetCameraPosition;
        private float cameraLerpFactor = 0.1f; // Adjust this value to control the smoothing amount

        int currentMenuIndex = 0; // Initialize the current menu index to 0
        //int menuIndexIsOne = 1;

        public MainSence()
        {
            _graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
            IsMouseVisible = true;
        }

        protected override void Initialize()
        {
            Window.Title = "SOMCHAI'S ADVENTURE";
            _graphics.PreferredBackBufferWidth = Singleton.SCREENWIDTH;
            _graphics.PreferredBackBufferHeight = Singleton.SCREENHEIGHT;
            _graphics.ApplyChanges();

            _gameObjects = new List<GameObject>();

            base.Initialize();
        }

        protected override void LoadContent()
        {
            _spriteBatch = new SpriteBatch(GraphicsDevice);
            _font = Content.Load<SpriteFont>("GameFont");
            Singleton.Instance.font = _font;

            _bg = Content.Load<Texture2D>("bg/factory");

            _howtoplay = Content.Load<Texture2D>("Overlay/HowToPlay");
            _howtoplay2 = Content.Load<Texture2D>("Overlay/HowToPlay2");
            _howtoplay3 = Content.Load<Texture2D>("Overlay/HowToPlay3");
            _howtoplay4 = Content.Load<Texture2D>("Overlay/HowToPlay4");
            _endgame = Content.Load<Texture2D>("Overlay/GameEnding");
            _overlay = Content.Load<Texture2D>("Overlay/overlay");
            _map = Content.Load<Texture2D>("Overlay/Map");

            Singleton.Instance.messageLog = new MessageLog(10, new Vector2(10, 10), _font);

            try
            {
                MediaPlayer.IsRepeating = true;
                MediaPlayer.Volume = 0.1f;
                MediaPlayer.Play(Content.Load<Song>("Sounds/Music"));
            }
            catch { }

            //Load GameSprite
            gameSprite = this.Content.Load<Texture2D>("sprite");

            // Initialize currentTime
            currentTime = TimeSpan.Zero;

            LoadNextLevel();
            Reset();
        }

        protected override void Update(GameTime gameTime)
        {
            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed || Keyboard.GetState().IsKeyDown(Keys.Escape))
                Exit();

            Singleton.Instance.CurrentKey = Keyboard.GetState();
            _numObject = _gameObjects.Count;

            switch (Singleton.Instance.CurrentGameState)
            {
                case Singleton.GameState.StartNewLife:
                    Singleton.Instance.CurrentGameState = Singleton.GameState.GamePlaying;
                    break;

                case Singleton.GameState.GamePlaying:
                    // Update the current time
                    currentTime += gameTime.ElapsedGameTime;

                    for (int i = 0; i < _numObject; i++)
                    {
                        if (_gameObjects[i].IsActive) _gameObjects[i].Update(gameTime, _gameObjects);

                    }
                    for (int i = 0; i < _numObject; i++)
                    {
                        if (!_gameObjects[i].IsActive)
                        {
                            _gameObjects.RemoveAt(i);
                            i--;
                            _numObject--;
                        }
                    }

                    // Level State
                    if (Singleton.Instance.level.ReachedExit)
                    {
                        if (_gameObjects.OfType<Enemy>().Count() == 0)
                        {
                            if (Singleton.Instance.isBoss1Dead && Singleton.Instance.levelIndex == 4)
                            {
                                LoadLevel(1);
                            }
                            else
                            {
                                LoadNextLevel();
                            }
                            Reset();
                        }
                    }

                    // Warp to Level 1 When Aquire SkillDefected
                    if ((Vector2.Distance(Singleton.Instance.Player.Position, new Vector2(1487, 1059)) < 5 &&
                        Singleton.Instance.isSkillDefected && Singleton.Instance.levelIndex == 2))
                    {
                        LoadLevel(1);
                        Reset();
                    }

                    if (Singleton.Instance.life == 0)
                    {
                        Singleton.Instance.playerDeadCount++;
                        Singleton.Instance.CurrentGameState = Singleton.GameState.GameOver;
                    }

                    if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed || Keyboard.GetState().IsKeyDown(Keys.H) && Singleton.Instance.CurrentKey != Singleton.Instance.PreviousKey)
                    {
                        Singleton.Instance.CurrentGameState = Singleton.GameState.Tutorial;
                    }

                    // UPDATE: Tiles Level
                    Singleton.Instance.level.Update();

                    break;

                case Singleton.GameState.GameOver:

                    if (!Singleton.Instance.CurrentKey.Equals(Singleton.Instance.PreviousKey) && Singleton.Instance.CurrentKey.GetPressedKeys().Length > 0)
                    {
                        //any keys pressed to start
                        ReloadCurrentLevel();
                        Reset();
                        Singleton.Instance.CurrentGameState = Singleton.GameState.StartNewLife;
                    }
                    break;

                  

                case Singleton.GameState.Tutorial:
                    KeyboardState currentKeyState = Keyboard.GetState();
                    Keys[] pressedKeys = currentKeyState.GetPressedKeys();
                    

                    // If no keys are pressed or the current key state is different from the previous key state
                    if (pressedKeys.Length == 0 || !currentKeyState.Equals(Singleton.Instance.PreviousKey))
                    {
                        // Update the menu based on the current menu index

                        // Update the previous key state
                        Singleton.Instance.PreviousKey = currentKeyState;

                        // Handle arrow key presses
                        switch (pressedKeys.Length)
                        {
                            case 0:
                                // No keys pressed
                                break;
                            case 1:
                                Keys pressedKey = pressedKeys[0];
                                switch (pressedKey)
                                {
                                    case Keys.H:
                                        menu[i] = 0;
                                        Singleton.Instance.CurrentGameState = Singleton.GameState.GamePlaying;
                                        break;

                                    case Keys.Down:
                                        // Handle down arrow key press
                                        break;
                                    case Keys.Left:
                                        // Move to the previous menu
                                        currentMenuIndex = Math.Max(0, currentMenuIndex - 1);
                                        Singleton.Instance.messageLog.AddMessage(currentMenuIndex.ToString(), gameTime);
                                        menu[i] = currentMenuIndex;
                                        break;
                                    case Keys.Right:
                                        // Move to the next menu
                                        currentMenuIndex = Math.Min(4, currentMenuIndex + 1);
                                        Singleton.Instance.messageLog.AddMessage(currentMenuIndex.ToString(), gameTime);
                                        menu[i] = currentMenuIndex;
                                        break;
                                }
                                break;
                            default:
                                // Multiple keys pressed
                                menu[currentMenuIndex] = 0;
                                //isHowToplay = false;
                                Singleton.Instance.CurrentGameState = Singleton.GameState.GamePlaying;
                                break;
                        }
                    }

                    

                    break;

                case Singleton.GameState.GameWin:
                    if (!Singleton.Instance.CurrentKey.Equals(Singleton.Instance.PreviousKey) && Singleton.Instance.CurrentKey.GetPressedKeys().Length > 0)
                    {
                        /*any keys pressed to start*/
                        LoadLevel(0);

                        /* clear Singleton to default */
                        Singleton.Instance.life = 3;
                        Singleton.Instance.playerDeadCount = 0;
                        Singleton.Instance.isBoss1Dead = false;

                        // Reset Player Ability in Game
                        Singleton.Instance.isCunning = false;
                        Singleton.Instance.isColorSight = false;
                        Singleton.Instance.isDig = false;
                        Singleton.Instance.isSkillDefected = false;

                        //Timer = 0;
                        currentTime = TimeSpan.Zero;

                        Reset();
                        Singleton.Instance.CurrentGameState = Singleton.GameState.StartNewLife;
                    }
                    break;
                
            }



            // Debug Zone
            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed || Keyboard.GetState().IsKeyDown(Keys.F1) && Singleton.Instance.CurrentKey != Singleton.Instance.PreviousKey)
                debug = !debug;

            if (debug)
            {

                if (Keyboard.GetState().IsKeyDown(Keys.F2) && Singleton.Instance.CurrentKey != Singleton.Instance.PreviousKey)
                {
                    ReloadCurrentLevel();
                    Reset();
                }

               

                if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed || Keyboard.GetState().IsKeyDown(Keys.F4) && Singleton.Instance.CurrentKey != Singleton.Instance.PreviousKey)
                    Singleton.Instance.CurrentGameState = Singleton.GameState.GameWin;

                if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed || Keyboard.GetState().IsKeyDown(Keys.F5) && Singleton.Instance.CurrentKey != Singleton.Instance.PreviousKey)
                {
                    LoadNextLevel();
                    Reset();
                }

                if (Keyboard.GetState().IsKeyDown(Keys.F6) && Singleton.Instance.CurrentKey != Singleton.Instance.PreviousKey)
                {
                    Singleton.Instance.CurrentGameState = Singleton.GameState.GameOver;
                }
            }

            if (Keyboard.GetState().IsKeyDown(Keys.M) && Singleton.Instance.CurrentKey != Singleton.Instance.PreviousKey)
                isMap = !isMap;

            if (Singleton.Instance.isColorSight && Keyboard.GetState().IsKeyDown(Keys.Tab) && Singleton.Instance.CurrentKey != Singleton.Instance.PreviousKey)
            {
                Singleton.Instance.tick++;
                if (Singleton.Instance.tick > 2) Singleton.Instance.tick = 0;
            }

            Singleton.Instance.PreviousKey = Singleton.Instance.CurrentKey;

            base.Update(gameTime);
        }

        int i = 0;
        int[] menu = { 0, 1, 2, 3,4 };

        protected override void Draw(GameTime gameTime)
        {
            //Load GameSprite

            GraphicsDevice.Clear(Color.Black);

            //draw bg
            _spriteBatch.Begin();

            _spriteBatch.Draw(_bg, Vector2.Zero, Singleton.Instance.color);

            _spriteBatch.End();

            // Calculate the target camera position based on the player's position
            targetCameraPosition = new Vector2(
                Singleton.Instance.Player.Position.X - (Singleton.SCREENWIDTH / 2),
                Singleton.Instance.Player.Position.Y - (Singleton.SCREENHEIGHT / 2)
            );

            // Interpolate the camera position towards the target position
            cameraPosition = Vector2.Lerp(cameraPosition, targetCameraPosition, cameraLerpFactor);

            // Create the camera translation matrix
            Matrix m = Matrix.CreateTranslation(-cameraPosition.X, -cameraPosition.Y, 0);

            _spriteBatch.Begin(samplerState: SamplerState.PointWrap, transformMatrix: m);

            // DRAW: Tiles Level
            Singleton.Instance.level.Draw(_spriteBatch);

            _numObject = _gameObjects.Count;

            for (int i = 0; i < _numObject; i++)
            {
                _gameObjects[i].Draw(_spriteBatch);
            }

            _spriteBatch.End();

            _spriteBatch.Begin();

            #region PLAYER DISPLAY

            if (debug)
            {
                _spriteBatch.DrawString(_font, String.Format("P.Pos: {0}", Singleton.Instance.Player.Position), new Vector2(0, 0), Color.Red);
                _spriteBatch.DrawString(_font, String.Format("Player: {0}", Singleton.Instance.Player), new Vector2(0, 32), Color.Red);
                _spriteBatch.DrawString(_font, String.Format("Health: {0}", Singleton.Instance.life), new Vector2(0, 64), Color.Red);
                _spriteBatch.DrawString(_font, String.Format("Level Map: {0}", Singleton.Instance.levelIndex), new Vector2(0, 96), Color.Red);
            }

            switch (Singleton.Instance.life)
            {
                case 3:
                    _spriteBatch.Draw(gameSprite, new Vector2(0, 448), new Rectangle(0, 512, 95, 32), Color.White); // hear 3
                    break;
                case 2:
                    _spriteBatch.Draw(gameSprite, new Vector2(0, 448), new Rectangle(0, 512, 63, 32), Color.White); // heart 2
                    break;
                case 1:
                    _spriteBatch.Draw(gameSprite, new Vector2(0, 448), new Rectangle(0, 512, 31, 32), Color.White); // heart 1
                    break;
                default:
                    _spriteBatch.Draw(gameSprite, new Vector2(0, 448), new Rectangle(0, 512, 95, 32), Color.White); // if heart > 3 will show default
                    break;

            }

            if (Singleton.Instance.isCunning)
            {
                _spriteBatch.Draw(gameSprite, new Vector2(64, 416), new Rectangle(0, 912, 32, 32), Color.White); 
            }

            if (Singleton.Instance.isDig)
            {
                _spriteBatch.Draw(gameSprite, new Vector2(0, 416), new Rectangle(0, 560, 32, 32), Color.White); 
            }

            if (Singleton.Instance.isColorSight)
            {
                _spriteBatch.Draw(gameSprite, new Vector2(96, 416), new Rectangle(0, 960, 32, 32), Color.White);
            }

            if (Singleton.Instance.isSkillDefected)
            {
                _spriteBatch.Draw(gameSprite, new Vector2(32, 416), new Rectangle(0, 864, 32, 32), Color.White); 

            }

            _spriteBatch.Draw(gameSprite, new Vector2(0, 384), new Rectangle(0, 720, 32, 32), Color.White); 
            _spriteBatch.DrawString(_font, String.Format("{0}", _gameObjects.OfType<Enemy>().Count()), new Vector2(32, 384), Color.Red);

            // own dead count hud
            _spriteBatch.Draw(gameSprite, new Vector2(0, 352), new Rectangle(0, 768, 32, 32), Color.White);
            _spriteBatch.DrawString(_font, String.Format("{0}", Singleton.Instance.playerDeadCount), new Vector2(32, 352), Color.Red);
            #endregion

            #region OVERLAY

            // OUTSIDE OF PLAYER ANNOY
            if (isMap)
            {
                _spriteBatch.Draw(_map, Vector2.Zero, Color.White);

                switch (Singleton.Instance.levelIndex)
                {
                    case 0:
                        _spriteBatch.Draw(gameSprite, new Vector2(362, 326), new Rectangle(0, 816, 32, 32), Color.White);
                        break;
                    case 1:
                        _spriteBatch.Draw(gameSprite, new Vector2(365, 183), new Rectangle(0, 816, 32, 32), Color.White);
                        break;
                    case 2:
                        _spriteBatch.Draw(gameSprite, new Vector2(337, 369), new Rectangle(0, 816, 32, 32), Color.White);
                        break;
                    case 3:
                        _spriteBatch.Draw(gameSprite, new Vector2(337, 88), new Rectangle(0, 816, 32, 32), Color.White);
                        break;
                    case 4:
                        _spriteBatch.Draw(gameSprite, new Vector2(456, 142), new Rectangle(0, 816, 32, 32), Color.White);
                        break;
                }
                _spriteBatch.DrawString(_font, "Level " + Singleton.Instance.levelIndex, new Vector2(100, 100), Color.Red);
            }

            if (Singleton.Instance.CurrentGameState == Singleton.GameState.Tutorial)
            {
                _spriteBatch.Draw(_howtoplay, Vector2.Zero, Color.White); // heart
            }

            if (menu[i] == 2)
            {
                _spriteBatch.Draw(_howtoplay2, Vector2.Zero, Color.White); // heart
            }

            if (menu[i] == 3)
            {
                _spriteBatch.Draw(_howtoplay3, Vector2.Zero, Color.White); // heart
            }

            if (menu[i] == 4)
            {
                _spriteBatch.Draw(_howtoplay4, Vector2.Zero, Color.White); // heart
            }


            if (Singleton.Instance.CurrentGameState == Singleton.GameState.GameWin)
            {
                _spriteBatch.Draw(_endgame, Vector2.Zero, Color.White); // heart
            }

            _spriteBatch.End();

            _spriteBatch.Begin();
            
            // Display Overlay Scene
            if (Singleton.Instance.CurrentGameState == Singleton.GameState.GameOver)
            {
                // Draw the semi-transparent overlay
                _spriteBatch.Draw(_overlay, Vector2.Zero, Color.Black * 0.7f);

                // Draw the "GAMEOVER" text with a larger font and a drop shadow effect
                Vector2 gameOverPosition = new Vector2(screenWidth / 2, screenHeight / 4);
                _spriteBatch.DrawString(_font, "GAMEOVER", gameOverPosition - new Vector2(64, 0), Color.Red);

                // Draw the player death count text with a smaller font and centered
                Vector2 deathCountPosition = new Vector2(screenWidth / 2, screenHeight / 2);
                string deathCountText = String.Format("You Died: {0} Time{1}", Singleton.Instance.playerDeadCount, Singleton.Instance.playerDeadCount == 1 ? "" : "s");
                Vector2 deathCountOrigin = _font.MeasureString(deathCountText) / 2;
                _spriteBatch.DrawString(_font, deathCountText, deathCountPosition, Color.White, 0, deathCountOrigin, 1.0f, SpriteEffects.None, 0);

                // Draw the "Keep Fight, Bro!!!" text with a larger font and a different color
                Vector2 keepFightingPosition = new Vector2(screenWidth / 2, (screenHeight / 4) * 3);
                _spriteBatch.DrawString(_font, "Keep Fight, Bro!!!", keepFightingPosition, Color.Yellow, 0, _font.MeasureString("Keep Fight, Bro!!!") / 2, 1.0f, SpriteEffects.None, 0);

            }

            // Render the current time
            _spriteBatch.DrawString(_font, currentTime.ToString(@"hh\:mm\:ss"), new Vector2(704, 10), Color.White);

            // Darw Text When defeat Boss
            if (Singleton.Instance.isBoss1Dead && Singleton.Instance.levelIndex == 4)
            {
                Vector2 iamuPosition = new Vector2(screenWidth / 2, screenHeight / 2);
                _spriteBatch.DrawString(_font, "I am You From The F U T U R E!!!", iamuPosition, Color.Red, 0, _font.MeasureString("I am You From The F U T U R E!!!") / 2, 2.0f, SpriteEffects.None, 0);
            }

            Singleton.Instance.messageLog.Draw(_spriteBatch, gameTime);

            _spriteBatch.End();

            #endregion OVERLAY

            _graphics.BeginDraw();

            base.Draw(gameTime);
        }

        #region RESET & RESET_ENEMY
        protected void Reset()
        {
            //Reset Value to Initialize Value
            Singleton.Instance.CurrentGameState = Singleton.GameState.StartNewLife;

            SoundEffect shotSound = this.Content.Load<SoundEffect>("Sounds/PlayerShot");

            _gameObjects.Clear();

            _gameObjects.Add(new Player(gameSprite, Content.Load<SoundEffect>("Sounds/TakeDamage"))
            {
                Name = "Player",
                Viewport = new Rectangle(0, 348, 32, 36),
                Position = new Vector2(500, 300),
                Left = Keys.Left,
                Right = Keys.Right,
                Fire = Keys.Space,
                SoundEffect = shotSound,
                Up = Keys.Up,
                bullet = new Bullet(gameSprite)
                {
                    Name = "PlayerBullet",
                    Viewport = new Rectangle(224, 352, 32, 32),
                    Velocity = new Vector2(200f, 0),
                    axisDirection = Bullet.Axis.X
                }
            });

            ResetEnemies();

            foreach (GameObject s in _gameObjects)
            {
                s.Reset();
            }
            
        }

        protected void ResetEnemies()
        {
            switch (Singleton.Instance.levelIndex)
            {

                case 1:
                    {
                        Skull skull = new Skull(gameSprite)
                        {
                            Name = "Skull",
                            Viewport = new Rectangle(0, 0, 32, 32),
                            Position = new Vector2(1060, 317),
                        };

                        _gameObjects.Add(skull);

                        ShieldMonster shieldMonster = new ShieldMonster(gameSprite)
                        {
                            Name = "ShieldMonster",
                            Viewport = new Rectangle(0, 128, 32, 32),
                            Position = new Vector2(2944, 799),
                            Speed = 50
                        };

                        _gameObjects.Add(shieldMonster);
                    }
                    
                    break;
                case 2:
                    {
                        Skull skull = new Skull(gameSprite)
                        {
                            Name = "Skull",
                            Viewport = new Rectangle(0, 0, 32, 32),
                            Position = new Vector2(1208, 2365),
                        };

                        _gameObjects.Add(skull);


                        var cloneSkull = skull.Clone() as Skull;
                        cloneSkull.Position = new Vector2(1200, 2365);
                        _gameObjects.Add(cloneSkull);

                        ShieldMonster shieldMonster = new ShieldMonster(gameSprite)
                        {
                            Name = "ShieldMonster",
                            Viewport = new Rectangle(0, 128, 32, 32),
                            Position = new Vector2(2480, 2270),
                            Speed = 0
                        };

                        _gameObjects.Add(shieldMonster);

                        var cloneShieldMonster = shieldMonster.Clone() as ShieldMonster;
                        cloneShieldMonster.Position = new Vector2(2520, 2367);
                        cloneShieldMonster.Speed = 50;
                        _gameObjects.Add(cloneShieldMonster);




                        GunMonster gunMonster = new GunMonster(gameSprite)
                        {
                            Name = "GunMonster",
                            Viewport = new Rectangle(0, 65, 32, 32),
                            Position = new Vector2(1170, 1697),
                            bullet = new Bullet(gameSprite)
                            {
                                Name = "EnemyBullet",
                                Viewport = new Rectangle(224, 352, 32, 32),
                                Velocity = new Vector2(100f, 0),
                                axisDirection = Bullet.Axis.X
                            }
                        };
                        _gameObjects.Add(gunMonster);

                        var cloneGunMonster = gunMonster.Clone() as GunMonster;
                        cloneGunMonster.Position = new Vector2(1160, 1570);
                        _gameObjects.Add(cloneGunMonster);

                        PlaneMonster planeMonster = new PlaneMonster(gameSprite)
                        {
                            Name = "PlaneMonster",
                            Viewport = new Rectangle(0, 193, 63, 37),
                            Position = new Vector2(2300, 1570),
                            bullet = new Bullet(gameSprite)
                            {
                                Name = "EnemyBullet",
                                Viewport = new Rectangle(224, 352, 32, 32),
                                Velocity = new Vector2(0, 100f),
                                axisDirection = Bullet.Axis.Y
                            }
                        };
                        _gameObjects.Add(planeMonster);

                    }
                    break;

                case 3:
                    {

                        GunMonster gunMonster = new GunMonster(gameSprite)
                        {
                            Name = "GunMonster",
                            Viewport = new Rectangle(0, 65, 32, 32),
                            Position = new Vector2(816, 348),
                            bullet = new Bullet(gameSprite)
                            {
                                Name = "EnemyBullet",
                                Viewport = new Rectangle(224, 352, 32, 32),
                                Velocity = new Vector2(100f, 0),
                                axisDirection = Bullet.Axis.X
                            }
                        };

                        _gameObjects.Add(gunMonster);

                        var cloneGunMonster = gunMonster.Clone() as GunMonster;
                        cloneGunMonster.Position = new Vector2(1248, 226);
                        _gameObjects.Add(cloneGunMonster);

                        var cloneGunMonster1 = gunMonster.Clone() as GunMonster;
                        cloneGunMonster1.Position = new Vector2(1655, 867);
                        _gameObjects.Add(cloneGunMonster1);

                        var cloneGunMonster2 = gunMonster.Clone() as GunMonster;
                        cloneGunMonster2.Position = new Vector2(2450, 732);
                        _gameObjects.Add(cloneGunMonster2);

                        var cloneGunMonster3 = gunMonster.Clone() as GunMonster;
                        cloneGunMonster3.Position = new Vector2(2083, 156);
                        _gameObjects.Add(cloneGunMonster3);

                        var cloneGunMonster4 = gunMonster.Clone() as GunMonster;
                        cloneGunMonster.Position = new Vector2(2962, 30);
                        _gameObjects.Add(cloneGunMonster4);

                        PlaneMonster planeMonster = new PlaneMonster(gameSprite)
                        {
                            Name = "PlaneMonster",
                            Viewport = new Rectangle(0, 193, 63, 37),
                            Position = new Vector2(1647, 50),
                            bullet = new Bullet(gameSprite)
                            {
                                Name = "EnemyBullet",
                                Viewport = new Rectangle(224, 352, 32, 32),
                                Velocity = new Vector2(0, 100f),
                                axisDirection = Bullet.Axis.Y
                            }
                        };

                        planeMonster.Reset();
                        _gameObjects.Add(planeMonster);

                    }
                    break;

                case 4:
                    {

                        FirstBoss firstBoss = new FirstBoss(gameSprite)
                        {
                            Name = "FirstBoss",
                            Viewport = new Rectangle(0, 608, 47, 48),
                            Position = new Vector2(1850, 1104),
                            bullet = new Bullet(gameSprite)
                            {
                                Name = "EnemyBullet",
                                Viewport = new Rectangle(224, 352, 32, 32),
                                Velocity = new Vector2(100f, 0),
                                axisDirection = Bullet.Axis.X
                            }
                        };

                        firstBoss.Reset();
                        _gameObjects.Add(firstBoss);
                    }
                    break;
                case 0:
                    {
                        Skull skull = new Skull(gameSprite)
                        {
                            Name = "Skull",
                            Viewport = new Rectangle(0, 0, 32, 32),
                            Position = new Vector2(1120, 383),
                        };

                        _gameObjects.Add(skull);

                        var cloneSkull1 = skull.Clone() as Skull;
                        cloneSkull1.Position = new Vector2(2120, 387);
                        _gameObjects.Add(cloneSkull1);

                        PlaneMonster planeMonster = new PlaneMonster(gameSprite)
                        {
                            Name = "PlaneMonster",
                            Viewport = new Rectangle(0, 193, 63, 37),
                            Position = new Vector2(960, 189),
                            bullet = new Bullet(gameSprite)
                            {
                                Name = "EnemyBullet",
                                Viewport = new Rectangle(224, 352, 32, 32),
                                Velocity = new Vector2(0, 100f),
                                axisDirection = Bullet.Axis.Y
                            }
                        };

                        _gameObjects.Add(planeMonster);

                        var clonePlaneMonster = planeMonster.Clone() as PlaneMonster;
                        clonePlaneMonster.Position = new Vector2(2000, 300);
                        _gameObjects.Add(clonePlaneMonster);


                        GunMonster gunMonster = new GunMonster(gameSprite)
                        {
                            Name = "GunMonster",
                            Viewport = new Rectangle(0, 65, 32, 32),
                            Position = new Vector2(1808, 387),
                            bullet = new Bullet(gameSprite)
                            {
                                Name = "EnemyBullet",
                                Viewport = new Rectangle(224, 352, 32, 32),
                                Velocity = new Vector2(100f, 0),
                                axisDirection = Bullet.Axis.X
                            }
                        };
                        _gameObjects.Add(gunMonster);

                        ShieldMonster shieldMonster = new ShieldMonster(gameSprite)
                        {
                            Name = "ShieldMonster",
                            Viewport = new Rectangle(0, 128, 32, 32),
                            Position = new Vector2(1927, 387)
                        };

                        _gameObjects.Add(shieldMonster);
                    }
                    break;
                default: return;
            }
        }

        #endregion

        private void LoadNextLevel()
        {
            // move to the next level
            levelIndex = (levelIndex + 1) % numberOfLevels;

            Singleton.Instance.levelIndex = levelIndex;

            // Unloads the content for the current level before loading the next one.
            if (Singleton.Instance.level != null)
                Singleton.Instance.level.Dispose();

            // Load the level.
            string levelPath = string.Format("Content/Levels/{0}.txt", levelIndex);
            using (Stream fileStream = TitleContainer.OpenStream(levelPath))
                Singleton.Instance.level = new TileBuilder(Services, fileStream);
        }

        private void ReloadCurrentLevel()
        {
            --levelIndex;
            LoadNextLevel();
        }

        private void LoadLevel(int levelDestination)
        {
            // move to the next level
            levelIndex = (levelDestination) % numberOfLevels;

            Singleton.Instance.levelIndex = levelIndex;

            // Unloads the content for the current level before loading the next one.
            if (Singleton.Instance.level != null)
                Singleton.Instance.level.Dispose();

            // Load the level.
            string levelPath = string.Format("Content/Levels/{0}.txt", levelIndex);
            using (Stream fileStream = TitleContainer.OpenStream(levelPath))
                Singleton.Instance.level = new TileBuilder(Services, fileStream);
        }

    }
}
