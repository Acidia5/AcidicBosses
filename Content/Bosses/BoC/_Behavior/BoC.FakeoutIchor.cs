using AcidicBosses.Core.Animation;
using AcidicBosses.Helpers;
using Microsoft.Xna.Framework;
using Terraria;

namespace AcidicBosses.Content.Bosses.BoC;

public partial class BoC
{
    private AcidAnimation? fakeoutIchorAnimation;

    private AcidAnimation PrepareFakeoutIchorAnimation()
    {
        var anim = new AcidAnimation();

        const float dist = 400;
        
        anim.AddInstantEvent(0, () =>
        {
            Npc.velocity = Vector2.Zero;
            
            if (AcidUtils.IsServer())
            {
                // Will be a full second before being used.
                // If a client can't sync in less than a second there's an issue
                var randAngle = Main.rand.NextVector2Unit().ToRotation();
                Npc.ai[0] = randAngle;
                NetSync(Npc);
            }
        });

        anim.AddSequencedEvent(90, (progress, frame) =>
        {
            if (frame % 15 == 0)
            {
                fastTeleportAnimation ??= PrepareFastTeleportAnimation();
                fastTeleportAnimation.Data.Set<Vector2>("goalPos", TargetPlayer.Center + Main.rand.NextVector2CircularEdge(dist, dist));
                PlayBackgroundAnimation(fastTeleportAnimation);
            }
        });
        
        anim.AddSequencedEvent(30, (progress, frame) =>
        {
            if (frame == 0)
            {
                fastTeleportAnimation ??= PrepareFastTeleportAnimation();
                fastTeleportAnimation.Data.Set<Vector2>("goalPos", TargetPlayer.Center + Npc.ai[0].ToRotationVector2() * dist);
                PlayBackgroundAnimation(fastTeleportAnimation);
            }
        });
        
        anim.AddSequencedEvent(15, (progress, frame) =>
        {
            if (frame == 0)
            {
                Attack_TripleIchorShot();
            }
        });
        
        return anim;
    }

    private bool Attack_FakeoutIchor()
    {
        fakeoutIchorAnimation ??= PrepareFakeoutIchorAnimation();
        if (fakeoutIchorAnimation.RunAnimation())
        {
            fakeoutIchorAnimation.Reset();
            return true;
        }
        
        return false;
    }
}