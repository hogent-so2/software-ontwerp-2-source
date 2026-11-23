using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace MonoGame_Pikachu.States;

/// <summary>
/// Must: toont de speluitleg en verwerkt de terugkeer naar het menu.
/// Een extra state vraagt geen extra toestandtak in Game1.Update of Game1.Draw.
/// </summary>
public class HelpState : AbstractState
{
    public HelpState(Game1 context) : base(context)
    {
    }

    public override void Update(GameTime gameTime)
    {
        if (IsKeyDown(Keys.M))
            Context.ChangeState(new MenuState(Context));
    }

    public override void Draw(GameTime gameTime, SpriteBatch spriteBatch)
    {
        // Korte, afzonderlijke regels passen ook met het bestaande font van 30 pt.
        DrawCenteredText(spriteBatch, "Speluitleg", 20);
        DrawCenteredText(spriteBatch, "Bewegen: pijltjes of ZQSD", 90);
        DrawCenteredText(spriteBatch, "P: pauzeren", 150);
        DrawCenteredText(spriteBatch, "Enter: hervatten bij pauze", 210);
        DrawCenteredText(spriteBatch, "N: nieuw spel bij pauze", 270);
        DrawCenteredText(spriteBatch, "G: game over (testtoets)", 330);
        DrawCenteredText(spriteBatch, "M: terug naar menu", 400);
        DrawCenteredText(spriteBatch, "Escape: afsluiten", 460);
    }
}
