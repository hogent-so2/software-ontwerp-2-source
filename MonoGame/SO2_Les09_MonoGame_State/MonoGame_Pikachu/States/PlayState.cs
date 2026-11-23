using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using MonoGame_Pikachu.Extensions;

namespace MonoGame_Pikachu.States;

/// <summary>
/// Eén speelsessie. Information Expert: deze state kent de posities, collectie
/// en spawnteller en voert de bijbehorende spelberekeningen uit.
/// </summary>
public class PlayState : AbstractState
{
    // De namen en waarden volgen de vorige MonoGame-versie en de cursus.
    private const float PLAYER_SCALE = 0.75f;
    private const float SHARK_SCALE = 0.30f;
    private const float PLAYER_SPEED_IN_PIX_PER_SEC = 300f;
    private const float ENEMY_SPEED_IN_PIX_PER_SEC = 120f;
    private const float BACKGROUND_SPEED_IN_PIX_PER_SEC = 120f;
    private const int ENEMY_SPAWNTIME_IN_MS = 3000;
    private const int MAX_ENEMY_COUNT = 8;

    // Encapsulatie: andere states krijgen geen toegang tot de wijzigbare lijst.
    // readonly beschermt de referentie; PlayState kan nog items toevoegen.
    private readonly List<Vector2> _enemyPositions;
    private readonly float _backgroundScale;

    private Vector2 _playerPosition;
    private Vector2 _backgroundPosition;
    private double _elapsedTimeForSpawn;

    public PlayState(Game1 context) : base(context)
    {
        // Een nieuw object is een nieuwe sessie. Hervatten gebruikt ditzelfde
        // object en voert deze constructor dus niet opnieuw uit.
        _playerPosition = new Vector2(0, Context.ScreenHeight / 2);
        _backgroundPosition = Vector2.Zero;
        _enemyPositions = new();

        _elapsedTimeForSpawn = 0;
        _backgroundScale = (float)Context.ScreenHeight / Context.BackgroundTexture.Height;
    }

    public override void Update(GameTime gameTime)
    {
        float elapsedSeconds = (float)gameTime.ElapsedGameTime.TotalSeconds;

        UpdateBackgroundPosition(elapsedSeconds);
        UpdatePlayerPosition(elapsedSeconds);
        KeepPlayerInsideWindow();
        UpdateEnemyPositions(elapsedSeconds);
        RemoveEnemiesOutsideWindow();
        SpawnEnemyIfNeeded(gameTime);

        // P krijgt voorrang op G bij gelijktijdige invoer.
        if (IsKeyDown(Keys.P))
        {
            // this is de huidige speelsessie; Context is de bestaande Game1.
            Context.ChangeState(new PauseState(Context, this));
        }
        // Should: tijdelijke testovergang. Botsingen en levens volgen later.
        else if (IsKeyDown(Keys.G))
        {
            Context.ChangeState(new GameOverState(Context));
        }
    }

    private void UpdatePlayerPosition(float elapsedSeconds)
    {
        // Pixels per seconde maal verstreken seconden geeft de verplaatsing.
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
        float maximumX = Context.ScreenWidth - Context.PlayerTexture.Width * PLAYER_SCALE;
        float maximumY = Context.ScreenHeight - Context.PlayerTexture.Height * PLAYER_SCALE;

        // De positie is de linkerbovenhoek; ook rechts en onder moet alles passen.
        _playerPosition.X = MathHelper.Clamp(_playerPosition.X, 0, maximumX);
        _playerPosition.Y = MathHelper.Clamp(_playerPosition.Y, 0, maximumY);
    }

    private void UpdateBackgroundPosition(float elapsedSeconds)
    {
        _backgroundPosition.X -= BACKGROUND_SPEED_IN_PIX_PER_SEC * elapsedSeconds;

        float backgroundWidth = Context.BackgroundTexture.Width * _backgroundScale;

        // Behoud de overschreden afstand, zodat het water zonder sprong doorloopt.
        while (_backgroundPosition.X <= -backgroundWidth)
            _backgroundPosition.X += backgroundWidth;
    }

    private void UpdateEnemyPositions(float elapsedSeconds)
    {
        for (var index = 0; index < _enemyPositions.Count; index++)
        {
            var position = _enemyPositions[index];
            _enemyPositions[index] = new Vector2(
                position.X - ENEMY_SPEED_IN_PIX_PER_SEC * elapsedSeconds,
                position.Y);
        }
    }

    private void RemoveEnemiesOutsideWindow()
    {
        float sharkWidth = Context.SharkTexture.Width * SHARK_SCALE;

        // Achterwaarts verwijderen voorkomt dat opschuivende items overgeslagen worden.
        for (var index = _enemyPositions.Count - 1; index >= 0; index--)
        {
            if (_enemyPositions[index].X + sharkWidth <= 0)
                _enemyPositions.RemoveAt(index);
        }
    }

    private void SpawnEnemyIfNeeded(GameTime gameTime)
    {
        _elapsedTimeForSpawn += gameTime.ElapsedGameTime.TotalMilliseconds;
        if (_elapsedTimeForSpawn < ENEMY_SPAWNTIME_IN_MS)
            return;

        // Ook bij acht haaien begint een nieuw interval: er is geen wachtrij.
        _elapsedTimeForSpawn = 0;
        if (_enemyPositions.Count >= MAX_ENEMY_COUNT)
            return;

        float maximumY = Context.ScreenHeight - Context.SharkTexture.Height * SHARK_SCALE;
        float spawnY = Random.Shared.NextSingle() * maximumY;
        var spawnPosition = new Vector2(Context.ScreenWidth, spawnY);
        _enemyPositions.Add(spawnPosition);
    }

    public override void Draw(GameTime gameTime, SpriteBatch spriteBatch)
    {
        // Draw leest de sessiegegevens. Pauze kan deze methode blijven aanroepen.
        DrawBackground(spriteBatch);
        DrawPlayer(spriteBatch);
        DrawEnemies(spriteBatch);
    }

    private void DrawBackground(SpriteBatch spriteBatch)
    {
        float backgroundWidth = Context.BackgroundTexture.Width * _backgroundScale;
        var nextBackgroundPosition = _backgroundPosition + new Vector2(backgroundWidth, 0);

        spriteBatch.Draw(Context.BackgroundTexture, _backgroundPosition, _backgroundScale);
        spriteBatch.Draw(Context.BackgroundTexture, nextBackgroundPosition, _backgroundScale);
    }

    private void DrawPlayer(SpriteBatch spriteBatch)
    {
        spriteBatch.Draw(Context.PlayerTexture, _playerPosition, PLAYER_SCALE);
    }

    private void DrawEnemies(SpriteBatch spriteBatch)
    {
        foreach (var enemyPosition in _enemyPositions)
            spriteBatch.Draw(Context.SharkTexture, enemyPosition, SHARK_SCALE);
    }

}
