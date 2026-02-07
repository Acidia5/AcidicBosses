using AcidicBosses.Content.Bosses.BoC;
using AcidicBosses.Content.Bosses.WoF.Projectiles;
using AcidicBosses.Content.ProjectileBases;
using AcidicBosses.Helpers;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace AcidicBosses.Content.Bosses.WoF;

public partial class WoF
{
    private Projectile NewFireball(Vector2 pos, Vector2 vel)
    {
        var proj = ProjHelper.NewUnscaledProjectile(Npc.GetSource_FromAI(), pos, vel, 
            ProjectileID.CursedFlameHostile, Npc.damage / 2, 3);
        proj.scale = 0.9f;
        proj.tileCollide = false;
        return proj;
    }
    
    private Projectile NewIchor(Vector2 pos, Vector2 vel)
    {
        var proj = ProjHelper.NewUnscaledProjectile(Npc.GetSource_FromAI(), pos, vel, 
            ModContent.ProjectileType<IchorShot>(), Npc.damage / 2, 3);
        proj.scale = 1.25f;
        proj.tileCollide = false;
        return proj;
    }
    
    private Projectile NewLaser(Vector2 pos, Vector2 vel, float rotation, int lifetime, int anchor = -1)
    {
        var proj = BaseLineProjectile.Create<WoFLaser>(Npc.GetSource_FromAI(), pos, vel, Npc.damage / 2, 3, rotation, lifetime, anchor);

        return proj;
    }

    private Projectile NewDeathray(Vector2 pos, float rotation, int lifetime, int anchor = -1)
    {
        return DeathrayBase.Create<WoFDeathray>(Npc.GetSource_FromAI(), pos, (int) (Npc.damage * 1.5f), 5, rotation, lifetime, anchor);
    }

    private Projectile NewDeathrayIndicator(Vector2 pos, float rotation, int lifetime, int anchor = -1)
    {
        return BaseLineProjectile.Create<WoFDeathrayIndicator>(Npc.GetSource_FromAI(), pos, rotation, lifetime, anchor);
    }
    
    private Projectile NewLineIndicator(Vector2 pos, float rotation, int lifetime, int anchor = -1)
    {
        return BaseLineProjectile.Create<WoFMoveIndicator>(Npc.GetSource_FromAI(), pos, rotation, lifetime, anchor);
    }
    
    private NPC NewEvilMob(PartPosition pos)
    {
        var crimsonMob = NPCID.Crimera;
        var corruptMob = NPCID.EaterofSouls;
        
        var position = PartPosToWorldPos(pos);

        // Spawn a mob based on which of the world evil it is
        int type;
        
        if (Main.drunkWorld)
            type = Main.rand.NextBool() ? crimsonMob : corruptMob;
        else if (WorldGen.crimson)
            type = crimsonMob;
        else 
            type = corruptMob;
        
        var npc = NPC.NewNPCDirect(Npc.GetSource_FromAI(), position, type, Npc.whoAmI);

        if ((pos & PartPosition.Right) != 0)
            npc.velocity.X = -10;
        else 
            npc.velocity.X = 10;

        return npc;
    }
    
    private NPC NewHallowMob(PartPosition pos)
    {
        var position = PartPosToWorldPos(pos);
        var type = ModContent.NPCType<EasyPixieNPC>();
        var npc = NPC.NewNPCDirect(Npc.GetSource_FromAI(), position, type, Npc.whoAmI);
        
        if ((pos & PartPosition.Right) != 0)
            npc.velocity.X = -10;
        else 
            npc.velocity.X = 10;

        return npc;
    }

    private NPC NewEye(PartPosition pos)
    {
        var position = PartPosToWorldPos(pos);

        var npc = NPC.NewNPCDirect(Npc.GetSource_FromAI(), position, NPCID.WallofFleshEye, Npc.whoAmI);
        npc.ai[0] = (float) pos;

        return npc;
    }

    private NPC NewMouth(PartPosition pos)
    {
        var position = PartPosToWorldPos(pos);

        var npc = NPC.NewNPCDirect(Npc.GetSource_FromAI(), position, ModContent.NPCType<WoFMouth>(), Npc.whoAmI);
        npc.ai[0] = (float) pos;

        return npc;
    }
}