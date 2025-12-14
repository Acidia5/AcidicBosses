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
    private AcidAnimation? iceSpikeAnimation;

    private AcidAnimation PrepareIceSpikeAnimation()
    {
        var anim = new AcidAnimation();
        
        const int spikes = 24;
        
        // Start slam windup
        anim.AddInstantEvent(0, () =>
        {
            Npc.velocity = Vector2.Zero;
            StartSlam();
            
            SoundEngine.PlaySound(SoundID.DeerclopsScream, Npc.Center);
            
            for (var i = 1; i <= spikes; i++)
            {
                var scale = MathHelper.Lerp(1f, 2f, (float)i / spikes);
                var offset = i * 32f * Npc.direction + 50f * Npc.direction;
                var pos = BottomPos + new Vector2(offset, 0f);

                new SharpTearParticle(
                    pos,
                    Vector2.Zero,
                    0f,
                    Color.LightBlue * 0.75f,
                    60
                )
                {
                    Scale = new Vector2(2f * scale),
                    Shrink = true,
                    FadeColor = true,
                }.Spawn();
            }
        });

        // Telegraph on ground
        var telegraph = anim.AddSequencedEvent(60, (progress, frame) =>
        {
            for (var i = 1; i <= spikes; i++)
            {
                var scale = MathHelper.Lerp(0.5f, 1f, (float)i / spikes);
                var offset = i * 32f * Npc.direction + 50f * Npc.direction;
                var pos = BottomPos + new Vector2(offset, 0f);

                var d = Dust.NewDustDirect(pos, 32, 0, DustID.SnowflakeIce, 0f, -15f * scale);
                d.noGravity = true;
            }
        });
        
        // Slam Down hands
        anim.AddInstantEvent(telegraph.EndTime, () =>
        {
            anim.Data.Set<int>("spikesSpawned", 0);
            
            ReleaseHeldAnimation();
        });

        // Spawn spikes
        var spawnSpikes = anim.AddSequencedEvent(30, (progress, frame) =>
        {
            var spikesSpawned = anim.Data.Get<int>("spikesSpawned");

            var spikeProgress = (float)spikesSpawned / spikes;
            if (progress >= spikeProgress)
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
        
        anim.AddInstantEvent(spawnSpikes.EndTime + 30, () =>
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