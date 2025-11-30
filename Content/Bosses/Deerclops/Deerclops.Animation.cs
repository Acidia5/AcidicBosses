using System;
using Luminance.Common.StateMachines;
using Luminance.Common.Utilities;
using Luminance.Core.Graphics;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.Audio;
using Terraria.ID;

namespace AcidicBosses.Content.Bosses.Deerclops;

public partial class Deerclops
{
    private Point animationFrame = Point.Zero;

    private Animations? playingAnimation = null;

    private int animationProgress = 0;

    private int slamHoldRaisedTime = 0;
    private int slamHoldLoweredTime = 0;
    private int flingHoldLoweredTime = 0;
    private int flingHoldRaisedTime = 0;
    private int roarHoldCloseTime = 0;
    private int roarHoldOpenTime = 0;

    private Point? overrideAnimationFrame = null;

    private bool doneSound = false;
    
    public override bool AcidFindFrame(NPC npc, int frameHeight)
    {
        if (overrideAnimationFrame != null)
        {
            animationFrame = overrideAnimationFrame.Value;
            return false;
        }
        
        switch (playingAnimation)
        {
            case Animations.Slam:
                ManageSlamAnimation();
                break;
            case Animations.Fling:
                ManageFlingAnimation();
                break;
            case Animations.Roar:
                ManageRoarAnimation();
                break;
            case null:
                ManageWalkAnimation();
                break;
        }
        
        return false;
    }

    private void PlaySlamAnimation(int handsRaisedFrames, int handsLoweredFrames)
    {
        playingAnimation = Animations.Slam;
        slamHoldRaisedTime = handsRaisedFrames;
        slamHoldLoweredTime = handsLoweredFrames;
    }
    
    private void PlayFlingAnimation(int handsLoweredFrames, int handsRaisedFrames)
    {
        playingAnimation = Animations.Fling;
        flingHoldRaisedTime = handsRaisedFrames;
        flingHoldLoweredTime = handsLoweredFrames;
    }
    
    private void PlayRoarAnimation(int mouthClosedFrames, int mouthOpenFrames)
    {
        playingAnimation = Animations.Roar;
        roarHoldCloseTime = mouthClosedFrames;
        roarHoldOpenTime = mouthOpenFrames;
    }

    private void ManageWalkAnimation()
    {
        if (Npc.velocity.X == 0 || Npc.velocity.Y != 0)
        {
            FrameCounter = 0;
            animationFrame = Point.Zero;
            return;
        }
        
        FrameCounter += Math.Abs(Npc.velocity.X / 14f);
        if ((int)FrameCounter >= WalkFrames.Length) FrameCounter = 0;
        animationFrame = WalkFrames[(int)FrameCounter];

        if (!doneSound && (animationFrame == new Point(0, 4) || animationFrame == new Point(1, 4)))
        {
            var sound = SoundID.DeerclopsStep with { Volume = 2f };
            if (animationFrame == new Point(1, 4)) sound.Pitch = 0.25f;
            SoundEngine.PlaySound(sound, Npc.Bottom);
            
            doneSound = true;
        }
        else if (animationFrame != new Point(0, 4) && animationFrame != new Point(1, 4))
        {
            doneSound = false;
        }
    }

    private void ManageSlamAnimation()
    {
        FrameCounter += 0.2f;
        
        // Raise hands
        if (animationProgress == 0)
        {
            if (FrameCounter >= SlamWindupFrames.Length)
            {
                animationProgress++;
                FrameCounter = 0;
            }
            else
            {
                animationFrame = SlamWindupFrames[(int)FrameCounter];
            }
        }

        // Hold raised
        if (animationProgress == 1)
        {
            var framesHeld = FrameCounter * 5f;
            if (framesHeld >= slamHoldRaisedTime)
            {
                animationProgress++;
                FrameCounter = 0;
            }
            else
            {
                animationFrame = SlamHoldFrames[(int)FrameCounter % SlamHoldFrames.Length];
            }
        }

        // Slam and hold lowered
        if (animationProgress == 2)
        {
            var framesHeld = FrameCounter * 5f;
            if (framesHeld >= slamHoldLoweredTime)
            {
                animationProgress = 0;
                FrameCounter = 0;
                playingAnimation = null;
            }
            else
            {
                animationFrame = SlamReleaseFrames[(int)MathF.Min((float)FrameCounter, SlamReleaseFrames.Length - 1)];
            }
        }
    }
    
