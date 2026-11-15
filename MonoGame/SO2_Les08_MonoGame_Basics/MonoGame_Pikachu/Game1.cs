using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using MonoGame_Pikachu.Extensions;

namespace MonoGame_Pikachu;

/// <summary>
/// Eerste MonoGame-les: speltoestanden met een enum en if/else.
/// Bevat ook de Must-, Should- en Could-uitbreidingen uit de cursus.
/// </summary>
public class Game1 : Game
{
    private const int WINDOW_WIDTH = 768;
    private const int WINDOW_HEIGHT = 540;
    private const float PLAYER_SCALE = 0.75f;
    private const float SHARK_SCALE = 0.30f;

    // Pixels per seconde: dezelfde beweging als 5, 2 en 2 pixels bij 60 updates/s.
    private const float PLAYER_SPEED_IN_PIX_PER_SEC = 300f;
    private const float ENEMY_SPEED_IN_PIX_PER_SEC = 120f;
    private const float BACKGROUND_SPEED_IN_PIX_PER_SEC = 120f;
    private const int ENEMY_SPAWNTIME_IN_MS = 3000;
    private const int MAX_ENEMY_COUNT = 8;

    public enum States
    {
        Menu,
        Play,
        Pause,
        GameOver
    }

    private readonly GraphicsDeviceManager _graphics;

    private States _currentState;

    private SpriteBatch _spriteBatch;
    private SpriteFont _spriteFont;

    private Texture2D _playerTexture;
    private Vector2 _playerPosition;

    private Texture2D _backgroundTexture;
    private Vector2 _backgroundPosition;

    private Texture2D _sharkTexture;
    private List<Vector2> _enemyPositions;
    
    private float _backgroundScale;
    private double _elapsedTimeForSpawn;

    public Game1()
    {
        _graphics = new GraphicsDeviceManager(this);

        Content.RootDirectory = "Content";
        IsMouseVisible = true;

        _graphics.PreferredBackBufferWidth = WINDOW_WIDTH;
        _graphics.PreferredBackBufferHeight = WINDOW_HEIGHT;
    }

    protected override void Initialize()
    {
        _currentState = States.Menu;

        _elapsedTimeForSpawn = 0;
        _playerPosition = new Vector2(0, _graphics.PreferredBackBufferHeight / 2);
        _backgroundPosition = Vector2.Zero;
        _enemyPositions = new();

        base.Initialize();
    }

    protected override void LoadContent()
    {
        _spriteBatch = new SpriteBatch(GraphicsDevice);

        _spriteFont = Content.Load<SpriteFont>("game-font");
        _playerTexture = Content.Load<Texture2D>("surfing-pikachu");
        _sharkTexture = Content.Load<Texture2D>("haai");
        _backgroundTexture = Content.Load<Texture2D>("water");

        // De texture is 2154 pixels hoog: schaal 0.25 laat onderaan nog een strook vrij.
        _backgroundScale = (float)_graphics.PreferredBackBufferHeight / _backgroundTexture.Height;
    }

    protected override void Update(GameTime gameTime)
    {
        if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed || IsKeyDown(Keys.Escape))
            Exit();

        if (_currentState == States.Menu)
        {
            if (IsKeyDown(Keys.Enter))
                _currentState = States.Play;
        }
        else if (_currentState == States.Play)
        {
            float elapsedSeconds = (float)gameTime.ElapsedGameTime.TotalSeconds;

            UpdateBackgroundPosition(elapsedSeconds);
            UpdatePlayerPosition(elapsedSeconds);
            KeepPlayerInsideWindow();
            UpdateEnemyPositions(elapsedSeconds);
            RemoveEnemiesOutsideWindow();
            SpawnEnemyIfNeeded(gameTime);

            if (IsKeyDown(Keys.P))
                _currentState = States.Pause;
        }
        else if (_currentState == States.Pause)
        {
            // Hervatten verandert alleen de toestand: posities en timer blijven bewaard.
            if (IsKeyDown(Keys.Enter))
                _currentState = States.Play;
        }

