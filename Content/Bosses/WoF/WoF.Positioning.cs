using System;
using System.Linq;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace AcidicBosses.Content.Bosses.WoF;

public partial class WoF
{
    [Flags]
    public enum PartPosition
    {
        Right = 1 << 1,
        Left = 1 << 2,
        Top = 1 << 3,
        Center = 1 << 4,
        Bottom = 1 << 5
    }
    
    [Flags]
    public enum PartState
    {
        FaceTarget = 1 << 0,
    }

    public const float WallThickness = 100f;
    
    public float BottomLeftY = -1;
    public float BottomRightY = -1;
    public float TopLeftY = -1;
    public float TopRightY = -1;
    public float WallDistance;

    private float EyeOffsetTL = 0f;
    private float EyeOffsetBL = 0f;
    private float EyeOffsetTR = 0f;
    private float EyeOffsetBR = 0f;
    private float MouthOffsetL = 0f;
    private float MouthOffsetR = 0f;

    public Rectangle LeftWallRect => new Rectangle(
        (int)(Npc.Center.X - WallDistance - WallThickness),
        (int)TopLeftY,
        (int)WallThickness,
        (int)(BottomLeftY - TopLeftY)
    );
    
    public Rectangle RightWallRect => new Rectangle(
        (int)(Npc.Center.X + WallDistance),
        (int)TopRightY,
        (int)WallThickness,
        (int)(BottomRightY - TopRightY)
    );
    
    private void SetWoFArea()
    {
        SetWoFAreaRight();
        SetWoFAreaLeft();
    }

    private void SetWoFAreaRight()
    {
        // Stolen from vanilla
        var leftSideBlockX = (int) (RightWallRect.Left / 16f);
        var rightSideBlockX = (int) (RightWallRect.Right / 16f);
        var centerBlockY = (int) (Npc.Center.Y / 16f);

        // Find bottom of area
        var i = 0;
        var testBlockY = centerBlockY + 7;
        while (i < 15 && testBlockY > Main.UnderworldLayer)
        {
            testBlockY++;
            for (int testBlockX = leftSideBlockX; testBlockX <= rightSideBlockX; testBlockX++)
            {
                try
                {
                    if (WorldGen.SolidTile(testBlockX, testBlockY) ||
                        Main.tile[testBlockX, testBlockY].LiquidAmount > 0)
                    {
                        i++;
                    }
                }
                catch (Exception _)
                {
                    i += 15;
                }
            }
        }

        testBlockY += 4;
        if (BottomRightY == -1)
        {
            BottomRightY = testBlockY * 16;
        }
        else if (BottomRightY > testBlockY * 16)
        {
            BottomRightY--;
            if (BottomRightY < testBlockY * 16)
            {
                BottomRightY = testBlockY * 16;
            }
        }
        else if (BottomRightY < testBlockY * 16)
        {
            BottomRightY++;
            if (BottomRightY > testBlockY * 16)
            {
                BottomRightY = testBlockY * 16;
            }
        }

        // Find Top of area
        i = 0;
        testBlockY = centerBlockY - 7;
        while (i < 15 && testBlockY < Main.maxTilesY - 10)
        {
            testBlockY--;
            for (int testBlockX = leftSideBlockX; testBlockX <= rightSideBlockX; testBlockX++)
            {
                try
                {
                    if (WorldGen.SolidTile(testBlockX, testBlockY) ||
                        Main.tile[testBlockX, testBlockY].LiquidAmount > 0)
                    {
                        i++;
                    }
                }
                catch (Exception _)
                {
                    i += 15;
                }
            }
        }

        testBlockY -= 4;
        if (TopRightY == -1)
        {
            TopRightY = testBlockY * 16;
        }
        else if (TopRightY > testBlockY * 16)
        {
            TopRightY--;
            if (TopRightY < testBlockY * 16)
            {
                TopRightY = testBlockY * 16;
            }
        }
        else if (TopRightY < testBlockY * 16)
        {
            TopRightY++;
            if (TopRightY > testBlockY * 16)
            {
                TopRightY = testBlockY * 16;
            }
        }
    }

