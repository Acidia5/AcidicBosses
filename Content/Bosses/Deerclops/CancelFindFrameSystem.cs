using AcidicBosses.Common.Configs;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace AcidicBosses.Content.Bosses.Deerclops;

// The Deerclops FindFrame() does too much, so I'm canceling it
public class CancelFindFrameSystem : ModSystem
{
    public override void Load()
    {
        // On_NPC.VanillaFindFrame += On_NPCOnVanillaFindFrame;
    }

    public override void Unload()
    {
        // On_NPC.VanillaFindFrame -= On_NPCOnVanillaFindFrame;
    }
    
    // private void On_NPCOnVanillaFindFrame(On_NPC.orig_VanillaFindFrame orig, NPC self, int num, bool isLikeATownNpc, int type)
    // {
    //     // Cancel if this is deerclops
    //     if (type == NPCID.Deerclops && !AcidicBosses.DisableReworks() && BossToggleConfig.Get().EnableDeerclops)
    //     {
    //         return;
    //     }
    //     
    //     orig(self, num, isLikeATownNpc, type);
    // }
}