using System;
using AcidicBosses.Core.StateManagement;
using AcidicBosses.Helpers;
using Microsoft.Xna.Framework;
using Terraria.Audio;
using Terraria.ID;

namespace AcidicBosses.Content.Bosses.WoF;

public partial class WoF
{
    private PhaseState PhaseIntro => new(Phase_Intro);
    private PhaseState PhaseOne => new(Phase_One, EnterPhaseOne);
    private PhaseState PhaseMoveTransition => new(Phase_MoveTransition);
    private PhaseState PhaseTwo => new(Phase_Two, EnterPhaseTwo);
    private PhaseState PhaseThree => new(Phase_Three, EnterPhaseThree);

    
    private void Phase_Intro()
    {
        AttackManager.CountUp = true;

        switch (AttackManager.AiTimer)
        {
            case < 120:
                var t = AttackManager.AiTimer / 120f;
                t = EasingHelper.QuadInOut(t);
                WallDistance = MathHelper.Lerp(3000, 750, t);
                break;
            case 120:
                AttackManager.Reset();
                phaseTracker.NextPhase();
                WallDistance = 750;
                break;
        }
    }

    private void EnterPhaseOne()
    {
        var laserShotgun = new AttackState(() => Attack_LaserShotgun(10, MathHelper.Pi / 2f), 10);
        var laserSpam = new AttackState(() => Attack_LaserSpam(5, 15), 30);
        var laserWall = new AttackState(() => Attack_LaserWall(), 30);
        var fireStaggeredBursts = new AttackState(() => Attack_FireballStaggeredBursts(2, 6, 5f, 30), 15);
        var deathray = new AttackState(() => Attack_Deathray(60), 15);
        
        AttackManager.SetAttackPattern([
            laserShotgun,
            fireStaggeredBursts,
            deathray,
            laserShotgun,
            laserWall,
        ]);
    }
    
    private void Phase_One()
    {
        if (AttackManager.AiTimer > 0 && !AttackManager.CountUp)
        {
            if (Npc.GetLifePercent() < 0.6f)
            {
                AttackManager.Reset();
                phaseTracker.NextPhase();
                return;
            }
            return;
        }

        AttackManager.RunAttackPattern();
    }
    
    private void Phase_MoveTransition()
    {
        AttackManager.CountUp = true;

        if (AttackManager.AiTimer == 0)
        {
            SoundEngine.PlaySound(SoundID.Roar, Npc.Center);
        }

        switch (AttackManager.AiTimer)
        {
            case < 120:
                var shrinkT = AttackManager.AiTimer / 120f;
                shrinkT = EasingHelper.QuadInOut(shrinkT);
                WallDistance = MathHelper.Lerp(750, 1000, shrinkT);
                break;
            case < 240:
                WallDistance = 1000;
                var speedT = (AttackManager.AiTimer - 120f) / 120f;
                speedT = EasingHelper.QuadInOut(speedT);
                Npc.velocity.X = MathHelper.Lerp(0f, Npc.spriteDirection * 2f, speedT);
                break;
            case 240:
                Npc.velocity.X = Npc.spriteDirection * 2f;
                AttackManager.Reset();
                phaseTracker.NextPhase();
                break;
        }
    }

    private void EnterPhaseTwo()
    {
        var deathray = new AttackState(() => Attack_Deathray(60), 60);
        var squeeze = new AttackState(() => Attack_Squeeze(250), 90);
        var doubleFireball = new AttackState(() => Attack_DoubleFireballBurst(10, 5f), 30);
        var summon = new AttackState(() => Attack_SpawnBiomeMobs(2, 10), 60);
        var laserSpam = new AttackState(() => Attack_LaserSpam(10, 10), 30);
        var laserShotgun = new AttackState(() => Attack_LaserShotgun(14, MathHelper.Pi / 2f), 15);
        
        AttackManager.SetAttackPattern([
            doubleFireball,
            squeeze,
            laserSpam,
            laserShotgun,
            summon
        ]);
    }
    
    private void Phase_Two()
    {
        Npc.velocity.X = Npc.spriteDirection * 2f;
        
        if (AttackManager.AiTimer > 0 && !AttackManager.CountUp)
        {
            if (Npc.GetLifePercent() < 0.4f)
            {
                AttackManager.Reset();
                phaseTracker.NextPhase();
                return;
            }
            return;
        }
        
        AttackManager.RunAttackPattern();
    }

    private void EnterPhaseThree()
    {
        var deathray = new AttackState(() => Attack_Deathray(60), 15);
        var doubleFireball = new AttackState(() => Attack_DoubleFireballBurst(10, 5f), 30);
        var staggeredBursts = new AttackState(() => Attack_IchorStaggeredBursts(4, 8, 5f, 60), 45);
        var summon = new AttackState(() => Attack_SpawnBiomeMobs(3, 7), 45);
        var laserSpam =  new AttackState(() => Attack_LaserSpam(15, 10), 30);
        var laserShotgun = new AttackState(() => Attack_LaserShotgun(16, MathHelper.Pi / 2f), 30);
        var laserWall = new AttackState(Attack_LaserWall, 30);
        
        AttackManager.SetAttackPattern([
            deathray,
            laserShotgun,
            summon,
            doubleFireball,
            laserSpam,
            staggeredBursts
        ]);
    }
    
    private void Phase_Three()
    {
        var speedT = (1f - Npc.GetLifePercent() - 0.2f) / 0.5f;
        speedT = MathF.Min(speedT, 1f);
        speedT = EasingHelper.QuadIn(speedT);
        Npc.velocity.X = MathHelper.Lerp(Npc.spriteDirection * 2, Npc.spriteDirection * 4, speedT);
        
        if (AttackManager.AiTimer > 0 && !AttackManager.CountUp)
        {
            return;
        }

        AttackManager.RunAttackPattern();
    }
}