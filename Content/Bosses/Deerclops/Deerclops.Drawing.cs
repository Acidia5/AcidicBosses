using System;
using Luminance.Common.StateMachines;
using Luminance.Common.Utilities;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Graphics;
using Terraria;
using Terraria.GameContent;

namespace AcidicBosses.Content.Bosses.Deerclops;

public partial class Deerclops
{
    private void DrawUpdate()
    {
        if (Npc.velocity.X < 0) Npc.direction = -1;
        if (Npc.velocity.X > 0) Npc.direction = 1;
        Npc.spriteDirection = Npc.direction;
    }
    
    public override bool AcidicDraw(NPC npc, SpriteBatch spriteBatch, Vector2 screenPos, Color lightColor)
    {
        var texAsset = TextureAssets.Npc[npc.type];
        
        npc.frame = texAsset.Frame(
            5, 5,
            animationFrame.X, animationFrame.Y,
            2, 2
        );
        
        var drawPos = npc.Bottom - Main.screenPosition;
        drawPos.Y += Npc.gfxOffY;
        drawPos.Y += GroundOffset;
        
        var origin = npc.frame.Size() * new Vector2(0.5f, 1f);
        
        spriteBatch.Draw(
            texAsset.Value,
            drawPos,
            npc.frame,
            lightColor,
            npc.rotation,
            origin,
            npc.scale,
            npc.spriteDirection.ToSpriteDirection(),
            0f
        );
        
        return false;
    }
}