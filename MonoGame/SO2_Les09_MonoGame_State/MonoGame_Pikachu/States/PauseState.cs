using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace MonoGame_Pikachu.States;

/// <summary>
/// Bewaart de gepauzeerde speelsessie en verwerkt hervatten of opnieuw beginnen.
/// De wereld blijft zichtbaar doordat alleen Draw wordt doorgegeven.
/// </summary>
public class PauseState : AbstractState
{
    private readonly PlayState _previousState;

    public PauseState(Game1 context, PlayState previousState) : base(context)
    {
        ArgumentNullException.ThrowIfNull(previousState);
        _previousState = previousState;
    }

    public override void Update(GameTime gameTime)
    {
        // Geen _previousState.Update: posities, vijanden en spawnteller staan stil.
        // De game loop blijft lopen, zodat deze state nog invoer kan verwerken.
        if (IsKeyDown(Keys.Enter))
        {
            Context.ChangeState(_previousState);
            return; // Could: hervatten krijgt voorrang op opnieuw beginnen.
        }

        if (IsKeyDown(Keys.N))
        {
            // Een nieuw object krijgt eigen beginwaarden. De oude sessie hoeft
            // niet leeggemaakt te worden en de gedeelde content blijft geladen.
            Context.ChangeState(new PlayState(Context));
        }
    }

    public override void Draw(GameTime gameTime, SpriteBatch spriteBatch)
    {
        _previousState.Draw(gameTime, spriteBatch);

        // Eerst de wereld, dan de tekst, binnen dezelfde batch van Game1.
        DrawCenteredText(spriteBatch, "Pauze", 20);
        DrawCenteredText(spriteBatch, "Enter: hervatten", 80);
        DrawCenteredText(spriteBatch, "N: nieuw spel", 140);
        DrawCenteredText(spriteBatch, "Escape: afsluiten", 200);
    }
}
