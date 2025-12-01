using System;
using AcidicBosses.Common.Configs;
using Luminance.Common.Utilities;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;

namespace AcidicBosses.Content.Bosses.WoF;

public class WoFTongueSystem : ModSystem
{
	private static WoF? Wall => WoF.GetInstance();
	
    public override void Load()
    {
        On_Player.WOFTongue += WoFTongue;
    }

    public override void Unload()
    {
        On_Player.WOFTongue -= WoFTongue;
    }
    
    private void WoFTongue(On_Player.orig_WOFTongue orig, Player self)
    {
        if (!BossToggleConfig.Get().EnableWallOfFlesh || AcidicBosses.DisableReworks())
        {
            orig(self);
            return;
        }
	    
        // Adapted from vanilla
        if (Wall == null) return;
        
        var leftWallX = Wall.LeftWallRect.Right;
        var rightWallX = Wall.RightWallRect.Left;

        if (!self.gross && self.position.Y > (float)((Main.maxTilesY - 250) * 16) && self.position.X > leftWallX - 1920f && self.position.X < rightWallX + 1920f)
        {
            self.AddBuff(BuffID.Horrified, 10);
            SoundEngine.PlaySound(SoundID.NPCDeath10, Wall.Npc.Center);
        }
		
        // Take damage from the wall
        if (self.position.X + self.width > leftWallX - 140f && self.position.X < leftWallX && self.gross)
        {
            self.noKnockback = false;
            var attackDamage_ScaledByStrength = Wall.Npc.GetAttackDamage_ScaledByStrength(50f);
            self.Hurt(PlayerDeathReason.LegacyDefault(), attackDamage_ScaledByStrength, 1);
        }
        if (self.position.X + self.width > rightWallX && self.position.X < rightWallX + 140f && self.gross)
        {
            self.noKnockback = false;
            var attackDamage_ScaledByStrength = Wall.Npc.GetAttackDamage_ScaledByStrength(50f);
            self.Hurt(PlayerDeathReason.LegacyDefault(), attackDamage_ScaledByStrength, -1);
        }

        if (self.gross && (self.Center.X > rightWallX || self.Center.X < leftWallX))
        {
            self.AddBuff(ModContent.BuffType<NewTonguedBuff>(), 10);
            self.tongued = true;
        }
        if (!self.tongued)
        {
            return;
        }
		
        self.controlHook = false;
        self.controlUseItem = false;
        for (int i = 0; i < 1000; i++)
        {
            if (Main.projectile[i].active && Main.projectile[i].owner == Main.myPlayer && Main.projectile[i].aiStyle == ProjAIStyleID.Hook)
            {
                Main.projectile[i].Kill();
            }
        }
    }
}