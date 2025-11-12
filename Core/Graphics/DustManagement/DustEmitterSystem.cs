using System.Collections.Generic;
using System.Threading.Tasks;
using ReLogic.Threading;
using Terraria.ModLoader;

namespace AcidicBosses.Core.Graphics.DustManagement;

public class DustEmitterSystem : ModSystem
{
    public static readonly List<DustEmitter> DustEmitters = [];
    
    public override void PreUpdateDusts()
    {
        FastParallel.For(0, DustEmitters.Count, (x, y, context) =>
        {
            for (var i = x; i < y; i++) DustEmitters[i].Update();
        });

        DustEmitters.RemoveAll(e => e.Time > e.Lifetime);
    }
}