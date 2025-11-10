using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using AcidicBosses.Common.Textures;
using AcidicBosses.Core.StateManagement;
using Luminance.Common.StateMachines;
using Luminance.Common.Utilities;
using Luminance.Common.VerletIntergration;
using Luminance.Core.Graphics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using ReLogic.Graphics;
using Terraria;
using Terraria.GameContent;
using Terraria.GameContent.Bestiary;
using Terraria.ID;
using Terraria.ModLoader;
using Vector2 = Microsoft.Xna.Framework.Vector2;

namespace AcidicBosses.Content.Bosses.Twins;

public partial class TwinsController : AcidicNPC
{
    public override string Texture => TextureRegistry.InvisPath;

    private int spazId
    {
        get => (int) NPC.ai[0];
        set => NPC.ai[0] = value;
    }

    private int retId
    {
        get => (int) NPC.ai[1];
        set => NPC.ai[1] = value;
    }

    private bool connectedTwins = false;

    private Retinazer? Retinazer => GetRet();
    private Spazmatism? Spazmatism => GetSpaz();

    private float AverageLifePercent => ((Spazmatism?.Npc.GetLifePercent() ?? 0f) + (Retinazer?.Npc.GetLifePercent() ?? 0f)) / 2f;

    // Only exists on client
    private List<VerletSegment> connectionSegments = [];
    private VerletSettings connectionSimSettings = new()
    {
        ConserveEnergy = true
    };
    
    private Asset<Texture2D> connectionTex = TextureAssets.Chain12;
    private int connectionLength = 500;

    public override void SetStaticDefaults()
    {
        // Yeet the bestiary entry
        var bestiary = new NPCID.Sets.NPCBestiaryDrawModifiers()
        {
            Hide = true
        };
        
        NPCID.Sets.NPCBestiaryDrawOffset.Add(NPC.type, bestiary);
    }

    public override void SetDefaults()
    {
        NPC.damage = 0;
        NPC.defense = 0;
        NPC.lifeMax = 1;
        NPC.life = 1;
        NPC.dontTakeDamage = true;

        NPC.width = 0;
        NPC.height = 0;
    }

    #region AI

    private AttackManager attackManager = new();

    private PhaseTracker phaseTracker;

    private bool changedState = false;

    public override void OnFirstFrame()
    {
        NPC.TargetClosest();
        NPC.position = Main.player[NPC.target].position;

        // Only these three are in order. The rest are managed by the phase ai
        phaseTracker = new PhaseTracker([
            PhaseUntransformed,
            PhaseTransformation,
            PhaseTransformed1,
            PhaseTransformed2,
        ]);


        // Fill out the connector on the client
        if (Main.netMode != NetmodeID.Server) FillTether();
    }

    public override void AcidAI()
    {
        if (!connectedTwins)
        {
            if (Spazmatism == null || Retinazer == null)
            {
                // Wait for the connection
                return;
            }
            else
            {
                connectedTwins = true;
            }
        }
        
        
        if (CheckTwinsDead())
        {
            NPC.active = false;
            return;
        }
        
        // If a player joins mid-fight, they need to create the tether on their screen.
        if (Main.netMode != NetmodeID.Server && connectionSegments.Count == 0) FillTether();
        
        attackManager.PreAttackAi();

        NPC.TargetClosest();
        var target = Main.player[NPC.target];
        NPC.Center = target.Center;

        // Despawn when day
        if (Main.IsItDay())
        {
            Flee();
        }
        
        // Despawn when no targets
        if (!target.active || target.dead)
        {
            NPC.TargetClosest();
            target = Main.player[NPC.target];
            if (!target.active || target.dead)
            {
                Flee();
            }
        }

        if (Spazmatism == null && !changedState)
        {
            phaseTracker.ChangeState(PhaseSoloRet);
            changedState = true;
        }
        if (Retinazer == null && !changedState)
        {
            phaseTracker.ChangeState(PhaseSoloSpaz);
            changedState = true;
        }
        
        phaseTracker.RunPhaseAI();
        
        attackManager.PostAttackAi();
    }

    private bool CheckTwinsDead()
    {
        return Spazmatism == null && Retinazer == null;
    }

