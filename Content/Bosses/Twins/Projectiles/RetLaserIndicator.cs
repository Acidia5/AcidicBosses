using AcidicBosses.Common.Textures;
using AcidicBosses.Content.Particles;
using AcidicBosses.Content.ProjectileBases;
using AcidicBosses.Helpers;
using Luminance.Common.Easings;
using Luminance.Common.Utilities;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using Terraria;
using Terraria.ID;

namespace AcidicBosses.Content.Bosses.Twins.Projectiles;

public class RetLaserIndicator : BaseLineProjectile
{
    protected override float Length { get; set; } = 12000;
    protected override float Width { get; set; } = 15;
    protected override Color Color => GetColor();
    protected override Asset<Texture2D> LineTexture => TextureRegistry.GlowLine;
    public override bool RotateAroundCenter => true;

    private Vector2 laserVel;
    
    private Color GetColor()
    {
        var fadeT = (float) Projectile.timeLeft / MaxTimeLeft;
        var color = Color.Red;
        color *= EasingHelper.CubicOut(fadeT);
        return color;
    }
    
    public override void AI()
    {
        base.AI();
        
        if (Projectile.velocity != Vector2.Zero)
        {
            laserVel = Projectile.velocity;
            Projectile.velocity = Vector2.Zero;
        }
        
        ref var timeAlive = ref Projectile.localAI[0];
        
        var inTime = 15;
        if (timeAlive < inTime)
        {
            var ease = EasingHelper.BackOut((float) timeAlive / inTime);
            Width = MathHelper.Lerp(0, 15, ease);
        }

        timeAlive++;
    }

    public override void OnKill(int timeLeft)
    {
        var scaleCurve = new PiecewiseCurve()
            .Add(EasingCurves.Quadratic, EasingType.Out, 0f, 1f, 4f);

        new GlowStarParticle(Projectile.position, Vector2.Zero, Projectile.rotation, Color.White, 30)
        {
            IgnoreLighting = true,
            GlowColor = Color.Red,
            OnUpdate = p =>
            {
                var scale = scaleCurve.Evaluate(p.LifetimeRatio);
                p.Scale = Vector2.One * scale;
                p.AngularVelocity = p.LifetimeRatio * MathHelper.Pi / 16f;
            }
        }.Spawn();

        for (var i = 0; i < 5; i++)
        {
            var vel = Main.rand.NextVector2Circular(5f, 5f);
            Dust.NewDustPerfect(Projectile.position, DustID.GemRuby, vel, Scale: 0.5f);
            
            new GlowStarParticle(
                Projectile.position,
                Main.rand.NextVector2Circular(3f, 3f),
                Main.rand.NextFloatDirection(),
                Color.White,
                15
            )
            {
                GlowColor = Color.Red,
                AngularVelocity = Main.rand.NextFloat(0.1f),
                OnUpdate = p =>
                {
                    p.Scale = Vector2.Lerp(new Vector2(1.5f), Vector2.Zero, p.LifetimeRatio);
                }
            }.Spawn();
        }
        
        if (Main.netMode == NetmodeID.MultiplayerClient) return;
        // Use vanilla to keep scaling
        var proj = Projectile.NewProjectileDirect(Projectile.GetSource_FromAI(), Projectile.position, Projectile.rotation.ToRotationVector2() * laserVel.Length(), ProjectileID.DeathLaser,
            Projectile.damage, 3);
        proj.tileCollide = false;
    }
}