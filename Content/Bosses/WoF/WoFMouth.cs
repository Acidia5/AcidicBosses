using AcidicBosses.Common.Textures;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace AcidicBosses.Content.Bosses.WoF;

[AutoloadBossHead]
public class WoFMouth : AcidicNPC
{
    public override string Texture => TextureRegistry.TerrariaNPC(NPCID.WallofFlesh);

    public override string BossHeadTexture =>
        $"Terraria/Images/NPC_Head_Boss_{NPCID.Sets.BossHeadTextures[NPCID.WallofFlesh]}";

    private NPC wall => Main.npc[Main.wofNPCIndex];
    
    private float WallDistance => wall.ai[3];

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

    #region AI

    private bool countUpTimer = false;

    private int AiTimer
    {
        get => (int) NPC.ai[1];
        set => NPC.ai[1] = value;
    }

    public override void OnFirstFrame()
    {
        AiTimer = 0;
        NPC.realLife = Main.wofNPCIndex;
        NPC.life = wall.life;
        NPC.lifeMax = wall.lifeMax;
    }

    public override void AcidAI()
    {
        if (AiTimer > 0 && !countUpTimer)
            AiTimer--;
        
        if (Main.wofNPCIndex < 0)
        {
            NPC.active = false;
            return;
        }

        NPC.realLife = Main.wofNPCIndex;
        if (wall.life > 0)
        {
            NPC.life = wall.life;
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
            NPC.position.X = wall.position.X - WallDistance;
            NPC.direction = 1;
            NPC.spriteDirection = NPC.direction;
            MoveToCenterLeft();
        }
        else
        {
            NPC.position.X = wall.position.X + WallDistance;
            NPC.direction = -1;
            NPC.spriteDirection = NPC.direction;
            MoveToCenterRight();
        }
        
        if (countUpTimer)
            AiTimer++;
    }

    #endregion
    
    private void MoveToCenterRight()
    {
        var areaCenter = (WoFSystem.WofDrawAreaBottomRight + WoFSystem.WofDrawAreaTopRight) / 2 - NPC.height / 2;
        MoveToCenter(areaCenter);
    }
    
    private void MoveToCenterLeft()
    {
        var areaCenter = (WoFSystem.WofDrawAreaBottomLeft + WoFSystem.WofDrawAreaTopLeft) / 2 - NPC.height / 2;
        MoveToCenter(areaCenter);
    }
    
    private void MoveToCenter(float areaCenter)
    {
        if (NPC.position.Y > areaCenter + 1f)
        {
            NPC.velocity.Y = -1f;
        }
        else if (NPC.position.Y < areaCenter - 1f)
        {
            NPC.velocity.Y = 1f;
        }
        else
        {
            NPC.velocity.Y = 0f;
            NPC.position.Y = areaCenter;
        }
        
        if (NPC.velocity.Y > 5f)
        {
            NPC.velocity.Y = 5f;
        }

        if (NPC.velocity.Y < -5f)
        {
            NPC.velocity.Y = -5f;
        }
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