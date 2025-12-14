using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.GameContent;
using Terraria.ID;

namespace AcidicBosses.Helpers;

public static class AcidUtils
{
    public static bool IsServer()
    {
        return Main.netMode != NetmodeID.MultiplayerClient;
    }

    public static bool IsClient()
    {
        return Main.netMode != NetmodeID.Server;
    }
    
    // Similar to Luminance's, but includes all walkable tiles 
    public static Point FindGroundVertical(Point p)
    {
        // The tile is solid. Check up to verify that this tile is not inside of solid ground.
        if (WorldGen.ActiveAndWalkableTile(p.X, p.Y))
        {
            while (WorldGen.ActiveAndWalkableTile(p.X, p.Y - 1) && p.Y >= 1)
                p.Y--;
        }

        // The tile is not solid. Check down to verify that this tile is not above ground in the middle of the air.
        else
        {
            while (!WorldGen.ActiveAndWalkableTile(p.X, p.Y + 1) && p.Y < Main.maxTilesY)
                p.Y++;
        }

        return p;
    }

    public static Color AddColor(Color a, Color b)
    {
        var newColor = Color.Black;
        newColor.R = (byte) MathHelper.Clamp(a.R + b.R, 0, 255);
        newColor.G = (byte) MathHelper.Clamp(a.G + b.G, 0, 255);
        newColor.B = (byte) MathHelper.Clamp(a.B + b.B, 0, 255);
        newColor.A = (byte) MathHelper.Clamp(a.A + b.A, 0, 255);
        
        return newColor;
    }
    
    // Copied from Main.DrawPrettyStarSparkle() which is private
    // Slightly modified for simplicity
    public static void DrawPrettyStarSparkle(float opacity, SpriteEffects dir, Vector2 drawPos, Color drawColor, Color shineColor, float rotation, Vector2 scale, Vector2 fatness) {
        Texture2D sparkleTexture = TextureAssets.Extra[ExtrasID.SharpTears].Value;
        Color bigColor = shineColor * opacity * 0.5f;
        bigColor.A = 0;
        Vector2 origin = sparkleTexture.Size() / 2f;
        Color smallColor = drawColor * 0.5f;
        Vector2 scaleLeftRight = new Vector2(fatness.X * 0.5f, scale.X);
        Vector2 scaleUpDown = new Vector2(fatness.Y * 0.5f, scale.Y);
        
        // Bright, large part
        Main.EntitySpriteDraw(sparkleTexture, drawPos, null, bigColor, MathHelper.PiOver2 + rotation, origin, scaleLeftRight, dir);
        Main.EntitySpriteDraw(sparkleTexture, drawPos, null, bigColor, 0f + rotation, origin, scaleUpDown, dir);
        // Dim, small part
        Main.EntitySpriteDraw(sparkleTexture, drawPos, null, smallColor, MathHelper.PiOver2 + rotation, origin, scaleLeftRight * 0.6f, dir);
        Main.EntitySpriteDraw(sparkleTexture, drawPos, null, smallColor, 0f + rotation, origin, scaleUpDown * 0.6f, dir);
    }
}