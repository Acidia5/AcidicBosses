using AcidicBosses.Content.Bosses.Deerclops.Projectiles;
using AcidicBosses.Content.Bosses.QueenSlime.Projectiles;
using AcidicBosses.Helpers;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace AcidicBosses.Content.Bosses.Deerclops;

public partial class Deerclops
{
    private enum DebrisType : int
    {
        Dirt = 0,
        Stone = 3,
        Snow = 6,
        Ice = 9
    }
    
    private Projectile? NewIceSpike(Vector2 position, float angle, float scale)
    {
        if (!AcidUtils.IsServer()) return null;
        var proj = ProjHelper.NewUnscaledProjectile(Npc.GetSource_FromAI(), position, angle.ToRotationVector2(),
            ProjectileID.DeerclopsIceSpike, Npc.damage, 4f, ai1: scale);
        return proj;
    }
    
    private Projectile? NewIceShot(Vector2 position, Vector2 velocity)
    {
        if (!AcidUtils.IsServer()) return null;
        var proj = ProjHelper.NewUnscaledProjectile(Npc.GetSource_FromAI(), position, velocity,
            ModContent.ProjectileType<IceShot>(), Npc.damage, 4f);
        return proj;
    }
    
    private Projectile? NewDebris(Vector2 position, Vector2 velocity, DebrisType debrisType)
    {
        if (!AcidUtils.IsServer()) return null;
        
        var debrisId = (int)debrisType + Main.rand.Next(0, 3);
        var proj = ProjHelper.NewUnscaledProjectile(Npc.GetSource_FromAI(), position, velocity,
            ProjectileID.IceSpike, Npc.damage, 4f, ai1: debrisId);
        return proj;
    }

    private NPC? NewShadowHand(float angle)
    {
        if (!AcidUtils.IsServer()) return null;

        var npc = NPC.NewNPCDirect(
            Npc.GetSource_FromAI(),
            Npc.Center + angle.ToRotationVector2() * DarknessRadius,
            ModContent.NPCType<ShadowHand>(),
            Npc.whoAmI,
            angle,
            Main.rand.NextFloat(2.5f, 3.5f)
        );

        return npc;
    }
}