using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace MonoGame_Pikachu.States;

/// <summary>
/// Verwerkt menu-invoer en toont de keuzes om te starten of uitleg te bekijken.
/// Er wordt pas een speelsessie gemaakt wanneer de speler daarvoor kiest.
/// </summary>
public class MenuState : AbstractState
{
    public MenuState(Game1 context) : base(context)
    {
    }

    public override void Update(GameTime gameTime)
    {
        if (IsKeyDown(Keys.Enter))
        {
            Context.ChangeState(new PlayState(Context));
            return; // Must: Enter krijgt voorrang op H; hoogstens één overgang.
        }

        if (IsKeyDown(Keys.H))
            Context.ChangeState(new HelpState(Context));
    }

    public override void Draw(GameTime gameTime, SpriteBatch spriteBatch)
    {
        DrawCenteredText(spriteBatch, "Surfen met Pikachu", 60);
        DrawCenteredText(spriteBatch, "Enter: nieuw spel", 180);
        DrawCenteredText(spriteBatch, "H: speluitleg", 240);
        DrawCenteredText(spriteBatch, "Escape: afsluiten", 360);
    }
}
