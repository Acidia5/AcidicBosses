using System;
using AcidicBosses.Common.Configs;
using Luminance.Common.Utilities;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;

namespace AcidicBosses.Content.Bosses.WoF;

/// <summary>
/// This is for managing hooks that are outside the control of a GlobalNPC
/// I need to use this to untangle the spaghetti that is the WoF code.
/// This doesn't work perfectly, but it works good enough
/// </summary>
public class WoFDrawSystem : ModSystem
{
	private static WoF? Wall => WoF.GetInstance();

	private static Asset<Texture2D> wofBack;
    
    public override void Load()
    {
        On_Main.DrawWoF += DrawWof;
        
        wofBack = ModContent.Request<Texture2D>("AcidicBosses/Content/Bosses/WoF/WoFBack");
    }

    public override void Unload()
    {
        On_Main.DrawWoF -= DrawWof;
    }
    
    private void DrawWof(On_Main.orig_DrawWoF orig, Main self)
    {
	    if (!BossToggleConfig.Get().EnableWallOfFlesh || AcidicBosses.DisableReworks())
	    {
		    orig(self);
		    return;
	    }

	    if (Wall == null) return;
	    
	    DrawBack();
	    
	    for (int i = 0; i < 255; i++) {
		    if (Main.player[i].active && Main.player[i].tongued && !Main.player[i].dead)
			    DrawWOFTongueToPlayer(i);
	    }
	    
	    DrawWall(Wall.LeftWallRect, SpriteEffects.FlipHorizontally);
	    DrawWall(Wall.RightWallRect, SpriteEffects.None);
    }

    private void DrawBack()
    {
	    var tileHeight = wofBack.Height();

	    var topLeftScreenPos = Wall.LeftWallRect.Top - Main.screenPosition.Y;
	    var topRightScreenPos = Wall.RightWallRect.Top - Main.screenPosition.Y;
	    var bottomScreenPos = Main.screenHeight;

	    var leftScreenPos = Wall.LeftWallRect.Center.X - Main.screenPosition.X;
	    var rightScreenPos = Wall.RightWallRect.Center.X - Main.screenPosition.X;
	    
	    // Fill to top of screen
	    if (topLeftScreenPos > Main.screenHeight) return;
	    if (topLeftScreenPos > 0f)
	    {
		    var segmentsNeededToFill = (int)(topLeftScreenPos / tileHeight) + 1;
		    topLeftScreenPos -= segmentsNeededToFill * tileHeight;
	    }
	    
	    if (topRightScreenPos > Main.screenHeight) return;
	    if (topRightScreenPos > 0f)
	    {
		    var segmentsNeededToFill = (int)(topRightScreenPos / tileHeight) + 1;
		    topRightScreenPos -= segmentsNeededToFill * tileHeight;
	    }

	    for (var x = leftScreenPos; x >= -tileHeight; x -= tileHeight)
	    {
		    for (var y = topLeftScreenPos; y < bottomScreenPos; y += tileHeight)
		    {
			    var pos = new Vector2(x, y);
			    var frame = wofBack.Frame();
			    var origin = Vector2.Zero;
			    
			    var worldCenter = pos + Main.screenPosition + frame.Size() / 2f;
			    var tilePos = worldCenter.ToTileCoordinates();
			    var color = Lighting.GetColor(tilePos);
			    
			    Main.spriteBatch.Draw(
				    wofBack.Value,
				    pos,
				    frame,
				    (color * 0.5f) with { A = 255 },
				    0f,
				    origin,
				    1f,
				    SpriteEffects.None,
				    0f
			    );
		    }
	    }
	    
	    for (var x = rightScreenPos; x < Main.screenWidth; x += tileHeight)
	    {
		    for (var y = topRightScreenPos; y < bottomScreenPos; y += tileHeight)
		    {
			    var pos = new Vector2(x, y);
			    var frame = wofBack.Frame();
			    var origin = Vector2.Zero;
			    
			    var worldCenter = pos + Main.screenPosition + frame.Size() / 2f;
			    var tilePos = worldCenter.ToTileCoordinates();
			    var color = Lighting.GetColor(tilePos);
			    
			    Main.spriteBatch.Draw(
				    wofBack.Value,
				    pos,
				    frame,
				    (color * 0.5f) with { A = 255 },
				    0f,
				    origin,
				    1f,
				    SpriteEffects.None,
				    0f
			    );
		    }
	    }
    }

    private void DrawWall(Rectangle wallRect, SpriteEffects effects)
    {
	    // Each segment is a loop of the texture
	    var segmentHeight = TextureAssets.Wof.Height() / 3;
	    
	    // Offset rect to screen space
	    wallRect.Offset((-Main.screenPosition).ToPoint());
	    
	    var topScreenPos = wallRect.Top;
	    var bottomScreenPos = Main.screenHeight;
	    
	    // Fill to top of screen
	    if (topScreenPos > Main.screenHeight) return;
	    if (topScreenPos > 0f)
	    {
		    var segmentsNeededToFill = (int)(topScreenPos / segmentHeight) + 1;
		    topScreenPos -= segmentsNeededToFill * segmentHeight;
	    }

	    // Reset frame
	    if (Main.wofDrawFrameIndex >= 18) Main.wofDrawFrameIndex = 0;
	    var frameY = Main.wofDrawFrameIndex / 6 * segmentHeight;
	    
	    // Draw Wall segments
	    for (var segmentY = topScreenPos; segmentY < bottomScreenPos; segmentY += segmentHeight)
	    {
		    // Break each segment into slices of 16 pixels for lighting
		    for (var sliceY = 0; sliceY < segmentHeight; sliceY += 16)
		    {
			    var pos = new Vector2(wallRect.Center.X, segmentY + sliceY);
			    var frame = new Rectangle(0, frameY + sliceY, TextureAssets.Wof.Width(), 16);
			    var origin = new Vector2(frame.Width / 2f, 0f);
			    
			    var worldCenter = pos + Main.screenPosition;
			    var tilePos = worldCenter.ToTileCoordinates();
			    var color = Lighting.GetColor(tilePos);
			    
			    Main.spriteBatch.Draw(
				    TextureAssets.Wof.Value,
				    pos,
				    frame,
				    color,
				    0f,
				    origin,
				    1f,
				    effects,
				    0f
				);
		    }
	    }
    }
    
    private void DrawWOFTongueToPlayer(int i)
    {
	    var player = Main.player[i];
	    
	    // Target the closer wall
	    var leftWallPos = Wall.LeftWallRect.Right();
	    var rightWallPos = Wall.RightWallRect.Left();
            
	    var distRightWall = player.Center.Distance(rightWallPos);
	    var distLeftWall = player.Center.Distance(leftWallPos);
	    
	    // Choose the closer wall as the target
            
	    var target = new Vector2(0);

	    if (distRightWall < distLeftWall) target = rightWallPos;
	    else target = leftWallPos;

	    var chainTex = TextureAssets.Chain12;
	    var distToTarget = player.Center.Distance(target);
	    var dirToTarget = player.Center.DirectionTo(target);
	    for (var d = 0; d < distToTarget; d += chainTex.Height())
	    {
		    var pos = player.Center + dirToTarget * d;
		    var frame = chainTex.Frame();
		    var color = Lighting.GetColor(pos.ToTileCoordinates());
		    var origin = frame.Top();
		    
		    Main.spriteBatch.Draw(
			    chainTex.Value,
			    pos - Main.screenPosition,
			    frame,
			    color,
			    dirToTarget.ToRotation() + MathHelper.PiOver2,
			    origin,
			    1f,
			    SpriteEffects.None,
			    0f
			);
	    }
    }
}