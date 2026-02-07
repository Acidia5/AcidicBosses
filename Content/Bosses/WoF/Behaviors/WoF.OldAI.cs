using AcidicBosses.Content.Particles.Animated;
using AcidicBosses.Helpers;
using Luminance.Common.Easings;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.Audio;
using Terraria.ID;

namespace AcidicBosses.Content.Bosses.WoF;

public partial class WoF
{
        private static PartPosition[] CwEyeOrder =
    {
        PartPosition.Top | PartPosition.Right,
        PartPosition.Bottom | PartPosition.Right,
        PartPosition.Bottom | PartPosition.Left,
        PartPosition.Top | PartPosition.Left,
    };
    
    private static PartPosition[] CcwEyeOrder =
    {
        PartPosition.Bottom | PartPosition.Right,
        PartPosition.Bottom | PartPosition.Left,
        PartPosition.Top | PartPosition.Left,
        PartPosition.Top | PartPosition.Right,
    };

    private Counter<PartPosition> deathrayOrder = new(CwEyeOrder);
    private Counter<PartPosition> laserSpamOrder = new(CwEyeOrder);
    private Counter<PartPosition> laserShotgunOrder = new(CcwEyeOrder);

    private bool Attack_Deathray(int telegraphTime)
    {
        AttackManager.CountUp = true;
        var done = false;

        var partPos = deathrayOrder.Get();
        var raySpawnPos = PartPosToWorldPosFront(partPos);

        if (AttackManager.AiTimer == 0 && TryFindPartAtPos(out var part2, partPos))
        {
            var pos = raySpawnPos;
                    
            var energy = new GatherEnergyParticle(pos, Npc.velocity, 0f, Color.White, telegraphTime);
            energy.Scale *= 2f;
            energy.IgnoreLighting = true;
            energy.Spawn();
        }

        if (Main.netMode != NetmodeID.MultiplayerClient)
        {
            ref var targetRot = ref Npc.localAI[0];
            if (AttackManager.AiTimer == 0)
            {
                raySpawnPos = PartPosToWorldPosBack(partPos);
                targetRot = raySpawnPos.DirectionTo(Main.player[Npc.target].Center).ToRotation();
                if (TryFindPartAtPos(out var part, partPos))
                {
                    var pos = raySpawnPos;
                    
                    var indicator = NewDeathrayIndicator(pos, targetRot, telegraphTime, part.whoAmI);
                }
            }

            if (AttackManager.AiTimer == telegraphTime)
            {
                raySpawnPos = PartPosToWorldPosBack(partPos);
                if (TryFindPartAtPos(out var part, partPos))
                {
                    var pos = raySpawnPos;
                    
                    var ray = NewDeathray(pos, targetRot, 120, part.whoAmI);
                }
                done = true;
            }
        }

        if (done)
        {
            deathrayOrder.Next();
            Npc.localAI[0] = 0;
            AttackManager.CountUp = false;
        }

        return done;
    }

    private bool Attack_Squeeze(float distance)
    {
        AttackManager.CountUp = true;
        var done = false;

        ref var initialDist = ref Npc.localAI[0];

        if (AttackManager.AiTimer == 0)
        {
            initialDist = WallDistance;
            SoundEngine.PlaySound(SoundID.Roar);

            if (Main.netMode != NetmodeID.MultiplayerClient)
            {
                var offsetL = new Vector2(-distance, 1750);
                var offsetR = new Vector2(distance, -1750);
                var l = NewLineIndicator(Npc.Center + offsetL, MathHelper.PiOver2, 120, Npc.whoAmI);
                var r = NewLineIndicator(Npc.Center + offsetR, -MathHelper.PiOver2, 120, Npc.whoAmI);
            }
        }
        
        switch (AttackManager.AiTimer)
        {
            case < 30:
                break;
            case < 30 + 120:
            {
                var t = (AttackManager.AiTimer - 30) / 120f;
                t = EasingHelper.QuadInOut(t);
                WallDistance = MathHelper.Lerp(initialDist, distance, t);
                break;
            }
            case < 30 + 120 + 60:
            {
                var curve = new PiecewiseCurve()
                    .Add(EasingCurves.Quadratic, EasingType.In, 0.5f, 0.5f)
                    .Add(MoreEasingCurves.Back, EasingType.Out, 1f, 1f);
                var t = (AttackManager.AiTimer - 30 - 120) / 60f;
                t = curve.Evaluate(t);
                WallDistance = MathHelper.Lerp(distance, initialDist, t);
                break;
            }
            default:
                done = true;
                break;
        }

        if (done)
        {
            Npc.localAI[0] = 0;
            Npc.localAI[1] = 0;
            AttackManager.CountUp = false;
        }

        return done;
    }

