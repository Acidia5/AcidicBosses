using System;
using Terraria;
using Terraria.Audio;
using Terraria.ID;

namespace AcidicBosses.Content.Bosses.BoC;

public partial class BoC
{
    private bool Attack_TripleIchorShot()
    {
        const float spread = MathF.PI / 6f;
        const float speed = 5f;

        SoundEngine.PlaySound(SoundID.Item21, Npc.Center);

        Npc.velocity = Npc.DirectionTo(TargetPlayer.Center) * -5f;

        if (Main.netMode == NetmodeID.MultiplayerClient) return true;
        
        for (var i = -1; i <= 1; i++)
        {
            var angleOffset = spread * i;
            var target = Main.player[Npc.target].Center;
            var angle = Npc.DirectionTo(target).ToRotation() + angleOffset;

            NewIchorShot(Npc.Center, angle.ToRotationVector2() * speed);
        }

        return true;
    }
}