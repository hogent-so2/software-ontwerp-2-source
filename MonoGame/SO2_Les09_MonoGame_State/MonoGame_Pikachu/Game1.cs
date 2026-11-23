using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using MonoGame_Pikachu.States;

namespace MonoGame_Pikachu;

/// <summary>
/// De context van het State Pattern: beheert het venster, de gedeelde resources
/// en de actieve state. Het toestandafhankelijke gedrag staat in de states.
/// </summary>
public class Game1 : Game
{
    private const int WINDOW_WIDTH = 768;
    private const int WINDOW_HEIGHT = 540;

    private readonly GraphicsDeviceManager _graphics;

    // MonoGame roept Initialize en LoadContent aan vóór de eerste Update/Draw.
    // null! beschrijft die levenscyclus voor de compiler; het maakt geen object.
    private AbstractState _currentState = null!;
    private SpriteBatch _spriteBatch = null!;

    public int ScreenWidth 
        => _graphics.PreferredBackBufferWidth;
    public int ScreenHeight 
        => _graphics.PreferredBackBufferHeight;

    // Alleen Game1 kent toe, in LoadContent. init kan hier niet: LoadContent
    // wordt pas na de constructie aangeroepen. States krijgen alleen leestoegang.
    // De MonoGame-objecten zelf worden daardoor niet onveranderlijk.
    public SpriteFont SpriteFont { get; private set; } = null!;
    public Texture2D PlayerTexture { get; private set; } = null!;
    public Texture2D SharkTexture { get; private set; } = null!;
    public Texture2D BackgroundTexture { get; private set; } = null!;

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
        // De basisinitialisatie laat ook LoadContent uitvoeren.
        base.Initialize();

        ChangeState(new MenuState(this));
    }

    protected override void LoadContent()
    {
        _spriteBatch = new SpriteBatch(GraphicsDevice);

        SpriteFont = Content.Load<SpriteFont>("game-font");
        PlayerTexture = Content.Load<Texture2D>("surfing-pikachu");
        SharkTexture = Content.Load<Texture2D>("haai");
        BackgroundTexture = Content.Load<Texture2D>("water");
    }

    public void ChangeState(AbstractState newActiveState)
    {
        ArgumentNullException.ThrowIfNull(newActiveState);

        // De aanvragende state kiest de overgang. De context bewaart alleen
        // de nieuwe referentie en roept hier geen Update of Initialize aan.
        _currentState = newActiveState;
    }

    protected override void Update(GameTime gameTime)
    {
        // Afsluiten geldt in iedere toestand en blijft daarom bij Game1.
        if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed || Keyboard.GetState().IsKeyDown(Keys.Escape))
        {
            Exit();
            return;
        }

        // Polymorfisme: ook HelpState en GameOverState gebruiken dit contract.
        _currentState.Update(gameTime);

        base.Update(gameTime);
    }

    protected override void Draw(GameTime gameTime)
    {
        GraphicsDevice.Clear(Color.CornflowerBlue);
        _spriteBatch.Begin();

        _currentState.Draw(gameTime, _spriteBatch);

        _spriteBatch.End();
        base.Draw(gameTime);
    }
}
