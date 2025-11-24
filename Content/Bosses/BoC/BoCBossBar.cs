using System;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using Terraria;
using Terraria.DataStructures;
using Terraria.GameContent;
using Terraria.GameContent.UI.BigProgressBar;
using Terraria.ID;
using Terraria.ModLoader;

namespace AcidicBosses.Content.Bosses.BoC;

public class BoCBossBar : ModBossBar
{
    public float MaxCreepers { get; set; } = 0;
    
    private int bossHeadIndex = -1;

    public override Asset<Texture2D> GetIconTexture(ref Rectangle? iconFrame)
    {
        return TextureAssets.NpcHeadBoss[NPCID.Sets.BossHeadTextures[NPCID.BrainofCthulhu]];
    }

    public override bool? ModifyInfo(ref BigProgressBarInfo info, ref float life, ref float lifeMax, ref float shield, ref float shieldMax)/* tModPorter Note: life and shield current and max values are now separate to allow for hp/shield number text draw */
    {
        var npc = Main.npc[info.npcIndexToAimAt];
        if(!npc.active) return false;
        
        bossHeadIndex = npc.GetBossHeadTextureIndex();
        
        life = npc.life;
        lifeMax = npc.lifeMax;
        
        // Show creepers as the shield
        shieldMax = MaxCreepers;
        if(shieldMax != 0)
            shield = Main.npc.Count(n => n.active && n.type == NPCID.Creeper);

        return true;
    }

    public override bool PreDraw(SpriteBatch spriteBatch, NPC npc, ref BossBarDrawParams drawParams)
    {
        // Adapted from BossBarLoader.DrawFancyBar_TML()
        // Mostly the same as normal, but changes icon rendering

		var (
			barTexture,
			barCenter,
			iconTexture,
			iconFrame,
			iconColor,
			life,
			lifeMax,
			shield,
			shieldMax,
			iconScale,
			showText,
			textOffset
		) = drawParams;

		Point barSize = new Point(456, 22); //Size of the bar
		Point topLeftOffset = new Point(32, 24); //Where the top left of the bar starts
		int frameCount = 6;

		Rectangle bgFrame = barTexture.Frame(verticalFrames: frameCount, frameY: 3);
		Color bgColor = Color.White * 0.2f;

		int scale = (int)(barSize.X * life / lifeMax);
		scale -= scale % 2;
		Rectangle barFrame = barTexture.Frame(verticalFrames: frameCount, frameY: 2);
		barFrame.X += topLeftOffset.X;
		barFrame.Y += topLeftOffset.Y;
		barFrame.Width = 2;
		barFrame.Height = barSize.Y;

		Rectangle tipFrame = barTexture.Frame(verticalFrames: frameCount, frameY: 1);
		tipFrame.X += topLeftOffset.X;
		tipFrame.Y += topLeftOffset.Y;
		tipFrame.Width = 2;
		tipFrame.Height = barSize.Y;

		int shieldScale = (int)(barSize.X * shield / shieldMax);
		shieldScale -= shieldScale % 2;

		Rectangle barShieldFrame = barTexture.Frame(verticalFrames: frameCount, frameY: 5);
		barShieldFrame.X += topLeftOffset.X;
		barShieldFrame.Y += topLeftOffset.Y;
		barShieldFrame.Width = 2;
		barShieldFrame.Height = barSize.Y;

		Rectangle tipShieldFrame = barTexture.Frame(verticalFrames: frameCount, frameY: 4);
		tipShieldFrame.X += topLeftOffset.X;
		tipShieldFrame.Y += topLeftOffset.Y;
		tipShieldFrame.Width = 2;
		tipShieldFrame.Height = barSize.Y;

		Rectangle barPosition = Utils.CenteredRectangle(barCenter, barSize.ToVector2());
		Vector2 barTopLeft = barPosition.TopLeft();
		Vector2 topLeft = barTopLeft - topLeftOffset.ToVector2();

		// Background
		spriteBatch.Draw(barTexture, topLeft, bgFrame, bgColor, 0f, Vector2.Zero, 1f, SpriteEffects.None, 0f);

		// Bar itself
		Vector2 stretchScale = new Vector2(scale / barFrame.Width, 1f);
		Color barColor = Color.White;
		spriteBatch.Draw(barTexture, barTopLeft, barFrame, barColor, 0f, Vector2.Zero, stretchScale, SpriteEffects.None, 0f);

		// Tip
		spriteBatch.Draw(barTexture, barTopLeft + new Vector2(scale - 2, 0f), tipFrame, barColor, 0f, Vector2.Zero, 1f, SpriteEffects.None, 0f);

		// Bar itself (shield)
		if (shield > 0f) {
			stretchScale = new Vector2(shieldScale / barFrame.Width, 1f);
			spriteBatch.Draw(barTexture, barTopLeft, barShieldFrame, barColor, 0f, Vector2.Zero, stretchScale, SpriteEffects.None, 0f);

			// Tip
			spriteBatch.Draw(barTexture, barTopLeft + new Vector2(shieldScale - 2, 0f), tipShieldFrame, barColor, 0f, Vector2.Zero, 1f, SpriteEffects.None, 0f);
		}

		// Frame
		Rectangle frameFrame = barTexture.Frame(verticalFrames: frameCount, frameY: 0);
		spriteBatch.Draw(barTexture, topLeft, frameFrame, Color.White, 0f, Vector2.Zero, 1f, SpriteEffects.None, 0f);

		// Icon
		// THIS HAS BEEN MODIFIED FOR THIS BAR
		var brain = npc.GetGlobalNPC<BoC>();
		if (!brain.ShowPhantoms)
		{
			// Use normal rendering
			var iconOffset = new Vector2(4f, 20f);
			var iconSize = new Vector2(26f, 28f);
			var iconPos = iconOffset + iconSize / 2f;
			spriteBatch.Draw(iconTexture, topLeft + iconPos, iconFrame, iconColor, 0f, iconFrame.Size() / 2f, iconScale, SpriteEffects.None, 0f);
		}
		else
		{
			// Fun
			var iconOffset = new Vector2(4f, 20f);
			var iconSize = new Vector2(26f, 28f);
			var iconPos = iconOffset + iconSize / 2f;
			
			// Draw four rotating icons
			var cycle = (float)Main.timeForVisualEffects % 60 / 60f;
			var angle = cycle * MathHelper.TwoPi;
			
			for (var i = 0; i < 4; i++)
			{
				var offset = (angle + MathHelper.PiOver2 * i).ToRotationVector2() * 4f;
				
				spriteBatch.Draw(
					iconTexture,
					topLeft + iconPos + offset,
					iconFrame,
					iconColor * 0.5f,
					0f,
					iconFrame.Size() / 2f,
					iconScale,
					SpriteEffects.None,
					0f
				);
			}
		}
		
		// Text
		if (BigProgressBarSystem.ShowText && showText) {
			if (shield > 0f)
				BigProgressBarHelper.DrawHealthText(spriteBatch, barPosition, textOffset, shield, shieldMax);
			else
				BigProgressBarHelper.DrawHealthText(spriteBatch, barPosition, textOffset, life, lifeMax);
		}

		return false;
    }
}