    private void SetWoFAreaLeft()
    {
        // Stolen from vanilla
        var leftSideBlockX = (int) (LeftWallRect.Left / 16f);
        var rightSideBlockX = (int) (LeftWallRect.Right / 16f);
        var centerBlockY = (int) (Npc.Center.Y / 16f);

        // Find bottom of area
        var i = 0;
        var testBlockY = centerBlockY + 7;
        while (i < 15 && testBlockY > Main.UnderworldLayer)
        {
            testBlockY++;
            for (int testBlockX = leftSideBlockX; testBlockX <= rightSideBlockX; testBlockX++)
            {
                try
                {
                    if (WorldGen.SolidTile(testBlockX, testBlockY) ||
                        Main.tile[testBlockX, testBlockY].LiquidAmount > 0)
                    {
                        i++;
                    }
                }
                catch (Exception _)
                {
                    i += 15;
                }
            }
        }

        testBlockY += 4;
        if (BottomLeftY == -1)
        {
            BottomLeftY = testBlockY * 16;
        }
        else if (BottomLeftY > testBlockY * 16)
        {
            BottomLeftY--;
            if (BottomLeftY < testBlockY * 16)
            {
                BottomLeftY = testBlockY * 16;
            }
        }
        else if (BottomLeftY < testBlockY * 16)
        {
            BottomLeftY++;
            if (BottomLeftY > testBlockY * 16)
            {
                BottomLeftY = testBlockY * 16;
            }
        }

        // Find Top of area
        i = 0;
        testBlockY = centerBlockY - 7;
        while (i < 15 && testBlockY < Main.maxTilesY - 10)
        {
            testBlockY--;
            for (int testBlockX = leftSideBlockX; testBlockX <= rightSideBlockX; testBlockX++)
            {
                try
                {
                    if (WorldGen.SolidTile(testBlockX, testBlockY) ||
                        Main.tile[testBlockX, testBlockY].LiquidAmount > 0)
                    {
                        i++;
                    }
                }
                catch (Exception _)
                {
                    i += 15;
                }
            }
        }

        testBlockY -= 4;
        if (TopLeftY == -1)
        {
            TopLeftY = testBlockY * 16;
        }
        else if (TopLeftY > testBlockY * 16)
        {
            TopLeftY--;
            if (TopLeftY < testBlockY * 16)
            {
                TopLeftY = testBlockY * 16;
            }
        }
        else if (TopLeftY < testBlockY * 16)
        {
            TopLeftY++;
            if (TopLeftY > testBlockY * 16)
            {
                TopLeftY = testBlockY * 16;
            }
        }
    }
    
    private bool TryFindPartAtPos(out NPC? found, PartPosition pos)
    {
        Func<NPC, bool> validType = n => n.type == NPCID.WallofFleshEye || n.type == ModContent.NPCType<WoFMouth>();
        var parts = Main.npc.Where(n => n.active && validType(n));

        foreach (var npc in parts)
        {
            if ((PartPosition) npc.ai[0] == pos)
            {
                found = npc;
                return true;
            };
        }

        found = null;
        return false;
    }

    /// <summary>
    /// Takes in a part position and returns the center position of that part
    /// </summary>
    /// <param name="pos"></param>
    /// <returns></returns>
    public Vector2 PartPosToWorldPos(PartPosition pos)
    {
        var position = new Vector2();

        var areaBottom = 0f;
        var areaTop = 0f;
        if ((pos & PartPosition.Right) != 0)
        {
            position.X = RightWallRect.Left;
            areaBottom = BottomRightY;
            areaTop = TopRightY;
        }
        else if ((pos & PartPosition.Left) != 0)
        {
            position.X = LeftWallRect.Right;
            areaBottom = BottomLeftY;
            areaTop = TopLeftY;
        }

        if ((pos & PartPosition.Bottom) != 0)
        {
            position.Y = (areaBottom + areaTop) / 2f;
            position.Y = (position.Y + areaTop) / 2f;
        }
        else if ((pos & PartPosition.Top) != 0)
        {
            position.Y = (areaBottom + areaTop) / 2f;
            position.Y = (position.Y + areaBottom) / 2f;
        }
        else if ((pos & PartPosition.Center) != 0)
        {
            position.Y = (areaBottom + areaTop) / 2f;
        }

        return position;
    }

    public Rectangle GetPartRect(PartPosition pos)
    {
        var center = PartPosToWorldPos(pos);
        var rect = new Rectangle();
        rect.Offset(center.ToPoint());
        rect.Inflate(60, 60);
        return rect;
    }

    private PartPosition RandomPartX()
    {
        return Main.rand.NextFromList(PartPosition.Left, PartPosition.Right);
    }
    
    private PartPosition RandomPartY()
    {
        return Main.rand.NextFromList(PartPosition.Top, PartPosition.Center, PartPosition.Bottom);
    }

    private PartPosition RandomEyeY()
    {
        return Main.rand.NextFromList(PartPosition.Top, PartPosition.Bottom);
    }

    private PartPosition RandomPartPos() => RandomPartX() | RandomPartY();

    private PartPosition RandomEyePos() => RandomPartX() | RandomEyeY();
}