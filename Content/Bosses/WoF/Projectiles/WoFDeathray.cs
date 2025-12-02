using System.IO;
using AcidicBosses.Common.Effects;
using AcidicBosses.Content.Particles;
using AcidicBosses.Content.ProjectileBases;
using AcidicBosses.Helpers;
using Luminance.Common.Easings;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace AcidicBosses.Content.Bosses.WoF.Projectiles;

public class WoFDeathray : DeathrayBase
{
    public override float Distance => 25000;
    protected override int CollisionWidth => 15;
    protected override Color Color => Color.White;
    protected override Asset<Texture2D> DrTexture => ModContent.Request<Texture2D>(Texture);

    public override bool AnchorRotation => false;

    private const float AberrationStrength = 0.001f;

    public override void FirstFrame()
    {
        base.FirstFrame();

        for (var i = 0; i < 20; i++)
        {
            var vel = Main.rand.NextVector2Circular(10f, 10f);
            new SharpTearParticle(
                Projectile.position + vel.SafeNormalize(Vector2.Zero) * 10,
                vel,
                vel.ToRotation() + MathHelper.PiOver2,
                Color.White,
                15
            )
            {
                IgnoreLighting = true,
                GlowColor = Color.Purple,
                Drag = 0.01f,
                EmitLight = true,
                Shrink = true,
                FadeColor = true
            }.Spawn();
        }
    }

    protected override void AiEffects()
    {
        EffectsManager.AberrationActivate(MathHelper.Lerp(0f, AberrationStrength, Projectile.timeLeft / (float) maxTimeLeft));
        
        const int animLen = 10;
        var timeAlive = maxTimeLeft - Projectile.timeLeft;
        
        if (timeAlive <= animLen)
        {
            // Bounce in
            var t = EasingHelper.BackOut((float) timeAlive / animLen);
            widthScale = MathHelper.Lerp(0f, 1f, t);
        }
        else if (Projectile.timeLeft <= animLen)
        {
            // Bounce in
            var t = EasingHelper.QuadIn((float) Projectile.timeLeft / animLen);
            widthScale = MathHelper.Lerp(0f, 1f, t);
        }
        else
        {
            widthScale = 1f;
        }
        
        if (Projectile.timeLeft > animLen)
        {
            // Spikey effect
            var vel = Main.rand.NextVector2Circular(5f, 5f);
            new SharpTearParticle(
                Projectile.position + vel.SafeNormalize(Vector2.Zero) * 10,
                vel,
                vel.ToRotation() + MathHelper.PiOver2,
                Color.White,
                15
            )
            {
                IgnoreLighting = true,
                GlowColor = Color.Purple,
                Drag = 0.01f,
                Shrink = true,
                FadeColor = true
            }.Spawn();
        }
    }

    protected override void SpawnDust(Vector2 position)
    {
        if (Main.rand.NextBool(1, 50))
        {
            Dust.NewDust(position, 0, 0, DustID.Shadowflame);
        }
    }
}