using AcidicBosses.Content.Particles;
using AcidicBosses.Core.Animation;
using AcidicBosses.Helpers;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.Audio;
using Terraria.ID;

namespace AcidicBosses.Content.Bosses.Deerclops;

public partial class Deerclops
{
    private AcidAnimation? iceShotsAnimation;

    private AcidAnimation PrepareIceShotsAnimation()
    {
        var anim = new AcidAnimation();
        
        anim.AddInstantEvent(0, () =>
        {
            Npc.velocity = Vector2.Zero;
            StartSlam();
            SoundEngine.PlaySound(SoundID.DeerclopsScream, Npc.Center);

            new GlowStarParticle(
                Npc.Center + new Vector2(0f, -100f),
                Vector2.Zero,
                0f,
                Color.White,
                45
            )
            {
                GlowColor = Color.LightBlue,
                AngularVelocity = 0.2f,
                Shrink = true,
                EmitLight = true,
            }.Spawn();
        });

        var telegraph = anim.AddSequencedEvent(45, (progress, frame) =>
        {
            if (Npc.Center.X > TargetPlayer.Center.X) Npc.direction = -1;
            if (Npc.Center.X < TargetPlayer.Center.X) Npc.direction = 1;
            
            var pos = Npc.Center + new Vector2(0f, -100f);

            var offset = Main.rand.NextVector2CircularEdge(16f, 16f);
            var d0 = Dust.NewDustPerfect(pos + offset, DustID.SnowflakeIce);
            d0.scale = 1.5f;
            d0.noGravity = true;
            d0.velocity = -offset / 5f;

            var vel = pos.DirectionTo(TargetPlayer.Center) * 10f;
            var d1 = Dust.NewDustDirect(pos, 0, 0, DustID.SnowflakeIce, vel.X, vel.Y);
            d1.noGravity = true;
        });
        
        anim.AddInstantEvent(telegraph.EndTime, () =>
        {
            ReleaseHeldAnimation();
        });

        var shootIce = anim.AddSequencedEvent(30, (progress, frame) =>
        {
            if (frame % 10 == 0 && AcidUtils.IsServer())
            {
                var pos = Npc.Center + new Vector2(0f, -100f);
                NewIceShot(pos, pos.DirectionTo(TargetPlayer.Center).RotatedBy(-0.1f) * 10f);
                NewIceShot(pos, pos.DirectionTo(TargetPlayer.Center) * 10f);
                NewIceShot(pos, pos.DirectionTo(TargetPlayer.Center).RotatedBy(0.1f) * 10f);
            }
        });
        
        anim.AddInstantEvent(shootIce.EndTime, () =>
        {
            ReleaseHeldAnimation();
        });

        return anim;
    }

    private bool Attack_IceShots()
    {
        iceShotsAnimation ??= PrepareIceShotsAnimation();

        if (iceShotsAnimation.RunAnimation())
        {
            iceShotsAnimation.Reset();
            return true;
        }

        return false;
    }
}