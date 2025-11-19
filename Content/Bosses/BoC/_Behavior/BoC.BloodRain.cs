using AcidicBosses.Content.Particles.Animated;
using AcidicBosses.Core.Animation;
using AcidicBosses.Core.Graphics.Sprites;
using AcidicBosses.Helpers;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;

namespace AcidicBosses.Content.Bosses.BoC;

public partial class BoC
{
    private bool Attack_BloodRain()
    {
        AttackManager.CountUp = true;

        const float spreadDist = 1000f;
        const int rainTime = 90;
        const int bloodDrops = 8;
        ref var spawnedDrops = ref Npc.localAI[0];
        ref var startX = ref Npc.localAI[1];

        if (AttackManager.AiTimer == 0)
        {
            spawnedDrops = 0;
            startX = Npc.Center.X;
            Npc.velocity = Vector2.Zero;
        }

        var progress = (float) AttackManager.AiTimer / rainTime;
        var moveEase = EasingHelper.QuadOut(progress);

        Npc.Center = Vector2.Lerp(new Vector2(startX, Npc.Center.Y), new Vector2(startX + spreadDist, Npc.Center.Y), moveEase);
        
        var dropProgress = (float) spawnedDrops / bloodDrops;
        if (moveEase >= dropProgress)
        {
            var pos = Npc.Bottom;
            new RingBurstParticle(pos, Vector2.Zero, 0f, Color.Maroon, 30).Spawn();
            new FakeAfterimage(Npc.Center, Npc.Center, Npc, 15).Spawn();
            for (var i = 0; i < 25; i++)
            {
                var vel = Main.rand.NextVector2Unit(MathHelper.PiOver2 - MathHelper.Pi / 8f, MathHelper.PiOver4);
                vel *= Main.rand.NextFloat(0f, 5f);
                Dust.NewDustPerfect(pos, DustID.Blood, vel, Scale: 1.5f);
            }
            
            NewBloodShot(pos, new Vector2(0, 5));
            spawnedDrops++;
        }

        if (AttackManager.AiTimer >= rainTime)
        {
            spawnedDrops = 0;
            startX = 0;
            AttackManager.CountUp = false;
            return true;
        }

        return false;
    }
}