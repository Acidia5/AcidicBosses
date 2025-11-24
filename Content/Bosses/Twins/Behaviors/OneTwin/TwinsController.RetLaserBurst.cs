using System;
using AcidicBosses.Content.Particles;
using AcidicBosses.Core.Animation;
using AcidicBosses.Helpers;
using Luminance.Common.Easings;
using Luminance.Common.Utilities;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;

namespace AcidicBosses.Content.Bosses.Twins;

public partial class TwinsController
{
    private AcidAnimation? laserBurstAnimation;

    private AcidAnimation CreateRetLaserBurstAnimation()
    {
        var anim = new AcidAnimation();

        const int lasers = 24;
        const int spinLength = 60;

        const string startingAngleKey = "startAngle";
        const string spawnedLasersKey = "spawnedLasers";

        anim.AddConstantEvent((progress, frame) =>
        {
            if (Spazmatism == null) return;
            Hover(Spazmatism, 10f, 0.15f);
        });

        anim.AddInstantEvent(0, () =>
        {
            if (Retinazer == null) return;
            anim.Data.Set(startingAngleKey, Retinazer.Npc.rotation);
            anim.Data.Set(spawnedLasersKey, 0);

            Retinazer.Npc.velocity = Vector2.Zero;
        });

        // Indicators
        var indicatorTiming = anim.AddSequencedEvent(spinLength, (progress, frame) =>
        {
            if (Retinazer == null) return;
            var startingAngle = anim.Data.Get<float>(startingAngleKey);
            var spawnedLasers = anim.Data.Get<int>(spawnedLasersKey);

            var ease = EasingHelper.QuadInOut(progress);

            var angle = MathHelper.Lerp(0, MathHelper.TwoPi, ease);
            Retinazer.Npc.rotation = MathHelper.WrapAngle(startingAngle + angle);
            
            // Collect energy particles
            var spawnPos = Main.rand.NextVector2CircularEdge(20f, 20f);
            var angVel = Main.rand.NextFloat(-0.1f, 0.1f);
            var partRot = Main.rand.NextFloatDirection();
            
            new GlowStarParticle(spawnPos + Retinazer.Front, Vector2.Zero, partRot, Color.White, 30)
            {
                AngularVelocity = angVel,
                IgnoreLighting = true,
                Scale = Vector2.One,
                OnUpdate = p =>
                {
                    var suck = EasingHelper.ExpOut(p.LifetimeRatio);
                    var shrink = EasingHelper.CubicIn(p.LifetimeRatio);
                    p.Position = Vector2.Lerp(spawnPos + Retinazer.Front, Retinazer.Front, suck);
                    p.Scale = Vector2.Lerp(Vector2.One, Vector2.Zero, shrink);
                }
            }.Spawn();

            var laserProgress = (float) spawnedLasers / lasers;
            if (ease >= laserProgress && Main.netMode != NetmodeID.MultiplayerClient)
            {
                var rot = MathHelper.Lerp(0, MathHelper.TwoPi, laserProgress) + startingAngle + MathHelper.PiOver2;
                NewRetLazer(Retinazer.Front, rot.ToRotationVector2() * 30f, rot, spinLength);
                
                anim.Data.Set(spawnedLasersKey, spawnedLasers + 1);
            }

            if (Spazmatism == null) return;
            if (frame % 15 == 0 && Main.netMode != NetmodeID.MultiplayerClient)
            {
                NewSpazFireball(Spazmatism.Npc.Center, Spazmatism.Npc.Center.DirectionTo(Main.player[NPC.target].Center).RotatedBy(Main.rand.NextFloat(-0.5f, 0.5f)) * 10f);
            }
        });

        // Reset laser count
        anim.AddInstantEvent(indicatorTiming.EndTime, () => anim.Data.Set(spawnedLasersKey, 0));

        // Spin with the lasers
        anim.AddSequencedEvent(spinLength, (progress, frame) =>
        {
            if (Retinazer == null) return;
            var startingAngle = anim.Data.Get<float>(startingAngleKey);

            var ease = EasingHelper.QuadInOut(progress);

            var angle = MathHelper.Lerp(0, MathHelper.TwoPi, ease);
            Retinazer.Npc.rotation = MathHelper.WrapAngle(startingAngle + angle);
        });

        return anim;
    }

    private bool Attack_RetLaserBurst()
    {
        if (Retinazer == null) return true;
        laserBurstAnimation ??= CreateRetLaserBurstAnimation();

        if (!laserBurstAnimation.RunAnimation()) return false;
        laserBurstAnimation.Reset();
        return true;
    }
}