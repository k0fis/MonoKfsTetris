using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace MonoKfsTetris;

public class Game1 : Game
{
    enum GameState
{
    Playing,
    GameOver
}
        GraphicsDeviceManager graphics;
        SpriteBatch spriteBatch;

        const int GridWidth = 10;
        const int GridHeight = 20;
        const int CellSize = 24;

        double lockTimer = 0;
        const double LockDelay = 0.5;
        bool isTouchingGround = false;

        GameState gameState = GameState.Playing;
        int score = 0;
        int level = 1;
        int linesCleared = 0;

        float gameOverInputDelay = 0.3f;
        float gameOverTimer = 0f;


        BitmapFont font;
        Point ghostPos;
        int[,] ghostTetromino;

        int[,] grid = new int[GridWidth, GridHeight];

        Texture2D blockTexture;

        // Tetromino pozice
        Point currentPos = new Point(4, 0);

        Random random = new Random();
        int currentType;        

        int[,] currentTetromino;

        double fallTimer = 0;
        double fallInterval = 0.5; // půl sekundy

        KeyboardState previousKeyboard;
        GamePadState previousPad;


        Color[] colors = { Color.Black, Color.Cyan, Color.Blue, Color.Orange, Color.Yellow, Color.Green, Color.Purple, Color.Red };

        Color ghostColor = Color.Gray * 0.3f;

        float gameOverBlinkTimer;
        readonly float gameOverBlinkInterval = 0.4f;

        Boolean gameOverTextVisible = true;
    public Game1()
    {
        graphics = new GraphicsDeviceManager(this);
        Content.RootDirectory = "Content";
                    
        graphics.PreferredBackBufferWidth = GridWidth * CellSize;
        graphics.PreferredBackBufferHeight = GridHeight * CellSize;
        SpawnTetromino();
    }

    protected override void LoadContent()
    {
        spriteBatch = new SpriteBatch(GraphicsDevice);
        using var stream = System.IO.File.OpenRead("Content/Fonts/PressStart2P-16.png");
        var fontTexture = Texture2D.FromStream(GraphicsDevice, stream);        
        font = new BitmapFont(fontTexture, "Content/Fonts/PressStart2P-16.fnt");

        blockTexture = new Texture2D(GraphicsDevice, 1, 1);
        blockTexture.SetData([Color.White]);        
    }

    protected override void Update(GameTime gameTime)
    {
        if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed ||
            Keyboard.GetState().IsKeyDown(Keys.Escape))
            Exit();

        if (gameState == GameState.GameOver) {
            gameOverBlinkTimer += (float)gameTime.ElapsedGameTime.TotalSeconds;

            if (gameOverBlinkTimer >= gameOverBlinkInterval)
            {
                gameOverBlinkTimer = 0f;
                gameOverTextVisible = !gameOverTextVisible; // přepnout viditelnost
            }
            if (
                Keyboard.GetState().IsKeyDown(Keys.Enter) ||
                GamePad.GetState(PlayerIndex.One).Buttons.Start == ButtonState.Pressed
            )
            {
                
                RestartGame();
            }
            return;
        }


        var keyboard = Keyboard.GetState();
        GamePadState pad = GamePad.GetState(PlayerIndex.One);

        bool leftPressed   = pad.DPad.Left  == ButtonState.Pressed && previousPad.DPad.Left  != ButtonState.Pressed;
        bool rightPressed  = pad.DPad.Right == ButtonState.Pressed && previousPad.DPad.Right != ButtonState.Pressed;
        bool downPressed   = pad.DPad.Down  == ButtonState.Pressed && previousPad.DPad.Down  != ButtonState.Pressed;
        bool hdownPressed  = pad.Buttons.A  == ButtonState.Pressed && previousPad.Buttons.A  != ButtonState.Pressed;

        bool rotatePressed = pad.DPad.Up == ButtonState.Pressed && previousPad.DPad.Up != ButtonState.Pressed;
        rotatePressed |= pad.Buttons.X == ButtonState.Pressed && previousPad.Buttons.X != ButtonState.Pressed;

        bool rotate2Pressed = pad.Buttons.B == ButtonState.Pressed && previousPad.Buttons.B != ButtonState.Pressed;

        previousPad = pad;

        bool moveLeft  = leftPressed  || (keyboard.IsKeyDown(Keys.Left)  && !previousKeyboard.IsKeyDown(Keys.Left));
        bool moveRight = rightPressed || (keyboard.IsKeyDown(Keys.Right) && !previousKeyboard.IsKeyDown(Keys.Right));
        bool softDrop  = downPressed  || (keyboard.IsKeyDown(Keys.Down)  && !previousKeyboard.IsKeyDown(Keys.Down));
        bool hardDrop  = hdownPressed || (keyboard.IsKeyDown(Keys.Space) && !previousKeyboard.IsKeyDown(Keys.Space));
        bool rotate    = rotatePressed || (keyboard.IsKeyDown(Keys.Up) && !previousKeyboard.IsKeyDown(Keys.Up));
        bool rotate2   = rotate2Pressed || (keyboard.IsKeyDown(Keys.Z) && !previousKeyboard.IsKeyDown(Keys.Z));

