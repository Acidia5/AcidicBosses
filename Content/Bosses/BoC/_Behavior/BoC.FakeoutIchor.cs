using AcidicBosses.Core.Animation;
using AcidicBosses.Helpers;
using Luminance.Common.Utilities;
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

        // Rapidly teleport randomly
        // This will be out of sync between players, but that's fine
        anim.AddSequencedEvent(90, (progress, frame) =>
        {
            Npc.damage = 0; // Don't telefrag
            if (frame % 15 == 0)
            {
                fastTeleportAnimation ??= PrepareFastTeleportAnimation();
                var pos = TargetPlayer.Center + Main.rand.NextVector2CircularEdge(dist, dist);
                
                // 5 tries to find a suitably different angle
                for (var i = 0; i < 5; i++)
                {
                    var diff = pos.DirectionTo(TargetPlayer.Center).ToRotation() -
                               Npc.Center.DirectionTo(TargetPlayer.Center).ToRotation();
                    if (diff < MathHelper.PiOver4)
                    {
                        pos = TargetPlayer.Center + Main.rand.NextVector2CircularEdge(dist, dist);
                    }
                    else break;
                }
                
                fastTeleportAnimation.Data.Set<Vector2>("goalPos", pos);
                PlayBackgroundAnimation(fastTeleportAnimation);
            }
        });
        
        // Teleport into final position.
        // This brings the position back into sync
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
            Npc.damage = Npc.defDamage;
            if (frame == 0)
            {
                Attack_IchorShot(3);
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