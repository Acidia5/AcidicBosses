using System.Linq;
using AcidicBosses.Core.StateManagement;
using Luminance.Core.Graphics;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.Audio;
using Terraria.ID;

namespace AcidicBosses.Content.Bosses.BoC;

public partial class BoC
{
    private PhaseState PhaseIntro => new(Phase_Intro);
    private PhaseState PhaseCreeperOne => new(Phase_CreeperOne);
    private PhaseState PhaseAngerOne => new(Phase_AngerOne, EnterPhaseAngerOne);
    private PhaseState PhaseTransitionOne => new(Phase_TransitionOne);
    private PhaseState PhaseCreeperTwo => new(Phase_CreeperTwo, EnterPhaseCreeperTwo);
    private PhaseState PhaseAngerTwo => new(Phase_AngerTwo, EnterPhaseAngerTwo);
    private PhaseState PhaseTransitionDesperation => new(Phase_TransitionDesperation);
    private PhaseState PhaseDesperation => new(Phase_Desperation, EnterPhaseDesperation);

    private void Phase_Intro()
    {
        AttackManager.CountUp = true;
        BossBar.MaxCreepers = 10;

        // 10 creepers
        if (AttackManager.AiTimer % 6 == 0 && AttackManager.AiTimer < 60)
            Attack_SummonCreeper(CreeperOverride.AttackType.Dash);

        if (AttackManager.AiTimer >= 60)
        {
            SoundEngine.PlaySound(SoundID.Roar);

            phaseTracker.NextPhase();
            AttackManager.Reset();
        }
    }

    private void Phase_CreeperOne()
    {
        var creepersAlive = Main.npc.Count(n => n.type == NPCID.Creeper && n.active);

        if (creepersAlive <= 0)
        {
            AttackManager.Reset();
            OpenBrain();
            phaseTracker.NextPhase();
            return;
        }

        // Slow boi
        HoverToPlayer(0.75f);
    }

    private void EnterPhaseAngerOne()
    {
        ScreenShakeSystem.StartShake(2f);

        var teleport = new AttackState(() => Attack_RandomTeleport(1.25f), 120);
        var quintIchor = new AttackState(() => Attack_IchorShot(5), 120);
        var fakeoutIchor = new AttackState(Attack_FakeoutIchor, 120);

        AttackManager.SetAttackPattern([
            teleport,
            quintIchor,
            teleport,
            new AttackState(() => Attack_FastTeleport(TargetPlayer.Center + new Vector2(-500, -300)), 0),
            new AttackState(Attack_BloodRain, 120),
            teleport,
            fakeoutIchor,
        ]);
    }

    private void Phase_AngerOne()
    {
        if (Npc.GetLifePercent() <= 0.6f && AttackManager.InWindDown)
        {
            ResetExtraAI();
            phaseTracker.NextPhase();
            AttackManager.Reset();
            return;
        }

        if (AttackManager.AiTimer > 0 && AttackManager.InWindDown)
        {
            HoverToPlayer(1.25f);
            return;
        }

        AttackManager.RunAttackPattern();
    }

    private void Phase_TransitionOne()
    {
        BossBar.MaxCreepers = 10;
        AttackManager.CountUp = true;

        if (AttackManager.AiTimer == 0)
        {
            CloseBrain();
            SoundEngine.PlaySound(SoundID.Roar, Npc.Center);
        }

        if (AttackManager.AiTimer % 6 == 0 && AttackManager.AiTimer < 60)
            Attack_SummonCreeper(CreeperOverride.AttackType.SuperDash);
        if (AttackManager.AiTimer >= 60)
        {
            ExtraAI[0] = 1;
            phaseTracker.NextPhase();
            AttackManager.Reset();
        }
    }

    private void EnterPhaseCreeperTwo()
    {
        var teleport = new AttackState(() => Attack_RandomTeleport(1f), 120);
        var quintIchor = new AttackState(() => Attack_IchorShot(5), 120);
        var tripleIchor = new AttackState(() => Attack_IchorShot(3), 120);

        AttackManager.SetAttackPattern([
            teleport,
            quintIchor,
            teleport,
            tripleIchor
        ]);
    }

