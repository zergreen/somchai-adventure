using System;
using System.Collections.Generic;
using System.IO;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using System.Text.RegularExpressions;
using SOMCHAIS_Adventure;

namespace SOMCHAISAdventure.GameObjects
{

    class TileBuilder : IDisposable
    {
        // Physical structure of the level.
        public Tile[,] tiles;

        //Get each line from text file;
        List<string> lines = new List<string>();
        char tileType;

        // Key locations in the level.        
        private Point exit = InvalidPosition;
        private static readonly Point InvalidPosition = new Point(-1, -1);

        // Level game state.
        private Random random = new Random(354668); // Arbitrary, but constant seed

        public bool ReachedExit
        {
            get { return reachedExit; }
        }
        bool reachedExit;

        // Level content.        
        public ContentManager Content
        {
            get { return content; }
        }
        ContentManager content;

        #region Loading

        public TileBuilder(IServiceProvider serviceProvider, Stream fileStream)
        {
            // Create a new content manager to load content used just by this level.
            content = new ContentManager(serviceProvider, "Content");

            LoadTiles(fileStream);

        }

        private void LoadTiles(Stream fileStream)
        {
            // Load the level and ensure all of the lines are the same length.
            int width;

            using (StreamReader reader = new StreamReader(fileStream))
            {
                string line = reader.ReadLine();
                width = line.Length;
                while (line != null)
                {
                    lines.Add(line);
                    if (line.Length != width)
                        throw new Exception(String.Format("The length of line {0} is different from all preceeding lines.", lines.Count));
                    line = reader.ReadLine();
                }
            }

            // Allocate the tile grid.
            tiles = new Tile[width, lines.Count];

            // Loop over every tile position,
            for (int y = 0; y < Height; ++y)
            {
                for (int x = 0; x < Width; ++x)
                {
                    // to load each tile.
                    tileType = lines[y][x];
                    tiles[x, y] = LoadTile(tileType, x, y);
                }
            }
        }

        private Tile LoadTile(char tileType, int x, int y)
        {
            switch (tileType)
            {
                // Blank space
                case '.':
                    return new Tile(null, TileCollision.Passable);

                // Exit
                case 'X':
                    return LoadExitTile(x, y);

                // Floating platform
                case '-':
                    return LoadTile("Platform", TileCollision.Platform);

                // Floating platform
                case 'L':
                    return LoadTile("LockDoor", TileCollision.Impassable);
                // Floating platform
                case 'K':
                    return LoadTile("Key", TileCollision.Passable);
                // Floating platform
                case 'S':
                    return LoadTile("DigSkill", TileCollision.Passable);

                case 'G':
                    return LoadTile("God", TileCollision.Passable);

                // Impassable block
                case '(':
                    return LoadTile("Invisible", TileCollision.Impassable);

                // Passable block
                case ')':
                    return LoadVarietyTile("BlockA", 7, TileCollision.Passable);

                // Platform block
                case '~':
                    return LoadVarietyTile("BlockB", 2, TileCollision.Impassable);

                // Passable block
                case ':':
                    return LoadVarietyTile("BlockB", 2, TileCollision.Passable);

                // Impassable block
                case '#':
                    return LoadVarietyTile("BlockA", 7, TileCollision.Impassable);
                // Impassable block
                case 'W':
                    return LoadVarietyTile("WarpA", 4, TileCollision.Passable);
                // Impassable block
                case '9':
                    return LoadTile("InDefectSkill", TileCollision.Passable);
                case '8':
                    return LoadTile("cunning", TileCollision.Passable);

                // Unknown tile type character
                default:
                    throw new NotSupportedException(String.Format("Unsupported tile type character '{0}' at position {1}, {2}.", tileType, x, y));
            }
        }

        private Tile LoadTile(string name, TileCollision collision)
        {
            return new Tile(Content.Load<Texture2D>("Tiles/" + name), collision);
        }

        private Tile LoadVarietyTile(string baseName, int variationCount, TileCollision collision)
        {
            int index = random.Next(variationCount);
            return LoadTile(baseName + index, collision);
        }

        private Tile LoadExitTile(int x, int y)
        {
            if (exit != InvalidPosition)
                throw new NotSupportedException("A level may only have one exit.");

            exit = GetBounds(x, y).Center;

            return LoadTile("Exit", TileCollision.Passable);
        }

        public void Dispose()
        {
            Content.Unload();
        }

        #endregion

        #region Bounds and collision

        public TileCollision GetCollision(int x, int y)
        {
            // Prevent escaping past the level ends.
            if (x < 0 || x >= Width)
                return TileCollision.Impassable;
            // Allow jumping past the level top and falling through the bottom.
            if (y < 0 || y >= Height)
                return TileCollision.Passable;

            return tiles[x, y].Collision;
        }


        public Rectangle GetBounds(int x, int y)
        {
            return new Rectangle(x * Tile.Width, y * Tile.Height, Tile.Width, Tile.Height);
        }


        public int Width
        {
            get { return tiles.GetLength(0); }
        }


        public int Height
        {
            get { return tiles.GetLength(1); }
        }

        #endregion

        #region Update

        public void Update()
        {
            // FEATURE: Player move to exit door
            if (Vector2.Distance(Singleton.Instance.Player.Position, new Vector2(exit.X, exit.Y)) < 20) // Assuming 20 unit is close enough
            {
                OnExitReached();
            }
        }


        private void OnExitReached()
        {
            reachedExit = true;
        }

        #endregion

        #region Draw


        public void Draw(SpriteBatch spriteBatch)
        {

            DrawTiles(spriteBatch);

        }

        private void DrawTiles(SpriteBatch spriteBatch)
        {
            Color color;

            switch (Singleton.Instance.levelIndex)
            {
                case 1:
                    color = Color.Green;
                    break;
                case 2:
                    color = Color.Blue;
                    break;
                case 3:
                    color = Color.Red;
                    break;
                default:
                    color = Color.White;
                    break;
            }

            Singleton.Instance.color = color;

            Color originalColor = color;

            // Get the RGB values of the original color
            int r = originalColor.R;
            int g = originalColor.G;
            int b = originalColor.B;

            // Calculate the complementary color by subtracting the RGB values from 255
            int complementaryR = 255 - r;
            int complementaryG = 255 - g;
            int complementaryB = 255 - b;

            // Create a new Color instance with the complementary RGB values
            Color complementaryColor = new Color(complementaryR, complementaryG, complementaryB);

            Color targetColor;

            switch (Singleton.Instance.tick)
            {
                case 0: targetColor = originalColor; break;
                case 1: targetColor = complementaryColor; break;
                case 2: targetColor = Color.White; break;
                default: targetColor = Color.White; break;
            }

            // Use the complementaryColor as desired

            // For each tile position
            for (int y = 0; y < Height; ++y)
            {
                for (int x = 0; x < Width; ++x)
                {
                    // If there is a visible tile in that position
                    Texture2D texture = tiles[x, y].Texture;


                    //char tileType = lines[y][x];

                    if (texture != null)
                    {
                        // Draw it in screen space.
                        Vector2 position = new Vector2(x, y) * Tile.Size;

                        // PREVENT: Color all scence will blast Color!!!
                        if (Regex.IsMatch(lines[y][x].ToString(), "[XWLKSG98]"))
                        {
                            spriteBatch.Draw(texture, position, Color.White);
                        }
                        else
                        {
                            spriteBatch.Draw(texture, position, targetColor);

                        }


                    }
                }
            }
        }

        #endregion
    }
}