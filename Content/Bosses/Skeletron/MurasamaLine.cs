using AcidicBosses.Common.Textures;
using AcidicBosses.Content.ProjectileBases;
using AcidicBosses.Helpers;
using Luminance.Common.Easings;
using Luminance.Common.Utilities;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using Terraria;
using Terraria.ID;

namespace AcidicBosses.Content.Bosses.Skeletron;

public class MurasamaLine : BaseLineProjectile
{
    protected override float Length { get; set; } = 1200f;
    protected override float Width { get; set; } = 10f;

    protected override Color Color => GetColor();
    protected override Asset<Texture2D> LineTexture => TextureRegistry.GlowLine;

    // Fade over Time
    private Color GetColor()
    {
        var fadeT = (float) Projectile.timeLeft / MaxTimeLeft;
        return Color.Blue * EasingHelper.QuadOut(fadeT);
    }

    public override void AI()
    {
        base.AI();

        var fadeT = (float) Projectile.timeLeft / MaxTimeLeft;
        var a = (int)((1f - EasingHelper.QuadOut(fadeT)) * 255);
        
        for (var i = 0; i < Length; i += 16)
        {
            if (!Main.rand.NextBool(50)) continue;
            var pos = Projectile.position + Projectile.rotation.ToRotationVector2() * i - new Vector2(4);
            var d = Dust.NewDustDirect(
                pos,
                8, 8,
                DustID.DungeonWater,
                Alpha: a
            );
            d.noGravity = true;
        }
    }
}