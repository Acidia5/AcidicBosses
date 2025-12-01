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
    
    private void SetWoFArea()
    {
        SetWoFAreaRight();
        SetWoFAreaLeft();
    }

    private void SetWoFAreaRight()
    {
        // Stolen from vanilla
        var leftSideBlockX = (int) ((Npc.position.X + WallDistance) / 16f);
        var rightSideBlockX = (int) ((Npc.position.X + Npc.width + WallDistance) / 16f);
        var centerBlockY = (int) ((Npc.position.Y + Npc.height / 2f) / 16f);

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
        if (WoFSystem.WofDrawAreaBottomRight == -1)
        {
            WoFSystem.WofDrawAreaBottomRight = testBlockY * 16;
        }
        else if (WoFSystem.WofDrawAreaBottomRight > testBlockY * 16)
        {
            WoFSystem.WofDrawAreaBottomRight--;
            if (WoFSystem.WofDrawAreaBottomRight < testBlockY * 16)
            {
                WoFSystem.WofDrawAreaBottomRight = testBlockY * 16;
            }
        }
        else if (WoFSystem.WofDrawAreaBottomRight < testBlockY * 16)
        {
            WoFSystem.WofDrawAreaBottomRight++;
            if (WoFSystem.WofDrawAreaBottomRight > testBlockY * 16)
            {
                WoFSystem.WofDrawAreaBottomRight = testBlockY * 16;
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
        if (WoFSystem.WofDrawAreaTopRight == -1)
        {
            WoFSystem.WofDrawAreaTopRight = testBlockY * 16;
        }
        else if (WoFSystem.WofDrawAreaTopRight > testBlockY * 16)
        {
            WoFSystem.WofDrawAreaTopRight--;
            if (WoFSystem.WofDrawAreaTopRight < testBlockY * 16)
            {
                WoFSystem.WofDrawAreaTopRight = testBlockY * 16;
            }
        }
        else if (WoFSystem.WofDrawAreaTopRight < testBlockY * 16)
        {
            WoFSystem.WofDrawAreaTopRight++;
            if (WoFSystem.WofDrawAreaTopRight > testBlockY * 16)
            {
                WoFSystem.WofDrawAreaTopRight = testBlockY * 16;
            }
        }
    }

    private void SetWoFAreaLeft()
    {
        // Stolen from vanilla
        var leftSideBlockX = (int) ((Npc.position.X - WallDistance) / 16f);
        var rightSideBlockX = (int) ((Npc.position.X + Npc.width - WallDistance) / 16f);
        var centerBlockY = (int) ((Npc.position.Y + Npc.height / 2f) / 16f);

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
        if (WoFSystem.WofDrawAreaBottomLeft == -1)
        {
            WoFSystem.WofDrawAreaBottomLeft = testBlockY * 16;
        }
        else if (WoFSystem.WofDrawAreaBottomLeft > testBlockY * 16)
        {
            WoFSystem.WofDrawAreaBottomLeft--;
            if (WoFSystem.WofDrawAreaBottomLeft < testBlockY * 16)
            {
                WoFSystem.WofDrawAreaBottomLeft = testBlockY * 16;
            }
        }
        else if (WoFSystem.WofDrawAreaBottomLeft < testBlockY * 16)
        {
            WoFSystem.WofDrawAreaBottomLeft++;
            if (WoFSystem.WofDrawAreaBottomLeft > testBlockY * 16)
            {
                WoFSystem.WofDrawAreaBottomLeft = testBlockY * 16;
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
        if (WoFSystem.WofDrawAreaTopLeft == -1)
        {
            WoFSystem.WofDrawAreaTopLeft = testBlockY * 16;
        }
        else if (WoFSystem.WofDrawAreaTopLeft > testBlockY * 16)
        {
            WoFSystem.WofDrawAreaTopLeft--;
            if (WoFSystem.WofDrawAreaTopLeft < testBlockY * 16)
            {
                WoFSystem.WofDrawAreaTopLeft = testBlockY * 16;
            }
        }
        else if (WoFSystem.WofDrawAreaTopLeft < testBlockY * 16)
        {
            WoFSystem.WofDrawAreaTopLeft++;
            if (WoFSystem.WofDrawAreaTopLeft > testBlockY * 16)
            {
                WoFSystem.WofDrawAreaTopLeft = testBlockY * 16;
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

    private Vector2 PartPosToWorldPos(PartPosition pos)
    {
        var position = new Vector2();

        var areaBottom = 0f;
        var areaTop = 0f;
        if ((pos & PartPosition.Right) != 0)
        {
            position.X = Npc.position.X + WallDistance;
            areaBottom = WoFSystem.WofDrawAreaBottomRight;
            areaTop = WoFSystem.WofDrawAreaTopRight;
        }
        else if ((pos & PartPosition.Left) != 0)
        {
            position.X = Npc.position.X - WallDistance;
            areaBottom = WoFSystem.WofDrawAreaBottomLeft;
            areaTop = WoFSystem.WofDrawAreaTopLeft;
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