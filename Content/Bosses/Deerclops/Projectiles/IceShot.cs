using AcidicBosses.Common.Textures;
using AcidicBosses.Content.Particles;
using AcidicBosses.Helpers;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.Audio;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;

namespace AcidicBosses.Content.Bosses.Deerclops.Projectiles;

public class IceShot : ModProjectile
{
    private bool firstFrame = true;
    
    public override void SetStaticDefaults()
    {
        Main.projFrames[Type] = 3;
    }
    
    public override void SetDefaults()
    {
        Projectile.hostile = true;
        Projectile.width = 16;
        Projectile.height = 16;
        Projectile.tileCollide = true;

        DrawOffsetX = -8;
        DrawOriginOffsetY = -60;
    }

    public override void AI()
    {
        if (++Projectile.frameCounter >= 5)
        {
            Projectile.frameCounter = 0;
            Projectile.frame = ++Projectile.frame % Main.projFrames[Projectile.type];
        }
        
        Projectile.rotation = Projectile.velocity.ToRotation() - MathHelper.PiOver2;

        Dust.NewDust(Projectile.position, Projectile.width, Projectile.height, DustID.IceTorch);
        
        if (firstFrame)
        {
            firstFrame = false;
            
            SoundEngine.PlaySound(SoundID.DeerclopsIceAttack, Projectile.position);
            
            for (var i = 0; i < 25; i++)
            {
                var offset = Main.rand.NextVector2Circular(16, 16);
                var d0 = Dust.NewDustPerfect(Projectile.Center, DustID.SnowflakeIce);
                d0.scale = 1.5f;
                d0.noGravity = true;
                d0.velocity = offset;
            }

            new SharpTearParticle(
                Projectile.Center,
                Vector2.Zero,
                Projectile.rotation,
                Color.White,
                30
            )
            {
                GlowColor = Color.LightBlue,
                FadeColor = true,
                Shrink = true
            }.Spawn();
        }
    }

    public override bool OnTileCollide(Vector2 oldVelocity)
    {
        for (var i = 0; i < 25; i++)
        {
            var vel = Main.rand.NextVector2Circular(5f, 5f);
            Dust.NewDust(Projectile.position, Projectile.width, Projectile.height, DustID.Ice, vel.X, vel.Y);
        }

        SoundEngine.PlaySound(SoundID.DeerclopsIceAttack, Projectile.position);
        
        return true;
    }
}