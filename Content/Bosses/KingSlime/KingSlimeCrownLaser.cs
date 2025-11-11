using System.Collections.Generic;
using AcidicBosses.Common.Effects;
using AcidicBosses.Common.RenderManagers;
using AcidicBosses.Content.Particles;
using AcidicBosses.Content.ProjectileBases;
using AcidicBosses.Helpers;
using Luminance.Common.Easings;
using Luminance.Common.Utilities;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using Terraria;
using Terraria.Audio;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;

namespace AcidicBosses.Content.Bosses.KingSlime;

public class KingSlimeCrownLaser : DeathrayBase
{
    public override float Distance => 12000;
    protected override int CollisionWidth => 5;
    protected override Color Color => Color.White;
    protected override Asset<Texture2D> DrTexture => ModContent.Request<Texture2D>(Texture);
    protected override bool StartAtEnd => true;

    public override void AI()
    {
        base.AI();

        const int animLen = 15;
        var timeAlive = maxTimeLeft - Projectile.timeLeft;

        if (timeAlive == 0)
        {
            // Burst Dust
            for (var i = 0; i < 25; i++)
            {
                Dust.NewDust(Projectile.position, 0, 0, DustID.PinkFairy);
            }
            
            // Star
            var scaleCurve = new PiecewiseCurve()
                .Add(EasingCurves.Quadratic, EasingType.In, 0f, 1f, 4f);
            new GlowStarParticle(
                Projectile.position,
                Vector2.Zero,
                0,
                Color.White,
                Projectile.timeLeft
            )
            {
                IgnoreLighting = true,
                OnUpdate = p =>
                {
                    var scale = scaleCurve.Evaluate(p.LifetimeRatio);
                    p.Scale = Vector2.One * scale;
                    p.AngularVelocity = (1f - p.LifetimeRatio) * MathHelper.Pi / 16f;
                    p.Opacity = 1f;
                    p.DrawColor = Color.White;
                }
            }.Spawn();
        }
        
        // Subtle Pulse
        var curve = new PiecewiseCurve()
            .Add(EasingCurves.Sine, EasingType.InOut, 1.2f, 0.5f, 0.8f)
            .Add(EasingCurves.Sine, EasingType.InOut, 0.8f, 1f);
        var cycleLen = 60f;
        var pulseT = (timeAlive % cycleLen) / cycleLen;
        var widthGoal = curve.Evaluate(pulseT);
        
        if (timeAlive <= animLen)
        {
            // Bounce in
            var t = EasingHelper.BackOut((float) timeAlive / animLen);
            widthScale = MathHelper.Lerp(0f, widthGoal, t);
        }
        else if (Projectile.timeLeft <= animLen)
        {
            // Bounce in
            var t = EasingHelper.QuadIn((float) Projectile.timeLeft / animLen);
            widthScale = MathHelper.Lerp(0f, widthGoal, t);
        }
        else
        {
            
            widthScale = widthGoal;
        }
    }

    protected override void SpawnDust(Vector2 position)
    {
        if (Projectile.timeLeft == maxTimeLeft)
        {
            Dust.NewDust(position, 0, 0, DustID.PinkFairy, Scale: 0.75f);
        }

        if (Main.rand.NextBool(100))
        {
            Dust.NewDust(position, 0, 0, DustID.PinkFairy, Scale: 0.5f);
        }
    }

    public override bool PreDraw(ref Color lightColor)
    {
        base.PreDraw(ref lightColor);
        
        return false;
    }
}