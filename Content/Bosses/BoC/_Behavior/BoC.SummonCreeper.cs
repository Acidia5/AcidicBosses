using Terraria;
using Terraria.Audio;
using Terraria.ID;

namespace AcidicBosses.Content.Bosses.BoC;

public partial class BoC
{
    private bool Attack_SummonCreeper(CreeperOverride.AttackType type)
    {
        SoundEngine.PlaySound(SoundID.NPCHit9, Npc.Center);

        if (Main.netMode != NetmodeID.MultiplayerClient)
        {
            var pos = Npc.Center + Main.rand.NextVector2Circular(250, 250);
            NPC.NewNPCDirect(Npc.GetSource_FromAI(), pos, NPCID.Creeper, start:Npc.whoAmI, ai1: (int) type);
        }

        return true;
    }
}