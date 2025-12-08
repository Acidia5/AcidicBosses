using System;
using AcidicBosses.Core.StateManagement;
using Luminance.Common.Utilities;

namespace AcidicBosses.Content.Bosses.Deerclops;

public partial class Deerclops
{
    private PhaseState PhaseIntro => new PhaseState(Phase_Intro);
    private PhaseState PhaseOne => new(Phase_One, EnterPhaseOne);

    private void Phase_Intro()
    {
        if (RunIntroAnimation())
        {
            phaseTracker.NextPhase();
        }
    }

    private void EnterPhaseOne()
    {
        AttackManager.Reset();
        AttackManager.SetAttackPattern([
            new AttackState(ApproachPlayer, 15),
            new AttackState(Attack_IceSpikes, 120),
            new AttackState(Attack_SummonShadowHands, 120),
        ]);
    }

    private void Phase_One()
    {
        if (AttackManager.InWindDown)
        {
            BasicWalk();
            return;
        }
        
        AttackManager.RunAttackPattern();
    }
}