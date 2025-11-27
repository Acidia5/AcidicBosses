using System;
using Luminance.Common.Easings;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;

namespace AcidicBosses.Content.Bosses.Deerclops;

public partial class Deerclops
{
    private void ApplyGravityAndCollision()
    {
        // Step up blocks
        Collision.StepUp(
            ref Npc.position,
            ref Npc.velocity,
            Npc.width,
            TileCollisionHeight,
            ref Npc.stepSpeed,
            ref Npc.gfxOffY
        );
        
        // Apply gravity
        Npc.velocity.Y += Npc.gravity;
        if (Npc.velocity.Y > Npc.maxFallSpeed)
            Npc.velocity.Y = Npc.maxFallSpeed;
        
        // Walk down slopes
        var downSlope = Collision.WalkDownSlope(
            Npc.position,
            Npc.velocity,
            Npc.width,
            TileCollisionHeight,
            Npc.gravity
        );
        Npc.position = downSlope.XY();
        Npc.velocity = downSlope.ZW();

        Npc.collideX = false;
        Npc.collideY = false;
        
        // Do tile collision
        Npc.velocity = Collision.TileCollision(
            Npc.position,
            Npc.velocity,
            Npc.width,
            TileCollisionHeight,
            CanFallThroughPlatforms(Npc) ?? false,
            CanFallThroughPlatforms(Npc) ?? false
        );
        
        if (Collision.up) Npc.velocity.Y = 0.01f;
        
        if (Npc.oldVelocity.X != Npc.velocity.X)
            Npc.collideX = true;

        if (Npc.oldVelocity.Y != Npc.velocity.Y)
            Npc.collideY = true;
        
        // Deal with slopes
        Npc.stairFall = CanFallThroughPlatforms(Npc) ?? false;
        var collideResult = Collision.SlopeCollision(
            Npc.position,
            Npc.velocity,
            Npc.width,
            TileCollisionHeight,
            Npc.gravity,
            Npc.stairFall
        );
        
        if (Collision.stairFall)
            Npc.stairFall = true;
        else if (CanFallThroughPlatforms(Npc) ?? false)
            Npc.stairFall = false;
        
        if (Collision.stair && Math.Abs(collideResult.Y - Npc.position.Y) > 8f) {
            Npc.gfxOffY -= collideResult.Y - Npc.position.Y;
            Npc.stepSpeed = 2f;
        }

        Npc.position = collideResult.XY();
        Npc.velocity = collideResult.ZW();
    }
}