    private bool Attack_FireballBurst(int projectiles, float spread, float angle, float speed, PartPosition pos)
    {
        var position = PartPosToWorldPosFront(pos);
        var rot = (pos & PartPosition.Left) != 0 ? MathHelper.PiOver2 : -MathHelper.PiOver2;
        
        if (TryFindPartAtPos(out var part, pos))
        {
            var smoke = new FireSmokeParticle(position, Npc.velocity, rot, Color.Gray, 30);
            smoke.Opacity = 0.5f;
            smoke.Scale *= 2f;
            smoke.Spawn();
        }
        
        if (Main.netMode == NetmodeID.MultiplayerClient) return true;
        
        for (int i = 0; i < projectiles; i++)
        {
            var offset = ((float) i / projectiles - 0.5f) * 2 * spread;
            var a = angle + offset + (spread / projectiles);
            var vel = (a.ToRotationVector2() * speed) + (Npc.velocity / 2);

            NewFireball(position, vel);
        }

        return true;
    }
    
    private bool Attack_IchorBurst(int projectiles, float spread, float angle, float speed, PartPosition pos)
    {
        var position = PartPosToWorldPosFront(pos);
        var rot = (pos & PartPosition.Left) != 0 ? MathHelper.PiOver2 : -MathHelper.PiOver2;
        if (TryFindPartAtPos(out var part, pos))
        {
            var smoke = new FireSmokeParticle(position + new Vector2(100, 200), Npc.velocity, rot, Color.Gray, 30);
            smoke.Opacity = 0.5f;
            smoke.Scale *= 2f;
            smoke.Spawn();
        }
        
        if (Main.netMode == NetmodeID.MultiplayerClient) return true;

        for (int i = 0; i < projectiles; i++)
        {
            var offset = ((float) i / projectiles - 0.5f) * 2 * spread;
            var a = angle + offset + (spread / projectiles);
            var vel = (a.ToRotationVector2() * speed) + (Npc.velocity / 2);

            NewIchor(position, vel);
        }

        return true;
    }

    private bool Attack_DoubleFireballBurst(int projectiles, float speed)
    {
        if (Main.netMode == NetmodeID.MultiplayerClient) return true;
        
        Attack_FireballBurst(projectiles, MathHelper.PiOver2, 0f, speed, PartPosition.Left | PartPosition.Center);
        Attack_FireballBurst(projectiles, MathHelper.PiOver2, MathHelper.Pi, speed, PartPosition.Right | PartPosition.Center);

        return true;
    }
    
    private bool Attack_IchorStaggeredBursts(int waves, int ballsPerWave, float speed, int waveInterval)
    {
        AttackManager.CountUp = true;

        var isDone = AttackManager.AiTimer > (waves - 1) * waveInterval;

        if (isDone)
        {
            AttackManager.CountUp = false;
            return true;
        }
        
        if (Main.netMode == NetmodeID.MultiplayerClient) return false;

        if (AttackManager.AiTimer == 0) Npc.localAI[0] = (int) Main.rand.NextFromList(PartPosition.Left, PartPosition.Right);

        if (AttackManager.AiTimer % waveInterval == 0)
        {
            var side = (PartPosition) Npc.localAI[0];
            var direction = 0f;
            if (side == PartPosition.Right) direction = MathHelper.Pi;

            if (AttackManager.AiTimer % (waveInterval * 2) == 0)
            {
                Attack_IchorBurst(ballsPerWave, MathHelper.PiOver2, direction, speed, side | PartPosition.Center);
            } 
            else
            {
                Attack_IchorBurst(ballsPerWave + 1, MathHelper.PiOver2, direction, speed, side | PartPosition.Center);
            }
        }

        return false;
    }
    
    private bool Attack_FireballStaggeredBursts(int waves, int ballsPerWave, float speed, int waveInterval)
    {
        AttackManager.CountUp = true;

        var isDone = AttackManager.AiTimer > (waves - 1) * waveInterval;

        if (isDone)
        {
            AttackManager.CountUp = false;
            return true;
        }
        
        if (Main.netMode == NetmodeID.MultiplayerClient) return false;

        if (AttackManager.AiTimer == 0) Npc.localAI[0] = (int) Main.rand.NextFromList(PartPosition.Left, PartPosition.Right);

        if (AttackManager.AiTimer % waveInterval == 0)
        {
            var side = (PartPosition) Npc.localAI[0];
            var direction = 0f;
            if (side == PartPosition.Right) direction = MathHelper.Pi;

            if (AttackManager.AiTimer % (waveInterval * 2) == 0)
            {
                Attack_FireballBurst(ballsPerWave, MathHelper.PiOver2, direction, speed, side | PartPosition.Center);
            } 
            else
            {
                Attack_FireballBurst(ballsPerWave + 1, MathHelper.PiOver2, direction, speed, side | PartPosition.Center);
            }
        }

        return false;
    }

