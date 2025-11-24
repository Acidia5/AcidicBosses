using AcidicBosses.Common.Textures;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.ModLoader;

namespace AcidicBosses.Content.Bosses.BoC;

public class BloodShot : ModProjectile
{
    public override string Texture => TextureRegistry.InvisPath;
    
    public override void SetDefaults()
    {
        Projectile.width = 10;
        Projectile.height = 10;
        Projectile.penetrate = -1;
        Projectile.hostile = true;
    }

    public override void AI()
    {
        // No complex ai, just spawn dust
        // Yoinked from ichor splash code

        if (Projectile.ai[0] == 0)
        {
            SoundEngine.PlaySound(SoundID.Item171, Projectile.Center);
        }
        
        Projectile.ai[0] += 0.6f;
        if (Projectile.ai[0] > 500f)
        {
            Projectile.Kill();
        }
        
        Lighting.AddLight(Projectile.position, TorchID.Crimson);
        
        for (var i = 0; i < 2; i++)
        {
            if (!Main.rand.NextBool(3))
            {
                var dustId = Dust.NewDust(
                    Projectile.position,
                    Projectile.width,
                    Projectile.height,
                    DustID.Blood,
                    0f, 0f,
                    100,
                    Scale: 2f
                );
                Main.dust[dustId].position = (Main.dust[dustId].position + Projectile.Center) / 2f;
                Main.dust[dustId].noGravity = true;
                var dust = Main.dust[dustId];
                dust.velocity *= 0.1f;
                if (i == 1)
                {
                    dust.position += Projectile.velocity / 2f;
                }
                
                var num840 = (800f - Projectile.ai[0]) / 800f;
                dust.scale *= num840 + 0.1f;
            }
        }
    }

    public override void OnKill(int timeLeft)
    {
        SoundEngine.PlaySound(SoundID.Item171, Projectile.Center);
        
        for (var i = 0; i < 50; i++)
        {
            var splashDir = Main.rand.NextVector2Circular(5f, 5f);
            Dust.NewDust(
                Projectile.position,
                Projectile.width,
                Projectile.height,
                DustID.Blood,
                splashDir.X, splashDir.Y,
                100,
                Scale: 2f
            );
        }
    }
}