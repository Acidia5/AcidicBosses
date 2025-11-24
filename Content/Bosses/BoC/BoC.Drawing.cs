using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;

namespace AcidicBosses.Content.Bosses.BoC;

public partial class BoC
{
    private bool isBrainOpen = false;
    public bool ShowPhantoms = false;
    private Vector2 scale = Vector2.One;
    private float phantomOpacity = 0f;

    private void DrawAI()
    {
        if (ShowPhantoms) phantomOpacity = MathHelper.Lerp(phantomOpacity, 1f, 0.1f);
        else phantomOpacity = MathHelper.Lerp(phantomOpacity, 0f, 0.1f);
    }
    
    public override bool AcidicDraw(NPC npc, SpriteBatch spriteBatch, Vector2 screenPos, Color lightColor)
    {
        var texAsset = TextureAssets.Npc[npc.type];
        var drawPos = npc.Center - Main.screenPosition;
        var brainTexture = TextureAssets.Npc[npc.type].Value;
        var origin = npc.frame.Size() * 0.5f;

        // I have to do this workaround to offset the frame when the brain is open.
        // This game's code is so spaghetti that it won't go past 4 frames and I have no clue why.
        var frame = npc.frame;

        // For fading on teleporting
        lightColor *= npc.Opacity;

        // Phantoms
        if (phantomOpacity > 0)
        {
            for (var i = 0; i < 4; i++)
            {
                var phantomPos = new Vector2();
                var offsetX = Math.Abs(npc.Center.X - Main.LocalPlayer.Center.X);
                var offsetY = Math.Abs(npc.Center.Y - Main.LocalPlayer.Center.Y);

                if (i is 0 or 2) phantomPos.X = Main.LocalPlayer.Center.X + offsetX;
                else phantomPos.X = Main.LocalPlayer.Center.X - offsetX;

                if (i is 0 or 1) phantomPos.Y = Main.LocalPlayer.Center.Y + offsetY;
                else phantomPos.Y = Main.LocalPlayer.Center.Y - offsetY;

                var phantomColor = Lighting.GetColor(phantomPos.ToTileCoordinates()) * 0.5f * npc.Opacity * phantomOpacity;

                spriteBatch.Draw(
                    brainTexture, phantomPos - Main.screenPosition,
                    frame, phantomColor,
                    npc.rotation, origin, scale,
                    SpriteEffects.None, 0f);
            }
        }

        spriteBatch.Draw(
            brainTexture, drawPos,
            frame, lightColor,
            npc.rotation, origin, scale,
            SpriteEffects.None, 0f);

        return false;
    }

    public override void AcidFindFrame(NPC npc, int frameHeight)
    {
        FrameCounter++;
        switch (FrameCounter)
        {
            case < 6.0:
                Frame.Y = 0;
                break;
            case < 12.0:
                Frame.Y = frameHeight;
                break;
            case < 18.0:
                Frame.Y = frameHeight * 2;
                break;
            case < 24.0:
                Frame.Y = frameHeight * 3;
                break;
            default:
                FrameCounter = 0.0;
                Frame.Y = 0;
                break;
        }
        
        if (isBrainOpen) Frame.Y += frameHeight * 4;
    }

    public override void BossHeadSlot(NPC npc, ref int index)
    {
        if (ShowPhantoms)
        {
            index = -1;
            return;
        }
        
        index = NPCID.Sets.BossHeadTextures[NPCID.BrainofCthulhu];
    }
}