using AcidicBosses.Common.Textures;
using AcidicBosses.Content.Particles;
using AcidicBosses.Content.Particles.Animated;
using AcidicBosses.Core.Graphics.DustManagement;
using AcidicBosses.Helpers;
using Luminance.Common.Utilities;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;

namespace AcidicBosses.Content.Bosses.Skeletron;

public class Muramasa : ModProjectile
{
    public override string Texture => TextureRegistry.TerrariaItem(ItemID.Muramasa);
    public override string GlowTexture => Texture;

    private Vector2 oldVel = Vector2.Zero;
    private bool setOldVel = false;

    public override void SetStaticDefaults()
    {
        ProjectileID.Sets.TrailingMode[Projectile.type] = 2;
        ProjectileID.Sets.TrailCacheLength[Projectile.type] = 20;
    }

    public override void SetDefaults()
    {
        Projectile.width = (int) (60 * 1.5);
        Projectile.height = (int) (60 * 1.5);
        Projectile.alpha = 255;
        Projectile.hostile = true;
        Projectile.tileCollide = false;
    }

    public override void AI()
    {
        const int rotationTime = 60;

        ref var aiTime = ref Projectile.ai[0];

        if (!setOldVel)
        {
            Projectile.rotation = Projectile.velocity.ToRotation() + MathHelper.PiOver4;
            oldVel = Projectile.velocity;
            Projectile.velocity = Vector2.Zero;
            setOldVel = true;
            Projectile.scale = 0f;
            Projectile.damage = 0;
        }

        if (aiTime == 0)
        {
            SoundEngine.PlaySound(SoundID.Item1);
            var puff = new BigPuffParticle(Projectile.Center, Vector2.Zero, 0f, Color.White, 30);
            puff.Opacity = 0.5f;
            puff.Spawn();
            
            for (var i = 0; i < 5; i++)
            {
                var vel = Main.rand.NextVector2Circular(2f, 2f);
                Dust.NewDustPerfect(Projectile.Center, DustID.DungeonWater, vel);
            }
        }

        // Spin
        if (aiTime < rotationTime)
        {
            var baseRot = oldVel.ToRotation() + MathHelper.PiOver4;
            var rotT = EasingHelper.BackOut(aiTime / rotationTime);
            var offset = MathHelper.TwoPi * rotT;
            Projectile.rotation = MathHelper.WrapAngle(baseRot + offset);
            Projectile.scale = MathHelper.Lerp(0f, 1.5f, rotT);
        }
        else
        {
            // Projectile.velocity = Vector2.Lerp(Projectile.velocity, , 0.15f);
            Projectile.velocity = oldVel;
            Projectile.rotation = Projectile.velocity.ToRotation() + MathHelper.PiOver4;
            Projectile.damage = Projectile.originalDamage;
        }
        
        // Takeoff effect
        if ((int)aiTime == rotationTime)
        {
            var centerOffset = Projectile.Hitbox.Size() / 2;
            centerOffset.Y *= -1;
            centerOffset = centerOffset.RotatedBy(Projectile.rotation);
            var swordBack = Projectile.Center + centerOffset;
            var swordEnd = Projectile.Center - centerOffset;

            for (var i = 0; i < 15; i++)
            {
                var velOff = oldVel.Length() * 0.5f;
                var vel = -oldVel * 0.5f + Main.rand.NextVector2Circular(velOff, velOff);
                var d = Dust.NewDustPerfect(
                    swordBack,
                    DustID.DungeonWater,
                    vel
                );
                d.noGravity = true;
            }
            
            for (var i = 0; i < 25; i++)
            {
                var velOff = oldVel.Length() * 0.25f;
                var vel = -oldVel * 0.5f + Main.rand.NextVector2Circular(velOff, velOff);
                Dust.NewDustPerfect(
                    swordBack,
                    DustID.Smoke,
                    vel
                );
            }

            new SmokeRingParticle(
                swordBack,
                -oldVel * 0.25f,
                oldVel.ToRotation() - MathHelper.PiOver2,
                Color.White,
                60
            )
            {
                Opacity = 0.5f
            }.Spawn();
        }

        Lighting.AddLight(Projectile.Center, Color.Cyan.ToVector3());
        
        // Dust
        if (Main.rand.NextBool(0.5f))
        {
            var centerOffset = Projectile.Hitbox.Size() / 2;
            centerOffset.Y *= -1;
            centerOffset = centerOffset.RotatedBy(Projectile.rotation);
            var swordBack = Projectile.Center + centerOffset;
            var swordEnd = Projectile.Center - centerOffset;
            var pos = Vector2.Lerp(swordBack, swordEnd, Main.rand.NextFloat());
            
            var d = Dust.NewDustDirect(
                pos,
                0,
                0,
                DustID.DungeonWater
            );
            d.noGravity = true;
        }

        aiTime++;
    }

    public override bool PreDraw(ref Color lightColor)
    {
        var texture = ModContent.Request<Texture2D>(Texture).Value;
        ProjHelper.DrawAfterimages(Projectile, texture, ref lightColor, 2);
        ProjHelper.Draw(Projectile, texture, ref lightColor);
        
        var effectPath = TextureRegistry.TerrariaProjectile(ProjectileID.Muramasa);
        var effectTexture = ModContent.Request<Texture2D>(effectPath).Value;

        return false;
    }

    public override bool? Colliding(Rectangle projHitbox, Rectangle targetHitbox)
    {
        var point = 0f;

        // Offset from center to top right
        var centerOffset = projHitbox.Size() / 2;
        centerOffset.Y *= -1;
        centerOffset = centerOffset.RotatedBy(Projectile.rotation);

        return Collision.CheckAABBvLineCollision(targetHitbox.TopLeft(), targetHitbox.Size(),
            Projectile.Center + centerOffset, Projectile.Center - centerOffset, 15, ref point
        );
    }
}