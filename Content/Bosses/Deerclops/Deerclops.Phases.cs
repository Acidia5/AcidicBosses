using System;
using AcidicBosses.Core.StateManagement;
using Luminance.Common.Utilities;

namespace AcidicBosses.Content.Bosses.Deerclops;

public partial class Deerclops
{
    private PhaseState PhaseIntro => new PhaseState(Phase_Intro);
    private PhaseState PhaseOne => new(Phase_One);

    private void Phase_Intro()
    {
        if (RunIntroAnimation())
        {
            phaseTracker.NextPhase();
        }
    }

    private void Phase_One()
    {
        ApproachPlayer();
    }
}