    private bool Attack_LaserSpam(int shots, int delay)
    {
        AttackManager.CountUp = true;
        const int indicateTime = 30;
        var isDone = AttackManager.AiTimer > shots * delay + indicateTime;

        var pos = laserSpamOrder.Get();

        if (AttackManager.AiTimer == 0)
        {
            // Set the face target bit to true
            TryFindPartAtPos(out var eye, pos);
            var state = (PartState) eye.ai[2];
            state |= PartState.FaceTarget;
            eye.ai[2] = (int) state;
        }
        
        if (isDone)
        {
            AttackManager.CountUp = false;
            laserSpamOrder.Next();
            
            // Set the face target bit to false
            TryFindPartAtPos(out var eye, pos);
            var state = (PartState) eye.ai[2];
            state &= ~PartState.FaceTarget;
            eye.ai[2] = (int) state;
        }

        if (Main.netMode == NetmodeID.MultiplayerClient) return isDone;

        if (AttackManager.AiTimer % delay == 0)
        {
            var position = PartPosToWorldPosFront(pos);
            TryFindPartAtPos(out var anchor, pos);
            
            var targetPos = position;
            
            var vel = targetPos.DirectionTo(Main.player[Npc.target].Center);
            
            var laser = NewLaser(position, vel * 25f, vel.ToRotation(), indicateTime);
        }

        return isDone;
    }
    
    private bool Attack_LaserShotgun(int lasers, float spread)
    {
        AttackManager.CountUp = true;
        const int indicateTime = 30;
        var isDone = AttackManager.AiTimer > indicateTime;

        var pos = laserShotgunOrder.Get();

        if (isDone)
        {
            AttackManager.CountUp = false;
            laserShotgunOrder.Next();
        }

        if (AttackManager.AiTimer != 0) return isDone;
        if (Main.netMode == NetmodeID.MultiplayerClient) return isDone;

        for (int i = 0; i < lasers; i++)
        {
            var position = PartPosToWorldPosBack(pos);
            TryFindPartAtPos(out var anchor, pos);

            // Face inwards
            var vel = Vector2.UnitX;
            if ((pos & PartPosition.Right) != 0) vel *= -1;
            
            // Spread
            var offset = ((float) i / lasers - 0.5f) * 2 * spread;
            vel = vel.RotatedBy(offset + (spread / lasers / 4));
            
            // Randomize spread a little
            vel = vel.RotateRandom(spread / lasers / 2);

            var laser = NewLaser(position, vel * 25f, vel.ToRotation(), indicateTime, anchor.whoAmI);
        }

        return isDone;
    }

    private bool Attack_LaserWall()
    {
        AttackManager.CountUp = true;
        const int indicateTime = 60;
        
        var isDone = AttackManager.AiTimer > indicateTime;
        
        if (isDone)
        {
            AttackManager.CountUp = false;
        }
        
        if (AttackManager.AiTimer != 0) return isDone;
        if (Main.netMode == NetmodeID.MultiplayerClient) return isDone;

        var side = Main.rand.NextFromList(PartPosition.Left, PartPosition.Right);
        
        var direction = 0f;
        if (side == PartPosition.Right) direction = MathHelper.Pi;

        var wallHeight = 0f;
        if (side == PartPosition.Right)
        {
            wallHeight = BottomRightY - TopRightY;
        }
        else
        {
            wallHeight = BottomLeftY - TopLeftY;
        }
        
        var laserSpacing = 125;
        var lasers =(int) (wallHeight / laserSpacing);

        var x = PartPosToWorldPosFront(side).X;

        for (int i = 0; i < lasers; i++)
        {
            var y = TopRightY + i * laserSpacing;

            var laser = NewLaser(new Vector2(x, y), direction.ToRotationVector2() * 25f, direction, indicateTime);
        }

        return isDone;
    }

    private bool Attack_SpawnBiomeMobs(int count, int delay)
    {
        AttackManager.CountUp = true;
        var isDone = AttackManager.AiTimer > delay * count;

        if (AttackManager.AiTimer % delay == 0) SoundEngine.PlaySound(SoundID.NPCDeath13, Npc.Center);

        if (isDone)
        {
            AttackManager.CountUp = false;
            AttackManager.AiTimer = 0;
            return true;
        }
        
        if (Main.netMode == NetmodeID.MultiplayerClient) return false;
        if (AttackManager.AiTimer % delay == 0)
        {
            if (Main.rand.NextBool())
            {
                // Evil Mob
                var pos = RandomPartX() | PartPosition.Center;
                NewEvilMob(pos);
            }
            else
            {
                // Good Mob
                var pos = RandomPartX() | PartPosition.Center;
                NewHallowMob(pos);
            }
        }

        return false;
    }
}