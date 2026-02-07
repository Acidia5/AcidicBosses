using System;
using AcidicBosses.Common.Textures;
using AcidicBosses.Content.Particles;
using AcidicBosses.Helpers;
using Luminance.Common.Utilities;
using Luminance.Core.Graphics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using Terraria;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;



namespace AcidicBosses.Content.Bosses.Deerclops.Projectiles;

public class ShadowSpike : ModProjectile
{
    public override string Texture => TextureRegistry.InvisPath;
    private static Asset<Texture2D> ChainTex;

    private bool firstFrame = true;
    private Vector2 dir;
    private Vector2 deerOffset;
    private float startWidth = 1f;
    private float endWidth = 0f;
    private float spikeT = 0.75f;

    private Vector2 startPos;
    private Vector2 endPos;
    
    private bool specialDrawReady = false;

    public override void SetStaticDefaults()
    {
        ChainTex = ModContent.Request<Texture2D>("AcidicBosses/Content/Bosses/Deerclops/ShadowHandChain");
    }
    
    public override void SetDefaults()
    {
        Projectile.width = 10;
        Projectile.height = 10;
        Projectile.hostile = true;
        Projectile.timeLeft = 120;
    }

    public override void AI()
    {
        specialDrawReady = true;
        
        if (NPC.deerclopsBoss < 0 || NPC.deerclopsBoss > Main.maxNPCs)
        {
            Projectile.active = false;
            return;
        }
        
        var deerclops = Main.npc[NPC.deerclopsBoss];
        if (!deerclops.active)
        {
            Projectile.active = false;
            return;
        }

        ref var timer = ref Projectile.ai[0];

        if (timer == 0)
        {
            dir = Projectile.velocity.SafeNormalize(Vector2.UnitX);
            Projectile.velocity = Vector2.Zero;
            deerOffset = Projectile.position - deerclops.Center;
        }

        Projectile.position = deerclops.Center + deerOffset;

        switch (timer)
        {
            case < 30:
            {
                
                break;
            }
            case < 60:
            {
                break;
            }
            case < 90:
            {
                break;
            }
            default:
            {
                break;
            }
        }
        
        // Meth
        // Computes the intersections on the darkness circle
        var p = deerOffset;
        var d = dir;
        var pd = Vector2.Dot(p, d);
        var discriminant = pd * pd - (p.LengthSquared() - Deerclops.DarknessRadius * Deerclops.DarknessRadius);
        var sqrt = MathF.Sqrt(discriminant);
        var t1 = -pd - sqrt;
        var t2 = -pd + sqrt;
        startPos = Projectile.position + d * t1;
        var int2 = Projectile.position + d * t2;
        endPos = startPos + startPos.DirectionTo(int2) * spikeT * startPos.Distance(int2);

        if (timer == 0)
        {
            new SharpTearNoBlendParticle(Projectile.position - dir * 40f, dir, dir.ToRotation(), Color.Black, 30)
            {
                GlowColor = Color.Purple,
                FadeColor = true
            }.Spawn();
            
            for (var i = 0; i < 20; i++)
            {
                Dust.NewDust(startPos, 0, 0, DustID.Smoke, dir.X * 5, dir.Y * 5, Scale: 2f, newColor: Color.Black);
            }
        }

        timer++;
    }

    public override bool PreDraw(ref Color lightColor)
    {
        if (!specialDrawReady) return false;
        
        var deer = Main.npc[NPC.deerclopsBoss];
        if (deer == null || !deer.active) return false;
        
        // Yes this is wasteful, but luminance needs it
        const int maxPoints = 8;
        var points = new Vector2[maxPoints];
        for (var i = 0; i < maxPoints; i++)
        {
            points[i] = Vector2.Lerp(startPos, endPos, (float)i / (maxPoints-2));
        }
        
        var connectionTex = ChainTex;
        
        // Outline
        var renderSettings2 = new PrimitiveSettings(
            x => MathHelper.Lerp(ChainTex.Width() * startWidth * 1.25f, ChainTex.Width() * endWidth * 1.25f, x),
            x => Color.Purple with { A = 0},
            x => Vector2.Lerp(new Vector2(-4f, 0f), new Vector2(4f, 0f), x),
            Shader: ShaderManager.GetShader("AcidicBosses.Rope")
        );
        renderSettings2.Shader.SetTexture(connectionTex, 1, SamplerState.PointClamp);
        renderSettings2.Shader.TrySetParameter("segments", 2);
        PrimitiveRenderer.RenderTrail(points, renderSettings2);
        
        // Inside
        var renderSettings = new PrimitiveSettings(
            x => MathHelper.Lerp(ChainTex.Width() * startWidth, ChainTex.Width() * endWidth, x),
            x => Color.Black,
            Shader: ShaderManager.GetShader("AcidicBosses.Rope")
        );
        renderSettings.Shader.SetTexture(connectionTex, 1, SamplerState.PointClamp);
        renderSettings.Shader.TrySetParameter("segments", 2);
        PrimitiveRenderer.RenderTrail(points, renderSettings);
        
        return false;
    }

    public override bool? Colliding(Rectangle projHitbox, Rectangle targetHitbox)
    {
        return Collision.CheckAABBvLineCollision(targetHitbox.TopLeft(), targetHitbox.Size(), startPos, endPos);
    }
}