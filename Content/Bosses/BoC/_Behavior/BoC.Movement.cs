using System;
using AcidicBosses.Content.Particles.Animated;
using AcidicBosses.Core.Animation;
using AcidicBosses.Core.Graphics.Sprites;
using AcidicBosses.Helpers;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Terraria;
using Terraria.Audio;
using Terraria.ID;

namespace AcidicBosses.Content.Bosses.BoC;

public partial class BoC
{
    private void HoverToPlayer(float speed)
    {
        var target = Main.player[Npc.target].Center;
        var direction = Npc.Center.DirectionTo(target);
        Npc.SimpleFlyMovement(direction * speed * MathF.Sqrt(5 * Npc.Distance(target)) / 10f, 0.05f);
    }

    private bool Attack_RandomTeleport(float hoverSpeed)
    {
        const int fadeTime = 45;
        ref var offsetX = ref ExtraAI[0];
        ref var offsetY = ref ExtraAI[1];

        AttackManager.CountUp = true;
        var isDone = false;

        // FX
        if (AttackManager.AiTimer == 0)
        {
            var smoke = new BigSmokeDisperseParticle(Npc.Center, Vector2.Zero, 0f, Color.Gray, 120);
            smoke.Opacity = 0.25f;
            smoke.FrameInterval = 4;
            smoke.Scale *= 2f;
            smoke.Spawn();
        }

        if (AttackManager.AiTimer == 0 && Main.netMode != NetmodeID.MultiplayerClient)
        {
            var target = Main.player[Npc.target].Center;

            var distance = MathF.Min(Npc.Distance(target), 750); // Don't teleport too far
            distance = MathF.Max(distance, 250); // Nor too close

            // Safely teleport
            var tile = Vector2.Zero;
            for (var i = 0; i < 50; i++)
            {
                var pos = Main.rand.NextVector2Unit() * distance + target;

                if (Npc.AI_AttemptToFindTeleportSpot(ref tile, pos.ToTileCoordinates().X, pos.ToTileCoordinates().Y))
                    break;
            }

            offsetX = tile.ToWorldCoordinates().X - target.X;
            offsetY = tile.ToWorldCoordinates().Y - target.Y;
            NetSync(Npc);
        }

        switch (AttackManager.AiTimer)
        {
            // Fade out
            case < fadeTime:
            {
                HoverToPlayer(hoverSpeed);

                var fadeT = EasingHelper.QuadOut((float)AttackManager.AiTimer / fadeTime);
                Npc.Opacity = 1f - fadeT;
                Npc.damage = 0;
                break;
            }
            // At Teleport
            case fadeTime:
            {
                var target = Main.player[Npc.target].Center;

                Npc.velocity = Vector2.Zero;
                Npc.position = target + new Vector2(offsetX, offsetY);
                break;
            }
            // Fade in
            case < fadeTime * 2:
            {
                HoverToPlayer(hoverSpeed);

                var fadeT = EasingHelper.QuadIn((float)(AttackManager.AiTimer - fadeTime) / fadeTime);
                Npc.Opacity = fadeT;
                break;
            }
            // Done
            case >= fadeTime * 2:
            {
                isDone = true;
                AttackManager.CountUp = false;
                Npc.Opacity = 1f;
                Npc.damage = Npc.defDamage;
                ResetExtraAI();
                break;
            }
        }

        return isDone;
    }

    private AcidAnimation? fastTeleportAnimation;

    private AcidAnimation PrepareFastTeleportAnimation()
    {
        var anim = new AcidAnimation();

        var shrinkTiming = anim.AddSequencedEvent(5, (progress, frame) =>
        {
            // Delay the sound slightly because it sounds wierd otherwise
            if (frame == 3)
            {
                SoundEngine.PlaySound(SoundID.Item12, Npc.Center);
            }
            
            var scaleEase = EasingHelper.QuadOut(progress);

            scale.Y = MathHelper.Lerp(1f, 0f, scaleEase);
            scale.X = MathHelper.Lerp(1f, 2f, scaleEase);
            
            Npc.Opacity = MathHelper.Lerp(1f, 0f, scaleEase);

            var slowEase = EasingHelper.QuadOut(progress);
            Npc.SimpleFlyMovement(Vector2.Zero, slowEase);
        });

        anim.AddInstantEvent(shrinkTiming.EndTime, () =>
        {
            var goalPos = anim.Data.Get<Vector2>("goalPos");

            new FakeAfterimage(Npc.Center, goalPos, Npc, 10).Spawn();

            Npc.Center = goalPos;
        });

        anim.AddSequencedEvent(5, (progress, frame) =>
        {
            var scaleEase = EasingHelper.QuadIn(progress);
            scale.Y = MathHelper.Lerp(0f, 1f, scaleEase);
            scale.X = MathHelper.Lerp(2f, 1f, scaleEase);
            Npc.Opacity = MathHelper.Lerp(0f, 1f, scaleEase);
        });

        anim.AddSequencedEvent(1, (progress, frame) =>
        {
            scale = Vector2.One;
            Npc.Opacity = 1f;
        });
        return anim;
    }

    private bool Attack_FastTeleport(Vector2 goalPos)
    {
        fastTeleportAnimation ??= PrepareFastTeleportAnimation();
        fastTeleportAnimation.Data.Set<Vector2>("goalPos", goalPos);
        if (fastTeleportAnimation.RunAnimation())
        {
            fastTeleportAnimation.Reset();
            return true;
        }

        return false;
    }
}