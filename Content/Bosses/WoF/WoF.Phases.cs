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
        
    }
    
    private void Phase_One()
    {
        if (AttackManager.InWindDown)
        {
            if (Npc.GetLifePercent() < 0.6f)
            {
                AttackManager.Reset();
                phaseTracker.NextPhase();
                return;
            }
            return;
        }

        // AttackManager.RunAttackPattern();
    }
}