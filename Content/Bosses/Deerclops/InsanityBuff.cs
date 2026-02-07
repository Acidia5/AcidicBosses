using System;
using AcidicBosses.Helpers;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using Terraria;
using Terraria.DataStructures;
using Terraria.GameContent;
using Terraria.GameContent.Events;
using Terraria.ID;
using Terraria.ModLoader;

namespace AcidicBosses.Content.Bosses.Deerclops;

public class InsanityBuff : ModBuff
{
    public override void SetStaticDefaults()
    {
        Main.buffNoTimeDisplay[Type] = true;
        Main.debuff[Type] = true;
    }

    public override void Update(Player player, ref int buffIndex)
    {
        player.GetModPlayer<InsanityModPlayer>().IsInsane = true;
        
        if (NPC.deerclopsBoss < 0 || NPC.deerclopsBoss >= Main.maxNPCs) return;
        var deerclops = Main.npc[NPC.deerclopsBoss];
        if (deerclops == null || !deerclops.active) return;

        // Dark smoke around edge
        for (var i = 0; i < 3; i++)
        {
            var dustPos = Main.rand.NextVector2Unit() 
                          * Main.rand.NextFloat(Deerclops.DarknessRadius - 100f, Deerclops.DarknessRadius)
                          + deerclops.Center;
            Dust.NewDust(dustPos, 0, 0, DustID.Smoke, newColor: Color.Black, Scale: 1.5f);
        }

        if (player.Distance(deerclops.Center) > Deerclops.DarknessRadius)
        {
            // Type 17 is darkness damage
            player.Hurt(
                PlayerDeathReason.ByOther(17),
                30,
                0,
                dodgeable: false,
                knockback: 0f
            );
        }
    }
}

public class InsanityModPlayer : ModPlayer
{
    public bool IsInsane = false;

    public override void ResetEffects()
    {
        IsInsane = false;
    }
}

public class InsanityOverlaySystem : ModSystem
{
    private float overlayOpacity = 0f;
    private Vector2 lastDeerPos = Vector2.Zero;
    private const int Buffer = 32;

    public static Asset<Texture2D> InsanityOverlay;
    public static Asset<Texture2D> DarknessOverlay;

    public override void Load()
    {
        InsanityOverlay = ModContent.Request<Texture2D>("AcidicBosses/Assets/Textures/InsanityOverlay");
        DarknessOverlay = ModContent.Request<Texture2D>("AcidicBosses/Assets/Textures/DstDarkness");
        On_ScreenDarkness.DrawFront += DrawOverlay;
    }

    public override void Unload()
    {
        On_ScreenDarkness.DrawFront -= DrawOverlay;
    }

    private void DrawOverlay(On_ScreenDarkness.orig_DrawFront orig, SpriteBatch spritebatch)
    {
        orig.Invoke(spritebatch);

        if (overlayOpacity <= 0f) return;
        DrawDarkness(spritebatch);
        DrawTendrils(spritebatch);
    }

    private void DrawDarkness(SpriteBatch spritebatch)
    {
        var tex = DarknessOverlay.Value;
        var drawRect = new Rectangle();
        drawRect.Offset((lastDeerPos - Main.screenPosition).ToPoint());
        drawRect.Inflate((int)Deerclops.DarknessRadius, (int)Deerclops.DarknessRadius);
        
        spritebatch.Draw(tex, drawRect, Color.White * overlayOpacity);

        // Left Darknexx
        if (drawRect.Left > 0)
        {
            var rect = new Rectangle(
                0, 0,
                drawRect.Left, Main.screenHeight
            );
            
            spritebatch.Draw(TextureAssets.MagicPixel.Value, rect, Color.Black * overlayOpacity);
        }

        // Right Darkness
        if (drawRect.Right < Main.screenWidth)
        {
            var rect = new Rectangle(
                drawRect.Right, 0,
                Main.screenWidth - drawRect.Right, Main.screenHeight
            );
            
            spritebatch.Draw(TextureAssets.MagicPixel.Value, rect, Color.Black * overlayOpacity);
        }

        // Top Darkness
        if (drawRect.Top > 0)
        {
            var rect = new Rectangle(
                drawRect.Left, 0,
                drawRect.Width, drawRect.Top
            );
            
            spritebatch.Draw(TextureAssets.MagicPixel.Value, rect, Color.Black * overlayOpacity);
        }

        // Bottom Darkness
        if (drawRect.Bottom < Main.screenHeight)
        {
            var rect = new Rectangle(
                drawRect.Left, drawRect.Bottom,
                drawRect.Width, Main.screenHeight - drawRect.Bottom
            );
            
            spritebatch.Draw(TextureAssets.MagicPixel.Value, rect, Color.Black * overlayOpacity);
        }
    }

    private void DrawTendrils(SpriteBatch spritebatch)
    {
        // This isn't fully accurate to Don't Starve, but I don't care.
        var tex = InsanityOverlay.Value;
        var screenRect = new Rectangle(0, 0, Main.screenWidth, Main.screenHeight);
        
        var scale = (int)MathHelper.Lerp(Buffer + 256, Buffer, EasingHelper.QuadOut(overlayOpacity));
        screenRect.Inflate(scale, scale);
        
        var offset = Main.rand.NextVector2Unit() * 4 * overlayOpacity;
        screenRect.Offset(offset.ToPoint());
        spritebatch.Draw(tex, screenRect, Color.White * overlayOpacity * 0.5f);
    }

    public override void PostUpdatePlayers()
    {
        if (!AcidUtils.IsClient()) return;

        var insane = Main.LocalPlayer.GetModPlayer<InsanityModPlayer>().IsInsane;
        var elapsedTime = (float)Main.gameTimeCache.ElapsedGameTime.TotalSeconds;

        if (insane)
        {
            overlayOpacity = MathF.Min(1f, overlayOpacity + elapsedTime * 2f);
        }
        else
        {
            overlayOpacity = MathF.Max(0f, overlayOpacity - elapsedTime * 2f);
        }
        
        if (NPC.deerclopsBoss < 0 || NPC.deerclopsBoss >= Main.maxNPCs) return;
        var deerclops = Main.npc[NPC.deerclopsBoss];
        if (deerclops == null || !deerclops.active) return;
        lastDeerPos = deerclops.Center;
    }
}