using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace MonoGame_Pikachu.Extensions;

internal static class SpriteBatchExtensions
{
    public static void Draw(this SpriteBatch spriteBatch, Texture2D texture, Vector2 position)
    {
        spriteBatch.Draw(texture, position, Color.White);
    }

    public static void Draw(this SpriteBatch spriteBatch, Texture2D texture, Vector2 position, float scale)
    {
        spriteBatch.Draw(texture, position, null, Color.White, 0f, Vector2.Zero, scale, SpriteEffects.None, 0f);
    }

    public static void DrawString(this SpriteBatch spriteBatch, SpriteFont spriteFont, string text, Vector2 position)
    {
        spriteBatch.DrawString(spriteFont, text, position, Color.White);
    }
}
