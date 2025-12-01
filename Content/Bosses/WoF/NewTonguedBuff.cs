using Microsoft.Xna.Framework;
using Terraria;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;

namespace AcidicBosses.Content.Bosses.WoF;

/// <summary>
/// Replacement for the vanilla tongued "buff". This makes it work for two walls
/// </summary>
public class NewTonguedBuff : ModBuff
{
    private static WoF? Wall => WoF.GetInstance();

    public override string Texture => "Terraria/Images/Buff_38";

    public override void SetStaticDefaults()
    {
        Main.debuff[Type] = true;
        Main.buffNoTimeDisplay[Type] = true;
    }

    public override void Update(Player player, ref int buffIndex)
    {
        player.tongued = true;
        
        // Modify vanilla code to support the second wall
        player.StopVanityActions();
        var done = false;
        
        if (Wall != null)
        {
            // Target the closer wall
            var leftWallPos = Wall.LeftWallRect.Right();
            var rightWallPos = Wall.RightWallRect.Left();
            
            var distRightWall = player.Center.Distance(rightWallPos);
            var distLeftWall = player.Center.Distance(leftWallPos);
	    
            // Choose the closer wall as the target
            
            var target = new Vector2(0);

            if (distRightWall < distLeftWall) target = rightWallPos;
            else target = leftWallPos;

            // The rest is vanilla logic with vector math
            var dist = target.Distance(player.Center);
            var releaseDist = 11f;
            
            var closeness = dist;
            if (dist > releaseDist)
            {
                closeness = releaseDist / dist;
            }
            else
            {
                closeness = 1f;
                done = true;
            }
            var vel = player.Center.DirectionTo(target) * closeness;
            player.velocity = vel;
        }
        else
        {
            done = true;
        }
        if (done && Main.myPlayer == player.whoAmI)
        {
            player.ClearBuff(ModContent.BuffType<NewTonguedBuff>());
        }
    }
}