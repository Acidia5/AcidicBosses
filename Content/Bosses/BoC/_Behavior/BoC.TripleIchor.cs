using System;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.Audio;
using Terraria.ID;

namespace AcidicBosses.Content.Bosses.BoC;

public partial class BoC
{
    private bool Attack_IchorShot(int shots)
    {
        const float spread = MathF.PI / 6f;
        const float speed = 5f;

        SoundEngine.PlaySound(SoundID.Item21, Npc.Center);

        Npc.velocity = Npc.DirectionTo(TargetPlayer.Center) * -5f;
        
        for (var i = 0; i < 25; i++)
        {
            var target = Main.player[Npc.target].Center;
            var angle = Npc.DirectionTo(target).ToRotation();
            
            var vel = (angle + Main.rand.NextFloat(-MathHelper.Pi / 8f, MathHelper.Pi / 8f)).ToRotationVector2();
            vel *= Main.rand.NextFloat(0f, 5f);
            Dust.NewDustPerfect(Npc.Center, DustID.Ichor, vel, Scale: 1.5f);
        }

        if (Main.netMode == NetmodeID.MultiplayerClient) return true;
        
        if (shots % 2 == 1)
        {
            for (var i = -shots / 2; i <= shots / 2; i++)
            {
                var angleOffset = spread * i;
                var target = Main.player[Npc.target].Center;
                var angle = Npc.DirectionTo(target).ToRotation() + angleOffset;

                NewIchorShot(Npc.Center, angle.ToRotationVector2() * speed);
            }
        }
        else
        {
            for (var i = -shots / 2; i < shots / 2; i++)
            {
                var angleOffset = spread * (i + 0.5f);
                var target = Main.player[Npc.target].Center;
                var angle = Npc.DirectionTo(target).ToRotation() + angleOffset;

                NewIchorShot(Npc.Center, angle.ToRotationVector2() * speed);
            }
        }
        return true;
    }
}