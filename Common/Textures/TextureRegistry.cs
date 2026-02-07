using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using Terraria;
using Terraria.ModLoader;

namespace AcidicBosses.Common.Textures;

/// <summary>
/// Simple class that holds all the general use textures for the mod.
/// Mainly used to avoid hard-coded strings when accessing textures.
/// </summary>
public class TextureRegistry : ModSystem
{
    public static string InvisPath = "AcidicBosses/Assets/Textures/Invisible";
    
    // Lines
    public static Asset<Texture2D> GlowLine;
    public static Asset<Texture2D> InvertedGlowLine;
    public static Asset<Texture2D> InvertedFadingGlowLine;
    public static Asset<Texture2D> SideGlowLine;
    public static Asset<Texture2D> Line;
    
    // Noise
    public static Asset<Texture2D> RgbPerlin;

    public override void Load()
    {
        // Preload textures for performance
        GlowLine = Tex("Lines/GlowLine");
        InvertedGlowLine = Tex("Lines/InvertedGlowLine");
        InvertedFadingGlowLine = Tex("Lines/InvertedFadingGlowLine");
        SideGlowLine = Tex("Lines/SideGlowLine");
        Line = Tex("Lines/Line");
        RgbPerlin = Tex("Noise/rgbPerlin");
    }

    // A simple function that loads a texture
    private static Asset<Texture2D> Tex(string name) => ModContent.Request<Texture2D>($"AcidicBosses/Assets/Textures/{name}");
    
    // Vanilla textures
    public static string TerrariaProjectile(int projID) => $"Terraria/Images/Projectile_{projID}";
    public static string TerrariaItem(int itemID) => $"Terraria/Images/Item_{itemID}";
    public static string TerrariaNPC(int npcID) => $"Terraria/Images/NPC_{npcID}";
}