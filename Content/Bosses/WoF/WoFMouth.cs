using AcidicBosses.Common.Textures;
using Luminance.Common.Utilities;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;

namespace AcidicBosses.Content.Bosses.WoF;

[AutoloadBossHead]
public class WoFMouth : AcidicNPC
{
    public override string Texture => TextureRegistry.TerrariaNPC(NPCID.WallofFlesh);

    public override string BossHeadTexture =>
        $"Terraria/Images/NPC_Head_Boss_{NPCID.Sets.BossHeadTextures[NPCID.WallofFlesh]}";

    private static WoF? Wall => WoF.GetInstance();

    private WoF.PartPosition MouthPos
    {
        get => (WoF.PartPosition) NPC.ai[0];
        set => NPC.ai[0] = (float) value;
    }
    
    private WoF.PartState PartState
    {
        get => (WoF.PartState) NPC.ai[2];
        set => NPC.ai[2] = (float) value;
    }

    public override void SetStaticDefaults()
    {
        Main.npcFrameCount[Type] = 2;
        NPCID.Sets.MustAlwaysDraw[Type] = true;
        
        // No bestiary entry since it's part of the WoF
        var bestiary = new NPCID.Sets.NPCBestiaryDrawModifiers()
        {
            Hide = true
        };
        
        NPCID.Sets.NPCBestiaryDrawOffset.Add(NPC.type, bestiary);
    }

    public override void SetDefaults()
    {
        NPC.width = 100;
        NPC.height = 100;
        NPC.damage = 50;
        NPC.defense = 12;
        NPC.lifeMax = 500;
        NPC.HitSound = SoundID.NPCHit8;
        NPC.DeathSound = SoundID.NPCDeath10;
        NPC.noGravity = true;
        NPC.noTileCollide = true;
        NPC.behindTiles = false;
        NPC.knockBackResist = 0f;
        NPC.scale = 1.2f;
        NPC.value = 80000f;

        NPC.BossBar = null;
    }

    public override bool CheckActive()
    {
        return false;
    }

    public override bool? DrawHealthBar(byte hbPosition, ref float scale, ref Vector2 position)
    {
        scale = 1.5f;
        return null;
    }

    private bool countUpTimer = false;

    private int AiTimer
    {
        get => (int) NPC.ai[1];
        set => NPC.ai[1] = value;
    }

    public override void OnFirstFrame()
    {
        if (Wall == null)
        {
            NPC.active = false;
            return;
        }
        
        AiTimer = 0;
        NPC.realLife = Main.wofNPCIndex;
        NPC.life = Wall.Npc.life;
        NPC.lifeMax = Wall.Npc.lifeMax;
    }

    public override void AcidAI()
    {
        if (Wall == null)
        {
            NPC.active = false;
            return;
        }
        
        if (AiTimer > 0 && !countUpTimer)
            AiTimer--;
        
        if (Main.wofNPCIndex < 0)
        {
            NPC.active = false;
            return;
        }

        NPC.realLife = Main.wofNPCIndex;
        if (Wall.Npc.life > 0)
        {
            NPC.life = Wall.Npc.life;
        }

        if ((PartState & WoF.PartState.FaceTarget) != 0)
        {
            LookTowards(Main.player[NPC.target].Center, 0.05f);
        }
        else
        {
            NPC.rotation = NPC.rotation.AngleLerp(0f, 0.05f);
        }
        
        // Sync stuff
        if ((MouthPos & WoF.PartPosition.Left) != 0)
        {
            NPC.direction = 1;
            NPC.spriteDirection = NPC.direction;
        }
        else
        {
            NPC.direction = -1;
            NPC.spriteDirection = NPC.direction;
        }
        
        var goalCenter = Wall.PartPosToWorldPos(MouthPos);
        NPC.Center = NPC.Center with
        {
            X = goalCenter.X,
            Y = MathHelper.Lerp(NPC.Center.Y, goalCenter.Y, 0.1f)
        };
        
        if (countUpTimer)
            AiTimer++;
    }

    public override bool PreDraw(SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor)
    {
        var tex = TextureAssets.Npc[Type];
        var pos = NPC.Center - screenPos;
        var origin = NPC.frame.Size() / 2f;
        origin.X += 16f * NPC.spriteDirection;
        
        Main.EntitySpriteDraw(
            tex.Value,
            pos,
            NPC.frame,
            drawColor,
            NPC.rotation,
            origin,
            NPC.scale,
            (-NPC.spriteDirection).ToSpriteDirection()
        );
        
        return false;
    }

    public override void FindFrame(int frameHeight)
    {
        NPC.frameCounter += 1.0;
        if (NPC.frameCounter >= 12.0)
        {
            NPC.frame.Y += frameHeight;
            NPC.frameCounter = 0.0;
        }
        if (NPC.frame.Y >= frameHeight * Main.npcFrameCount[Type])
        {
            NPC.frame.Y = 0;
        }
    }
    
    protected override void LookTowards(Vector2 target, float power)
    {
        var offset = 0f;
        if (NPC.direction < 0) offset = MathHelper.Pi;
        NPC.rotation = NPC.rotation.AngleLerp(NPC.AngleTo(target) + offset, power);
    }
}