    private void ManageFlingAnimation()
    {
        FrameCounter += 0.2f;
        
        // Lower hands
        if (animationProgress == 0)
        {
            if (FrameCounter >= FlingWindupFrames.Length)
            {
                animationProgress++;
                FrameCounter = 0;
            }
            else
            {
                animationFrame = FlingWindupFrames[(int)FrameCounter];
            }
        }

        // Hold lowered
        if (animationProgress == 1)
        {
            var framesHeld = FrameCounter * 5f;
            if (framesHeld >= flingHoldLoweredTime)
            {
                animationProgress++;
                FrameCounter = 0;
            }
            else
            {
                animationFrame = FlingHoldFrames[(int)FrameCounter % FlingHoldFrames.Length];
            }
        }

        // Fling and hold raised
        if (animationProgress == 2)
        {
            var framesHeld = FrameCounter * 5f;
            if (framesHeld >= flingHoldRaisedTime)
            {
                animationProgress = 0;
                FrameCounter = 0;
                playingAnimation = null;
            }
            else
            {
                animationFrame = FlingReleaseFrames[(int)MathF.Min((float)FrameCounter, FlingReleaseFrames.Length - 1)];
            }
        }
    }
    
    private void ManageRoarAnimation()
    {
        FrameCounter += 0.2f;
        
        // Turn and close mouth
        if (animationProgress == 0)
        {
            if (FrameCounter >= RoarWindupFrames.Length)
            {
                animationProgress++;
                FrameCounter = 0;
            }
            else
            {
                animationFrame = RoarWindupFrames[(int)FrameCounter];
            }
        }

        // Hold closed mouth
        if (animationProgress == 1)
        {
            var framesHeld = FrameCounter * 5f;
            if (framesHeld >= roarHoldCloseTime)
            {
                animationProgress++;
                FrameCounter = 0;
            }
            else
            {
                animationFrame = RoarHoldClosedFrames[(int)FrameCounter % RoarHoldClosedFrames.Length];
            }
        }

        // Hold open mouth
        if (animationProgress == 2)
        {
            var framesHeld = FrameCounter * 5f;
            if (framesHeld >= roarHoldOpenTime)
            {
                animationProgress++;
                FrameCounter = 0;
            }
            else
            {
                animationFrame = RoarHoldOpenFrames[(int)FrameCounter % RoarHoldOpenFrames.Length];
            }
        }
        
        // Turn back
        if (animationProgress == 3)
        {
            if (FrameCounter >= RoarReleaseFrames.Length)
            {
                animationProgress = 0;
                FrameCounter = 0;
                playingAnimation = null;
            }
            else
            {
                animationFrame = RoarReleaseFrames[(int)FrameCounter];
            }
        }
    }

    private enum Animations
    {
        Slam, Fling, Roar
    }
    
    // This is the hard coded frame data for each attack.
    // They have been split into a windup, hold, and release period for adaptable animation
    #region Frame Data
    
    private static readonly Point[] SlamWindupFrames =
    [
        new(2, 2),
        new(2, 3),
        new(2, 4),
    ];

    private static readonly Point[] SlamHoldFrames =
    [
        new(2, 3),
        new(2, 4),
    ];

    private static readonly Point[] SlamReleaseFrames =
    [
        new(3, 0),
        new(3, 1),
        new(3, 2),
    ];

    private static readonly Point[] FlingWindupFrames =
    [
        new(2, 2),
        new(3, 0),
        new(3, 1),
        new(3, 2),
    ];
    
    private static readonly Point[] FlingHoldFrames =
    [
        new(3, 1),
        new(3, 2),
    ];
    
    private static readonly Point[] FlingReleaseFrames =
    [
        new(2, 3),
        new(3, 3),
    ];
    
    private static readonly Point[] RoarWindupFrames =
    [
        new(3, 4),
        new(4, 0),
        new(4, 1),
        new(4, 2),
    ];
    
    private static readonly Point[] RoarHoldClosedFrames =
    [
        new(4, 1),
        new(4, 2),
    ];
    
    private static readonly Point[] RoarHoldOpenFrames =
    [
        new(4, 3),
        new(4, 4),
    ];
    
    private static readonly Point[] RoarReleaseFrames =
    [
        new(4, 0),
        new(3, 4),
    ];

    private static readonly Point[] WalkFrames =
    [
        new(0, 2),
        new(0, 3),
        new(0, 4),
        new(1, 0),
        new(1, 1),
        new(1, 2),
        new(1, 3),
        new(1, 4),
        new(2, 0),
        new(2, 1),
    ];

    private static readonly Point FunnyLook = new(3, 4);
    private static readonly Point JumpFacingCam = new(4, 4);
    private static readonly Point LandFacingCam = new(4, 1);
    private static readonly Point IdleFacingCam = new(4, 0);
    private static readonly Point JumpFacingSide = new(2, 3);
    private static readonly Point LandFacingSide = new(3, 1);

    #endregion
}