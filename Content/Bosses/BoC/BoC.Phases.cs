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
        HoverToPlayer(0.5f);
    }

    private void EnterPhaseAngerOne()
    {
        ScreenShakeSystem.StartShake(2f);

        var teleport = new AttackState(() => Attack_RandomTeleport(1.5f), 120);
        var tripleIchor = new AttackState(Attack_TripleIchorShot, 120);
        var fakeoutIchor = new AttackState(Attack_FakeoutIchor, 120);

        AttackManager.SetAttackPattern([
            teleport,
            tripleIchor,
            new AttackState(() => Attack_FastTeleport(TargetPlayer.Center + new Vector2(-500, -300)), 0),
            new AttackState(Attack_BloodRain, 120),
            fakeoutIchor,
        ]);
    }

    private void Phase_AngerOne()
    {
        if (Npc.GetLifePercent() <= 0.6f && !AttackManager.CountUp)
        {
            ResetExtraAI();
            phaseTracker.NextPhase();
            AttackManager.Reset();
            return;
        }

        if (AttackManager.AiTimer > 0 && !AttackManager.CountUp)
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
}