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
        
        anim.AddInstantEvent(60, () =>
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
        summonShadowHandsAnimation ??= PrepareSummonShadowHandsAnimation();

        if (summonShadowHandsAnimation.RunAnimation())
        {
            summonShadowHandsAnimation.Reset();
            return true;
        }

        return false;
    }
}