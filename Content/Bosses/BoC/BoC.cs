using System;
using System.IO;
using System.Linq;
using AcidicBosses.Common;
using AcidicBosses.Common.Configs;
using AcidicBosses.Common.Effects;
using AcidicBosses.Content.Particles;
using AcidicBosses.Content.Particles.Animated;
using AcidicBosses.Core.StateManagement;
using AcidicBosses.Helpers;
using Luminance.Common.Utilities;
using Luminance.Core.Graphics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.Audio;
using Terraria.GameContent;
using Terraria.GameContent.ItemDropRules;
using Terraria.Graphics.CameraModifiers;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.ModLoader.IO;

namespace AcidicBosses.Content.Bosses.BoC;

public partial class BoC : AcidicNPCOverride
{
    protected override int OverriddenNpc => NPCID.BrainofCthulhu;
    

    protected override bool BossEnabled => BossToggleConfig.Get().EnableBrainOfCthulhu;

    private BoCBossBar BossBar => (BoCBossBar)Npc.BossBar;

    public override void SetDefaults(NPC entity)
    {
        base.SetDefaults(entity);

        entity.BossBar = ModContent.GetInstance<BoCBossBar>();
        entity.knockBackResist = 0f; // Remove knockback
        entity.lifeMax = (int)(entity.lifeMax * 1.5f); // Compensate for fewer Creepers
    }

    #region AI

    private PhaseTracker phaseTracker;
    private bool isFleeing = false;

    public override void OnFirstFrame(NPC npc)
    {
        NPC.crimsonBoss = npc.whoAmI;

        phaseTracker = new PhaseTracker([
            PhaseIntro,
            PhaseCreeperOne,
            PhaseAngerOne,
            PhaseTransitionOne,
            PhaseCreeperTwo,
            PhaseAngerTwo,
            PhaseTransitionDesperation,
            PhaseDesperation
        ]);

        CloseBrain();
    }

    public override bool AcidAI(NPC npc)
    {
        DrawAI();
        
        if (isBrainOpen && Main.rand.NextBool(20))
        {
            var pos = Main.rand.NextVector2FromRectangle(Npc.getRect());
            var d = Dust.NewDustPerfect(
                pos,
                DustID.Blood,
                Vector2.UnitY,
                Alpha: Npc.alpha,
                Scale: 2f
            );
        }
        
        // Flee when no players are alive or out of crimson
        var target = Main.player[npc.target];
        if ((IsTargetGone(npc)) && !isFleeing)
        {
            npc.TargetClosest();
            target = Main.player[npc.target];
            if (IsTargetGone(npc))
            {
                AttackManager.CountUp = true;
                isFleeing = true;
                AttackManager.AiTimer = 0;
            }
        }

        if (isFleeing) FleeAI();
        else phaseTracker.RunPhaseAI();

        return false;
    }

    private void FleeAI()
    {
        AttackManager.CountUp = true;

        var target = Main.player[Npc.target];
        if (!IsTargetGone(Npc))
        {
            AttackManager.CountUp = false;
            AttackManager.AiTimer = 0;
            isFleeing = false;
            return;
        }

        if (AttackManager.AiTimer < 120)
        {
            Npc.velocity.Y += AttackManager.AiTimer * 0.025f;
        }
        else
        {
            Npc.active = false;
        }
    }

    #endregion

    private void OpenBrain()
    {
        Npc.dontTakeDamage = false;
        isBrainOpen = true;

        // Just taken from vanilla
        SoundEngine.PlaySound(SoundID.NPCHit9, Npc.Center);
        Gore.NewGore(Npc.GetSource_FromAI(), Npc.Center,
            new Vector2(Main.rand.Next(-30, 31) * 0.2f, Main.rand.Next(-30, 31) * 0.2f), 392);
        Gore.NewGore(Npc.GetSource_FromAI(), Npc.Center,
            new Vector2(Main.rand.Next(-30, 31) * 0.2f, Main.rand.Next(-30, 31) * 0.2f), 393);
        Gore.NewGore(Npc.GetSource_FromAI(), Npc.Center,
            new Vector2(Main.rand.Next(-30, 31) * 0.2f, Main.rand.Next(-30, 31) * 0.2f), 394);
        Gore.NewGore(Npc.GetSource_FromAI(), Npc.Center,
            new Vector2(Main.rand.Next(-30, 31) * 0.2f, Main.rand.Next(-30, 31) * 0.2f), 395);
        for (var num1414 = 0; num1414 < 20; num1414++)
        {
            Dust.NewDust(Npc.position, Npc.width, Npc.height, DustID.Blood, Main.rand.Next(-30, 31) * 0.2f,
                Main.rand.Next(-30, 31) * 0.2f);
        }

        SoundEngine.PlaySound(SoundID.Roar, Npc.Center);
    }

    private void CloseBrain()
    {
        // No fancy effects for now
        Npc.dontTakeDamage = true;
        isBrainOpen = false;
    }

    private Projectile NewIchorShot(Vector2 position, Vector2 velocity)
    {
        return ProjHelper.NewUnscaledProjectile(Npc.GetSource_FromAI(), position, velocity,
            ModContent.ProjectileType<IchorShot>(),
            (int)(Npc.damage * 1.2f), 3);
    }

    private Projectile NewBloodShot(Vector2 position, Vector2 velocity)
    {
        return ProjHelper.NewUnscaledProjectile(Npc.GetSource_FromAI(), position, velocity,
            ModContent.ProjectileType<BloodShot>(),
            Npc.damage, 3);
    }

    public override void ModifyNPCLoot(NPC npc, NPCLoot npcLoot)
    {
        // Drop Tissue Samples directly if the player isn't getting a treasure bag
        var notExpertRule = new LeadingConditionRule(new Conditions.NotExpert());
        notExpertRule.OnSuccess(ItemDropRule.Common(ItemID.TissueSample, 1, 75, 125));

        npcLoot.Add(notExpertRule);
    }

    public override void SendAcidAI(BitWriter bitWriter, BinaryWriter binaryWriter)
    {
        phaseTracker.Serialize(binaryWriter);

        bitWriter.WriteBit(isFleeing);
    }

    public override void ReceiveAcidAI(BitReader bitReader, BinaryReader binaryReader)
    {
        phaseTracker.Deserialize(binaryReader);

        isFleeing = bitReader.ReadBit();
    }
}