using System;
using System.IO;
using System.Linq;
using AcidicBosses.Common.Configs;
using AcidicBosses.Content.Bosses.BoC;
using AcidicBosses.Content.Bosses.WoF.Projectiles;
using AcidicBosses.Content.Particles;
using AcidicBosses.Content.Particles.Animated;
using AcidicBosses.Content.ProjectileBases;
using AcidicBosses.Core.StateManagement;
using AcidicBosses.Helpers;
using Luminance.Common.Easings;
using Luminance.Core.Graphics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.Audio;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.ModLoader.IO;

namespace AcidicBosses.Content.Bosses.WoF;

// I am changing the WoF NPC from the mouth to an invisible coordinator
public partial class WoF : AcidicNPCOverride
{
    protected override int OverriddenNpc => NPCID.WallofFlesh;
    protected override bool BossEnabled => BossToggleConfig.Get().EnableWallOfFlesh;

    public override void SetStaticDefaults()
    {
        if (!ShouldOverride()) return;
        NPCID.Sets.NeedsExpertScaling[NPCID.WallofFlesh] = true;
    }

    public override void SetDefaults(NPC entity)
    {
        if (!ShouldOverride()) return;
        entity.dontTakeDamage = true;
        entity.ShowNameOnHover = false;
    }

    public override bool ModifyCollisionData(NPC npc, Rectangle victimHitbox, ref int immunityCooldownSlot,
        ref MultipliableFloat damageMultiplier, ref Rectangle npcHitbox)
    {
        if (!ShouldOverride()) return true;
        
        // Nuke the hitbox because I still use npc.damage for projectiles
        damageMultiplier *= 0f;
        npcHitbox = new Rectangle();
        return false;
    }

    public override void BossHeadSlot(NPC npc, ref int index)
    {
        if (!ShouldOverride()) return;
        index = -1;
    }

    private PhaseTracker phaseTracker;

    private bool isFleeing = false;

    public override void OnFirstFrame(NPC npc)
    {
        phaseTracker = new PhaseTracker([
            PhaseIntro,
            PhaseOne,
            PhaseMoveTransition,
            PhaseTwo,
            PhaseThree
        ]);
        AttackManager.Reset();
        WallDistance = 3000;

        Main.wofNPCIndex = Npc.whoAmI;
        Main.wofDrawAreaBottom = -1;
        Main.wofDrawAreaTop = -1;
        Npc.TargetClosest_WOF();

        Npc.position.X = Main.player[Npc.target].position.X;
        
        SetWoFArea();

        if (Main.netMode == NetmodeID.MultiplayerClient) return;
        NewEye(PartPosition.Left | PartPosition.Top);
        NewMouth(PartPosition.Left | PartPosition.Center);
        NewEye(PartPosition.Left | PartPosition.Bottom);
        NewEye(PartPosition.Right | PartPosition.Top);
        NewMouth(PartPosition.Right | PartPosition.Center);
        NewEye(PartPosition.Right | PartPosition.Bottom);
    }

    public override bool AcidAI(NPC npc)
    {
        Main.wofDrawFrameIndex++;
        
        if (IsTargetGone(npc) && !isFleeing)
        {
            npc.TargetClosest();
            if (IsTargetGone(npc))
            {
                AttackManager.CountUp = true;
                isFleeing = true;
                AttackManager.AiTimer = 0;
            }
        }

        // Fill Arena Area
        SetWoFArea();

        if (isFleeing) FleeAI();
        else phaseTracker.RunPhaseAI();

        return false;
    }

    private void FleeAI()
    {
        Npc.TargetClosest();

        Npc.velocity.X = Npc.spriteDirection * EasingHelper.QuadIn(AttackManager.AiTimer / 30f);
        Npc.EncourageDespawn(10);
    }

    public override bool AcidicDraw(NPC npc, SpriteBatch spriteBatch, Vector2 screenPos, Color lightColor)
    {
        // Don't draw lmao
        return false;
    }

    public override bool? DrawHealthBar(NPC npc, byte hbPosition, ref float scale, ref Vector2 position)
    {
        return false;
    }

    public override void SendAcidAI(BitWriter bitWriter, BinaryWriter binaryWriter)
    {
        phaseTracker.Serialize(binaryWriter);
        
        binaryWriter.Write(WallDistance);
        binaryWriter.Write(BottomLeftY);
        binaryWriter.Write(BottomRightY);
        binaryWriter.Write(TopRightY);
        binaryWriter.Write(TopLeftY);
    }

    public override void ReceiveAcidAI(BitReader bitReader, BinaryReader binaryReader)
    {
        phaseTracker.Deserialize(binaryReader);

        WallDistance = binaryReader.ReadSingle();
        BottomLeftY = binaryReader.ReadSingle();
        BottomRightY = binaryReader.ReadSingle();
        TopRightY = binaryReader.ReadSingle();
        TopLeftY = binaryReader.ReadSingle();
    }

    public static WoF? GetInstance()
    {
        if (Main.wofNPCIndex < 0 || Main.wofNPCIndex >= Main.maxNPCs) return null;
        return Main.npc[Main.wofNPCIndex]?.GetGlobalNPC<WoF>();
    }
}