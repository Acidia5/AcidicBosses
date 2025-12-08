using System.Collections.Generic;
using System.Linq;
using AcidicBosses.Common.Textures;
using AcidicBosses.Content.Particles;
using AcidicBosses.Core.Animation;
using AcidicBosses.Helpers;
using Luminance.Common.Utilities;
using Luminance.Common.VerletIntergration;
using Luminance.Core.Graphics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using Terraria;
using Terraria.GameContent;
using Terraria.GameContent.Bestiary;
using Terraria.Graphics;
using Terraria.ID;
using Terraria.ModLoader;

namespace AcidicBosses.Content.Bosses.Deerclops;

public class ShadowHand : AcidicNPC
{
    public override string Texture => TextureRegistry.TerrariaProjectile(ProjectileID.InsanityShadowHostile);
    private static Asset<Texture2D> ChainTex;
    
    private float SpawnAngle => NPC.ai[0];
    private float Speed => NPC.ai[1];
    private bool specialDrawReady = false;
    
    public override void SetStaticDefaults()
    {
        ChainTex = ModContent.Request<Texture2D>("AcidicBosses/Content/Bosses/Deerclops/ShadowHandChain");
        
        NPCID.Sets.NPCBestiaryDrawOffset.Add(Type, new NPCID.Sets.NPCBestiaryDrawModifiers()
        {
            
        });
    }

    public override void SetDefaults()
    {
        NPC.width = 24;
        NPC.height = 24;
        
        NPC.lifeMax = 20;
        NPC.damage = 20;
        NPC.defense = 0;
        NPC.knockBackResist = 0f;
        
        NPC.HitSound = SoundID.NPCHit36;
        NPC.DeathSound = SoundID.NPCDeath39;
        
        NPC.noGravity = true;
        NPC.noTileCollide = true;
    }

    public override void SetBestiary(BestiaryDatabase database, BestiaryEntry bestiaryEntry)
    {
        bestiaryEntry.Info.AddRange([
            BestiaryDatabaseNPCsPopulator.CommonTags.SpawnConditions.Biomes.Snow,
            new FlavorTextBestiaryInfoElement("Mods.AcidicBosses.Bestiary.ShadowHand"),
        ]);
    }

    public override void OnFirstFrame()
    {
        specialDrawReady = true;
        
        if (NPC.deerclopsBoss < 0 || NPC.deerclopsBoss > Main.maxNPCs)
        {
            NPC.active = false;
            return;
        }
        
        var deerclops = Main.npc[NPC.deerclopsBoss];
        if (!deerclops.active)
        {
            NPC.active = false;
            return;
        }

        var spawnDir = SpawnAngle.ToRotationVector2();

        new SharpTearParticle(
            deerclops.Center,
            Vector2.Zero,
            SpawnAngle + MathHelper.PiOver2,
            Color.Black,
            30
        )
        {
            GlowColor = Color.Purple,
            Shrink = true,
            OnUpdate = p =>
            {
                var ease = EasingHelper.QuadOut(p.LifetimeRatio);
                p.Position = Vector2.Lerp(deerclops.Center, NPC.Center, ease);
            }
        }.Spawn();
    }

    public override void AcidAI()
    {
        if (NPC.deerclopsBoss < 0 || NPC.deerclopsBoss > Main.maxNPCs)
        {
            NPC.active = false;
            return;
        }
        
        var deerclops = Main.npc[NPC.deerclopsBoss];
        if (!deerclops.active)
        {
            NPC.active = false;
            return;
        }

        // Always go after the closest player
        NPC.TargetClosest();
        var targetPlayer = Main.player[NPC.target];
        NPC.FaceTarget();
        NPC.rotation = NPC.DirectionTo(targetPlayer.Center).ToRotation();
        
        NPC.SimpleFlyMovement(NPC.rotation.ToRotationVector2() * 3f, 0.1f);
    }

    public override bool PreDraw(SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor)
    {
        var tex = TextureAssets.Npc[Type];
        var frame = tex.Frame();

        if (!specialDrawReady)
        {
            Main.EntitySpriteDraw(
                tex.Value,
                NPC.Center - screenPos,
                frame,
                Color.Black,
                NPC.rotation,
                frame.Size() / 2f,
                1f,
                SpriteEffects.None
            );
            return false;
        }
        
        // Hard coded origin :)
        var origin = new Vector2(14f, 42f);

        var effects = SpriteEffects.None;
        if (NPC.direction < 0)
        {
            effects |= SpriteEffects.FlipVertically;
        }
        
        Main.EntitySpriteDraw(
            tex.Value,
            NPC.Center - screenPos,
            frame,
            Color.Black,
            NPC.rotation,
            origin,
            1f,
            effects
        );
        
        DrawTether();
        
        return false;
    }
    
    private void DrawTether()
    {
        var deer = Main.npc[NPC.deerclopsBoss];
        if (deer == null || !deer.active) return;

        var edge = deer.Center + SpawnAngle.ToRotationVector2() * (Deerclops.DarknessRadius + 128);
        var midpoint = NPC.Center + (NPC.rotation.ToRotationVector2() * -1f * 128f);

        const int maxPoints = 32;
        var points = new Vector2[maxPoints];
        for (var i = 0; i < maxPoints; i++)
        {
            var p = (float)i / maxPoints;
            p = EasingHelper.QuadOut(p);
            points[i] = Utilities.QuadraticBezier(NPC.Center, midpoint, edge, p);
        }

        var connectionTex = ChainTex;
        var renderSettings = new PrimitiveSettings(
            x => MathHelper.Lerp(ChainTex.Width() * 0.5f, ChainTex.Width() * 0.4f, x),
            x => Color.Black,
            Shader: ShaderManager.GetShader("AcidicBosses.Rope")
        );
        renderSettings.Shader.SetTexture(connectionTex, 1, SamplerState.PointClamp);
        renderSettings.Shader.TrySetParameter("segments", maxPoints * 2);
    
        PrimitiveRenderer.RenderTrail(points, renderSettings);
    }
}