        previousKeyboard = keyboard;

        fallTimer += gameTime.ElapsedGameTime.TotalSeconds;

        // ====== MOVE ======
        if (moveLeft && CanMove(currentPos.X - 1, currentPos.Y, currentTetromino))
        {
            currentPos.X--;
            lockTimer = 0;
        }

        if (moveRight && CanMove(currentPos.X + 1, currentPos.Y, currentTetromino))
        {
            currentPos.X++;
            lockTimer = 0;
        }

        // ====== SOFT DROP ======
        if (softDrop && CanMove(currentPos.X, currentPos.Y + 1, currentTetromino))
        {
            currentPos.Y++;
            lockTimer = 0;
        }

        // ====== ROTATION ======
        if (rotate)
        {
            var r = RotateClockwise(currentTetromino);
            if (CanMove(currentPos.X, currentPos.Y, r))
            {
                currentTetromino = r;
                lockTimer = 0;
            }
        }

        if (rotate2)
        {
            var r = RotateCounterClockwise(currentTetromino);
            if (CanMove(currentPos.X, currentPos.Y, r))
            {
                currentTetromino = r;
                lockTimer = 0;
            }
        }

        // ====== HARD DROP ======
        if (hardDrop)
        {
            while (CanMove(currentPos.X, currentPos.Y + 1, currentTetromino))
                currentPos.Y++;

            FixTetromino();
            ApplyLineClearAndScore();
            SpawnTetromino();

            lockTimer = 0;
            fallTimer = 0;
            return;
        }

        // ====== GRAVITY + LOCK DELAY ======
        bool onGround = IsOnGround();

        if (!onGround)
        {
            lockTimer = 0;

            if (fallTimer >= fallInterval)
            {
                fallTimer = 0;
                currentPos.Y++;
            }
        }
        else
        {
            lockTimer += gameTime.ElapsedGameTime.TotalSeconds;

            if (lockTimer >= LockDelay)
            {
                FixTetromino();
                ApplyLineClearAndScore();
                SpawnTetromino();

                lockTimer = 0;
                fallTimer = 0;
            }
        }

