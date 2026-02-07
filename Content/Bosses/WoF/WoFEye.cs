using AcidicBosses.Common.Configs;
using Luminance.Common.Utilities;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.GameContent;
using Terraria.ID;

namespace AcidicBosses.Content.Bosses.WoF;

public class WoFEye : AcidicNPCOverride
{
    protected override int OverriddenNpc => NPCID.WallofFleshEye;
    protected override bool BossEnabled => BossToggleConfig.Get().EnableWallOfFlesh;

    private static WoF? Wall => WoF.GetInstance();
    
    private WoF.PartPosition EyePos
    {
        get => (WoF.PartPosition) Npc.ai[0];
        set => Npc.ai[0] = (float) value;
    }
    
    private WoF.PartState PartState
    {
        get => (WoF.PartState) Npc.ai[2];
        set => Npc.ai[2] = (float) value;
    }

    private bool countUpTimer = false;

    private int AiTimer
    {
        get => (int) Npc.ai[1];
        set => Npc.ai[1] = value;
    }

    public override void OnFirstFrame(NPC npc)
    {
        if (Wall == null)
        {
            Npc.active = false;
            return;
        }
        
        AiTimer = 0;
        Npc.realLife = Main.wofNPCIndex;
        Npc.life = Wall.Npc.life;
        Npc.lifeMax = Wall.Npc.lifeMax;
        Npc.behindTiles = false;
    }

    public override bool AcidAI(NPC npc)
    {
        if (Wall == null)
        {
            Npc.active = false;
            return false;
        }
        
        if (AiTimer > 0 && !countUpTimer)
            AiTimer--;
        
        if (Main.wofNPCIndex < 0)
        {
            Npc.active = false;
            return false;
        }

        Npc.realLife = Main.wofNPCIndex;
        if (Wall.Npc.life > 0)
        {
            Npc.life = Wall.Npc.life;
        }
        
        if ((PartState & WoF.PartState.FaceTarget) != 0)
        {
            LookTowards(Main.player[Npc.target].Center, 0.05f);
        }
        else
        {
            Npc.rotation = Npc.rotation.AngleLerp(0f, 0.05f);
        }
        
        // Sync stuff
        if ((EyePos & WoF.PartPosition.Left) != 0)
        {
            Npc.direction = 1;
            Npc.spriteDirection = Npc.direction;
        }
        else
        {
            Npc.direction = -1;
            Npc.spriteDirection = Npc.direction;
        }

        var goalCenter = Wall.PartPosToWorldPos(EyePos);
        Npc.Center = Npc.Center with
        {
            X = goalCenter.X,
            Y = MathHelper.Lerp(Npc.Center.Y, goalCenter.Y, 0.1f)
        };
        
        Npc.TargetClosest_WOF();
 
        if (countUpTimer)
            AiTimer++;

        return false;
    }

    public override bool AcidicDraw(NPC npc, SpriteBatch spriteBatch, Vector2 screenPos, Color lightColor)

    {
        var tex = TextureAssets.Npc[Npc.type];
        var pos = Npc.Center - screenPos;
        var origin = Npc.frame.Size() / 2f;
        origin.X += 16f * Npc.spriteDirection;
        
        Main.EntitySpriteDraw(
            tex.Value,
            pos,
            Npc.frame,
            lightColor,
            Npc.rotation,
            origin,
            Npc.scale,
            (-Npc.spriteDirection).ToSpriteDirection() // We love left facing sprites :)
        );
        
        return false;
    }

    public override void LookTowards(Vector2 target, float power)
    {
        var offset = 0f;
        if (Npc.direction < 0) offset = MathHelper.Pi;
        Npc.rotation = Npc.rotation.AngleLerp(Npc.AngleTo(target) + offset, power);
    }
}