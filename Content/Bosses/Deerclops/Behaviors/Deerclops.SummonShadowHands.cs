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
    private AcidAnimation? summonShadowHandsAnimation;

    private AcidAnimation PrepareSummonShadowHandsAnimation()
    {
        var anim = new AcidAnimation();

        anim.AddInstantEvent(0, () =>
        {
            Npc.velocity = Vector2.Zero;
            StartRoar();
            SoundEngine.PlaySound(SoundID.DeerclopsHit, Npc.Center);
        });

        var gather = anim.AddSequencedEvent(30, (progress, frame) =>
        {
            if (AcidUtils.IsClient())
            {
                var spot = Main.rand.NextVector2Unit() * Main.rand.NextFloat(32f, 64f);
                new SharpTearNoBlendParticle(
                    Npc.Center + spot,
                    Vector2.Zero,
                    spot.ToRotation() + MathHelper.PiOver2,
                    Color.Black,
                    30
                )
                {
                    GlowColor = Color.Purple with { A = 0 },
                    Shrink = true,
                    OnUpdate = p =>
                    {
                        p.Opacity = MathHelper.Lerp(0f, 1f, p.LifetimeRatio);
                        var ease = EasingHelper.QuadIn(p.LifetimeRatio);
                        p.Position = Vector2.Lerp(Npc.Center + spot, Npc.Center, ease);
                    }
                }.Spawn();
            }
        });
        
        anim.AddInstantEvent(gather.EndTime + 30, () =>
        {
            ReleaseHeldAnimation();
            SoundEngine.PlaySound(SoundID.DeerclopsScream, Npc.Center);
        });

        var summon = anim.AddSequencedEvent(30, (progress, frame) =>
        {
            if (frame % 10 == 0 && AcidUtils.IsServer())
            {
                var angle = Main.rand.NextFloat(MathHelper.TwoPi);
                NewShadowHand(angle);
            }
        });
        
        anim.AddInstantEvent(summon.EndTime, () =>
        {
            ReleaseHeldAnimation();
        });
        
        return anim;
    }
    
    private bool Attack_SummonShadowHands()
    {
        RetractShadowHands = false;
        summonShadowHandsAnimation ??= PrepareSummonShadowHandsAnimation();

        if (summonShadowHandsAnimation.RunAnimation())
        {
            summonShadowHandsAnimation.Reset();
            return true;
        }

        return false;
    }

    private bool Attack_RetractShadowHands()
    {
        RetractShadowHands = true;
        return true;
    }
}