    private void Flee()
    {
        // No setting with conditionals in this version of C# :(
        if (Spazmatism != null) Spazmatism.Npc.active = false;
        if (Retinazer != null) Retinazer.Npc.active = false;
        NPC.active = false;
    }

    private void FillTether()
    {
        connectionSegments.Clear(); // Just to be safe

        if (Retinazer is null || Spazmatism is null) return;
        
        for (var i = 0; i < connectionLength; i += connectionTex.Value.Height)
        {
            connectionSegments.Add(
                new VerletSegment(new Vector2(Spazmatism.Npc.Center.X + i, Spazmatism.Npc.Center.Y), Vector2.Zero));
        }
            
        // Remove one segment
        connectionSegments.RemoveAt(0);
    }

    #endregion

    public override void SendAcidAI(BinaryWriter binaryWriter)
    {
        attackManager.Serialize(binaryWriter);
        phaseTracker.Serialize(binaryWriter);
    }

    public override void ReceiveAcidAI(BinaryReader binaryReader)
    {
        attackManager.Deserialize(binaryReader);
        phaseTracker.Deserialize(binaryReader);
    }

    public override bool PreDraw(SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor)
    {
        // Don't draw on the first frame to fix null errors
        if (IsFirstFrame) return false;
        
        if (!ShouldDrawTether()) return false;
        
        // Since ShouldDrawTether() inherently checks null
        Debug.Assert(Spazmatism != null, nameof(Spazmatism) + " != null");
        Debug.Assert(Retinazer != null, nameof(Retinazer) + " != null");
        
        // Yes I'm running this every frame. If I don't the rope is jittery.
        // You can't stop me
        connectionSegments = VerletSimulations.RopeVerletSimulation(connectionSegments, Retinazer.Npc.Center,
            connectionLength * 0.75f, connectionSimSettings, Spazmatism.Npc.Center);
        
        var renderSettings = new PrimitiveSettings(
            _ => connectionTex.Value.Width / 2f,
            p =>
            {
                var index = (int) (p * connectionSegments.Count);
                if (index == connectionSegments.Count) index--;
                return Lighting.GetColor(connectionSegments[index].Position.ToTileCoordinates());
            },
            Shader: ShaderManager.GetShader("AcidicBosses.Rope")
        );
        renderSettings.Shader.SetTexture(connectionTex, 1, SamplerState.PointClamp);
        renderSettings.Shader.TrySetParameter("segments", connectionSegments.Count);
    
        PrimitiveRenderer.RenderTrail(connectionSegments.Select(s => s.Position), renderSettings);
        
        // spriteBatch.DrawString(FontAssets.MouseText.Value, attackManager.AiTimer.ToString(), NPC.position - Main.screenPosition + new Vector2(50, 50), Color.White);

        return false;
    }

    private bool ShouldDrawTether()
    {
        return Retinazer != null && Spazmatism != null;
    }

    // Returns null when both are alive
    private Twin? AliveTwin()
    {
        if (Retinazer != null && Spazmatism != null) return null;
        if (Spazmatism != null) return Spazmatism;
        return Retinazer;
    }

    private void DoBothTwins(Action<Twin> action)
    {
        if (Spazmatism != null) action(Spazmatism);
        if (Retinazer != null) action(Retinazer);
    }

    private Retinazer? GetRet()
    {
        var npc = Main.npc[retId];
        if (!npc.active) return null;

        return npc.TryGetGlobalNPC<Retinazer>(out var ret) ? ret : null;
    }
    
    private Spazmatism? GetSpaz()
    {
        var npc = Main.npc[spazId];
        if (!npc.active) return null;

        return npc.TryGetGlobalNPC<Spazmatism>(out var spaz) ? spaz : null;
    }

    public static int Link(NPC npc)
    {
        var controller = NPC.FindFirstNPC(ModContent.NPCType<TwinsController>());
        if (controller != -1 && Main.npc[controller].active)
        {
            return controller;
        }

        return NPC.NewNPC(npc.GetSource_FromAI(), (int) npc.position.X, (int) npc.position.Y,
            ModContent.NPCType<TwinsController>(), npc.whoAmI);
    }
}