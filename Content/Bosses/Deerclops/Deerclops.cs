using System.IO;
using AcidicBosses.Common.Configs;
using AcidicBosses.Core.StateManagement;
using Luminance.Common.Utilities;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.ModLoader.IO;

namespace AcidicBosses.Content.Bosses.Deerclops;

public partial class Deerclops : AcidicNPCOverride
{
    protected override int OverriddenNpc => NPCID.Deerclops;
    protected override bool BossEnabled => BossToggleConfig.Get().EnableDeerclops;

    private PhaseTracker phaseTracker;
    
    // Constants for how far off the ground her hitbox is
    public const int GroundOffset = 70;
    public const int FeetHeightCorrection = 6; // Correction for hooves touching ground
    public int TileCollisionHeight => Npc.height + GroundOffset - FeetHeightCorrection;
    public Vector2 BottomPos
    {
        get => Npc.Top + new Vector2(0, TileCollisionHeight);
        set => Npc.Top = value - new Vector2(0, TileCollisionHeight);
    }

    // These are different to the vanilla values because she uses custom movement
    private bool useCollision = true;
    private bool useGravity = true;

    public override void SetDefaults(NPC entity)
    {
        if (!ShouldOverride()) return;

        // Demote Deerclops in progression to pre-skeletron
        // 15k -> 11k on master
        // I would move her in boss checklist if I could
        entity.lifeMax = (int)(entity.lifeMax * 0.75f);
        entity.height -= GroundOffset;
    }

    public override void OnFirstFrame(NPC npc)
    {
        // Cancel generic roar in favor of Deerclops scream
        // Idk which one is used so I just try to cancel both :P
        SoundEngine.FindActiveSound(SoundID.Roar)?.Stop();
        SoundEngine.FindActiveSound(SoundID.ForceRoar)?.Stop();
        SoundEngine.PlaySound(SoundID.DeerclopsScream with { Volume = 1.5f });
        
        phaseTracker = new PhaseTracker([
            PhaseIntro,
            PhaseOne
        ]);
    }

    public override bool AcidAI(NPC npc)
    {
        NPC.deerclopsBoss = Npc.whoAmI;
        
        // Retarget on target lost
        // TODO: DON'T FORGET FLEE BEHAVIOR!!!
        if (IsTargetGone(npc))
        {
            npc.TargetClosest();
        }

        // Force snow biome for nearby players
        foreach (var player in Main.player)
        {
            if (!player.active) continue;
            if (Npc.Distance(player.Center) > 10_000) continue;
            player.ZoneSnow = true;
        }
        
        DrawUpdate();
        phaseTracker.RunPhaseAI();
        
        if (useGravity) ApplyGravity();
        if (useCollision) ApplyCollision();
        
        return false;
    }

    public override bool ModifyCollisionData(NPC npc, Rectangle victimHitbox, ref int immunityCooldownSlot,
        ref MultipliableFloat damageMultiplier, ref Rectangle npcHitbox)
    {
        // Don't use vanilla's collision modification
        return !ShouldOverride();
    }

    public override void SendAcidAI(BitWriter bitWriter, BinaryWriter binaryWriter)
    {
        phaseTracker.Serialize(binaryWriter);
    }

    public override void ReceiveAcidAI(BitReader bitReader, BinaryReader binaryReader)
    {
        phaseTracker.Deserialize(binaryReader);
    }
}