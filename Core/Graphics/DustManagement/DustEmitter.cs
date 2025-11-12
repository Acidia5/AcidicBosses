using System;
using AcidicBosses.Helpers;
using Luminance.Common.Utilities;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;

namespace AcidicBosses.Core.Graphics.DustManagement;

/// <summary>
/// A dust emitter similar to particle emitters in game engines
/// </summary>
public class DustEmitter
{
    public int DustId;
    public Color Color = Color.White;
    public Vector2 Position;
    public Vector2 DustVelocity = Vector2.Zero;
    public Vector2 EmitterVelocity = Vector2.Zero;
    public Point Size = Point.Zero;
    public float Scale = 1f;
    public int Alpha = 0;
    public float DustPerFrame; // Is a probability if < 1
    public bool NoGravity = false;
    public bool NoLight = false;
    
    public int Time = 0;
    public int Lifetime = 0;
    
    public Action<DustEmitter>? OnUpdate;
    public Func<Vector2> PositionOffsetSupplier = () => Vector2.Zero;
    public Func<Vector2> VelocityOffsetSupplier = () => Vector2.Zero;

    public DustEmitter(int dustId, Vector2 position, float dustPerFrame, int lifetime)
    {
        DustId = dustId;
        Position = position;
        DustPerFrame = dustPerFrame;
        Lifetime = lifetime;
    }
    
    public void Kill() => Time = Lifetime;

    public void Update()
    {
        Position += EmitterVelocity;
        
        OnUpdate?.Invoke(this);
        
        if (DustPerFrame < 1f)
        {
            if (Main.rand.NextBool(DustPerFrame)) MakeDust();
        }
        else
        {
            for (var i = 0; i < DustPerFrame; i++) MakeDust();
        }

        Time++;
    }

    private void MakeDust()
    {
        var pos = Main.rand.NextVector2FromRectangle(new Rectangle((int)Position.X, (int)Position.Y, Size.X, Size.Y));
        pos += PositionOffsetSupplier.Invoke();
        var vel = DustVelocity + VelocityOffsetSupplier.Invoke();
        
        var d = Dust.NewDustPerfect(
            pos,
            DustId,
            vel,
            Alpha,
            Color,
            Scale
        );

        d.noGravity = NoGravity;
        d.noLight = NoLight;
    }

    public DustEmitter Spawn()
    {
        if (!AcidUtils.IsClient()) return this;
        
        DustEmitterSystem.DustEmitters.Add(this);
        return this;
    }
}