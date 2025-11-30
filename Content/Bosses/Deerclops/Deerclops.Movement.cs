using System;
using AcidicBosses.Core.Animation;
using Luminance.Common.Easings;
using Luminance.Common.Utilities;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.Audio;
using Terraria.ID;

namespace AcidicBosses.Content.Bosses.Deerclops;

public partial class Deerclops
{
    private AcidAnimation? jumpAnimation;
    private AcidAnimation? landAnimation;

    private bool prevGrounded = false;
    private bool grounded = false;
    private bool jumping = false;
    private int framesStuck = 0;
    
    private void ApplyCollision()
    {
        prevGrounded = grounded;
        
        // Step up blocks
        Collision.StepUp(
            ref Npc.position,
            ref Npc.velocity,
            Npc.width,
            TileCollisionHeight,
            ref Npc.stepSpeed,
            ref Npc.gfxOffY
        );
        
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

        var preCollision = Npc.velocity;
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
        
        if (preCollision.X != Npc.velocity.X)
            Npc.collideX = true;

        if (preCollision.Y != Npc.velocity.Y)
        {
            grounded = true;
            Npc.collideY = true;
        }

        if (Npc.velocity.Y > 0) grounded = false;
    }

    private void ApplyGravity()
    {
        // Apply gravity
        Npc.velocity.Y += Npc.gravity;
        if (Npc.velocity.Y > Npc.maxFallSpeed)
            Npc.velocity.Y = Npc.maxFallSpeed;
    }

    private bool ApproachPlayer()
    {
        if (jumping || (framesStuck > 30 && grounded))
        {
            var jumpDone = JumpToPlayer();
            if (!jumpDone) return false;
            
            framesStuck = 0;
        }

        if (!grounded) return false;
        
        var xDist = MathF.Abs(Npc.Center.X - TargetPlayer.Center.X);
        var fullDist = Npc.Distance(TargetPlayer.Center);
        if (xDist < 100 && fullDist < 150)
        {
            framesStuck = 0;
            return true;
        }
        
        if (Npc.collideX || fullDist > 500) framesStuck++;
        else framesStuck = 0;
        
        if (MathF.Abs(Npc.Center.X - TargetPlayer.Center.X) > 100)
        {
            Npc.velocity.X = 3f * Npc.HorizontalDirectionTo(TargetPlayer.Center);
        }
        
        return false;
    }

    private AcidAnimation PrepareJumpAnimation()
    {
        var anim = new AcidAnimation();
        
        const int jumpLength = 60;
        
        anim.AddInstantEvent(0, () =>
        {
            useCollision = false;
            useGravity = false;
            grounded = false;
            jumping = true;
            overrideAnimationFrame = JumpFacingSide;

            SoundEngine.PlaySound(SoundID.DeerclopsScream, BottomPos);
            
            var xTile = Npc.position.ToTileCoordinates().X;
            var yTile = (Npc.Top + new Vector2(0, TileCollisionHeight + 16)).ToTileCoordinates().Y;
            for (var x = xTile; x < Npc.width / 16f + xTile; x++)
            {
                var ground = new Point(x, yTile);
                WorldGen.KillTile(ground.X, ground.Y, true, true);
            }

            var goalPos = TargetPlayer.Bottom + (Vector2.UnitX * TargetPlayer.velocity.X * jumpLength);
            var relativePos = goalPos - BottomPos;

            var velX = relativePos.X / jumpLength;
            var velY = (-relativePos.Y / jumpLength) + (0.5f * Npc.gravity * jumpLength);

            var peakTime = velY / Npc.gravity;
            var peakProgress = peakTime / jumpLength;
            var peakHeight = (velY * velY) / (2f * Npc.gravity);

            if (peakProgress <= 0f)
            {
                peakProgress = 0f;
                peakHeight = 0f;
            }

            if (peakProgress >= 1f)
            {
                peakProgress = 1f;
                peakHeight = -relativePos.Y;
            }
            
            anim.Data.Set("startPos", BottomPos);
            anim.Data.Set("targetPos", goalPos);
            anim.Data.Set("velX", velX);
            anim.Data.Set("movementCurve", new PiecewiseCurve()
                .Add(EasingCurves.Quadratic, EasingType.Out, peakHeight, peakProgress)
                .Add(EasingCurves.Quadratic, EasingType.In, -relativePos.Y, 1f));
        });
        
        var jumpEvent = anim.AddSequencedEvent(jumpLength, (progress, frame) =>
        {
            var startPos = anim.Data.Get<Vector2>("startPos");
            var targetPos = anim.Data.Get<Vector2>("targetPos");
            var jumpCurve = anim.Data.Get<PiecewiseCurve>("movementCurve");

            var x = MathHelper.Lerp(startPos.X, targetPos.X, progress);
            var y = startPos.Y - jumpCurve.Evaluate(progress);

            BottomPos = new Vector2(x, y);
        });
        
        anim.AddInstantEvent(jumpEvent.EndTime, () =>
        {
            var jumpCurve = anim.Data.Get<PiecewiseCurve>("movementCurve");
            var velX = anim.Data.Get<float>("velX");
            var velY = jumpCurve.Evaluate(1f) - jumpCurve.Evaluate((float)(jumpLength - 1) / jumpLength);
            Npc.velocity = new Vector2(velX, -velY);
            
            useCollision = true;
            useGravity = true;
            jumping = false;
        });

        return anim;
    }

    private bool JumpToPlayer()
    {
        jumpAnimation ??= PrepareJumpAnimation();
        if (jumpAnimation.RunAnimation())
        {
            if (grounded && !prevGrounded)
            {
                Npc.velocity = Vector2.Zero;
                overrideAnimationFrame = null;
                jumpAnimation.Reset();
                return true;
            }
        }
        
        return false;
    }

    public override bool? CanFallThroughPlatforms(NPC npc)
    {
        return TargetPlayer.Center.Y > BottomPos.Y;
    }
}