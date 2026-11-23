using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace MonoGame_Pikachu.States;

/// <summary>
/// Should: voorlopig eindscherm, bereikbaar met G vanuit PlayState.
/// Deze state hoeft de beëindigde speelsessie niet te bewaren of te wijzigen.
/// </summary>
public class GameOverState : AbstractState
{
    public GameOverState(Game1 context) : base(context)
    {
    }

    public override void Update(GameTime gameTime)
    {
        if (IsKeyDown(Keys.S))
            Context.ChangeState(new MenuState(Context));
    }

    public override void Draw(GameTime gameTime, SpriteBatch spriteBatch)
    {
        DrawCenteredText(spriteBatch, "Game over", 100);
        DrawCenteredText(spriteBatch, "S: terug naar menu", 220);
        DrawCenteredText(spriteBatch, "Escape: afsluiten", 340);
    }
}
