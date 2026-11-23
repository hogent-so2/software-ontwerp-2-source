using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using MonoGame_Pikachu.Extensions;

namespace MonoGame_Pikachu.States;

/// <summary>
/// Gemeenschappelijk contract voor iedere speltoestand, met gedeelde hulpjes.
/// Zoals Bewoner en ViewModelBase uit de vorige lessen is deze klasse abstract.
/// </summary>
public abstract class AbstractState
{
    protected Game1 Context { get; }

    protected AbstractState(Game1 context)
    {
        ArgumentNullException.ThrowIfNull(context);
        Context = context;
    }

    public abstract void Update(GameTime gameTime);

    // Elke state tekent binnen de batch die Game1 heeft geopend.
    // Draw verandert geen spelgegevens en roept geen Begin, End of Clear aan.
    public abstract void Draw(GameTime gameTime, SpriteBatch spriteBatch);

    // Polling, zoals in les 8: is de toets NU ingedrukt?
    // Een ingedrukte toets is niet hetzelfde als een nieuwe toetsdruk.
    protected bool IsKeyDown(Keys key)
        => Keyboard.GetState().IsKeyDown(key);

    protected bool IsKeyDown(Keys firstKey, Keys secondKey)
        => IsKeyDown(firstKey) || IsKeyDown(secondKey);

    protected void DrawCenteredText(SpriteBatch spriteBatch, string text, float y)
    {
        // De volledige tekstpositionering hoort bij deze gedeelde tekenbewerking.
        float textWidth = Context.SpriteFont.MeasureString(text).X;
        var position = new Vector2((Context.ScreenWidth - textWidth) / 2f, y);

        // De schaduw houdt de witte tekst ook boven het water leesbaar.
        spriteBatch.DrawString(Context.SpriteFont, text,
            position + new Vector2(2, 2), Color.Black);
        spriteBatch.DrawString(Context.SpriteFont, text, position);
    }
}
