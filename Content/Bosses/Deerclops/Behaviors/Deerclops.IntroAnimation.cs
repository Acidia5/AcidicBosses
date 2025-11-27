using System;
using AcidicBosses.Core.Animation;
using AcidicBosses.Helpers;
using Luminance.Common.Easings;
using Luminance.Common.Utilities;
using Luminance.Core.Graphics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Terraria;
using Terraria.Audio;
using Terraria.Graphics.CameraModifiers;
using Terraria.ID;

namespace AcidicBosses.Content.Bosses.Deerclops;

public partial class Deerclops
{
    private AcidAnimation? introAnimation;

    private AcidAnimation PrepareIntroAnimation()
    {
        var anim = new AcidAnimation();

        anim.AddInstantEvent(0, () =>
        {
            Npc.damage = 0;
            Npc.scale = 0; ;
            Npc.dontTakeDamage = true;
            useCollision = false;
            Npc.behindTiles = true;

            var groundTile = Utilities.FindGroundVertical(TargetPlayer.Center.ToTileCoordinates());
            Npc.Top = groundTile.ToWorldCoordinates(8, 0) + new Vector2(0f, 36f);
            
            anim.Data.Set("startPos", Npc.Top);

            overrideAnimationFrame = JumpFacingCam;
        });

        var jump = anim.AddSequencedEvent(60, (progress, frame) =>
        {
            // Cut out music and delay Deerclops theme
            if (Main.audioSystem is LegacyAudioSystem audioSystem)
            {
                audioSystem.AudioTracks[Main.curMusic].Pause();
                for (var i = 0; i < Main.musicFade.Length; i++)
                {
                    Main.musicFade[i] = 0f;
                }
            }
            
            Npc.scale = MathHelper.Lerp(0f, 1f, progress);
            var curve = new PiecewiseCurve()
                .Add(EasingCurves.Quadratic, EasingType.Out, TileCollisionHeight + 250, 0.6f)
                .Add(EasingCurves.Quadratic, EasingType.In, TileCollisionHeight, 1f);
            
            Npc.Top = anim.Data.Get<Vector2>("startPos") - new Vector2(0f, curve.Evaluate(progress));
        });
        
        anim.AddInstantEvent(jump.EndTime, () =>
        {
            // Start deerclops theme
            if (Main.audioSystem is LegacyAudioSystem audioSystem)
            {
                audioSystem.AudioTracks[Main.curMusic].Resume();
            }
            Main.musicFade[Main.curMusic] = 1f;
            
            SoundEngine.PlaySound(SoundID.DeerclopsStep with { Volume = 3f, Pitch = -0.25f}, Npc.Bottom);
            SoundEngine.PlaySound(SoundID.DeerclopsRubbleAttack, Npc.Bottom);
            var mod = new PunchCameraModifier(Npc.Bottom, Npc.DirectionTo(Main.LocalPlayer.Center), 5f, 2f, 30);
            Main.instance.CameraModifiers.Add(mod);
            
            var xTile = Npc.position.ToTileCoordinates().X;
            var yTile = (Npc.Top + new Vector2(0, TileCollisionHeight + 16)).ToTileCoordinates().Y;
            for (var x = xTile; x < Npc.width / 16f + xTile; x++)
            {
                var ground = new Point(x, yTile);
                WorldGen.KillTile(ground.X, ground.Y, true, true);
            }
        });

        var land = anim.AddSequencedEvent(20, (progress, frame) =>
        {
            Npc.scale = 1f;
            Npc.behindTiles = false;
            Npc.direction = (int) Npc.HorizontalDirectionTo(TargetPlayer.Center);
            
            overrideAnimationFrame = LandFacingCam;
            if (progress > 0.5f) overrideAnimationFrame = IdleFacingCam;
            if (progress >= 0.75f) overrideAnimationFrame = FunnyLook;
        });
        
        anim.AddInstantEvent(land.EndTime, () =>
        {
            Npc.damage = Npc.defDamage;
            Npc.dontTakeDamage = false;
            useCollision = true;

            overrideAnimationFrame = null;
        });

        return anim;
    }
    
    private bool RunIntroAnimation()
    {
        introAnimation ??= PrepareIntroAnimation();
        if (introAnimation.RunAnimation())
        {
            introAnimation.Reset();
            return true;
        }

        return false;
    }
}