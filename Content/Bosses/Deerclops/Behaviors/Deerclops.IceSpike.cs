using AcidicBosses.Core.Animation;
using AcidicBosses.Helpers;
using Microsoft.Xna.Framework;
using Terraria.Audio;
using Terraria.ID;

namespace AcidicBosses.Content.Bosses.Deerclops;

public partial class Deerclops
{
    private AcidAnimation? iceSpikeAnimation;

    private AcidAnimation PrepareIceSpikeAnimation()
    {
        var anim = new AcidAnimation();
        
        // Start slam windup
        anim.AddInstantEvent(0, () =>
        {
            Npc.velocity = Vector2.Zero;
            StartSlam();
            SoundEngine.PlaySound(SoundID.DeerclopsScream, Npc.Center);
        });
        
        // Slam Down hands
        anim.AddInstantEvent(60, () =>
        {
            anim.Data.Set<int>("spikesSpawned", 0);
            
            ReleaseHeldAnimation();
        });

        // Spawn spikes
        var spikes = anim.AddSequencedEvent(30, (progress, frame) =>
        {
            const int spikes = 12;
            var spikesSpawned = anim.Data.Get<int>("spikesSpawned");

            var spikeProgress = (float)spikesSpawned / spikes;
            if (spikeProgress >= progress)
            {
                spikesSpawned++;

                var scale = MathHelper.Lerp(0.5f, 1f, spikeProgress);
                var offset = spikesSpawned * 32f * Npc.direction + 50f * Npc.direction;
                var pos = BottomPos + new Vector2(offset, 0f);

                if (AcidUtils.IsServer())
                {
                    NewIceSpike(pos, -MathHelper.PiOver2, scale);
                }
            }
            
            anim.Data.Set<int>("spikesSpawned", spikesSpawned);
        });
        
        anim.AddInstantEvent(spikes.EndTime + 30, () =>
        {
            ReleaseHeldAnimation();
        });

        anim.AddSequencedEvent(15, (progress, frame) =>
        {

        });

        return anim;
    }

    private bool Attack_IceSpikes()
    {
        iceSpikeAnimation ??= PrepareIceSpikeAnimation();

        if (iceSpikeAnimation.RunAnimation())
        {
            iceSpikeAnimation.Reset();
            return true;
        }

        return false;
    }
}