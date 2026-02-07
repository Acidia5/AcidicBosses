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
        NPC.knockBackResist = 1.5f;
        
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

        new SharpTearNoBlendParticle(
            deerclops.Center,
            Vector2.Zero,
            SpawnAngle + MathHelper.PiOver2,
            Color.Black,
            30
        )
        {
            GlowColor = Color.Purple with { A = 0 },
            Shrink = true,
            Scale = new Vector2(3f),
            OnUpdate = p =>
            {
                var ease = EasingHelper.QuadOut(p.LifetimeRatio);
                p.Position = Vector2.Lerp(deerclops.Center, NPC.Center, ease);
                Dust.NewDust(p.Position, 0, 0, DustID.Smoke, newColor: Color.Black);
                Dust.NewDust(p.Position, 0, 0, DustID.PurpleTorch);
            }
        }.Spawn();
        
        for (var i = 0; i < 20; i++)
        {
            var vel = Main.rand.NextVector2Circular(5f, 5f);
            Dust.NewDust(deerclops.Center, 0, 0, DustID.Smoke, vel.X, vel.Y, newColor: Color.Black);
        }
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

        var reworkedDeer = deerclops.GetGlobalNPC<Deerclops>();

        if (AcidUtils.IsClient())
        {
            var edge = deerclops.Center + SpawnAngle.ToRotationVector2() * (Deerclops.DarknessRadius + 128);
            var midpoint = NPC.Center + (NPC.rotation.ToRotationVector2() * -1f * 128f);

            const int maxPoints = 32;
            for (var i = 0; i < maxPoints; i++)
            {
                var p = (float)i / maxPoints;
                p = EasingHelper.QuadOut(p);
                var pos = Utilities.QuadraticBezier(NPC.Center, midpoint, edge, p);

                if (Main.rand.NextBool(1, 50))
                {
                    Dust.NewDust(pos, 0, 0, DustID.Smoke, newColor: Color.Black);
                }
            }
            
            if (Main.rand.NextBool(1, 5))
            {
                Dust.NewDust(NPC.Center, 0, 0, DustID.Smoke, newColor: Color.Black);
            }
            
            if (Main.rand.NextBool(1, 50))
            {
                Dust.NewDust(NPC.Center, 0, 0, DustID.PurpleTorch);
            }
        }

        // Always go after the closest player
        NPC.TargetClosest();
        var targetPlayer = Main.player[NPC.target];
        NPC.FaceTarget();
        NPC.rotation = NPC.DirectionTo(targetPlayer.Center).ToRotation();

        if (!reworkedDeer.RetractShadowHands)
        {
            NPC.SimpleFlyMovement(NPC.rotation.ToRotationVector2() * 3f, 0.1f);
        }
        else
        {
            var edge = deerclops.Center + SpawnAngle.ToRotationVector2() * (Deerclops.DarknessRadius + 128);
            NPC.SimpleFlyMovement(NPC.DirectionTo(edge) * 5f, 0.2f);

            if (NPC.Distance(deerclops.Center) > Deerclops.DarknessRadius)
            {
                NPC.active = false;
            }
        }
    }

    public override void HitEffect(NPC.HitInfo hit)
    {
        var deer = Main.npc[NPC.deerclopsBoss];
        if (deer == null || !deer.active) return;
        
        if (AcidUtils.IsClient())
        {
            for (var i = 0; i < 5; i++)
            {
                var vel = Main.rand.NextVector2Circular(3f, 3f);
                Dust.NewDust(NPC.position, NPC.width, NPC.height, DustID.Smoke, vel.X, vel.Y, newColor: Color.Black);
            }
            
            if (NPC.life <= 0f)
            {
                for (var i = 0; i < 20; i++)
                {
                    Dust.NewDust(NPC.position, NPC.width, NPC.height, DustID.Smoke, newColor: Color.Black);
                    Dust.NewDust(NPC.position, NPC.width, NPC.height, DustID.PurpleTorch);
                }
                
                var edge = deer.Center + SpawnAngle.ToRotationVector2() * (Deerclops.DarknessRadius + 128);
                var midpoint = NPC.Center + (NPC.rotation.ToRotationVector2() * -1f * 128f);

                const int maxPoints = 32;
                for (var i = 0; i < maxPoints; i++)
                {
                    var p = (float)i / maxPoints;
                    p = EasingHelper.QuadOut(p);
                    var pos = Utilities.QuadraticBezier(NPC.Center, midpoint, edge, p);
                    
                    Dust.NewDust(pos, 0, 0, DustID.Smoke, newColor: Color.Black);
                    Dust.NewDust(pos, 0, 0, DustID.Smoke, newColor: Color.Black);
                }
            }
        }
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
        
        var backglowColor = Color.Purple with { A = 0 } * (1f / 12f);
        var backglowArea = 2f;

        for (var i = 0; i < 12; i++)
        {
            var drawOffset = (NPC.rotation + MathHelper.Lerp(0f, MathHelper.TwoPi, i / 12f)).ToRotationVector2() *
                             (backglowArea + 0.25f);
            Main.spriteBatch.Draw(
                tex.Value, NPC.Center - screenPos + drawOffset,
                frame, backglowColor,
                NPC.rotation, origin, NPC.scale - 0.01f,
                effects, 0f);
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
            x => Color.Black * 0.75f,
            Shader: ShaderManager.GetShader("AcidicBosses.Rope")
        );
        renderSettings.Shader.SetTexture(connectionTex, 1, SamplerState.PointClamp);
        renderSettings.Shader.TrySetParameter("segments", maxPoints * 2);
    
        PrimitiveRenderer.RenderTrail(points, renderSettings);
    }
}