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

    private bool isBrainOpen = false;

    private bool showPhantoms = false;

    private bool isFleeing = false;

    private Vector2 scale = Vector2.One;

    private Color colorAdd = Color.Transparent;

    public override void OnFirstFrame(NPC npc)
    {
        NPC.crimsonBoss = npc.whoAmI;

        phaseTracker = new PhaseTracker([
            PhaseIntro,
            PhaseCreeperOne,
            PhaseAngerOne,
            PhaseTransitionOne,
        ]);

        CloseBrain();
    }

    public override bool AcidAI(NPC npc)
    {
        // Flee when no players are alive or out of crimson
        var target = Main.player[npc.target];
        if ((IsTargetGone(npc) || !target.ZoneCrimson) && !isFleeing)
        {
            npc.TargetClosest();
            target = Main.player[npc.target];
            if (IsTargetGone(npc) || !target.ZoneCrimson)
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
        if (!IsTargetGone(Npc) && target.ZoneCrimson)
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
            Npc.damage / 4, 3);
    }

    private Projectile NewBloodShot(Vector2 position, Vector2 velocity)
    {
        return ProjHelper.NewUnscaledProjectile(Npc.GetSource_FromAI(), position, velocity,
            ProjectileID.BloodNautilusShot,
            Npc.damage / 4, 3);
    }

    #region Drawing

    public override bool AcidicDraw(NPC npc, SpriteBatch spriteBatch, Vector2 screenPos, Color lightColor)
    {
        var texAsset = TextureAssets.Npc[npc.type];
        var drawPos = npc.Center - Main.screenPosition;
        var brainTexture = TextureAssets.Npc[npc.type].Value;
        var origin = npc.frame.Size() * 0.5f;

        // I have to do this workaround to offset the frame when the brain is open.
        // This game's code is so spaghetti that it won't go past 4 frames and I have no clue why.
        var frame = npc.frame;
        if (isBrainOpen) frame.Y += npc.frame.Height * 4;

        // For fading on teleporting
        lightColor *= npc.Opacity;

        // Phantoms
        if (showPhantoms)
        {
            for (var i = 0; i < 4; i++)
            {
                var phantomPos = new Vector2();
                var offsetX = Math.Abs(npc.Center.X - Main.player[Main.myPlayer].Center.X);
                var offsetY = Math.Abs(npc.Center.Y - Main.player[Main.myPlayer].Center.Y);

                if (i is 0 or 2) phantomPos.X = Main.player[Main.myPlayer].Center.X + offsetX;
                else phantomPos.X = Main.player[Main.myPlayer].Center.X - offsetX;

                if (i is 0 or 1) phantomPos.Y = Main.player[Main.myPlayer].Center.Y + offsetY;
                else phantomPos.Y = Main.player[Main.myPlayer].Center.Y - offsetY;

                var phantomColor = Lighting.GetColor(phantomPos.ToTileCoordinates()) * 0.5f * npc.Opacity;

                spriteBatch.Draw(
                    brainTexture, phantomPos - Main.screenPosition,
                    frame, phantomColor,
                    npc.rotation, origin, scale,
                    SpriteEffects.None, 0f);
            }
        }

        spriteBatch.Draw(
            brainTexture, drawPos,
            frame, lightColor,
            npc.rotation, origin, scale,
            SpriteEffects.None, 0f);

        return false;
    }

    public override void FindFrame(NPC npc, int frameHeight)
    {
        if (npc.frameCounter > 6.0)
        {
            npc.frameCounter = 0.0;
            npc.frame.Y += frameHeight;
        }

        if (npc.frame.Y > frameHeight * 3) npc.frame.Y = 0;
    }

    public override void BossHeadSlot(NPC npc, ref int index)
    {
        if (showPhantoms)
        {
            index = -1;
            return;
        }

        base.BossHeadSlot(npc, ref index);
    }

    #endregion

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