        base.Update(gameTime);
    }

    private void UpdatePlayerPosition(float elapsedSeconds)
    {
        // Snelheid gaan we bekijken in pixels per seconde, maar de update gebeurt in milliseconden. Daarom vermenigvuldigen we met elapsedSeconds.
        // Indien onze game de 60fps niet kan halen lost dit het probleem op: even snel, ongeacht de framerate.
        float distance = PLAYER_SPEED_IN_PIX_PER_SEC * elapsedSeconds;

        if (IsKeyDown(Keys.Right, Keys.D))
            _playerPosition.X += distance;

        if (IsKeyDown(Keys.Left, Keys.Q))
            _playerPosition.X -= distance;

        if (IsKeyDown(Keys.Up, Keys.Z))
            _playerPosition.Y -= distance;

        if (IsKeyDown(Keys.Down, Keys.S))
            _playerPosition.Y += distance;
    }

    private void KeepPlayerInsideWindow()
    {
        float maximumX = _graphics.PreferredBackBufferWidth - _playerTexture.Width * PLAYER_SCALE;
        float maximumY = _graphics.PreferredBackBufferHeight - _playerTexture.Height * PLAYER_SCALE;

        // De positie is de linkerbovenhoek; ook de rechter- en onderkant moeten passen.
        _playerPosition.X = MathHelper.Clamp(_playerPosition.X, 0, maximumX);
        _playerPosition.Y = MathHelper.Clamp(_playerPosition.Y, 0, maximumY);
    }

    private void UpdateBackgroundPosition(float elapsedSeconds)
    {
        // Snelheid gaan we bekijken in pixels per seconde, maar de update gebeurt in milliseconden. Daarom vermenigvuldigen we met elapsedSeconds.
        // Indien onze game de 60fps niet kan halen lost dit het probleem op: even snel, ongeacht de framerate.
        _backgroundPosition.X -= BACKGROUND_SPEED_IN_PIX_PER_SEC * elapsedSeconds;
        float backgroundWidth = _backgroundTexture.Width * _backgroundScale;

        // Bewaar de overschreden afstand, zodat het water zonder sprong doorloopt.
        while (_backgroundPosition.X <= -backgroundWidth)
            _backgroundPosition.X += backgroundWidth;
    }

    private void UpdateEnemyPositions(float elapsedSeconds)
    {
        for (var index = 0; index < _enemyPositions.Count; index++)
        {
            var position = _enemyPositions[index];

            // Snelheid gaan we bekijken in pixels per seconde, maar de update gebeurt in milliseconden. Daarom vermenigvuldigen we met elapsedSeconds.
            // Indien onze game de 60fps niet kan halen lost dit het probleem op:  even snel, ongeacht de framerate.
            _enemyPositions[index] = new Vector2(position.X - ENEMY_SPEED_IN_PIX_PER_SEC * elapsedSeconds, position.Y);
        }
    }

    private void RemoveEnemiesOutsideWindow()
    {
        float sharkWidth = _sharkTexture.Width * SHARK_SCALE;

        // Achterwaarts verwijderen slaat geen volgende haai over wanneer indices opschuiven.
        for (var index = _enemyPositions.Count - 1; index >= 0; index--)
        {
            // We kijken of de volledige haai buiten het scherm is: de linkerkant + breedte moet kleiner of gelijk aan 0 zijn.
            if (_enemyPositions[index].X + sharkWidth <= 0)
                _enemyPositions.RemoveAt(index);
        }
    }

    private void SpawnEnemyIfNeeded(GameTime gameTime)
    {
        // Hoeveel tijd is er verstreken sinds de laatste spawn? Indien dit kleiner is dan de spawn-interval, doen we niets.
        _elapsedTimeForSpawn += gameTime.ElapsedGameTime.TotalMilliseconds;
        if (_elapsedTimeForSpawn < ENEMY_SPAWNTIME_IN_MS)
            return;

        // Ook bij acht haaien begint een nieuw interval: er is geen wachtrij.
        _elapsedTimeForSpawn = 0;
        if (_enemyPositions.Count >= MAX_ENEMY_COUNT)
            return;

        float maximumY = _graphics.PreferredBackBufferHeight - _sharkTexture.Height * SHARK_SCALE;
        float spawnY = Random.Shared.NextSingle() * maximumY;
        var spawnPosition = new Vector2(_graphics.PreferredBackBufferWidth, spawnY);

        // De hoogte wordt alleen bij de creatie gekozen. Bewegen wijzigt daarna alleen X.
        _enemyPositions.Add(spawnPosition);
    }

    protected override void Draw(GameTime gameTime)
    {
        GraphicsDevice.Clear(Color.CornflowerBlue);

        _spriteBatch.Begin();

        if (_currentState == States.Menu)
        {
            string menuText = "Druk op enter om te beginnen";
            _spriteBatch.DrawString(_spriteFont, menuText, GetCenteredTextPosition(menuText));
        }
        else if (_currentState == States.Play || _currentState == States.Pause)
        {
            DrawBackground();

            _spriteBatch.Draw(_playerTexture, _playerPosition, PLAYER_SCALE);

            foreach (var enemyPosition in _enemyPositions)
                _spriteBatch.Draw(_sharkTexture, enemyPosition, SHARK_SCALE);

            if (_currentState == States.Pause)
            {
                string pauseText = "Pause. Druk op enter om door te doen";
                _spriteBatch.DrawString(_spriteFont, pauseText, GetCenteredTextPosition(pauseText));
            }
        }

        _spriteBatch.End();
        base.Draw(gameTime);
    }

    private void DrawBackground()
    {
        float backgroundWidth = _backgroundTexture.Width * _backgroundScale;
        var nextBackgroundPosition = _backgroundPosition + new Vector2(backgroundWidth, 0);

        _spriteBatch.Draw(_backgroundTexture, _backgroundPosition, _backgroundScale);
        _spriteBatch.Draw(_backgroundTexture, nextBackgroundPosition, _backgroundScale);
    }

    private Vector2 GetCenteredTextPosition(string text)
    {
        float textWidth = _spriteFont.MeasureString(text).X;
        return new Vector2((_graphics.PreferredBackBufferWidth - textWidth) / 2f, 0);
    }

    private bool IsKeyDown(Keys key)
        => Keyboard.GetState().IsKeyDown(key);

    private bool IsKeyDown(Keys firstKey, Keys secondKey)
        => IsKeyDown(firstKey) || IsKeyDown(secondKey);
}