        UpdateGhostPiece();
        base.Update(gameTime);
    }

    void ApplyLineClearAndScore()
    {
        int cleared = ClearFullRows();
        if (cleared > 0)
        {
            linesCleared += cleared;
            score += ScoreForLines(cleared) * level;
            level = linesCleared / 10 + 1;
            fallInterval = Math.Max(0.1, 0.5 - (level - 1) * 0.05);
        }
    }


    protected override void Draw(GameTime gameTime)
    {
        GraphicsDevice.Clear(Color.Black);

        spriteBatch.Begin();

        font.DrawString(spriteBatch, $"Score: {score}", new Vector2(5, 5), Color.White);
        font.DrawString(spriteBatch, $"Level: {level}", new Vector2(5, 25), Color.White);        

        // vykreslení gridu
        for(int x=0;x<GridWidth;x++)
            for(int y=0;y<GridHeight;y++)
                if(grid[x,y] != 0)
                    spriteBatch.Draw(blockTexture, new Rectangle(x*CellSize, y*CellSize, CellSize, CellSize), colors[grid[x,y]]);

        // vykreslení aktuálního tetromina
        for(int x=0;x<4;x++)
            for(int y=0;y<4;y++) {
                if (ghostTetromino[x, y] != 0)
                    spriteBatch.Draw(blockTexture,
                            new Rectangle((ghostPos.X + x) * CellSize,
                              (ghostPos.Y + y) * CellSize,
                              CellSize,
                              CellSize),
                            ghostColor);

                if(currentTetromino[x,y] != 0)
                    //spriteBatch.Draw(blockTexture, new Rectangle((currentPos.X + x)*CellSize, (currentPos.Y + y)*CellSize, CellSize, CellSize), Color.Cyan);
                    spriteBatch.Draw(
                        blockTexture,
                        new Rectangle(
                            (currentPos.X + x) * CellSize,
                            (currentPos.Y + y) * CellSize,
                            CellSize,
                            CellSize),
                        colors[currentType + 1]
                    );
            }

            if (gameState == GameState.GameOver)
            {
                string []sub  = ["GAME OVER", "", "Press", "START", "or", "ENTER", "", "for a new", "GAME"];

                spriteBatch.Draw(
                    blockTexture,
                    new Rectangle(0, 0, GridWidth * CellSize, GridHeight * CellSize),
                    Color.Black * 0.7f
                );

                float y = 100;
                foreach (string line in sub) {
                    if (string.IsNullOrEmpty(line))
                    {
                        y += font.LineHeight;
                        continue;
                    }

                    Vector2 size = font.MeasureString(line);

                    float x = (GridWidth * CellSize - size.X) / 2f;

                    Color color = Color.White;
                    if (line == "GAME OVER") {
                        color = Color.Red;
                        if (gameOverTextVisible) {
                            color *= 0.4f;
                        }
                    }

                    font.DrawString(
                        spriteBatch,
                        line,
                        new Vector2(x, y),
                        color
                    );

                    y += font.LineHeight;                
                }
            }


        spriteBatch.End();
        base.Draw(gameTime);
    }

    bool CanMove(int newX, int newY, int[,] tetromino)
    {
        for(int x=0;x<4;x++)
            for(int y=0;y<4;y++)
                if(tetromino[x,y] != 0)
                {
                    int gx = newX + x;
                    int gy = newY + y;
                    if(gx < 0 || gx >= GridWidth || gy < 0 || gy >= GridHeight)
                        return false;
                    if(grid[gx,gy] != 0)
                        return false;
                }
        return true;
    }

    void FixTetromino()
    {
        for(int x=0;x<4;x++)
            for(int y=0;y<4;y++)
                if(currentTetromino[x,y] != 0)
                {
                    int gx = currentPos.X + x;
                    int gy = currentPos.Y + y;
                    if(gy >= 0 && gy < GridHeight && gx >=0 && gx < GridWidth)
                        grid[gx,gy] = currentType + 1; // typ bloku, později barevný
                }
    }

    void SpawnTetromino()
    {
        currentType = random.Next(tetrominos.Length);
        currentTetromino = (int[,])tetrominos[currentType].Clone();
        currentPos = new Point(3, 0);

        if (!CanMove(currentPos.X, currentPos.Y, currentTetromino))
        {
            gameState = GameState.GameOver;
        }
    }

    void RestartGame()
    {
        Array.Clear(grid, 0, grid.Length);

        score = 0;
        level = 1;
        linesCleared = 0;

        fallInterval = 0.5;
        fallTimer = 0;
        lockTimer = 0;

        gameState = GameState.Playing;
        SpawnTetromino();
    }

    int[,] RotateClockwise(int[,] matrix)
    {
        int[,] result = new int[4, 4];

        for (int x = 0; x < 4; x++)
            for (int y = 0; y < 4; y++)
                result[x, y] = matrix[3 - y, x];

        return result;
    }

    int[,] RotateCounterClockwise(int[,] matrix)
    {
        int[,] result = new int[4, 4];

        for (int x = 0; x < 4; x++)
            for (int y = 0; y < 4; y++)
                result[x, y] = matrix[y, 3 - x];

        return result;
    }

    bool IsRowFull(int y)
    {
        for (int x = 0; x < GridWidth; x++)
            if (grid[x, y] == 0)
                return false;

        return true;
    }

    void ClearRow(int row)
    {
        for (int y = row; y > 0; y--)
            for (int x = 0; x < GridWidth; x++)
                grid[x, y] = grid[x, y - 1];

        // horní řada = prázdná
        for (int x = 0; x < GridWidth; x++)
            grid[x, 0] = 0;
    }

    int ClearFullRows()
    {
        int cleared = 0;

        for (int y = GridHeight - 1; y >= 0; y--)
        {
            if (IsRowFull(y))
            {
                ClearRow(y);
                cleared++;
                y++;
            }
        }

        return cleared;
    }
   

    void UpdateGhostPiece()
    {
        ghostTetromino = (int[,])currentTetromino.Clone();
        ghostPos = new Point(currentPos.X, currentPos.Y);

        while (CanMove(ghostPos.X, ghostPos.Y + 1, ghostTetromino))
        {
            ghostPos.Y++;
        }
    }


    int ScoreForLines(int lines)
    {
        return lines switch
        {
            1 => 100,
            2 => 300,
            3 => 500,
            4 => 800,
            _ => 0
        };
    }

    bool IsOnGround()
    {
        return !CanMove(currentPos.X, currentPos.Y + 1, currentTetromino);
    }



int[][,] tetrominos = new int[][,]
{
    // I
    new int[4,4]
    {
        {0,0,0,0},
        {1,1,1,1},
        {0,0,0,0},
        {0,0,0,0}
    },

    // O
    new int[4,4]
    {
        {0,1,1,0},
        {0,1,1,0},
        {0,0,0,0},
        {0,0,0,0}
    },

    // T
    new int[4,4]
    {
        {0,1,0,0},
        {1,1,1,0},
        {0,0,0,0},
        {0,0,0,0}
    },

    // S
    new int[4,4]
    {
        {0,1,1,0},
        {1,1,0,0},
        {0,0,0,0},
        {0,0,0,0}
    },

    // Z
    new int[4,4]
    {
        {1,1,0,0},
        {0,1,1,0},
        {0,0,0,0},
        {0,0,0,0}
    },

    // J
    new int[4,4]
    {
        {1,0,0,0},
        {1,1,1,0},
        {0,0,0,0},
        {0,0,0,0}
    },

    // L
    new int[4,4]
    {
        {0,0,1,0},
        {1,1,1,0},
        {0,0,0,0},
        {0,0,0,0}
    }
};


}
