using System;
using Luminance.Common.Utilities;
using Luminance.Core.Graphics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.ID;

namespace AcidicBosses.Content.Particles;

public abstract class BetterParticle : Particle
{
    private bool doneFirstFrame = false;
    private Vector2 startSize;
    private Color baseColor;
    
    /// <summary>
    /// Should the particle ignore lighting conditions
    /// </summary>
    public bool IgnoreLighting = false;

    /// <summary>
    /// The angular velocity of the particle
    /// </summary>
    public float AngularVelocity = 0;

    /// <summary>
    /// Coefficient for linearly interpolating velocity and angular velocity to zero
    /// </summary>
    public float Drag = 0f;

    /// <summary>
    /// Acceleration to apply every update
    /// </summary>
    public Vector2 Acceleration = Vector2.Zero;

    /// <summary>
    /// If this particle should emit light
    /// </summary>
    public bool EmitLight = false;
    
    /// <summary>
    /// Should this particle shrink over time
    /// </summary>
    public bool Shrink = false;

    /// <summary>
    /// Should this particle fade to invisible over time
    /// </summary>
    public bool FadeColor = false;

    /// <summary>
    /// An optional color to glow
    /// </summary>
    public Color? GlowColor;

    /// <summary>
    /// An action to perform at the end of all other properties have been updated
    /// </summary>
    public Action<BetterParticle>? OnUpdate;
    
    public BetterParticle(Vector2 position, Vector2 velocity, float rotation, Color color, int lifetime)
    {
        // For some ungodly reason Lighting.GetColor freaks out on the server is kills the boss that created the particle
        // Best practice would be to not ever make particles on the server, but I don't care.
        if (Main.netMode == NetmodeID.Server) return;
        
        Position = position;
        Velocity = velocity;
        Rotation = rotation;
        baseColor = color;
        DrawColor = IgnoreLighting ? color : color.MultiplyRGB(Lighting.GetColor(position.ToTileCoordinates()));
        Lifetime = lifetime;
        Scale = new Vector2(2f, 2f); // 2x scale matches Terraria's pixel size
    }

    public virtual void FirstFrame()
    {
        
    }
    
    public override void Update()
    {
        base.Update();
        if (!doneFirstFrame)
        {
            doneFirstFrame = true;
            startSize = Scale;
            FirstFrame();
        }

        DrawColor = baseColor;

        if (!IgnoreLighting) DrawColor = baseColor.MultiplyRGB(Lighting.GetColor(Position.ToTileCoordinates()));
        
        Velocity += Acceleration;
        Velocity = Vector2.Lerp(Velocity, Vector2.Zero, Drag);
        AngularVelocity = MathHelper.Lerp(AngularVelocity, 0f, Drag);
        
        Rotation = MathHelper.WrapAngle(Rotation + AngularVelocity);

        if (Shrink) Scale = Vector2.Lerp(startSize, Vector2.Zero, LifetimeRatio);

        if (FadeColor) DrawColor = Color.Lerp(DrawColor, Color.Transparent, LifetimeRatio);
        
        if (EmitLight) Main.QueueMainThreadAction(() =>
        {
            Lighting.AddLight(Position, DrawColor.ToVector3());
        });
        
        OnUpdate?.Invoke(this);
    }

    public override void Draw(SpriteBatch spriteBatch)
    {
        if (GlowColor != null)
        {
            spriteBatch.Draw(
                Texture,
                Position - Main.screenPosition,
                Frame,
                DrawColor.MultiplyRGBA(GlowColor.Value) * Opacity,
                Rotation,
                null,
                Scale * 1.25f,
                Direction.ToSpriteDirection()
            );
        }
        
        spriteBatch.Draw(
            Texture,
            Position - Main.screenPosition,
            Frame,
            DrawColor * Opacity,
            Rotation,
            null,
            Scale,
            Direction.ToSpriteDirection()
        );
    }
}