    private void Phase_CreeperTwo()
    {
        var creepersAlive = Main.npc.Count(n => n.type == NPCID.Creeper && n.active);

        if (creepersAlive <= 0)
        {
            AttackManager.Reset();
            OpenBrain();
            phaseTracker.NextPhase();
            return;
        }

        if (AttackManager.AiTimer > 0 && AttackManager.InWindDown)
        {
            HoverToPlayer(1f);
            return;
        }

        AttackManager.RunAttackPattern();
    }

    private void EnterPhaseAngerTwo()
    {
        ScreenShakeSystem.StartShake(2f);

        var teleport = new AttackState(() => Attack_RandomTeleport(1.5f), 90);
        var quintIchor = new AttackState(() => Attack_IchorShot(5), 30);
        var quadIchor = new AttackState(() => Attack_IchorShot(4), 60);
        var fakeoutIchor = new AttackState(Attack_FakeoutIchor, 90);

        AttackManager.SetAttackPattern([
            teleport,
            quintIchor,
            quadIchor,
            teleport,
            new AttackState(() => Attack_FastTeleport(TargetPlayer.Center + new Vector2(-500, -300)), 0),
            new AttackState(Attack_BloodRain, 90),
            teleport,
            fakeoutIchor,
        ]);
    }

    private void Phase_AngerTwo()
    {
        ShowPhantoms = true;
        
        if (Npc.GetLifePercent() <= 0.25f && AttackManager.InWindDown)
        {
            ResetExtraAI();
            phaseTracker.NextPhase();
            AttackManager.Reset();
            return;
        }

        if (AttackManager.AiTimer > 0 && AttackManager.InWindDown)
        {
            HoverToPlayer(1.5f);
            return;
        }

        AttackManager.RunAttackPattern();
    }

    private void Phase_TransitionDesperation()
    {
        BossBar.MaxCreepers = 0;
        AttackManager.CountUp = true;

        if (AttackManager.AiTimer == 0)
        {
            SoundEngine.PlaySound(SoundID.Roar, Npc.Center);
            ScreenShakeSystem.StartShake(5f);
        }

        if (AttackManager.AiTimer % 12 == 0 && AttackManager.AiTimer < 60)
            Attack_SummonCreeper(CreeperOverride.AttackType.Dash);
        if (AttackManager.AiTimer >= 60)
        {
            phaseTracker.NextPhase();
            AttackManager.Reset();
        }
    }

    private void EnterPhaseDesperation()
    {
        var teleport = new AttackState(() => Attack_RandomTeleport(1.5f), 90);
        var fastQuintIchor = new AttackState(() => Attack_IchorShot(5), 15);
        var fastQuadIchor = new AttackState(() => Attack_IchorShot(4), 15);
        var quintIchor = new AttackState(() => Attack_IchorShot(5), 60);
        var quadIchor = new AttackState(() => Attack_IchorShot(4), 60);
        var fakeoutIchor = new AttackState(Attack_FakeoutIchor, 90);

        AttackManager.SetAttackPattern([
            fastQuintIchor,
            quadIchor,
            teleport,
            new AttackState(() => Attack_FastTeleport(TargetPlayer.Center + new Vector2(-500, -300)), 0),
            new AttackState(Attack_BloodRain, 30),
            fastQuintIchor,
            teleport,
            quintIchor,
            fakeoutIchor,
            teleport,
            new AttackState(() => Attack_SummonCreeper(CreeperOverride.AttackType.Dash), 60)
        ]);
    }

    private void Phase_Desperation()
    {
        ShowPhantoms = true;
        Npc.knockBackResist = 0.5f;

        if (AttackManager.AiTimer > 0 && AttackManager.InWindDown)
        {
            HoverToPlayer(2f);
            return;
        }

        AttackManager.RunAttackPattern();
    }
}