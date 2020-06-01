using System;
using System.Collections.Generic;
using System.Drawing;

namespace BizHawk.Tool.Ecco
{
    partial class Ecco2Tool : EccoToolBase
    {
        #region Fields
        private const int _signalAlpha = 255;
        private readonly Color[] _turnSignalColors =
        {
            Color.FromArgb(_signalAlpha, 127, 127,   0),
            Color.FromArgb(_signalAlpha, 255,   0,   0),
            Color.FromArgb(_signalAlpha, 192,   0,  63),
            Color.FromArgb(_signalAlpha,  63,   0, 192),
            Color.FromArgb(_signalAlpha,   0,   0, 255),
            Color.FromArgb(_signalAlpha,   0,  63, 192),
            Color.FromArgb(_signalAlpha,   0, 192,  63),
            Color.FromArgb(_signalAlpha,   0, 255,   0)
        };
        #endregion

        #region Object Type Helpers
        private enum CollType
        {
            Rhomb0000 = 0, //The first 16 entries are for Rhombi, with point distance scaling based on bitwise flags
            Rhomb0001,
            Rhomb0010,
            Rhomb0011,
            Rhomb0100,
            Rhomb0101,
            Rhomb0110,
            Rhomb0111,
            Rhomb1000,
            Rhomb1001,
            Rhomb1010,
            Rhomb1011,
            Rhomb1100,
            Rhomb1101,
            Rhomb1110,
            Rhomb1111,
            SolidRect,  // Solid from all 4 sides
            LeftRect,   // Solid from left, top, and bottom, but not right
            UpRect,     // Solid from left, top, and right, but not bottom
            RightRect,  // Solid from top, right, and bottom, but not left
            DownRect,   // Solid from left, right, and bottom, but not top
            TrigDR,     // Right triangle with hypotenus going /, solid from right and bottom
            TrigDR2a,   // Scaled DR triangle
            TrigDR2b,   // Scaled DR triangle
            TrigDR3a,   // Differently scaled DR triangle
            TrigDR3b,   // Differently scaled DR triangle
            TrigDL,     // Right triangle with hypotenus going \, solid from left and bottom
            TrigDL2a,   // Scaled DL triangle
            TrigDL2b,   // Scaled DL triangle
            TrigDL3a,   // Differently scaled DL triangle
            TrigDL3b,   // Differently scaled DL triangle
            TrigUL,     // Right triangle with hypotenus going \, solid from right and top
            TrigUL2a,   // Scaled UL triangle
            TrigUL2b,   // Scaled UL triangle
            TrigUL3a,   // Differently scaled UL triangle
            TrigUL3b,   // Differently scaled UL triangle
            TrigUR,     // Right triangle with hypotenus going \, solid from right and top
            TrigUR2a,   // Scaled UR triangle
            TrigUR2b,   // Scaled UR triangle
            TrigUR3a,   // Differently scaled UR triangle
            TrigUR3b,   // Differently scaled UR triangle
            InvRectH,   // Makes a horizontal tube
            InvRectV,   // Makes a vertical tube
            Trapezoid,  // Hahaha fuck everything
            None,       // Because of course collision ID 44 means no collision. As opposed to 0 or something.
            SolidRect2, // Same as SolidRect, but object usually also has custom collsion handler
            SolidRect3  // Same as SolidRect, I don't know why this one exists
        }
        private enum TubeType
        {
            FullRect = 16,
            null0,
            null1,
            null2,
            null3,
            TrigDR1,
            TrigDR2,
            TrigDR3,
            TrigDR4,
            TrigDR5,
            TrigDL1,
            TrigDL2,
            TrigDL3,
            TrigDL4,
            TrigDL5,
            TrigUL1,
            TrigUL2,
            TrigUL3,
            TrigUL4,
            TrigUL5,
            TrigUR1,
            TrigUR2,
            TrigUR3,
            TrigUR4,
            TrigUR5
        }
        private enum GFXFuncType
        {
			None,
            PartialAsterite,
            FullAsterite,
            unknown
        }
        private enum ObjType
        {
            NullFunc,
            basic,
            RemnantStars,
            MovingBlock,
            MovingBlock2,
            VortexGate,
            AtlantisGateHM,
            AtlantisGateHS,
            AtlantisGateV,
            AntiGravBall,
            ConchBoss,
            ChainLinkNoseTail,
            ChainLinkAnyPoint,
            RockWorm,
            RetractedRockWorm,
            AbyssEel,
            unknown,
            NoDisplay,
            DefaultEnemy,
            StuckMagicArm,
            MedusaBoss,
            VortexSunflower,
            GlobeHolderBoss,
            VortexQueenBoss,
            FutureDolphin,
            PushableRock,
            PushableShieldingRock,
            BlueWhale,
            VortexLarva,
            VortexSoldier,
            Glyph,
            BarrierGlyph,
			CetaceanGuide,
            OrcaLost,
            OrcaIgnorable,
            OrcaMother,
            GlyphBaseBroken,
            VortexAmeoboid,
            VortexAmeoboid2,
            VortexBomb,
            SwarmSpawner,
            Fish,
            GlyphTopReparing,
            GlyphTopBroken,
            MirrorDolphin,
            VortexLightningTrap,
            ForceField,
            PulsarPowerUp,
            VortexBulletSpawner,
            SkyBubbles,
            AirPocket,
            PushableFish,
            SlowKelp,
            MetaSphere,
            Turtle,
            RetractingTurtle,
            StarWreath,
            EnemyDolphin,
            DroneFightingDolphin,
            DroneFightingDolphinSonarBlast,
            AsteriteGlobe,
            AsteriteGlobeFollowing,
            AsteriteGlobeOrbiting,
            FourIslandsControlPoint,
            MergingGlyphBound,
            MergingGlyphFree,
            MergingGlyphPulled,
            MergingGlyphDelivered,
            MergingGlyphGoal,
            MergingGlyphMerging1,
            MergingGylphMerging2,
            VortexCaptureDrone,
            ImmobileEnemy,
            SongEraser,
            TubeWhirlpool,
            TubeWhirlpoolInactive,
            TwoTidesEventStartTrigger,
            TwoTidesEventEndTrigger,
            LevelEndCutsceneTrigger,
            PoisonBubble,
            LunarBayCutsceneEcco,
            FriendlyDolphin,
            MirrorDolphinCharging1,
            MirrorDolphinCharging2,
            TrelliaAfterCutscene,
            MetaSphereInactive,
            TrelliaDuringCutscene,
            EnemySpawner,
            CreditsDolphin,
            TeleportRingFixedRad,
            Current,
            AbyssDeathEel,
            Eagle,
            ScrollController,
            ScrollWayPoint,
            BombSpawner,
            Explosion,
            PulsarBlast
		}
        private abstract class Obj2DTypeProvider
        {
            public abstract ObjType GetType(uint addr);
            public abstract GFXFuncType GetGFXType(uint addr);
        }
        private class J2DProvider : Obj2DTypeProvider
        {
            private Dictionary<uint, ObjType> _typeMap = new Dictionary<uint, ObjType>();
            private Dictionary<uint, GFXFuncType> _GFXMap = new Dictionary<uint, GFXFuncType>();
            private void InitTypeMap()
            {
				_typeMap[0xD8E9C] = ObjType.NoDisplay;
				_typeMap[0xD8D0A] = ObjType.NoDisplay;
				_typeMap[0x9E88A] = ObjType.NoDisplay;
				_typeMap[0x9EA88] = ObjType.NoDisplay;
				_typeMap[0x9E786] = ObjType.NoDisplay;
				_typeMap[0xAD1FE] = ObjType.NoDisplay;
				_typeMap[0xD9998] = ObjType.NoDisplay;
				_typeMap[0xD9D5C] = ObjType.NoDisplay;
				_typeMap[0xD9560] = ObjType.NoDisplay;
				_typeMap[0x9E6BE] = ObjType.NoDisplay;
				_typeMap[0xDFB9A] = ObjType.NoDisplay;
				_typeMap[0xB1A7A] = ObjType.NoDisplay;
				_typeMap[0xDABB8] = ObjType.NoDisplay;
				_typeMap[0xDAA40] = ObjType.NoDisplay;
				_typeMap[0xDA2FC] = ObjType.NoDisplay;
				_typeMap[0xC126A] = ObjType.NoDisplay;
				_typeMap[0xC1254] = ObjType.NoDisplay;
				_typeMap[0xDD0CC] = ObjType.NoDisplay;
				_typeMap[0xC105E] = ObjType.NoDisplay;
				_typeMap[0xE4002] = ObjType.NoDisplay;
				_typeMap[0xE3B8E] = ObjType.NoDisplay;
				_typeMap[0xC2604] = ObjType.NoDisplay;
				_typeMap[0xC27C2] = ObjType.NoDisplay;
				_typeMap[0xC36D0] = ObjType.NoDisplay;
				_typeMap[0xAA3D0] = ObjType.NoDisplay;
				_typeMap[0xAA270] = ObjType.NoDisplay;
				_typeMap[0xC6896] = ObjType.NoDisplay;
				_typeMap[0xC2CD0] = ObjType.NoDisplay;
				_typeMap[0xAF348] = ObjType.NoDisplay;
				_typeMap[0xD9E4A] = ObjType.NoDisplay;
				_typeMap[0xD98CE] = ObjType.NoDisplay;
				_typeMap[0x9C6DA] = ObjType.NoDisplay;
				_typeMap[0x9CBB0] = ObjType.NoDisplay;
				_typeMap[0x9F252] = ObjType.NoDisplay;
//				_typeMap[0xA57F2] = ObjType.NoDisplay;
				_typeMap[0xC3F5E] = ObjType.NoDisplay;
//				_typeMap[0xB33D8] = ObjType.NoDisplay;
				_typeMap[0xB356A] = ObjType.NoDisplay;
				_typeMap[0xA1B56] = ObjType.NoDisplay;
				_typeMap[0x0] = ObjType.NoDisplay;
				_typeMap[0xB6D02] = ObjType.NoDisplay;
				_typeMap[0xD15C8] = ObjType.NoDisplay;
				_typeMap[0xA7AC8] = ObjType.DefaultEnemy;
				_typeMap[0x9D556] = ObjType.DefaultEnemy;
				_typeMap[0xA7572] = ObjType.DefaultEnemy;
				_typeMap[0xC0806] = ObjType.DefaultEnemy;
//				_typeMap[0xA5378] = ObjType.DefaultEnemy;
				_typeMap[0x9D76C] = ObjType.DefaultEnemy;
				_typeMap[0xA3222] = ObjType.DefaultEnemy;
				_typeMap[0xC02C0] = ObjType.DefaultEnemy;
				_typeMap[0xACC46] = ObjType.DefaultEnemy;
				_typeMap[0xB764E] = ObjType.DefaultEnemy;
				_typeMap[0x9EA26] = ObjType.DefaultEnemy;
				_typeMap[0xC31D4] = ObjType.DefaultEnemy;
//				_typeMap[0xA0F04] = ObjType.DefaultEnemy;
				_typeMap[0xA6FAA] = ObjType.DefaultEnemy;
				_typeMap[0xAA60E] = ObjType.DefaultEnemy;
				_typeMap[0xA96E2] = ObjType.DefaultEnemy;
				_typeMap[0xA74BE] = ObjType.DefaultEnemy;
				_typeMap[0xA7442] = ObjType.DefaultEnemy;
				_typeMap[0xA793C] = ObjType.DefaultEnemy;
				_typeMap[0xC440C] = ObjType.DefaultEnemy;
//				_typeMap[0xC3F90] = ObjType.DefaultEnemy;
				_typeMap[0xC42D4] = ObjType.DefaultEnemy;
//				_typeMap[0xC3DB8] = ObjType.DefaultEnemy;
				_typeMap[0xACC16] = ObjType.DefaultEnemy;
				_typeMap[0xC64F8] = ObjType.DefaultEnemy;
				_typeMap[0xB115E] = ObjType.DefaultEnemy;
				_typeMap[0xB1CD2] = ObjType.DefaultEnemy;
				_typeMap[0xB11BC] = ObjType.DefaultEnemy;
				_typeMap[0xC2622] = ObjType.DefaultEnemy;
				_typeMap[0xC25A8] = ObjType.DefaultEnemy;
				_typeMap[0xC23D6] = ObjType.DefaultEnemy;
				_typeMap[0xC2A38] = ObjType.DefaultEnemy;
				_typeMap[0xC37E4] = ObjType.DefaultEnemy;
				_typeMap[0xABAC6] = ObjType.DefaultEnemy;
//				_typeMap[0xAC796] = ObjType.DefaultEnemy;
				_typeMap[0xA9C3E] = ObjType.ImmobileEnemy;
				_typeMap[0xA3FF8] = ObjType.TubeWhirlpool;
				_typeMap[0xA44F8] = ObjType.TubeWhirlpoolInactive;
				_typeMap[0xC099A] = ObjType.TwoTidesEventStartTrigger;
				_typeMap[0xC0A8A] = ObjType.TwoTidesEventEndTrigger;
				_typeMap[0xD9F2E] = ObjType.LevelEndCutsceneTrigger;
				_typeMap[0xDAD0A] = ObjType.LevelEndCutsceneTrigger;
				_typeMap[0xBF720] = ObjType.PoisonBubble;
				_typeMap[0xDA5E0] = ObjType.LunarBayCutsceneEcco;
				_typeMap[0xA82EC] = ObjType.FriendlyDolphin;
				_typeMap[0xAAAA6] = ObjType.FriendlyDolphin;
				_typeMap[0xB5614] = ObjType.FriendlyDolphin;
				_typeMap[0xAFE40] = ObjType.MirrorDolphinCharging1;
				_typeMap[0xAFD43] = ObjType.MirrorDolphinCharging2;
				_typeMap[0xD917C] = ObjType.TrelliaAfterCutscene;
				_typeMap[0xAD194] = ObjType.MetaSphereInactive;
				_typeMap[0xAD25E] = ObjType.MetaSphereInactive;
				_typeMap[0xD90B6] = ObjType.TrelliaDuringCutscene;
				_typeMap[0xA9A3E] = ObjType.EnemySpawner;
				_typeMap[0xC0AF8] = ObjType.CreditsDolphin;
				_typeMap[0xC2BA0] = ObjType.CreditsDolphin;
//				_typeMap[0xB7DF4] = ObjType.StuckMagicArm;
				_typeMap[0xE4B1E] = ObjType.VortexSunflower;
				_typeMap[0xDC184] = ObjType.MedusaBoss;
				_typeMap[0xDD200] = ObjType.GlobeHolderBoss;
				_typeMap[0xE1ED2] = ObjType.VortexQueenBoss;
				_typeMap[0xA60B2] = ObjType.FutureDolphin;
				_typeMap[0x9FA90] = ObjType.PushableRock;
				_typeMap[0x9F9BC] = ObjType.PushableRock;
				_typeMap[0x9FB80] = ObjType.PushableShieldingRock;
				_typeMap[0xA0DFE] = ObjType.BlueWhale;
				_typeMap[0xE6A08] = ObjType.VortexLarva;
				_typeMap[0xA9E5C] = ObjType.VortexSoldier;
				_typeMap[0xA712A] = ObjType.Glyph;
				_typeMap[0xC48F0] = ObjType.BarrierGlyph;
				_typeMap[0xB4FAC] = ObjType.CetaceanGuide;
				_typeMap[0xB5052] = ObjType.CetaceanGuide;
				_typeMap[0xB50F7] = ObjType.CetaceanGuide;
				_typeMap[0xB52FC] = ObjType.CetaceanGuide;
				_typeMap[0xB5426] = ObjType.CetaceanGuide;
				_typeMap[0xB5E18] = ObjType.OrcaLost;
				_typeMap[0xB5A0A] = ObjType.OrcaIgnorable;
				_typeMap[0xB5FDE] = ObjType.OrcaIgnorable;
				_typeMap[0xB6122] = ObjType.OrcaIgnorable;
				_typeMap[0xB672A] = ObjType.OrcaMother;
				_typeMap[0xC4724] = ObjType.GlyphBaseBroken;
				_typeMap[0xAC722] = ObjType.GlyphTopReparing;
				_typeMap[0xBEEE4] = ObjType.GlyphTopBroken;
				_typeMap[0xAFEAC] = ObjType.MirrorDolphin;
				_typeMap[0xAF91E] = ObjType.VortexLightningTrap;
				_typeMap[0xA7304] = ObjType.ForceField;
				_typeMap[0xC4F60] = ObjType.PulsarPowerUp;
				_typeMap[0xAA80C] = ObjType.VortexBulletSpawner;
				_typeMap[0xC341C] = ObjType.SkyBubbles;
				_typeMap[0x9FF5E] = ObjType.AirPocket;
				_typeMap[0xC0130] = ObjType.PushableFish;
				_typeMap[0xBEE98] = ObjType.SlowKelp;
				_typeMap[0xAD28E] = ObjType.MetaSphere;
				_typeMap[0xAD022] = ObjType.Turtle;
				_typeMap[0xACF5E] = ObjType.RetractingTurtle;
				_typeMap[0x9E366] = ObjType.StarWreath;
				_typeMap[0x9DC54] = ObjType.Fish;
				_typeMap[0x9DF06] = ObjType.Fish;
				_typeMap[0xADD5C] = ObjType.EnemyDolphin;
				_typeMap[0xC66BA] = ObjType.DroneFightingDolphin;
				_typeMap[0xC65EC] = ObjType.DroneFightingDolphinSonarBlast;
				_typeMap[0xB20C0] = ObjType.AsteriteGlobe;
				_typeMap[0xB1EF0] = ObjType.AsteriteGlobeFollowing;
				_typeMap[0xB1E00] = ObjType.AsteriteGlobeOrbiting;
				_typeMap[0xC2DBC] = ObjType.FourIslandsControlPoint;
				_typeMap[0xC6AB0] = ObjType.MergingGlyphBound;
				_typeMap[0xC6D76] = ObjType.MergingGlyphFree;
				_typeMap[0xC6F02] = ObjType.MergingGlyphPulled;
				_typeMap[0xC6FFE] = ObjType.MergingGlyphDelivered;
				_typeMap[0xC713A] = ObjType.MergingGlyphGoal;
				_typeMap[0xC7514] = ObjType.MergingGlyphMerging1;
				_typeMap[0xC7030] = ObjType.MergingGylphMerging2;
				_typeMap[0xC1A48] = ObjType.VortexCaptureDrone;
				_typeMap[0xA354E] = ObjType.VortexAmeoboid;
				_typeMap[0xA3A86] = ObjType.VortexAmeoboid2;
				_typeMap[0xACED2] = ObjType.SwarmSpawner;
				_typeMap[0xA538A] = ObjType.VortexBomb;
				_typeMap[0x9D31A] = ObjType.RemnantStars;
//				_typeMap[0x9CA10] = ObjType.MovingBlock;
				_typeMap[0x9D0E6] = ObjType.MovingBlock2;
				_typeMap[0x9BAB8] = ObjType.NullFunc;
				_typeMap[0xC5D38] = ObjType.basic;
				_typeMap[0xDF16C] = ObjType.basic;
				_typeMap[0xDFBD0] = ObjType.basic;
				_typeMap[0xDFDC8] = ObjType.basic;
				_typeMap[0xA10C4] = ObjType.basic;
				_typeMap[0xA0392] = ObjType.basic;
//				_typeMap[0xA5670] = ObjType.basic;
				_typeMap[0xAF0FA] = ObjType.basic;
				_typeMap[0xABB3A] = ObjType.basic;
				_typeMap[0x9F7CC] = ObjType.basic;
				_typeMap[0xC066E] = ObjType.VortexGate;
				_typeMap[0xC384C] = ObjType.AtlantisGateHS;
				_typeMap[0xC3ACC] = ObjType.AtlantisGateHM;
				_typeMap[0xC3956] = ObjType.AtlantisGateV;
//				_typeMap[0xA579A] = ObjType.AntiGravBall;
				_typeMap[0xDF812] = ObjType.ConchBoss;
				_typeMap[0xA24C6] = ObjType.ChainLinkNoseTail;
				_typeMap[0xA256E] = ObjType.ChainLinkNoseTail;
				_typeMap[0xA2768] = ObjType.ChainLinkNoseTail;
				_typeMap[0xA2C84] = ObjType.ChainLinkNoseTail;
				_typeMap[0xA3090] = ObjType.ChainLinkNoseTail;
				_typeMap[0xA3130] = ObjType.ChainLinkNoseTail;
				_typeMap[0xA1DC2] = ObjType.ChainLinkAnyPoint;
				_typeMap[0xB7966] = ObjType.ChainLinkAnyPoint;
/*				_typeMap[0xB864E] = ObjType.ChainLinkAnyPoint;
				_typeMap[0xB8A64] = ObjType.ChainLinkAnyPoint;
				_typeMap[0xB8C1A] = ObjType.ChainLinkAnyPoint;*/
				_typeMap[0xB952A] = ObjType.ChainLinkAnyPoint;
/*				_typeMap[0xB9728] = ObjType.ChainLinkAnyPoint;
				_typeMap[0xB9B6A] = ObjType.ChainLinkAnyPoint;*/
				_typeMap[0xE09CA] = ObjType.ChainLinkAnyPoint;
				_typeMap[0xBAB4E] = ObjType.RockWorm;
				_typeMap[0xBAA0E] = ObjType.RetractedRockWorm;
				_typeMap[0xE0CB8] = ObjType.AbyssEel;
//				_typeMap[0xC7052] = ObjType.SongEraser;
				_typeMap[0xA49CE] = ObjType.TeleportRingFixedRad;
				_typeMap[0xD14F2] = ObjType.TeleportRingFixedRad;
				_typeMap[0x9F5B0] = ObjType.Current;
				_typeMap[0xDF2C4] = ObjType.AbyssDeathEel;
				_typeMap[0xA6A64] = ObjType.Eagle;
				_typeMap[0x9Bf6A] = ObjType.ScrollController;
//				_typeMap[0xE26C2] = ObjType.ScrollController;
				_typeMap[0xE2A3E] = ObjType.ScrollController;
				_typeMap[0xE2B04] = ObjType.ScrollController;
				_typeMap[0x9BE28] = ObjType.ScrollWayPoint;
/*				_typeMap[0xA5448] = ObjType.BombSpawner;
				_typeMap[0xA529C] = ObjType.Explosion;
				_typeMap[0xA5236] = ObjType.Explosion;
				_typeMap[0xA51E6] = ObjType.Explosion;
				_typeMap[0xC9222] = ObjType.PulsarBlast;
				_typeMap[0xC9456] = ObjType.PulsarBlast;*/
			}
			private void InitGFXMap()
            {
				_GFXMap[0x9BAB8] = GFXFuncType.None;
				_GFXMap[0xB167A] = GFXFuncType.PartialAsterite;
				_GFXMap[0xB3198] = GFXFuncType.FullAsterite;
			}
			public J2DProvider()
            {
                InitTypeMap();
                InitGFXMap();
            }
            public override ObjType GetType(uint addr)
            {
                if (_typeMap.ContainsKey(addr))
                    return _typeMap[addr];
                else return ObjType.unknown;
            }
            public override GFXFuncType GetGFXType(uint addr)
            {
                if (_GFXMap.ContainsKey(addr))
                    return _GFXMap[addr];
                else return GFXFuncType.unknown;
            }
        }
        private class U2DProvider : Obj2DTypeProvider
        {
            private Dictionary<uint, ObjType> _typeMap = new Dictionary<uint, ObjType>();
            private Dictionary<uint, GFXFuncType> _GFXMap = new Dictionary<uint, GFXFuncType>();
            private void InitTypeMap()
            {
                _typeMap[0xD8B7C] = ObjType.NoDisplay;
                _typeMap[0xD89EA] = ObjType.NoDisplay;
                _typeMap[0x9E3AA] = ObjType.NoDisplay;
                _typeMap[0x9E5A8] = ObjType.NoDisplay;
                _typeMap[0x9E2A6] = ObjType.NoDisplay;
                _typeMap[0xACD1E] = ObjType.NoDisplay;
                _typeMap[0xD9678] = ObjType.NoDisplay;
                _typeMap[0xD9A3C] = ObjType.NoDisplay;
                _typeMap[0xD9240] = ObjType.NoDisplay;
                _typeMap[0x9E1DE] = ObjType.NoDisplay;
                _typeMap[0xDF86A] = ObjType.NoDisplay;
                _typeMap[0xB159A] = ObjType.NoDisplay;
                _typeMap[0xDA898] = ObjType.NoDisplay;
                _typeMap[0xDA720] = ObjType.NoDisplay;
                _typeMap[0xD9FDC] = ObjType.NoDisplay;
                _typeMap[0xC0D4E] = ObjType.NoDisplay;
                _typeMap[0xC0D38] = ObjType.NoDisplay;
                _typeMap[0xDCDAC] = ObjType.NoDisplay;
                _typeMap[0xC0B42] = ObjType.NoDisplay;
                _typeMap[0xE3CD2] = ObjType.NoDisplay;
                _typeMap[0xE385E] = ObjType.NoDisplay;
                _typeMap[0xC20E8] = ObjType.NoDisplay;
                _typeMap[0xC22A6] = ObjType.NoDisplay;
                _typeMap[0xC31B4] = ObjType.NoDisplay;
                _typeMap[0xA9EF0] = ObjType.NoDisplay;
                _typeMap[0xA9D90] = ObjType.NoDisplay;
                _typeMap[0xC6304] = ObjType.NoDisplay;
                _typeMap[0xC26E4] = ObjType.NoDisplay;
                _typeMap[0xAEE68] = ObjType.NoDisplay;
                _typeMap[0xD9B2A] = ObjType.NoDisplay;
                _typeMap[0xD95AE] = ObjType.NoDisplay;
                _typeMap[0x9C1FA] = ObjType.NoDisplay;
                _typeMap[0x9C6D0] = ObjType.NoDisplay;
                _typeMap[0x9ED72] = ObjType.NoDisplay;
                _typeMap[0xA57F2] = ObjType.NoDisplay;
                _typeMap[0xC3A42] = ObjType.NoDisplay;
                _typeMap[0xB33D8] = ObjType.NoDisplay;
                _typeMap[0xB308A] = ObjType.NoDisplay;
                _typeMap[0xA1676] = ObjType.NoDisplay;
                _typeMap[0xB6822] = ObjType.NoDisplay;
                _typeMap[0xD12E2] = ObjType.NoDisplay;
                _typeMap[0x0] = ObjType.NoDisplay;
                _typeMap[0xA75E8] = ObjType.DefaultEnemy;
                _typeMap[0x9D076] = ObjType.DefaultEnemy;
                _typeMap[0xA7092] = ObjType.DefaultEnemy;
                _typeMap[0xC02EA] = ObjType.DefaultEnemy;
                _typeMap[0xA5378] = ObjType.DefaultEnemy;
                _typeMap[0x9D28C] = ObjType.DefaultEnemy;
                _typeMap[0xA2D42] = ObjType.DefaultEnemy;
                _typeMap[0xBFDA4] = ObjType.DefaultEnemy;
                _typeMap[0xAC736] = ObjType.DefaultEnemy;
                _typeMap[0xB716E] = ObjType.DefaultEnemy;
                _typeMap[0x9E546] = ObjType.DefaultEnemy;
                _typeMap[0xC2CB8] = ObjType.DefaultEnemy;
                _typeMap[0xA0F04] = ObjType.DefaultEnemy;
                _typeMap[0xA6ACA] = ObjType.DefaultEnemy;
                _typeMap[0xAA12E] = ObjType.DefaultEnemy;
                _typeMap[0xA9202] = ObjType.DefaultEnemy;
                _typeMap[0xA6FDE] = ObjType.DefaultEnemy;
                _typeMap[0xA6F62] = ObjType.DefaultEnemy;
                _typeMap[0xA745C] = ObjType.DefaultEnemy;
                _typeMap[0xC3EF0] = ObjType.DefaultEnemy;
                _typeMap[0xC3F90] = ObjType.DefaultEnemy;
                _typeMap[0xC3FFC] = ObjType.DefaultEnemy;
                _typeMap[0xC3DB8] = ObjType.DefaultEnemy;
                _typeMap[0xAC766] = ObjType.DefaultEnemy;
                _typeMap[0xC5F66] = ObjType.DefaultEnemy;
                _typeMap[0xB0C7E] = ObjType.DefaultEnemy;
                _typeMap[0xB17F2] = ObjType.DefaultEnemy;
                _typeMap[0xB0CDC] = ObjType.DefaultEnemy;
                _typeMap[0xC2106] = ObjType.DefaultEnemy;
                _typeMap[0xC208C] = ObjType.DefaultEnemy;
                _typeMap[0xC1EBA] = ObjType.DefaultEnemy;
                _typeMap[0xC251C] = ObjType.DefaultEnemy;
                _typeMap[0xC32C8] = ObjType.DefaultEnemy;
                _typeMap[0xAB5E6] = ObjType.DefaultEnemy;
                _typeMap[0xAC796] = ObjType.DefaultEnemy;
                _typeMap[0xA975E] = ObjType.ImmobileEnemy;
                _typeMap[0xA3B18] = ObjType.TubeWhirlpool;
                _typeMap[0xA4018] = ObjType.TubeWhirlpoolInactive;
                _typeMap[0xC047E] = ObjType.TwoTidesEventStartTrigger;
                _typeMap[0xC056E] = ObjType.TwoTidesEventEndTrigger;
                _typeMap[0xD9C0E] = ObjType.LevelEndCutsceneTrigger;
                _typeMap[0xDA9EA] = ObjType.LevelEndCutsceneTrigger;
                _typeMap[0xBF204] = ObjType.PoisonBubble;
                _typeMap[0xDA2C0] = ObjType.LunarBayCutsceneEcco;
                _typeMap[0xA7E0C] = ObjType.FriendlyDolphin;
                _typeMap[0xAA5C6] = ObjType.FriendlyDolphin;
                _typeMap[0xB5134] = ObjType.FriendlyDolphin;
                _typeMap[0xAF960] = ObjType.MirrorDolphinCharging1;
                _typeMap[0xAF86B] = ObjType.MirrorDolphinCharging2;
                _typeMap[0xD8E5C] = ObjType.TrelliaAfterCutscene;
                _typeMap[0xACCB4] = ObjType.MetaSphereInactive;
                _typeMap[0xACD7E] = ObjType.MetaSphereInactive;
                _typeMap[0xD8D96] = ObjType.TrelliaDuringCutscene;
                _typeMap[0xA955E] = ObjType.EnemySpawner;
				_typeMap[0xC05DC] = ObjType.CreditsDolphin;
				_typeMap[0xC2684] = ObjType.CreditsDolphin;
                _typeMap[0xB7DF4] = ObjType.StuckMagicArm;
                _typeMap[0xE47EE] = ObjType.VortexSunflower;
                _typeMap[0xDBE64] = ObjType.MedusaBoss;
                _typeMap[0xDCEE0] = ObjType.GlobeHolderBoss;
                _typeMap[0xE1BA2] = ObjType.VortexQueenBoss;
                _typeMap[0xA5BD2] = ObjType.FutureDolphin;
                _typeMap[0x9F5B0] = ObjType.PushableRock;
                _typeMap[0x9F4DC] = ObjType.PushableRock;
                _typeMap[0x9F6A0] = ObjType.PushableShieldingRock;
                _typeMap[0xA091E] = ObjType.BlueWhale;
                _typeMap[0xE66D8] = ObjType.VortexLarva;
                _typeMap[0xA997C] = ObjType.VortexSoldier;
                _typeMap[0xA6C4A] = ObjType.Glyph;
                _typeMap[0xC43D4] = ObjType.BarrierGlyph;
                _typeMap[0xB4ACC] = ObjType.CetaceanGuide;
                _typeMap[0xB4B72] = ObjType.CetaceanGuide;
                _typeMap[0xB4C17] = ObjType.CetaceanGuide;
                _typeMap[0xB4E1C] = ObjType.CetaceanGuide;
                _typeMap[0xB4F46] = ObjType.CetaceanGuide;
                _typeMap[0xB5938] = ObjType.OrcaLost;
                _typeMap[0xB552A] = ObjType.OrcaIgnorable;
                _typeMap[0xB5AFE] = ObjType.OrcaIgnorable;
                _typeMap[0xB5C42] = ObjType.OrcaIgnorable;
                _typeMap[0xB624A] = ObjType.OrcaMother;
                _typeMap[0xC4208] = ObjType.GlyphBaseBroken;
                _typeMap[0xAC242] = ObjType.GlyphTopReparing;
                _typeMap[0xBE9C8] = ObjType.GlyphTopBroken;
                _typeMap[0xAF9CC] = ObjType.MirrorDolphin;
                _typeMap[0xAF43E] = ObjType.VortexLightningTrap;
                _typeMap[0xA6E24] = ObjType.ForceField;
                _typeMap[0xC4A44] = ObjType.PulsarPowerUp;
                _typeMap[0xAA32C] = ObjType.VortexBulletSpawner;
                _typeMap[0xC2F00] = ObjType.SkyBubbles;
                _typeMap[0x9FA7E] = ObjType.AirPocket;
                _typeMap[0xBFC14] = ObjType.PushableFish;
                _typeMap[0xBE97C] = ObjType.SlowKelp;
                _typeMap[0xACDAE] = ObjType.MetaSphere;
                _typeMap[0xACB42] = ObjType.Turtle;
                _typeMap[0xACA7E] = ObjType.RetractingTurtle;
                _typeMap[0x9DE86] = ObjType.StarWreath;
                _typeMap[0x9D774] = ObjType.Fish;
                _typeMap[0x9DA26] = ObjType.Fish;
                _typeMap[0xAD87C] = ObjType.EnemyDolphin;
                _typeMap[0xC6128] = ObjType.DroneFightingDolphin;
                _typeMap[0xC605A] = ObjType.DroneFightingDolphinSonarBlast;
                _typeMap[0xB1BE0] = ObjType.AsteriteGlobe;
                _typeMap[0xB1A10] = ObjType.AsteriteGlobeFollowing;
                _typeMap[0xB1920] = ObjType.AsteriteGlobeOrbiting;
                _typeMap[0xC28A0] = ObjType.FourIslandsControlPoint;
                _typeMap[0xC651E] = ObjType.MergingGlyphBound;
                _typeMap[0xC67E4] = ObjType.MergingGlyphFree;
                _typeMap[0xC6970] = ObjType.MergingGlyphPulled;
                _typeMap[0xC6A6C] = ObjType.MergingGlyphDelivered;
                _typeMap[0xC6BA8] = ObjType.MergingGlyphGoal;
                _typeMap[0xC6F82] = ObjType.MergingGlyphMerging1;
                _typeMap[0xC6A9E] = ObjType.MergingGylphMerging2;
                _typeMap[0xC152C] = ObjType.VortexCaptureDrone;
                _typeMap[0xA306E] = ObjType.VortexAmeoboid;
                _typeMap[0xA35A6] = ObjType.VortexAmeoboid2;
                _typeMap[0xAC9F2] = ObjType.SwarmSpawner;
                _typeMap[0xA538A] = ObjType.VortexBomb;
                _typeMap[0x9CE3A] = ObjType.RemnantStars;
                _typeMap[0x9CA10] = ObjType.MovingBlock;
                _typeMap[0x9CC06] = ObjType.MovingBlock2;
                _typeMap[0x9B5D8] = ObjType.NullFunc;
                _typeMap[0xC57A6] = ObjType.basic;
                _typeMap[0xDEE3C] = ObjType.basic;
                _typeMap[0xDF8A0] = ObjType.basic;
                _typeMap[0xDFA98] = ObjType.basic;
                _typeMap[0xA0BE4] = ObjType.basic;
                _typeMap[0x9FEB2] = ObjType.basic;
                _typeMap[0xA5670] = ObjType.basic;
                _typeMap[0xAEC1A] = ObjType.basic;
                _typeMap[0xAB65A] = ObjType.basic; 
                _typeMap[0x9F2EC] = ObjType.basic;
                _typeMap[0xC0152] = ObjType.VortexGate;
                _typeMap[0xC3330] = ObjType.AtlantisGateHS;
                _typeMap[0xC35B0] = ObjType.AtlantisGateHM;
                _typeMap[0xC343A] = ObjType.AtlantisGateV;
                _typeMap[0xA579A] = ObjType.AntiGravBall;
                _typeMap[0xDF4E2] = ObjType.ConchBoss;
                _typeMap[0xA1FE6] = ObjType.ChainLinkNoseTail;
                _typeMap[0xA208E] = ObjType.ChainLinkNoseTail;
                _typeMap[0xA2288] = ObjType.ChainLinkNoseTail;
                _typeMap[0xA27A4] = ObjType.ChainLinkNoseTail;
                _typeMap[0xA2BB0] = ObjType.ChainLinkNoseTail;
                _typeMap[0xA2C50] = ObjType.ChainLinkNoseTail;
                _typeMap[0xA18E2] = ObjType.ChainLinkAnyPoint;
                _typeMap[0xB7486] = ObjType.ChainLinkAnyPoint;
                _typeMap[0xB864E] = ObjType.ChainLinkAnyPoint;
                _typeMap[0xB8A64] = ObjType.ChainLinkAnyPoint;
                _typeMap[0xB8C1A] = ObjType.ChainLinkAnyPoint;
                _typeMap[0xB904A] = ObjType.ChainLinkAnyPoint;
                _typeMap[0xB9728] = ObjType.ChainLinkAnyPoint;
                _typeMap[0xB9B6A] = ObjType.ChainLinkAnyPoint;
                _typeMap[0xE069A] = ObjType.ChainLinkAnyPoint;
                _typeMap[0xBA66E] = ObjType.RockWorm;
                _typeMap[0xBA52E] = ObjType.RetractedRockWorm;
                _typeMap[0xE0988] = ObjType.AbyssEel;
                _typeMap[0xC7052] = ObjType.SongEraser;
                _typeMap[0xA44EE] = ObjType.TeleportRingFixedRad;
                _typeMap[0xD120C] = ObjType.TeleportRingFixedRad;
                _typeMap[0x9F0D0] = ObjType.Current;
                _typeMap[0xDEF94] = ObjType.AbyssDeathEel;
                _typeMap[0xA6584] = ObjType.Eagle;
                _typeMap[0x9BA8A] = ObjType.ScrollController;
                _typeMap[0xE26C2] = ObjType.ScrollController;
                _typeMap[0xE270E] = ObjType.ScrollController;
                _typeMap[0xE27D4] = ObjType.ScrollController;
                _typeMap[0x9B948] = ObjType.ScrollWayPoint;
                _typeMap[0xA5448] = ObjType.BombSpawner;
                _typeMap[0xA529C] = ObjType.Explosion;
                _typeMap[0xA5236] = ObjType.Explosion;
                _typeMap[0xA51E6] = ObjType.Explosion;
                _typeMap[0xC9222] = ObjType.PulsarBlast;
                _typeMap[0xC9456] = ObjType.PulsarBlast;
			}
            private void InitGFXMap()
            {
				_GFXMap[0x9B5D8] = GFXFuncType.None;
                _GFXMap[0xB119A] = GFXFuncType.PartialAsterite;
                _GFXMap[0xB2CB8] = GFXFuncType.FullAsterite;
            }
            public U2DProvider()
            {
                InitTypeMap();
                InitGFXMap();
            }
            public override ObjType GetType(uint addr)
            {
                if (_typeMap.ContainsKey(addr))
                    return _typeMap[addr];
                else return ObjType.unknown;
            }
            public override GFXFuncType GetGFXType(uint addr)
            {
                if (_GFXMap.ContainsKey(addr))
                    return _GFXMap[addr];
                else return GFXFuncType.unknown;
            }

        }
        private class E2DProvider : Obj2DTypeProvider
        {
            private Dictionary<uint, ObjType> _typeMap = new Dictionary<uint, ObjType>();
            private Dictionary<uint, GFXFuncType> _GFXMap = new Dictionary<uint, GFXFuncType>();
            private void InitTypeMap()
            {

            }
            private void InitGFXMap()
            {

            }
            public E2DProvider()
            {
                InitTypeMap();
                InitGFXMap();
            }
            public override ObjType GetType(uint addr)
            {
                if (_typeMap.ContainsKey(addr))
                    return _typeMap[addr];
                else return ObjType.unknown;
            }
            public override GFXFuncType GetGFXType(uint addr)
            {
                if (_GFXMap.ContainsKey(addr))
                    return _GFXMap[addr];
                else return GFXFuncType.unknown;
            }

        }
        private Obj2DTypeProvider _2DTypeProvider;
        private static class ColorMap
        {
            public static readonly Color Tubes = Color.Purple;
            public static readonly Color HeadTail = Color.PowderBlue;
            public static readonly Color Solid = HeadTail;
            public static readonly Color WallEject = Color.Yellow;
            public static readonly Color Sonar = Color.Blue;
            public static readonly Color Enemy = Color.FromArgb(255, 0, 127);
            public static readonly Color Nose = Color.Lime;
            public static readonly Color Mid = Color.Red;
            public static readonly Color Tail = Color.Blue;
            public static readonly Color NoseTail = Color.FromArgb(Nose.R | Tail.R, Nose.G | Tail.G, Nose.B | Tail.B);
            public static readonly Color MidTail = Color.FromArgb(Mid.R | Tail.R, Mid.G | Tail.G, Mid.B | Tail.B);
            public static readonly Color Spine = Color.FromArgb(Nose.R | Mid.R, Nose.G | Mid.G, Nose.B | Mid.B);
            public static readonly Color AnyPoint = Color.FromArgb(Nose.R | Mid.R | Tail.R, Nose.G | Mid.G | Tail.G, Nose.B | Mid.B | Tail.B);
        }
        #endregion

        #region Definitions and Object Loaders
        private unsafe struct Obj2D
        {
            public uint PtrNext;       // Ptr to next object in RAM
            public uint PtrSubObj;     // Ptr to next subobj in chain
            public uint PtrSubObj2;    // Ptr to prev subobj in chain or next subobj in chain 2
            public uint PtrMainFunc;   // Ptr to main function address. Proxy for object type, because it defines obj behavior
            public ushort Flags;       // Bitwise bool array. Unsure what values mean but all 0s is marker for deletion
            public byte PID;           // Persistence ID, every obj loaded from level layouts has a unique one
            public byte ObjDefIndex;   // Partial proxy for object's *original* type, but spawned objs and subobjs don't have useful info here
            public byte unkb0;
            public byte Mode;          // Basically an enum that controls flow through obj logic;
            public byte unkb1;
            public byte unkb2;
            public short XChunk;       // Pos.X / 128 pixels, often used by loading/unloading logic
            public short YChunk;       // Pos.Y / 128 pixels, often used by loading/unloading logic
            public Point Orig;         // Initial position on loading
            public Point Mid;          // Center point of obect. All other points calculated from this.
            public Point TopLeft;      // Top-left corner of rectangular hitbox, if any
            public Point BottomRight;  // Bottom-right corner of rectangular hitbox, if any
            public int Var1X;          // Scratch variable often related to x position
            public int Var1Y;          // Scratch variable often related to Y position
            public Size Dims;         // Object width and height
            public int Var2X;          // Scratch variable often related to x position
            public int Var2Y;          // Scratch variable often related to Y position
            public Size Vel1;         // Object movement speed
            public int Vel2X;          // Secondary horizontal movement speed or scratch variable
            public int Vel2Y;          // Secondary vertical movement speed or scratch variable
            public short ScreenX;      // X position relative to camera, pixel truncated
            public short ScreenY;      // Y position relative to camera, pixel truncated
            public uint PtrPLC;        // Pointer to sprite information structure
            public ushort AnimFrame;   // Frame of current animation, sometimes used in behavior logic
            public ushort StateCtr;    // Animation related nonlag frame counter, often used in behavior logic
            public byte Animation;     // ID of current animation, somtimes used in behavior logic
            public byte unkb3;
            public int Vel3X;          // Scratch variable sometimes related to horizontal movement speed
            public int Vel3Y;          // Scratch variable sometimes related to vertical movement speed
            public sbyte TrgAng;       // 8Bit scratch variable often related to movement angle
            public sbyte HP;            // Hitpoints remaining. Death usually occurs on 1, not 0.
            public byte State;         // Behavior control variable
            public sbyte CurAng;       // 8Bit scratch variable often related to movement angle
            public CollType CollisionType; // Enum id of collision type
            public sbyte SonarCtr;     // Objects will only respond to sonar when this is 0. Usually counts down every frame it isn't.
        }
        private unsafe struct PlayerObj
        {
            public ushort Flags;       // Bitwise bool array. Unsure what values mean.
            public Size CurrentVel;     // Speed and direction of air/water stream effects
            public byte unkb0;
            public byte unkb1;
            public int unk0;
            public int unk1;
            public int unk2;
            public Point SonarPos;      // Midpoint of sonar blast
            public int unkSnr0;
            public int unkSnr1;
            public Size SonarSize;      // Width and Height of sonar blast hitrect
            public Size SonarVel;       // Speed and dir of sonar blast
            public byte SonarChrgFlag;   // Is this a charge sonar?
            public byte unkSnrb0;
            public short unkSnrs0;
            public short SonarAng;     // Direction sonar is moving
            public short Form;         // Animal that Ecco currently is
            public ushort SonarFlags;  // Should Sonar break rocks/dislodge barrier glyphs. Also a bunch of nonsonar stuff I guess
            public short HP;           // Ecco's HP. Usually capped at 56, usually goes down by increments of 4. Game GUI only displays first 40.
            public short Air;          // Ecco's air meter. Starts at 448, goes down by 1 per frame. Game GUI only displays first 320.
            public Point Mid;           // Ecco's midpoint. All other Ecco positions calculated from this
            public Point Nose;          // Tip of Ecco's nose
            public Point Tail;          // Rear-most of Ecco's collision points
            public int SwimSpeed;       // Magnitude of Ecco's swim velocity, can be negative which means backwards movement relative to angle
            public Size SwimVel;        // Absolute horizontal and vertical swim vectors
            public Size ZipVel;         // Velocity of terrain ejection effects on Ecco
            public int TurnSpeedLoss;   // Related to how Ecco loses speed during 180s
            public int unk3;
            public int unk4;
            public short Angle;        // Direction Ecco is pointing, in units of 1/65536 of a circle
            public short RotSpeed;     // Ecco's basic angle rotation speed (3/256 of a circle)
            public int FlopSpeed;       // Related to how fast Ecco flips in air. Didn't find exact formula relating this to angle change
            public short TgtAng;       // Direction Ecco wants to point.
            public Point Focus;         // Where camera should focus. Determined from Pos and sum of Vels
            public short unks0;
            public short unks1;
            public byte unkb2;
            public byte PrevControls;  // What buttons were pressed last frame
            public byte CurControls;   // What buttons are pressed now
            public byte unkb3;
            public byte unkb4;
            public byte DecelTimer;    // Counts down from 60 after acceleration happens. Slow speedloss when <= 30, fast speed loss when 0
            public byte AccelTimer;    // Where we are in accel cycle caps at 12. Slow accel from 1-6, fast accel from 7-11. No accel at 0 or 12.
            public byte unkb5;
            public byte InvincTimer;   // How long until next vulnerable
            public byte AccelState;
            public int WaterLevel;     // Because ecco needs a local copy, I guess
            public uint PLC;           // Pointer to sprite information structure
            public short unks2;
            public short Animation;
            public short AnimFrame;
            public short unks3;
            public short unks4;
            public fixed int Xhist[16];
            public fixed int YHist[16];
            public fixed short AngHist[16];
            public fixed short unkSTable[16];
            public fixed short unkSTable1[16];
            public short unks5;
            public short unks6;
            public short XChunk;       // Pos.X / 128 pixels. Used for loading logic.
            public short YChunk;       // Pos.Y / 128 pixels. Used for loading logic.
            public short unks7;
            public short unks8;
            public byte ChargeCounter; // Where we are in a charge/countdown to when ecco is next able to charge
            public byte unkb6;
            public byte MoveMode;      // 0 for swimming, 1 for tailwalking, 2 for midair, 3 for plunging, 4 for 180, 5 for midair 180, 6 for null, 7 for frozen
            public byte unkb7;
            public byte unkb8;
            public byte HistIdx;
            public byte unkb9;
            public byte SonarState;
            public short unks9;
            public byte unkba;
            public byte unkbb;
            public short unksa;
            public uint PtrCarriedObj;
            public byte unkbc;
            public byte unkbd;
            public byte unkbe;
            public byte unkbf;
            public byte unkbg;
            public byte unkbh;
            public byte unkbi;
            public byte unkbj;
            public byte GravDir;
            public byte unkbk;
            public ushort Flags2;
            public int unk5;
            public int unk6;
        }
        private void ReadObj2D(uint addr, out Obj2D obj)
        {
            obj = new Obj2D();
            obj.PtrNext = ReadPtrAndAdvance(ref addr);
            obj.PtrSubObj = ReadPtrAndAdvance(ref addr);
            obj.PtrSubObj2 = ReadPtrAndAdvance(ref addr);
            obj.PtrMainFunc = ReadPtrAndAdvance(ref addr);
            obj.Flags = ReadU16AndAdvance(ref addr);
            obj.PID = ReadByteAndAdvance(ref addr);
            obj.ObjDefIndex = ReadByteAndAdvance(ref addr);
            obj.unkb0 = ReadByteAndAdvance(ref addr);
            obj.Mode = ReadByteAndAdvance(ref addr);
            obj.unkb1 = ReadByteAndAdvance(ref addr);
            obj.unkb2 = ReadByteAndAdvance(ref addr);
            obj.XChunk = ReadS16AndAdvance(ref addr);
            obj.YChunk = ReadS16AndAdvance(ref addr);
            obj.Orig.X = ReadS32AndAdvance(ref addr);
            obj.Orig.Y = ReadS32AndAdvance(ref addr);
            obj.Mid.X = ReadS32AndAdvance(ref addr);
            obj.Mid.Y = ReadS32AndAdvance(ref addr);
            obj.TopLeft.X = ReadS32AndAdvance(ref addr);
            obj.TopLeft.Y = ReadS32AndAdvance(ref addr);
            obj.BottomRight.X = ReadS32AndAdvance(ref addr);
            obj.BottomRight.Y = ReadS32AndAdvance(ref addr);
            obj.Var1X = ReadS32AndAdvance(ref addr);
            obj.Var1Y = ReadS32AndAdvance(ref addr);
            obj.Dims.Width = ReadS32AndAdvance(ref addr);
            obj.Dims.Height = ReadS32AndAdvance(ref addr);
            obj.Var2X = ReadS32AndAdvance(ref addr);
            obj.Var2Y = ReadS32AndAdvance(ref addr);
            obj.Vel1.Width = ReadS32AndAdvance(ref addr);
            obj.Vel1.Height = ReadS32AndAdvance(ref addr);
            obj.Vel2X = ReadS32AndAdvance(ref addr);
            obj.Vel2Y = ReadS32AndAdvance(ref addr);
            obj.ScreenX = ReadS16AndAdvance(ref addr);
            obj.ScreenY = ReadS16AndAdvance(ref addr);
            obj.PtrPLC = ReadPtrAndAdvance(ref addr);
            obj.AnimFrame = ReadU16AndAdvance(ref addr);
            obj.StateCtr = ReadU16AndAdvance(ref addr);
            obj.Animation = ReadByteAndAdvance(ref addr);
            obj.unkb3 = ReadByteAndAdvance(ref addr);
            obj.Vel3X = ReadS32AndAdvance(ref addr);
            obj.Vel3Y = ReadS32AndAdvance(ref addr);
            obj.TrgAng = ReadSByteAndAdvance(ref addr);
            obj.HP = ReadSByteAndAdvance(ref addr);
            obj.State = ReadByteAndAdvance(ref addr);
            obj.CurAng = ReadSByteAndAdvance(ref addr);
            obj.CollisionType = (CollType)(long)ReadByteAndAdvance(ref addr);
            obj.SonarCtr = ReadSByteAndAdvance(ref addr);
        }
        private void ReadPlayerObj(uint addr, out PlayerObj obj)
        {
            obj = new PlayerObj();
            obj.Flags = ReadU16AndAdvance(ref addr);			 // 0x000
            obj.CurrentVel.Width = ReadS32AndAdvance(ref addr);  // 0x002
            obj.CurrentVel.Height = ReadS32AndAdvance(ref addr); // 0x006
            obj.unkb0 = ReadByteAndAdvance(ref addr);            // 0x00A
            obj.unkb1 = ReadByteAndAdvance(ref addr);            // 0x00B
			obj.unk0 = ReadS32AndAdvance(ref addr);              // 0x00C
			obj.unk1 = ReadS32AndAdvance(ref addr);              // 0x010
			obj.unk2 = ReadS32AndAdvance(ref addr);              // 0x014
			obj.SonarPos.X = ReadS32AndAdvance(ref addr);        // 0x018
			obj.SonarPos.Y = ReadS32AndAdvance(ref addr);        // 0x01C
			obj.unkSnr0 = ReadS32AndAdvance(ref addr);           // 0x020
			obj.unkSnr1 = ReadS32AndAdvance(ref addr);           // 0x024
			obj.SonarSize.Width = ReadS32AndAdvance(ref addr);   // 0x028
            obj.SonarSize.Height = ReadS32AndAdvance(ref addr);  // 0x02C
			obj.SonarVel.Width = ReadS32AndAdvance(ref addr);    // 0x030
			obj.SonarVel.Height = ReadS32AndAdvance(ref addr);   // 0x034
			obj.SonarChrgFlag = ReadByteAndAdvance(ref addr);    // 0x038
			obj.unkSnrb0 = ReadByteAndAdvance(ref addr);         // 0x039
			obj.unkSnrs0 = ReadS16AndAdvance(ref addr);          // 0x03A
			obj.SonarAng = ReadS16AndAdvance(ref addr);          // 0x03C
			obj.Form = ReadS16AndAdvance(ref addr);              // 0x03E
			obj.SonarFlags = ReadU16AndAdvance(ref addr);        // 0x040
			obj.HP = ReadS16AndAdvance(ref addr);                // 0x042
			obj.Air = ReadS16AndAdvance(ref addr);               // 0x044
			obj.Mid.X = ReadS32AndAdvance(ref addr);             // 0x046
			obj.Mid.Y = ReadS32AndAdvance(ref addr);             // 0x04A
			obj.Nose.X = ReadS32AndAdvance(ref addr);            // 0x04E
			obj.Nose.Y = ReadS32AndAdvance(ref addr);            // 0x052
			obj.Tail.X = ReadS32AndAdvance(ref addr);            // 0x056
			obj.Tail.Y = ReadS32AndAdvance(ref addr);            // 0x05A
			obj.SwimSpeed = ReadS32AndAdvance(ref addr);         // 0x05E
			obj.SwimVel.Width = ReadS32AndAdvance(ref addr);     // 0x062
			obj.SwimVel.Height = ReadS32AndAdvance(ref addr);    // 0x066
			obj.ZipVel.Width = ReadS32AndAdvance(ref addr);      // 0x06A
			obj.ZipVel.Height = ReadS32AndAdvance(ref addr);     // 0x06E
			obj.TurnSpeedLoss = ReadS32AndAdvance(ref addr);     // 0x072
			obj.unk3 = ReadS32AndAdvance(ref addr);              // 0x076
			obj.unk4 = ReadS32AndAdvance(ref addr);              // 0x07A
			obj.Angle = ReadS16AndAdvance(ref addr);             // 0x07E
			obj.RotSpeed = ReadS16AndAdvance(ref addr);          // 0x080
			obj.FlopSpeed = ReadS32AndAdvance(ref addr);         // 0x082
			obj.TgtAng = ReadS16AndAdvance(ref addr);            // 0x086
			obj.Focus.X = ReadS32AndAdvance(ref addr);           // 0x088
			obj.Focus.Y = ReadS32AndAdvance(ref addr);           // 0x08C
			obj.unks0 = ReadS16AndAdvance(ref addr);             // 0x08E
			obj.unks1 = ReadS16AndAdvance(ref addr);             // 0x090
			obj.unkb2 = ReadByteAndAdvance(ref addr);            // 0x092
			obj.PrevControls = ReadByteAndAdvance(ref addr);     // 0x093
			obj.CurControls = ReadByteAndAdvance(ref addr);      // 0x094
			obj.unkb3 = ReadByteAndAdvance(ref addr);            // 0x095
			obj.unkb4 = ReadByteAndAdvance(ref addr);            // 0x096
			obj.DecelTimer = ReadByteAndAdvance(ref addr);       // 0x097
			obj.AccelTimer = ReadByteAndAdvance(ref addr);       // 0x098
			obj.unkb5 = ReadByteAndAdvance(ref addr);            // 0x099
			obj.InvincTimer = ReadByteAndAdvance(ref addr);      // 0x09A
			obj.AccelState = ReadByteAndAdvance(ref addr);       // 0x09B
			obj.WaterLevel = ReadS32AndAdvance(ref addr);        // 0x09C
			obj.PLC = ReadPtrAndAdvance(ref addr);               // 0x0A0
			obj.unks2 = ReadS16AndAdvance(ref addr);             // 0x0A4
			obj.Animation = ReadS16AndAdvance(ref addr);         // 0x0A6
			obj.AnimFrame = ReadS16AndAdvance(ref addr);         // 0x0A8
			obj.unks3 = ReadS16AndAdvance(ref addr);             // 0x0AA
			obj.unks4 = ReadS16AndAdvance(ref addr);             // 0x0AE
			addr += 0xE0;
            obj.unks5 = ReadS16AndAdvance(ref addr);             // 0x18E
			obj.unks6 = ReadS16AndAdvance(ref addr);             // 0x190
			obj.XChunk = ReadS16AndAdvance(ref addr);            // 0x192
			obj.YChunk = ReadS16AndAdvance(ref addr);            // 0x194
			obj.unks7 = ReadS16AndAdvance(ref addr);             // 0x196
			obj.unks8 = ReadS16AndAdvance(ref addr);             // 0x198
			obj.ChargeCounter = ReadByteAndAdvance(ref addr);    // 0x19A
			obj.unkb6 = ReadByteAndAdvance(ref addr);            // 0x19B
			obj.MoveMode = ReadByteAndAdvance(ref addr);         // 0x19C
			obj.unkb7 = ReadByteAndAdvance(ref addr);            // 0x19D
			obj.unkb8 = ReadByteAndAdvance(ref addr);            // 0x19E
			obj.HistIdx = ReadByteAndAdvance(ref addr);          // 0x19F
			obj.unkb9 = ReadByteAndAdvance(ref addr);            // 0x1A0
			obj.SonarState = ReadByteAndAdvance(ref addr);       // 0x1A1
			obj.unks9 = ReadS16AndAdvance(ref addr);             // 0x1A2
			obj.unkba = ReadByteAndAdvance(ref addr);            // 0x1A4
			obj.unkbb = ReadByteAndAdvance(ref addr);            // 0x1A5
			obj.unksa = ReadS16AndAdvance(ref addr);             // 0x1A6
			obj.PtrCarriedObj = ReadPtrAndAdvance(ref addr);     // 0x1A8
			obj.unkbc = ReadByteAndAdvance(ref addr);            // 0x1AC
			obj.unkbd = ReadByteAndAdvance(ref addr);            // 0x1AD
			obj.unkbe = ReadByteAndAdvance(ref addr);            // 0x1AE
			obj.unkbf = ReadByteAndAdvance(ref addr);            // 0x1AF
			obj.unkbg = ReadByteAndAdvance(ref addr);            // 0x1B0
			obj.unkbh = ReadByteAndAdvance(ref addr);            // 0x1B1
			obj.unkbi = ReadByteAndAdvance(ref addr);            // 0x1B2
			obj.unkbj = ReadByteAndAdvance(ref addr);            // 0x1B3
			obj.GravDir = ReadByteAndAdvance(ref addr);          // 0x1B4
			obj.unkbk = ReadByteAndAdvance(ref addr);            // 0x1B5
			obj.Flags2 = ReadU16AndAdvance(ref addr);            // 0x1B6
			obj.unk5 = ReadS32AndAdvance(ref addr);              // 0x1B8
			obj.unk6 = ReadS32AndAdvance(ref addr);              // 0x1B8
        }
        private static class Addr2D	
        {
            public const uint LevelWidth = 0xFFA7A8;
            public const uint LevelHeight = 0xFFA7AC;
            public const uint WaterLevel = 0xFFA7B2;
            public const uint P1ExistsFlag = 0xFFA7B5;
            public const uint EccoHeadPtr = 0xFFA9CC;
            public const uint EccoTailPtr = 0xFFA9D0;
            public const uint PlayerObj = 0xFFA9D4;
            public const uint CamXDest = 0xFFAD8C;
            public const uint CamYDest = 0xFFAD90;
            public const uint CamX = 0xFFAD9C;
            public const uint CamY = 0xFFAD9E;
            public const uint EventLLHead = 0xFFCFB4;
            public const uint AnimLLHead = 0xFFCFB8;
            public const uint StaticLLHead = 0xFFCFBC;
            public const uint WallLLHead = 0xFFCFC0;
            public const uint TubeLLHead = 0xFFCFC4;
            public const uint SpecLLHead = 0xFFCFC8;
            public const uint SubObjLLHead = 0xFFCFCC;
            public const uint MultObjLLHead = 0xFFCFD0;
            public const uint AsteriteDrop = 0xFFD424;
            public const uint AsteriteHead = 0xFFD428;
            public const uint GlobeFlags = 0xFFD434;
            public const uint GlobeFlags2 = 0xFFD438;
            public const uint PtrGFXFunc = 0xFFD440;
        }
        private Point GetScreenLoc(Point abs)
        {
            Point rel = abs;
            rel.X >>= 16;
            rel.Y >>= 16;
            rel.X -= _camX;
            rel.Y -= _camY;
            return rel;
        }
        #endregion
        private PlayerObj _player2D;
        #region CommonDrawFunctions
        private void DrawStandardCollisionType(Point topLeft, Point bottomRight, Size dims, CollType type, Color? outline = null)
        {
            Point mid = new Point((topLeft.X + bottomRight.X) >> 1, (topLeft.Y + bottomRight.Y) >> 1);
            switch (type)
            {
                case CollType.SolidRect:
                case CollType.SolidRect2:
                case CollType.SolidRect3:
                    DrawBox(topLeft.X, topLeft.Y, bottomRight.X, bottomRight.Y, ColorMap.Solid);
                    DrawBox(topLeft.X, topLeft.Y, bottomRight.X, bottomRight.Y, outline ?? ColorMap.HeadTail, 0);
                    break;
                case CollType.LeftRect:
                    mid.X = (topLeft.X + bottomRight.X) >> 1;
                    bottomRight.X = mid.X;
                    DrawBox(topLeft.X, topLeft.Y, bottomRight.X, bottomRight.Y, Color.FromArgb(63, ColorMap.WallEject));
                    Gui.DrawLine(bottomRight.X, topLeft.Y, bottomRight.X, bottomRight.Y, outline ?? ColorMap.HeadTail);
                    break;
                case CollType.UpRect:
                    mid.X = (topLeft.X + bottomRight.X) >> 1;
                    topLeft.X = mid.X;
                    DrawBox(topLeft.X, topLeft.Y, bottomRight.X, bottomRight.Y, Color.FromArgb(63, ColorMap.WallEject));
                    Gui.DrawLine(topLeft.X, topLeft.Y, topLeft.X, bottomRight.Y, outline ?? ColorMap.HeadTail);
                    break;
                case CollType.RightRect:
                    mid.Y = (topLeft.Y + bottomRight.Y) >> 1;
                    topLeft.Y = mid.Y;
                    DrawBox(topLeft.X, topLeft.Y, bottomRight.X, bottomRight.Y, Color.FromArgb(63, ColorMap.WallEject));
                    Gui.DrawLine(topLeft.X, topLeft.Y, bottomRight.X, topLeft.Y, outline ?? ColorMap.HeadTail);
                    break;
                case CollType.DownRect:
                    mid.Y = (topLeft.Y + bottomRight.Y) >> 1;
                    bottomRight.Y = mid.Y;
                    DrawBox(topLeft.X, topLeft.Y, bottomRight.X, bottomRight.Y, Color.FromArgb(63, ColorMap.WallEject));
                    Gui.DrawLine(topLeft.X, bottomRight.Y, bottomRight.X, bottomRight.Y, outline ?? ColorMap.HeadTail);
                    break;
                case CollType.TrigDR:
                case CollType.TrigDR2a:
                case CollType.TrigDR2b:
                case CollType.TrigDR3a:
                case CollType.TrigDR3b:
                    mid.X = (topLeft.X + bottomRight.X) >> 1;
                    mid.Y = (topLeft.Y + bottomRight.Y) >> 1;
                    DrawEccoTriangle(mid.X, mid.Y, mid.X, bottomRight.Y, bottomRight.X, mid.Y, Color.FromArgb(63, ColorMap.WallEject));
                    Gui.DrawLine(mid.X, bottomRight.Y, bottomRight.X, mid.Y, outline ?? ColorMap.HeadTail);
                    break;
                case CollType.TrigDL:
                case CollType.TrigDL2a:
                case CollType.TrigDL2b:
                case CollType.TrigDL3a:
                case CollType.TrigDL3b:
                    mid.X = (topLeft.X + bottomRight.X) >> 1;
                    mid.Y = (topLeft.Y + bottomRight.Y) >> 1;
                    DrawEccoTriangle(mid.X, mid.Y, mid.X, bottomRight.Y, topLeft.X, mid.Y, Color.FromArgb(63, ColorMap.WallEject));
                    Gui.DrawLine(topLeft.X, mid.Y, mid.X, bottomRight.Y, outline ?? ColorMap.HeadTail);
                    break;
                case CollType.TrigUL:
                case CollType.TrigUL2a:
                case CollType.TrigUL2b:
                case CollType.TrigUL3a:
                case CollType.TrigUL3b:
                    mid.X = (topLeft.X + bottomRight.X) >> 1;
                    mid.Y = (topLeft.Y + bottomRight.Y) >> 1;
                    DrawEccoTriangle(mid.X, mid.Y, mid.X, topLeft.Y, topLeft.X, mid.Y, Color.FromArgb(63, ColorMap.WallEject));
                    Gui.DrawLine(topLeft.X, mid.Y, mid.X, topLeft.Y, outline ?? ColorMap.HeadTail);
                    break;
                case CollType.TrigUR:
                case CollType.TrigUR2a:
                case CollType.TrigUR2b:
                case CollType.TrigUR3a:
                case CollType.TrigUR3b:
                    mid.X = (topLeft.X + bottomRight.X) >> 1;
                    mid.Y = (topLeft.Y + bottomRight.Y) >> 1;
                    DrawEccoTriangle(mid.X, mid.Y, mid.X, topLeft.Y, bottomRight.X, mid.Y, Color.FromArgb(63, ColorMap.WallEject));
                    Gui.DrawLine(mid.X, topLeft.Y, bottomRight.X, mid.Y, outline ?? ColorMap.HeadTail);
                    break;
                case CollType.InvRectH:
                    {
                        int top = topLeft.Y - 16;
                        int bottom = bottomRight.Y + 16;
                        DrawBox(topLeft.X, top, bottomRight.X, topLeft.Y, Color.FromArgb(63, ColorMap.WallEject));
                        DrawBox(topLeft.X, bottomRight.Y, bottomRight.X, bottom, Color.FromArgb(63, ColorMap.WallEject));
                        Gui.DrawLine(topLeft.X, topLeft.Y, bottomRight.X, topLeft.Y, outline ?? ColorMap.HeadTail);
                        Gui.DrawLine(topLeft.X, bottomRight.Y, bottomRight.X, bottomRight.Y, outline ?? ColorMap.HeadTail);
                    }
                    break;
                case CollType.InvRectV:
                    {
                        int left = topLeft.X - 16;
                        int right = bottomRight.X + 16;
                        DrawBox(left, topLeft.Y, topLeft.X, bottomRight.Y, Color.FromArgb(63, ColorMap.WallEject));
                        Gui.DrawLine(topLeft.X, topLeft.Y, topLeft.X, bottomRight.Y, outline ?? ColorMap.HeadTail);
                        DrawBox(bottomRight.X, topLeft.Y, right, bottomRight.Y, Color.FromArgb(63, ColorMap.WallEject));
                        Gui.DrawLine(bottomRight.X, topLeft.Y, bottomRight.X, bottomRight.Y, outline ?? ColorMap.HeadTail);
                    }
                    break;
                case CollType.Trapezoid:
                    mid.X = (topLeft.X + bottomRight.X) >> 1;
                    mid.Y = (topLeft.Y + bottomRight.Y) >> 1;
                    Point[] trapPoints =
                    {
                            new Point(topLeft.X, mid.Y),
                            new Point(topLeft.X + (mid.Y - topLeft.Y >> 1), topLeft.Y),
                            new Point(bottomRight.X - (mid.Y - topLeft.Y >> 1), topLeft.Y),
                            new Point(bottomRight.X, mid.Y)
                        };
                    Gui.DrawPolygon(trapPoints, outline ?? ColorMap.Solid, Color.FromArgb(63, ColorMap.Solid));
                    Gui.DrawPolygon(trapPoints, outline ?? ColorMap.HeadTail, Color.Transparent);
                    break;
                case CollType.None:
                    break;
                default:
                    {
                        int right, bottom, left, top;
                        int flags = (int)type;
                        right = (flags & 1);
                        flags >>= 1;
                        bottom = (flags & 1);
                        flags >>= 1;
                        left = (flags & 1);
                        flags >>= 1;
                        top = (flags & 1);
                        DrawEccoRhomb_scaled(mid.X, mid.Y, dims.Width >> 16, dims.Width >> 16, right, bottom, left, top, ColorMap.Solid);
                        DrawEccoRhomb_scaled(mid.X, mid.Y, dims.Width >> 16, dims.Width >> 16, right, bottom, left, top, outline ?? ColorMap.HeadTail, 0);
                    }
                    break;
            }
        }
        private void DrawChainLinks(Obj2D curObj, Color wColor, Color hColor, Color eColor)
        {
            Obj2D subObj;
            Point pos = new Point();
            Point mid = new Point();
            uint subAddr = curObj.PtrSubObj;
            while (subAddr != 0)
            {
                ReadObj2D(subAddr, out subObj);
                pos = GetScreenLoc(subObj.Mid);
                if (subObj.Dims.Width == subObj.Dims.Height)
                {
                    DrawOct(pos.X, pos.Y, subObj.Dims.Width >> 16, ColorMap.Enemy);
                    DrawOct(pos.X, pos.Y, subObj.Dims.Width >> 16, eColor, 0);
                }
                else
                {
                    DrawOct(pos.X, pos.Y, subObj.Dims.Width >> 16, ColorMap.Enemy);
                    DrawOct(pos.X, pos.Y, subObj.Dims.Width >> 16, wColor, 0);
                    DrawOct(pos.X, pos.Y, subObj.Dims.Height >> 16, ColorMap.Enemy);
                    DrawOct(pos.X, pos.Y, subObj.Dims.Height >> 16, hColor, 0);
                }
                DrawMotionVector(subObj.Mid, subObj.Vel1);
                subAddr = subObj.PtrSubObj;
            }
            if (curObj.HP > 2)
            {
                mid = GetScreenLoc(curObj.Mid);
                PutText($"{curObj.HP - 1}", mid.X, mid.Y, 1, 1, -1, -9, Color.Blue, Color.Red);
            }
        }
        private void DrawDefaultBounds(Obj2D curObj, Color outline, Color? fill = null)
        {
            Point topLeft = GetScreenLoc(curObj.TopLeft);
            Point bottomRight = GetScreenLoc(curObj.BottomRight);
            DrawBox(topLeft.X, topLeft.Y, bottomRight.X, bottomRight.Y, fill ?? outline);
            if (fill != null)
                DrawBox(topLeft.X, topLeft.Y, bottomRight.X, bottomRight.Y, outline, 0);
        }
        private void DrawPushableBounds(Obj2D curObj, Color? fill = null)
        {
            DrawDefaultBounds(curObj, ColorMap.Nose, fill);
            Point topLeft = GetScreenLoc(curObj.TopLeft);
            Point bottomRight = GetScreenLoc(curObj.BottomRight);
            Gui.DrawLine(topLeft.X, topLeft.Y, bottomRight.X, bottomRight.Y, ColorMap.Nose);
            Gui.DrawLine(topLeft.X, bottomRight.Y, bottomRight.X, topLeft.Y, ColorMap.Nose);
        }
        private void DrawShieldBounds(Obj2D curObj)
        {
            Size shieldSize = new Size();
            shieldSize.Width = curObj.Dims.Width << 1;
            shieldSize.Height = shieldSize.Width << 2;
            Point topLeft = GetScreenLoc(curObj.TopLeft - shieldSize);
            shieldSize.Height = 0;
            Point bottomRight = GetScreenLoc(curObj.BottomRight + shieldSize);
            DrawBox(topLeft.X, topLeft.Y, bottomRight.X, bottomRight.Y, ColorMap.Nose);
        }
        private void DrawMotionVector(Point start, Size dist, Color? outline = null)
        {
            Point end = GetScreenLoc(start + dist);
            start = GetScreenLoc(start);
            DrawBoxMWH(start.X, start.Y, 1, 1, outline ?? Color.Blue, 0);
            Gui.DrawLine(start.X, start.Y, end.X, end.Y, Color.Orange);
        }
        #endregion
        private void DrawAsterite()
        {
            uint addr = 0;
            Obj2D curObj;
            Point mid1 = new Point();
            Point mid2 = new Point();
            GFXFuncType type = _2DTypeProvider.GetGFXType(Mem.ReadU32(Addr2D.PtrGFXFunc));
            switch (type)
            {
                case GFXFuncType.PartialAsterite:
                    addr = ReadPtr(Addr2D.AsteriteHead);
                    ReadObj2D(addr, out curObj);
                    addr = curObj.PtrSubObj;
                    while (addr != 0)
                    {
                        ReadObj2D(addr, out curObj);
                        mid1 = GetScreenLoc(curObj.Mid);
                        mid2 = GetScreenLoc(new Point(curObj.Var1X, curObj.Var1Y));
                        if (curObj.unkb3 != 0)
                        {
                            DrawOct(mid1.X, mid1.Y, 48, ColorMap.Sonar, 16);
                            DrawOct(mid2.X, mid2.Y, 48, ColorMap.Sonar, 16);
                        }
                        addr = curObj.PtrSubObj;
                    }
                    if ((Mem.ReadU8(AddrGlobal.LevelID) == 30))
                    {
                        addr = ReadPtr(Addr2D.AsteriteDrop);
                        ReadObj2D(addr, out curObj);
                        if ((addr != 0) && (curObj.PtrSubObj2 != 0))
                        {
                            mid1 = GetScreenLoc(curObj.Orig);
                            DrawOct(mid1.X, mid1.Y, 20, Color.Orange);
                        }
                    }
                    break;
                case GFXFuncType.FullAsterite:
                    addr = ReadPtr(Addr2D.AsteriteHead);
                    ReadObj2D(addr, out curObj);
                    addr = curObj.PtrSubObj;
                    while (addr != 0)
                    {
                        ReadObj2D(addr, out curObj);
                        mid1 = GetScreenLoc(curObj.Mid);
                        mid2 = GetScreenLoc(new Point(curObj.Var1X, curObj.Var1Y));
                        if (curObj.unkb3 != 0)
                        {
                            if (Mem.ReadByte(AddrGlobal.LevelID) != 0x1F)
                            {
                                DrawOct(mid1.X, mid1.Y, 40, ColorMap.Nose);
                                DrawOct(mid2.X, mid2.Y, 40, ColorMap.Nose);
                            }
                            DrawOct(mid1.X, mid1.Y, 48, ColorMap.Sonar, 16);
                            DrawOct(mid2.X, mid2.Y, 48, ColorMap.Sonar, 16);
                        }
                        addr = curObj.PtrSubObj;
                    }
                    break;
				case GFXFuncType.None:
					break;
                default:
					StatusText($"GFX Func: {Mem.ReadU32(Addr2D.PtrGFXFunc):X5}", Color.Blue);
					break;
            }
        }
        private void DrawTubes()
        {
            uint addr = ReadPtr(Addr2D.TubeLLHead);
            Obj2D curObj;
            Point topLeft = new Point();
            Point bottomRight = new Point();
            Point mid = new Point();
            while (addr != 0)
            {
                ReadObj2D(addr, out curObj);
                topLeft = GetScreenLoc(curObj.TopLeft);
                bottomRight = GetScreenLoc(curObj.BottomRight);
                mid.X = (topLeft.X + bottomRight.X) >> 1;
                mid.Y = (topLeft.Y + bottomRight.Y) >> 1;
                TubeType type = (TubeType)curObj.CollisionType;
                switch (type)
                {
                    case TubeType.FullRect:
                        DrawBox(topLeft.X, topLeft.Y, bottomRight.X, bottomRight.Y, ColorMap.Tubes);
                        break;
                    case TubeType.TrigDR1:
                    case TubeType.TrigDR2:
                    case TubeType.TrigDR3:
                    case TubeType.TrigDR4:
                    case TubeType.TrigDR5:
                        DrawEccoTriangle(mid.X, mid.Y, mid.X, bottomRight.Y, bottomRight.X, mid.Y, ColorMap.Tubes);
                        break;
                    case TubeType.TrigDL1:
                    case TubeType.TrigDL2:
                    case TubeType.TrigDL3:
                    case TubeType.TrigDL4:
                    case TubeType.TrigDL5:
                        DrawEccoTriangle(mid.X, mid.Y, mid.X, bottomRight.Y, topLeft.X, mid.Y, ColorMap.Tubes);
                        break;
                    case TubeType.TrigUL1:
                    case TubeType.TrigUL2:
                    case TubeType.TrigUL3:
                    case TubeType.TrigUL4:
                    case TubeType.TrigUL5:
                        DrawEccoTriangle(mid.X, mid.Y, mid.X, topLeft.Y, topLeft.X, mid.Y, ColorMap.Tubes);
                        break;
                    case TubeType.TrigUR1:
                    case TubeType.TrigUR2:
                    case TubeType.TrigUR3:
                    case TubeType.TrigUR4:
                    case TubeType.TrigUR5:
                        DrawEccoTriangle(mid.X, mid.Y, mid.X, topLeft.Y, bottomRight.X, mid.Y, ColorMap.Tubes);
                        break;
                    default:
                        DrawBox(topLeft.X, topLeft.Y, bottomRight.X, bottomRight.Y, ColorMap.Tubes);
                        PutText(((int)(curObj.CollisionType)).ToString("X2"), mid.X, mid.Y, 1, 1, -1, -1, Color.Red, Color.Blue);
                        break;
                }
                addr = curObj.PtrNext;
            }
        }
        private void DrawWalls()
        {
            uint addr = ReadPtr(Addr2D.WallLLHead);
            Obj2D curObj;
            while (addr != 0)
            {
                ReadObj2D(addr, out curObj);
                DrawStandardCollisionType(GetScreenLoc(curObj.TopLeft),GetScreenLoc(curObj.BottomRight),curObj.Dims,curObj.CollisionType);
                addr = curObj.PtrNext;
            }
        }
        private void DrawStatics()
        {
            uint addr = ReadPtr(Addr2D.StaticLLHead);
            Obj2D curObj;
            Point topLeft = new Point();
            Point bottomRight = new Point();
            Point mid = new Point();
            Point vecStart = new Point();
            Size vel = new Size();
            ObjType type;
            while (addr != 0)
            {
                ReadObj2D(addr, out curObj);
                type = _2DTypeProvider.GetType(curObj.PtrMainFunc);
                mid = curObj.Mid;
                vecStart = mid;
                vel = curObj.Vel1;
                topLeft = GetScreenLoc(curObj.TopLeft);
                bottomRight = GetScreenLoc(curObj.BottomRight);
                switch (type)
                {
					case ObjType.Glyph:
					case ObjType.basic:
                    case ObjType.MovingBlock:
                    case ObjType.MovingBlock2:
                        break;
                    case ObjType.NullFunc:
                        vel.Width = vel.Height = 0;
                        break;
                    case ObjType.RemnantStars:
                        if ((curObj.AnimFrame <= 7) && (curObj.PtrSubObj == Addr2D.PlayerObj))
                        {
                            mid = GetScreenLoc(mid);
                            DrawRhomb(mid.X, mid.Y, 96, ColorMap.Mid);
                            PutText($"{((7 - curObj.AnimFrame) * 4) - (int)((_levelTime & 3) - 4)}", mid.X, mid.Y + 4, 1, 1, -1, -1, Color.Lime, Color.Blue);
                        }
                        break;
                    case ObjType.VortexGate: // 0xC0152
                        {
                            vecStart = curObj.Orig;
                            vel.Width = 0;
                            vel.Height = curObj.Vel2Y;
                            mid = GetScreenLoc(vecStart + vel);
                            Gui.DrawLine(mid.X, 0, mid.X, 448, ColorMap.HeadTail);
                            DrawBoxMWH(mid.X, mid.Y, 1, 1, Color.Blue, 0);
                        }
                        break;
                    case ObjType.AtlantisGateHS: //0xC3330
                        {
                            vecStart = curObj.Orig;
                            vel.Width = curObj.Vel2X;
                            vel.Height = 0;
                            mid = GetScreenLoc(vecStart + vel);
                            DrawBoxMWH(mid.X, mid.Y, 1, 1, Color.Blue, 0);
                        }
                        break;
                    case ObjType.AtlantisGateHM: //0xC35B0
                        {
                            vecStart = curObj.Orig;
                            vel.Width = curObj.Vel2X;
                            vel.Height = 0;
                            mid = GetScreenLoc(vecStart + vel);
							vecStart = GetScreenLoc(vecStart);
                            if ((curObj.Mode == 1) || (curObj.Mode == 3))
                            {
                                DrawOct(vecStart.X, vecStart.Y, 128, Color.Orange);
                            }
                            DrawBoxMWH(mid.X, mid.Y, 1, 1, Color.Blue, 0);
                        }
                        break;
                    case ObjType.AtlantisGateV: //0xC343A
                        {
                            vecStart = curObj.Orig;
                            vel.Width = 0;
                            vel.Height = curObj.Vel2Y;
                            mid = GetScreenLoc(vecStart + vel);
							vecStart = GetScreenLoc(vecStart);
							if ((curObj.Mode == 1) || (curObj.Mode == 3))
                            {
                                DrawOct(vecStart.X, vecStart.Y, 128, Color.Orange);
                            }
                            DrawBoxMWH(mid.X, mid.Y, 1, 1, Color.Blue, 0);
                        }
                        break;
                    case ObjType.AntiGravBall: //0xA579A
                        mid = GetScreenLoc(mid);
                        topLeft = GetScreenLoc(topLeft);
                        bottomRight = GetScreenLoc(bottomRight);
                        DrawOct(mid.X, mid.Y, Mem.ReadS16(addr + 0x4C), (_levelTime & 7) == 7 ? Color.Orange : Color.Gray);
                        DrawBox(topLeft.X, topLeft.Y, bottomRight.X, bottomRight.Y, ColorMap.Sonar);
                        DrawBox(topLeft.X, topLeft.Y, bottomRight.X, bottomRight.Y, ColorMap.HeadTail, 0);
                        topLeft.X = topLeft.Y = bottomRight.X = bottomRight.Y = _camX - 128;
                        break;
                    case ObjType.ConchBoss: //0xDF4E2
                        topLeft = GetScreenLoc(curObj.Orig);
                        DrawBox(topLeft.X - 96, 0 - _camY, topLeft.X + 96, Mem.ReadS16(Addr2D.LevelHeight) - _camY - 64, Color.Orange, 0);
                        Size vel2 = new Size(curObj.Vel2X, curObj.Vel2Y);
                        var Yvel1 = Mem.ReadS32(addr + 0x58) / 65536.0;
                        var Xvel2 = Mem.ReadS32(addr + 0x5C) / 65536.0;
                        var Yvel2 = Mem.ReadS32(addr + 0x60) / 65536.0;
                        StatusText($"Boss Mode: {curObj.Mode} Mode Counter: {curObj.StateCtr} Height Remaining: {Mem.ReadS16(Addr2D.LevelHeight) - 64 - mid.Y - _camY}", Color.Red);
                        StatusText($"Boss Vel1 X: {vel.Width / 65536.0,10:0.000000} Y: {vel.Height / 65536.0,10:0.000000}", Color.Red);
                        StatusText($"Boss Vel2 X: {vel2.Width / 65536.0,10:0.000000} Y: {vel2.Height / 65536.0,10:0.000000}", Color.Red);
                        vel += vel2;
                        StatusText($"Boss  Vel X: {vel.Width / 65536.0,10:0.000000} Y: {vel.Height / 65536.0,10:0.000000}", Color.Red);
                        switch (curObj.Mode)
                        {
                            case 0:
                                bottomRight.X = Math.Abs(mid.X - topLeft.X);
                                if (bottomRight.X > 0x48)
                                {
                                    bottomRight.X = (0x60 - bottomRight.X) << 1;
                                    bottomRight.Y = mid.Y + bottomRight.X;
                                }
                                else
                                {
                                    bottomRight.Y = mid.Y - (bottomRight.X >> 1) + 0x60;
                                }
                                DrawBoxMWH(topLeft.X, bottomRight.Y, 1, 1, Color.Gray, 0);
                                DrawBoxMWH(topLeft.X, 112 + _top, 72, 224, (curObj.StateCtr <= 1) ? Color.Orange : Color.Gray, 0);
                                break;
                            case 1:
                                DrawBoxMWH(topLeft.X, topLeft.Y, 1, 1, Color.Orange, 0);
                                bottomRight = _player2D.Mid - (Size)mid;
                                var rad = OctRad(bottomRight.X, bottomRight.Y) / 65536.0;
                                bottomRight.X = (int)(bottomRight.X * (256.0 / (rad + 1))) >> 20;
                                bottomRight.Y = (int)(bottomRight.Y * (256.0 / (rad + 1))) >> 20;
                                mid = GetScreenLoc(mid);
                                Gui.DrawLine(mid.X, mid.Y, mid.X + bottomRight.X, mid.Y + bottomRight.Y, Color.Gray);
                                StatusText($"Boss Slam X: {bottomRight.X / 512.0,10:0.000000} Y: {bottomRight.Y / 512.0,10:0.000000}", Color.Red);
                                break;
                            case 2:
                                StatusText($"Boss Slam X: {curObj.Var2X / 65536.0,10:0.000000} Y: {curObj.Var2Y / 65536.0,10:0.000000}", Color.Red);
                                break;
                        }
                        topLeft = GetScreenLoc(curObj.TopLeft);
                        bottomRight = GetScreenLoc(curObj.BottomRight);
                        break;
					case ObjType.unknown:
						mid = GetScreenLoc(curObj.Mid);
						PutText($"{curObj.PtrMainFunc:X5}:{curObj.ObjDefIndex}", mid.X, mid.Y - 4, 1, 1, -1, -9, Color.Lime, Color.Blue);
						PutText(addr.ToString("X6"), mid.X, mid.Y + 4, 1, 9, -1, -1, Color.Lime, Color.Blue);
						break;
                    default:
						mid = GetScreenLoc(curObj.Mid);
                        break;
                }
				if (_showNumbers)
				{
					mid = GetScreenLoc(curObj.Mid);
					PutText($"{curObj.PtrMainFunc:X5}:{curObj.ObjDefIndex}", mid.X, mid.Y - 4, 1, 1, -1, -9, Color.Lime, Color.Blue);
				}
				DrawStandardCollisionType(topLeft, bottomRight, curObj.Dims, curObj.CollisionType);
				DrawMotionVector(vecStart, vel);
                addr = curObj.PtrNext;
            }
        }
        private void DrawAnims()
        {
            Point topLeft = new Point();
            Point bottomRight = new Point();
            Point pos = new Point();
            Size vel = new Size();
            uint addr = ReadPtr(Addr2D.AnimLLHead);
            uint subAddr;
            Obj2D curObj, subObj;
            while (addr != 0)
            {
                ReadObj2D(addr, out curObj);
				ObjType o = _2DTypeProvider.GetType(curObj.PtrMainFunc);
				switch (o)
                {
                    case ObjType.ChainLinkNoseTail:
                        DrawChainLinks(curObj, ColorMap.Nose, ColorMap.Tail, ColorMap.NoseTail);
                        break;
                    case ObjType.ChainLinkAnyPoint:
                        DrawChainLinks(curObj, ColorMap.Nose, ColorMap.MidTail, ColorMap.AnyPoint);
                        break;
                    case ObjType.RockWorm:
                        DrawChainLinks(curObj, ColorMap.Nose, ColorMap.MidTail, ColorMap.AnyPoint);
                        pos = GetScreenLoc(curObj.Mid);
                        DrawOct(pos.X, pos.Y, 32, ColorMap.Sonar, 16);
                        StatusText($"Worm State: {curObj.unkb0} Mode: {curObj.Mode} Mode Counter: {curObj.Animation}", Color.Red);
                        StatusText($"Worm Vel X: {curObj.Vel1.Width / 65536.0,10:0.000000} Y: {curObj.Vel1.Height / 65536.0,10:0.000000}", Color.Red);
                        StatusText($"Worm Speed: {curObj.Var2X / 65536.0,10:0.000000} Tgt Speed:{curObj.Var2Y / 65536.0,10:0.000000}", Color.Red);
                        switch (curObj.Mode)
                        {
                            case 0:
                            case 2:
                            case 4:
                                bottomRight.X = _player2D.Nose.X - curObj.Mid.X;
                                bottomRight.Y = _player2D.Nose.Y - curObj.Mid.Y;
                                var rad = OctRad(bottomRight.X, bottomRight.Y) / 65536.0;
                                bottomRight.X = (int)(bottomRight.X * (256.0 / (rad + 1))) >> 20;
                                bottomRight.Y = (int)(bottomRight.Y * (256.0 / (rad + 1))) >> 20;
                                Gui.DrawLine(pos.X, pos.Y, pos.X + bottomRight.X, pos.Y + bottomRight.Y, ColorMap.Mid);
                                break;
                            default:
                                break;

                        }
                        break;
                    case ObjType.RetractedRockWorm:
                        DrawChainLinks(curObj, ColorMap.Nose, ColorMap.MidTail, ColorMap.AnyPoint);
                        {
                            ReadObj2D((uint)(curObj.Orig.X & 0xFFFFFF), out subObj);
                            pos = GetScreenLoc(subObj.Mid);
                            DrawOct(pos.X, pos.Y, 32, (subObj.Animation == 0) ? ColorMap.Sonar : Color.Gray, 16);
                            StatusText($"Worm State: {curObj.unkb0} Mode: {curObj.Mode} State Counter: {curObj.StateCtr} Mode Counter: {subObj.Animation}", Color.Red);
                            StatusText($"Worm Vel X: {subObj.Vel1.Width / 65536.0,10:0.000000} Y: {subObj.Vel1.Height / 65536.0,10:0.000000}", Color.Red);
                            StatusText($"Worm Speed: {curObj.Var2X / 65536.0,10:0.000000} Tgt Speed: {curObj.Var2Y / 65536.0,10:0.000000}", Color.Red);
                        }
                        break;
                    case ObjType.AbyssEel:
                        {
                            DrawChainLinks(curObj, ColorMap.Nose, ColorMap.MidTail, ColorMap.AnyPoint);
                            subAddr = curObj.PtrSubObj;
                            while (subAddr != 0)
                            {
                                ReadObj2D(subAddr, out subObj);
                                pos = GetScreenLoc(subObj.Mid);
                                DrawOct(pos.X, pos.Y, 48, Color.FromArgb(64, ColorMap.Sonar), 16);
                                subAddr = subObj.PtrSubObj;
                            }
                        }
                        break;
                    case ObjType.RemnantStars:
                        if ((curObj.AnimFrame <= 7) && (curObj.PtrSubObj == Addr2D.PlayerObj))
                        {
                            pos = GetScreenLoc(pos);
                            DrawRhomb(pos.X, pos.Y, 96, ColorMap.Mid);
                            PutText($"{((7 - curObj.AnimFrame) * 4) - (int)((_levelTime & 3) - 4)}", pos.X, pos.Y + 4, 1, 1, -1, -1, Color.Blue, Color.Red);
                        }
                        break;
                    case ObjType.StuckMagicArm:
                        pos = GetScreenLoc(curObj.Mid);
                        DrawOct(pos.X, pos.Y, 26, ColorMap.AnyPoint);
                        DrawMotionVector(curObj.Mid, curObj.Vel1);
                        break;
                    case ObjType.VortexSunflower:
                        {
                            subAddr = curObj.PtrSubObj;
                            while (subAddr != 0)
                            {
                                ReadObj2D(subAddr, out subObj);
                                pos = GetScreenLoc(subObj.Orig);
                                DrawOct(pos.X, pos.Y, 16 + (subObj.TopLeft.X >> 17), ColorMap.AnyPoint);
                                DrawOct(pos.X, pos.Y, 16 + (subObj.TopLeft.X >> 17), ColorMap.Spine, 0);
                                DrawMotionVector(subObj.Orig, subObj.Vel1);
                                subAddr = subObj.PtrSubObj;
                            }
                        }
                        break;
                    case ObjType.TubeWhirlpool:
                        pos = GetScreenLoc(curObj.Mid);
                        DrawOct(pos.X, pos.Y, curObj.Dims.Width >> 16, ColorMap.WallEject);
                        DrawMotionVector(curObj.Mid, curObj.Vel1);
                        break;
                    case ObjType.TubeWhirlpoolInactive:
                        pos = GetScreenLoc(curObj.Mid);
                        DrawOct(pos.X, pos.Y, curObj.Dims.Width >> 16, Color.Gray);
                        DrawMotionVector(curObj.Mid, curObj.Vel1);
                        break;
                    case ObjType.TwoTidesEventStartTrigger:
                        pos = GetScreenLoc(curObj.Mid);
                        if (curObj.Mode == 0)
                        {
                            DrawBoxMWH(pos.X, pos.Y, curObj.Dims.Width >> 16, curObj.Dims.Height >> 16, ColorMap.Nose);
                        }
                        else DrawBoxMWH(pos.X, pos.Y, 2, 2, ColorMap.Nose);
                        break;
                    case ObjType.TwoTidesEventEndTrigger:
                        pos = GetScreenLoc(curObj.Mid);
                        DrawBoxMWH(pos.X, pos.Y, curObj.Dims.Width >> 16, curObj.Dims.Height >> 16, ColorMap.Nose);
                        break;
                    case ObjType.LevelEndCutsceneTrigger:
                        pos = GetScreenLoc(curObj.Mid);
                        DrawBoxMWH(pos.X, pos.Y, 0xA0, 0x70, ColorMap.Mid);
                        break;
                    case ObjType.PoisonBubble:
                    case ObjType.LunarBayCutsceneEcco:  //By coincidence, these two objects have the exact same draw function
                        DrawDefaultBounds(curObj, ColorMap.HeadTail);
                        vel = new Size(curObj.Vel2X, curObj.Vel2Y);
                        vel += curObj.Vel1;
                        DrawMotionVector(curObj.Mid, vel);
                        break;
                    case ObjType.MedusaBoss:
                        subAddr = curObj.PtrSubObj;
                        uint next = ReadPtr(subAddr + 0x4);
                        while (next != 0)
                        {
                            subAddr = next;
                            next = ReadPtr(subAddr + 0x4);
                        }
                        ReadObj2D(subAddr, out subObj);
                        topLeft = GetScreenLoc(subObj.TopLeft);
                        bottomRight = GetScreenLoc(subObj.BottomRight);
                        DrawOct(topLeft.X, topLeft.Y, 32, ColorMap.Mid);
                        DrawOct(bottomRight.X, bottomRight.Y, 32, ColorMap.Mid);
                        pos = GetScreenLoc(curObj.Mid);
                        var octOff = (int)(Math.Sqrt(2) * 60) >> 1;
                        Point[] hemiOctPoints =
                        {
                            new Point(pos.X - 60, pos.Y),
                            new Point(pos.X - octOff, pos.Y - octOff),
                            new Point(pos.X, pos.Y - 60),
                            new Point(pos.X + octOff, pos.Y - octOff),
                            new Point(pos.X + 60, pos.Y)
                        };
                        Gui.DrawPolygon(hemiOctPoints, ColorMap.NoseTail, Color.FromArgb(63, ColorMap.NoseTail));
                        for (int l = 0; l < 4; l++)
                        {
                            Gui.DrawLine(hemiOctPoints[l].X, hemiOctPoints[l].Y, hemiOctPoints[l + 1].X, hemiOctPoints[l + 1].Y, ColorMap.NoseTail);
                        }
                        DrawBoxMWH(pos.X, pos.Y + 12, 52, 12, ColorMap.NoseTail);
                        DrawMotionVector(curObj.Mid, curObj.Vel1);
                        break;
                    case ObjType.GlobeHolderBoss:
                        if (curObj.Mode < 4)
                        {
                            // Draw top chain
                            subAddr = curObj.PtrSubObj2;
                            while (subAddr != 0)
                            {
                                ReadObj2D(subAddr, out subObj);
                                pos = GetScreenLoc(subObj.Mid);
                                DrawOct(pos.X, pos.Y, 12, Color.Orange);
                                DrawMotionVector(subObj.Mid, subObj.Vel1 + new Size(subObj.Vel2X, subObj.Vel2Y));
                                subAddr = subObj.PtrSubObj2;
                                if ((subAddr == 0) && ((curObj.Mode & 1) != 0))
                                {
                                    DrawOct(pos.X, pos.Y, subObj.Var1X >> 16, Color.Orange);
                                }
                            }
                            // Draw bottom chain
                            subAddr = curObj.PtrSubObj;
                            while (subAddr != 0)
                            {
                                ReadObj2D(subAddr, out subObj);
                                pos = GetScreenLoc(subObj.Mid);
                                DrawOct(pos.X, pos.Y, 12, Color.Orange);
                                DrawMotionVector(subObj.Mid, subObj.Vel1 + new Size(subObj.Vel2X, subObj.Vel2Y));
                                subAddr = subObj.PtrSubObj;
                                if ((subAddr == 0) && ((curObj.Mode & 2) != 0))
                                {
                                    DrawOct(pos.X, pos.Y, subObj.Var1X >> 16, Color.Orange);
                                }
                            }
                        }
                        pos = GetScreenLoc(curObj.Mid);
                        Point tmp = curObj.TopLeft;
                        Point tmp2 = curObj.BottomRight;
                        vel = curObj.Vel1 + new Size(curObj.Vel2X, curObj.Vel2Y);
                        DrawMotionVector(curObj.Mid, vel);
                        if (curObj.Mode < 7)
                        {
                            double overlap = 0;
                            DrawOct(pos.X, pos.Y, 0x5C, curObj.State == 0 ? ColorMap.Sonar : Color.Gray);
                            DrawOct(pos.X, pos.Y, 0x5C, ColorMap.NoseTail, 0);
                            subAddr = curObj.PtrPLC;
                            if (subAddr != 0)
                            {
                                ReadObj2D(subAddr, out subObj);
                                topLeft = subObj.TopLeft;
                                bottomRight = subObj.BottomRight;
                                while ((tmp.X > topLeft.X) && (tmp2.X < bottomRight.X) && (tmp.Y > topLeft.Y) && (tmp2.Y < bottomRight.Y) && ((vel.Width != 0) || (vel.Height != 0)))
                                {
                                    tmp += vel;
                                    tmp2 += vel;
                                }
                                overlap = Math.Max(Math.Max(topLeft.X - tmp.X, tmp2.X - bottomRight.X), Math.Max(topLeft.Y - tmp.Y, tmp2.Y - bottomRight.Y)) / 65536.0;
                                topLeft = GetScreenLoc(topLeft);
                                bottomRight = GetScreenLoc(bottomRight);
                                DrawBox(topLeft.X, topLeft.Y, bottomRight.X, bottomRight.Y, (overlap >= 6) ? Color.Orange : ColorMap.AnyPoint, 0);
                            }
                            topLeft = GetScreenLoc(curObj.TopLeft);
                            bottomRight = GetScreenLoc(curObj.BottomRight);
                            DrawBox(topLeft.X, topLeft.Y, bottomRight.X, bottomRight.Y, (overlap >= 6) ? Color.Orange : ColorMap.AnyPoint, (overlap >= 6) ? 63 : 0);
                            if (curObj.Mode < 4)
                            {
                                pos = GetScreenLoc(new Point(curObj.Var2X, curObj.Var2Y));
                                if ((curObj.Mode & 1) == 0) DrawOct(pos.X, pos.Y - 0xAE, 32, Color.Orange);
                                if ((curObj.Mode & 2) == 0) DrawOct(pos.X, pos.Y + 0xAE, 32, Color.Orange);
                            }
                            StatusText($"Bose Mode: {curObj.Mode} HP: {curObj.AnimFrame,2} Invuln Counter: {curObj.State}", Color.Red);
                        }
                        else if (curObj.Mode == 8)
                        {
                            DrawOct(pos.X - 16, pos.Y - 16, 12, ColorMap.Mid);
                        }
                        break;
                    case ObjType.VortexQueenBoss:
                        {
                            var mode = curObj.ScreenX;
                            var modeCounter = curObj.ScreenY;
                            pos = GetScreenLoc(curObj.Mid);
                            Point temp;
                            if (curObj.Mode < 5)
                            {
                                octOff = (int)(80 * Math.Sqrt(2)) >> 1;
                                Point hexOff = Intersection(new Point(-80, 0), new Point(-octOff, -octOff), new Point(-80, -32), new Point(-octOff, -32)).Value;
                                Point[] roundedRect = {
                                    new Point(pos.X -       80, pos.Y),
                                    new Point(pos.X + hexOff.X, pos.Y - 32),
                                    new Point(pos.X - hexOff.X, pos.Y - 32),
                                    new Point(pos.X +       80, pos.Y),
                                    new Point(pos.X - hexOff.X, pos.Y + 32),
                                    new Point(pos.X + hexOff.X, pos.Y + 32)
                                };
                                Gui.DrawPolygon(roundedRect, Color.Orange, Color.FromArgb(63, Color.Orange));
                            }
                            DrawMotionVector(curObj.Mid, curObj.Vel1);
                            StatusText($"Tongue State: {curObj.State:X2} State Counter: {curObj.StateCtr} Mode: {mode} Mode Counter: {modeCounter}:{curObj.Animation & 0xF}", Color.Red);
                            subAddr = curObj.PtrSubObj;
                            ReadObj2D(subAddr, out subObj);
                            var subModeCounter = subObj.StateCtr;
                            pos = GetScreenLoc(new Point(subObj.Mid.X, subObj.Var1Y));
                            vel = new Size(subObj.Vel2X, subObj.Vel2Y);
                            pos.Y -= 32;
                            var levelHeight = Mem.ReadS16(Addr2D.LevelHeight) - _camY;
                            switch (subObj.Mode)
                            {
                                case 0:
                                    DrawBox(pos.X - 32, pos.Y - ((curObj.State == 5) ? 0x60 : 0x70), pos.X + 32, pos.Y - 16, ColorMap.Mid);
                                    break;
                                case 2:
                                    temp = GetScreenLoc(new Point(subObj.Var2X, subObj.Var2Y));
                                    Gui.DrawLine(pos.X - 48, temp.Y, pos.X + 48, temp.Y, Color.Orange);
                                    DrawBoxMWH(pos.X, pos.Y + 32, 1, 1, Color.Orange, 0);
                                    break;
                                case 3:
                                    subModeCounter = subObj.State;
                                    break;
                                case 5:
                                    Point[] throatShape =
                                    {
                                        new Point(pos.X - 48, levelHeight),
                                        new Point(pos.X - 48, pos.Y + 60),
                                        new Point(pos.X - 16, pos.Y + 20),
                                        new Point(pos.X + 16, pos.Y + 20),
                                        new Point(pos.X + 48, pos.Y + 60),
                                        new Point(pos.X + 48, levelHeight)
                                    };
                                    Gui.DrawPolygon(throatShape, ColorMap.Mid, Color.FromArgb(63, ColorMap.Mid));
                                    DrawOct(pos.X, pos.Y, 24, ColorMap.Mid);
                                    DrawOct(pos.X, pos.Y, 24, ColorMap.AnyPoint, 0);
                                    break;
                                case 6:
                                    if ((curObj.State != 7) && (curObj.SonarCtr == 0) && (mode != 7))
                                    {
                                        DrawOct(pos.X, pos.Y + 16, 64, ColorMap.Sonar);
                                    }
                                    if (mode == 7)
                                    {
                                        uint subAddr2 = ReadPtr(Addr2D.SubObjLLHead);
                                        while (subAddr2 != 0)
                                        {
                                            ReadObj2D(subAddr2, out Obj2D subObj2);
                                            if (subObj2.Flags == 0xFF)
                                            {
                                                DrawMotionVector(subObj2.Mid, subObj2.Vel1);
                                            }
                                            subAddr2 = subObj2.PtrNext;
                                        }
                                    }
                                    int MaxY = subObj.Var2Y >> 16 - _camY;
                                    temp = GetScreenLoc(new Point(subObj.Var2X, subObj.Var2Y));
                                    Gui.DrawLine(pos.X - 48, temp.Y - 94, pos.X + 48, temp.Y - 94, Color.Orange);
                                    Gui.DrawLine(pos.X - 48, temp.Y, pos.X + 48, temp.Y, Color.Orange);
                                    DrawBoxMWH(pos.X, pos.Y + 32, 1, 1, Color.Orange, 0);
                                    break;
                                default:
                                    break;
                            }
                            if ((subObj.Mode < 7) || ((subObj.Mode == 7) && (ReadPtr(Addr2D.SpecLLHead) != 0)))
                            {
                                if (subObj.Animation == 0)
                                {
                                    DrawOct(pos.X, pos.Y, 32, ColorMap.Mid);
                                    DrawBox(pos.X - 48, pos.Y + 32, pos.X + 48, levelHeight, ColorMap.Mid);
                                }
                                temp = GetScreenLoc(new Point(subObj.Var2X, subObj.Var2Y));
                                temp.Y -= 94;
                                Gui.DrawLine(pos.X - 48, temp.Y, pos.X + 48, temp.Y, Color.Orange);
                                DrawBoxMWH(pos.X, pos.Y + 32, 1, 1, Color.Orange, 0);
                            }
                            if (subObj.PtrMainFunc == 0xE17B4)
                            {
                                Point[] shapePoints =
                                {
                                    new Point(pos.X - 48, levelHeight),
                                    new Point(pos.X - 48, pos.Y + 60),
                                    new Point(pos.X - 16, pos.Y + 20),
                                    new Point(pos.X + 16, pos.Y + 20),
                                    new Point(pos.X + 48, pos.Y + 60),
                                    new Point(pos.X + 48, levelHeight)
                                };
                                Gui.DrawPolygon(shapePoints, ColorMap.Mid, Color.FromArgb(63, ColorMap.Mid));
                                DrawOct(pos.X, pos.Y, 24, ColorMap.Mid);
                                DrawOct(pos.X, pos.Y, 24, ColorMap.AnyPoint, 0);
                            }
                            temp = GetScreenLoc(new Point(subObj.Var2X, subObj.Var2Y));
                            temp.Y -= 264;
                            DrawBoxMWH(160 + _left, temp.Y, 320, 12, (32 < curObj.StateCtr) && (curObj.StateCtr < 160) ? Color.Brown : Color.Gray);
                            if ((32 < curObj.StateCtr) && (curObj.StateCtr < 160))
                            {
                                DrawBoxMWH(_left + 160, temp.Y, 320, 12, ColorMap.AnyPoint, 0);
                            }
                            DrawMotionVector(new Point(subObj.Mid.X, subObj.Var1Y), vel);
                            StatusText($"Body Mode: {subObj.Mode:X2} Mode Counter: {subModeCounter} HP: {curObj.HP} Sonar Counter: {curObj.SonarCtr}", Color.Red);
                            curObj.HP = 0;
                        }
                        break;
                    case ObjType.VortexLarva:
                        subAddr = curObj.PtrSubObj;
                        while (subAddr != 0)
                        {
                            ReadObj2D(subAddr, out subObj);
                            topLeft = GetScreenLoc(subObj.Orig);
                            bottomRight = GetScreenLoc(subObj.Mid);
                            DrawOct(topLeft.X, topLeft.Y, 30, ColorMap.Solid, 32);
                            DrawOct(topLeft.X, topLeft.Y, 30, ColorMap.Spine, 0);
                            DrawOct(bottomRight.X, bottomRight.Y, 30, ColorMap.Solid, 32);
                            DrawOct(bottomRight.X, bottomRight.Y, 30, ColorMap.Spine, 0);
                            subAddr = subObj.PtrSubObj;
                        }
                        pos = GetScreenLoc(curObj.Mid);
                        vel = curObj.Vel1 + new Size(curObj.Vel2X, curObj.Vel2Y);
                        bottomRight = GetScreenLoc(new Point(curObj.Var2X, curObj.Var2Y));
                        DrawOct(pos.X, pos.Y, 0xB0, ColorMap.Spine);
                        DrawOct(pos.X, pos.Y, 0xB0, ColorMap.Mid, 0);
                        DrawOct(pos.X, pos.Y, 0x70, ColorMap.Mid);
                        DrawOct(pos.X, pos.Y, 0x38, ColorMap.AnyPoint);
                        DrawOct(pos.X, pos.Y, 0x38, ColorMap.Mid, 0);
                        DrawOct(pos.X, pos.Y, 48, ColorMap.Sonar, ((curObj.HP > 2) && (curObj.unkb0 != 0)) ? 63 : 0);
                        DrawOct(bottomRight.X, bottomRight.Y, 32, Color.Orange);
                        DrawMotionVector(pos, vel);
                        Gui.DrawLine(pos.X, pos.Y, bottomRight.X, bottomRight.Y, Color.Orange);
                        StatusText($"Larva  Mode: {curObj.unkb0:X2}  Mode Counter: {curObj.StateCtr:D2}", Color.Red);
                        StatusText($"Larva State: {curObj.State:X2} State Counter: {curObj.XChunk:D3}", Color.Red);
                        StatusText($"Larva Vel X: {vel.Width / 65536.0,10:0.000000} Y: {vel.Height / 65536.0,10:0.000000}", Color.Red);
                        break;
                    case ObjType.FutureDolphin:
                        DrawDefaultBounds(curObj, ColorMap.Mid, ColorMap.Sonar);
                        pos = GetScreenLoc(curObj.Mid);
                        DrawOct(pos.X, pos.Y, 4, ColorMap.Mid);
                        DrawMotionVector(curObj.Mid, curObj.Vel1);
                        break;
                    case ObjType.PushableRock:
                        DrawPushableBounds(curObj);
                        DrawMotionVector(curObj.Mid, curObj.Vel1);
                        StatusText($"Rock Vel X: {curObj.Vel1.Width / 65536.0,10:0.000000} Y: {curObj.Vel1.Height / 65536.0,10:0.000000}", Color.Green);
                        break;
                    case ObjType.PushableShieldingRock:
                        { 
                            DrawPushableBounds(curObj);
                            DrawMotionVector(curObj.Mid, curObj.Vel1);
                            DrawShieldBounds(curObj);
                            StatusText($"Rock Vel X: {curObj.Vel1.Width / 65536.0,10:0.000000} Y: {curObj.Vel1.Height / 65536.0,10:0.000000}", Color.Green);
                        }
                        break;
                    case ObjType.Turtle:
                        switch (curObj.Mode)
                        {
                            case 0:
                            case 1:
                            case 2:
                            case 3:
                                vel.Width = curObj.Var2X;
                                break;
                            case 4:
                            case 5:
                            case 6:
                            case 7:
                                vel.Width = -curObj.Var2X;
                                break;
                            default:
                                vel.Width = 0;
                                break;
                        }
                        vel.Height = 0;
                        DrawPushableBounds(curObj);
                        DrawShieldBounds(curObj);
                        StatusText($"Turtle Vel X: {curObj.Var2X / 65536.0,10:0.000000} Y: {curObj.Var2Y / 65536.0,10:0.000000}", Color.Green);
                        DrawMotionVector(curObj.Mid, vel);
                        break;
                    case ObjType.RetractingTurtle:
                        vel.Width = curObj.Var2X >> 1;
                        vel.Height = curObj.Var2Y >> 1;
                        DrawPushableBounds(curObj);
                        DrawShieldBounds(curObj);
                        StatusText($"Turtle Vel X: {(curObj.Var2X >> 1) / 65536.0,10:0.000000} Y: {(curObj.Var2Y >> 1) / 65536.0,10:0.000000}", Color.Green);
                        DrawMotionVector(curObj.Mid, vel);
                        break;
                    case ObjType.BlueWhale:
                        pos = GetScreenLoc(curObj.Mid);
                        pos.Y -= 64;
                        DrawEccoOct_scaled(pos.X, pos.Y, 2, 0, 0x50, ColorMap.Mid, 31);
                        DrawEccoOct_scaled(pos.X, pos.Y, 2, 0, 0x40, ColorMap.Mid, 31);
                        DrawEccoOct_scaled(pos.X, pos.Y, 2, 0, 0x30, ColorMap.Mid, 31);
                        DrawEccoOct_scaled(pos.X, pos.Y, 2, 0, 0x20, ColorMap.Mid, 31);
                        DrawEccoOct_scaled(pos.X, pos.Y, 2, 0, 0x10, ColorMap.Mid, 31);
                        topLeft = pos;
                        if (curObj.SonarCtr == 0)
                        {
                            topLeft.X += (curObj.StateCtr == 0) ? -278 : 162;
                            topLeft.Y += 44 - (curObj.Dims.Height >> 16);
                            DrawOct(topLeft.X, topLeft.Y, 32, ColorMap.Sonar);
                        }
                        DrawMotionVector(curObj.Mid + new Size(0, -64 << 16), curObj.Vel1);
                        break;
                    case ObjType.VortexSoldier:
                        pos = GetScreenLoc(curObj.Mid);
                        vel = curObj.Vel1 + new Size(curObj.ScreenX << 16, curObj.ScreenY << 16);
                        if (curObj.TrgAng == 0)
                        {
                            DrawRectRhombusIntersection(new Point(pos.X, pos.Y + 6), new Point(pos.X, pos.Y), 50, 44, 64, ColorMap.Mid);
                        }
                        DrawRectRhombusIntersection(new Point(pos.X, pos.Y - 25), new Point(pos.X, pos.Y), 38, 47, 64, ColorMap.Mid);
                        DrawBoxMWH(pos.X, pos.Y, curObj.Dims.Width >> 16, curObj.Dims.Height >> 16, ColorMap.Sonar, 16);
                        DrawMotionVector(curObj.Mid, vel);
                        break;
                    case ObjType.Glyph:
                        pos = GetScreenLoc(curObj.Mid);
                        if ((curObj.TrgAng == 0) && (Mem.ReadByte(Addr2D.P1ExistsFlag) != 0) 
                            && ((curObj.ObjDefIndex == 0x14) || (curObj.ObjDefIndex == 0x97))) // Is actually a barrier glyph
                        {
                            DrawOct(pos.X, pos.Y, 70, ColorMap.Mid);
                        }
                        DrawBoxMWH(pos.X, pos.Y, curObj.Dims.Width >> 16, curObj.Dims.Height >> 16, ColorMap.Sonar);
                        DrawBoxMWH(pos.X, pos.Y, curObj.Dims.Width >> 16, curObj.Dims.Height >> 16, ColorMap.HeadTail, 0);
                        DrawMotionVector(curObj.Mid, curObj.Vel1);
                        break;
                    case ObjType.BarrierGlyph:
                        pos = GetScreenLoc(curObj.Mid);
                        if ((curObj.TrgAng == 0) && (Mem.ReadByte(Addr2D.P1ExistsFlag) != 0))
                        {
                            DrawOct(pos.X, pos.Y, 70, ColorMap.Mid);
                        }
                        DrawBoxMWH(pos.X, pos.Y, curObj.Dims.Width >> 16, curObj.Dims.Height >> 16, ColorMap.Sonar);
                        DrawBoxMWH(pos.X, pos.Y, curObj.Dims.Width >> 16, curObj.Dims.Height >> 16, ColorMap.HeadTail, 0);
                        DrawMotionVector(curObj.Mid, curObj.Vel1);
                        break;
                    case ObjType.ForceField:
                        {
                            vel = new Size(_player2D.Mid.X - curObj.Mid.X, _player2D.Mid.Y - curObj.Mid.Y);
                            var div = Math.Abs(vel.Width) + Math.Abs(vel.Height);
                            vel.Width /= div;
                            vel.Height /= div;
                            DrawMotionVector(curObj.Mid, vel);
                        }
                        break;
                    case ObjType.CetaceanGuide:
                        {
                            topLeft = GetScreenLoc(curObj.TopLeft);
                            bottomRight = GetScreenLoc(curObj.BottomRight);
                            pos = GetScreenLoc(curObj.Mid);
                            vel = curObj.Vel1;
                            Point dest = new Point((curObj.ScreenX << 7) + 0x40 - _camX, (curObj.ScreenY << 7) + 0x40 - _camY);
                            DrawBox(topLeft.X, topLeft.Y, bottomRight.X, bottomRight.Y, ColorMap.Sonar);
                            DrawMotionVector(curObj.Mid, vel);
                            DrawBoxMWH(dest.X, dest.Y, 64, 64, Color.Orange);
                            Gui.DrawLine(pos.X, pos.Y, dest.X, dest.Y, Color.Orange);
                            StatusText($"Guide Pos X: {pos.X} Y: {pos.Y} Angle: {curObj.CurAng} Target Angle: {curObj.TrgAng}", Color.Green);
                            StatusText($"Guide Vel X: {vel.Width / 65536.0,10:0.000000} Y:{vel.Height / 65536.0,10:0.000000}", Color.Green);
                            StatusText($"Guide Speed: {curObj.Var2X / 65536.0,10:0.000000} Target: {curObj.Var2Y / 65536.0,10:0.000000}", Color.Green);
                        }
                        break;
                    case ObjType.OrcaLost:
                        pos = GetScreenLoc(curObj.Mid);
                        topLeft = GetScreenLoc(curObj.Orig);
                        if (curObj.StateCtr == 0)
                        {
                            if (curObj.CurAng == 0)
                            {
                                DrawBox(pos.X + 8, pos.Y - 32, pos.X + 64, pos.Y + 32, ColorMap.Mid);
                            }
                            else
                            {
                                DrawBox(pos.X - 64, pos.Y - 32, pos.X - 8, pos.Y + 32, ColorMap.Mid);
                            }
                        }
                        Gui.DrawLine(topLeft.X - 80, pos.Y, topLeft.X + 80, pos.Y, Color.Green);
                        DrawMotionVector(curObj.Mid, curObj.Vel1);
                        break;
                    case ObjType.OrcaIgnorable:
                        DrawMotionVector(curObj.Mid, curObj.Vel1);
                        break;
                    case ObjType.OrcaMother:
                        pos = GetScreenLoc(curObj.Mid);
                        DrawBoxMWH(pos.X, pos.Y, 80, 32, ColorMap.Mid, 31);
                        DrawBoxMWH(pos.X, pos.Y, curObj.Dims.Width >> 16, curObj.Dims.Height >> 16, ColorMap.Sonar);
                        if (_player2D.PtrCarriedObj != 0)
                        {
                            DrawOct(pos.X, pos.Y, 0x50, ColorMap.Mid, 31);
                        }
                        DrawMotionVector(curObj.Mid, curObj.Vel1);
                        break;
                    case ObjType.GlyphBaseBroken:
                        pos = GetScreenLoc(curObj.Mid);
                        DrawBoxMWH(pos.X, pos.Y, curObj.Dims.Width >> 16, curObj.Dims.Height >> 16, ColorMap.HeadTail);
                        if (curObj.Mode == 0)
                        {
                            DrawRectRhombusIntersection(new Point(pos.X, pos.Y), new Point(pos.X, pos.Y), 80, 80, 120, Color.Orange);
                        }
                        DrawMotionVector(curObj.Mid, curObj.Vel1);
                        break;
                    case ObjType.GlyphTopBroken:
                        DrawPushableBounds(curObj, ColorMap.Sonar);
                        DrawMotionVector(curObj.Mid, curObj.Vel1);
                        StatusText($"Glyph Vel X: {curObj.Vel1.Width / 65536.0,10:0.000000} Y: {curObj.Vel1.Height / 65536.0,10:0.000000}", Color.Green);
                        break;
                    case ObjType.GlyphTopReparing:
                        {
                            ReadObj2D(curObj.PtrSubObj, out subObj);
                            pos = GetScreenLoc(curObj.Mid);
                            vel = new Size(subObj.Mid.X - curObj.Mid.X, subObj.Mid.Y - curObj.Mid.Y);
                            DrawBoxMWH(pos.X, pos.Y, curObj.Dims.Width >> 16, curObj.Dims.Height >> 16, Color.Gray);
                            Point dest = GetScreenLoc(subObj.Mid);
                            DrawRhomb(dest.X, dest.Y, 3, Color.Orange);
                            DrawMotionVector(curObj.Mid, vel);
                        }
                        break;
                    case ObjType.MirrorDolphin:
                        pos = GetScreenLoc(curObj.Mid);
                        topLeft = GetScreenLoc(curObj.Orig);
                        topLeft.X += curObj.Mode == 0 ? 27 : -27;
                        vel = new Size(curObj.Vel2X, curObj.Vel2Y);
                        vel += curObj.Vel1;
                        DrawBoxMWH(pos.X, pos.Y, curObj.Dims.Width >> 16, curObj.Dims.Height >> 16, ColorMap.Sonar, 31);
                        if (curObj.ObjDefIndex != 0xAC) // If we aren't the type that doesn't need a fish
                        {
                            DrawBoxMWH(pos.X, pos.Y, 96, 96, Color.Orange); // Show fish detection box
                        }
                        Gui.DrawLine(topLeft.X, 0, topLeft.X, 448, ColorMap.Mid);
                        DrawMotionVector(curObj.Mid, vel);
                        break;
                    case ObjType.VortexLightningTrap:
                        pos = GetScreenLoc(curObj.Mid);
                        if (_player2D.Form != 0)
                        {
                            pos.Y -= 8;
                        }
                        if (curObj.Mode != 0)
                        {
                            DrawBoxMWH(pos.X, pos.Y, 92, 16, ColorMap.Mid);
                            PutText(curObj.Mode.ToString(), pos.X, pos.Y, 1, 1, -1, -1, Color.Blue, Color.Red);
                        }
                        else
                        {
                            DrawBoxMWH(pos.X, pos.Y, 92, 16, Color.Gray);
                            PutText(curObj.SonarCtr.ToString(), pos.X, pos.Y, 1, 1, -1, -1, Color.Blue, Color.Red);
                        }
                        break;
                    case ObjType.SkyBubbles:
                        topLeft = GetScreenLoc(curObj.TopLeft);
                        bottomRight = GetScreenLoc(curObj.BottomRight);
                        pos = GetScreenLoc(curObj.Mid);
                        switch (curObj.Mode)
                        {
                            case 0:
                                DrawRectRhombusIntersection(new Point(pos.X, pos.Y), new Point(pos.X, pos.Y), 70, 70, 105, Color.Gray);
                                DrawRectRhombusIntersection(new Point(pos.X, pos.Y), new Point(pos.X, pos.Y), 70, 70, 105, ColorMap.Mid, 0);
                                DrawBox(topLeft.X, topLeft.Y, bottomRight.X, bottomRight.Y, ColorMap.Sonar);
                                break;
                            case 1:
                                DrawRectRhombusIntersection(new Point(pos.X, pos.Y), new Point(pos.X, pos.Y), 70, 70, 105, ColorMap.Mid);
                                break;
                            case 2:
                                DrawBox(topLeft.X, topLeft.Y, bottomRight.X, bottomRight.Y, ColorMap.Mid);
                                break;
                            default:
                                DrawBox(topLeft.X, topLeft.Y, bottomRight.X, bottomRight.Y, Color.Gray);
                                break;
                        }
                        DrawMotionVector(curObj.Mid, curObj.Vel1);
                        break;
                    case ObjType.AirPocket:
                        DrawDefaultBounds(curObj, Color.Transparent, ColorMap.Nose);
                        break;
                    case ObjType.PushableFish:
                        DrawDefaultBounds(curObj, ColorMap.Nose, ColorMap.Sonar);
                        DrawMotionVector(curObj.Mid, curObj.Vel1);
                        StatusText($"Fish Vel X: {curObj.Vel1.Width / 65536.0,10:0.000000} Y: {curObj.Vel1.Height / 65536.0,10:0.000000}", Color.Orange);
                        break;
                    case ObjType.SlowKelp:
                    case ObjType.MetaSphere:
                        DrawDefaultBounds(curObj, ColorMap.Nose);
                        DrawMotionVector(curObj.Mid, curObj.Vel1);
                        break;
                    case ObjType.StarWreath:
                        pos = GetScreenLoc(curObj.Mid);
                        DrawMotionVector(curObj.Mid, curObj.Vel1, Color.Orange);
                        if ((curObj.PID & 7) == 0)
                        {
                            DrawMotionVector(curObj.Orig, new Size(curObj.Vel2X, curObj.Vel2Y), curObj.SonarCtr == 0 ? Color.Blue : Color.Black);
                            StatusText($"Wreath Vel X: {curObj.Vel2X / 65536.0,10:0.000000} Y: {curObj.Vel2Y / 65536.0,10:0.000000} Sonar Counter: {curObj.SonarCtr}", Color.Green);
                        }
                        break;
                    case ObjType.Fish:
                        pos = GetScreenLoc(curObj.Mid);
                        DrawBoxMWH(pos.X, pos.Y, 0x14, 0x14, ColorMap.Nose);
                        DrawMotionVector(curObj.Mid, curObj.Vel1);
                        break;
                    case ObjType.EnemyDolphin:
                        pos = GetScreenLoc(curObj.Orig);
                        DrawBoxMWH(pos.X, pos.Y, 1024, 1024, Color.Orange, 0);
                        DrawDefaultBounds(curObj, ColorMap.Mid);
                        DrawMotionVector(curObj.Mid, curObj.Vel1);
                        break;
                    case ObjType.DroneFightingDolphin:
                        pos = GetScreenLoc(new Point(curObj.Var2X, curObj.Var2Y));
                        DrawOct(pos.X, pos.Y, 360, ColorMap.Mid, 0);
                        break;
                    case ObjType.DroneFightingDolphinSonarBlast:
                        pos = GetScreenLoc(curObj.Mid);
                        DrawBox(pos.X, pos.Y - 1, pos.X + 32, pos.Y + 1, Color.Orange);
                        break;
                    case ObjType.AsteriteGlobe:
                        if (curObj.Mode == 1)
                        {
                            DrawPushableBounds(curObj, ColorMap.Sonar);
                        }
                        DrawMotionVector(curObj.Mid, curObj.Vel1);
                        break;
                    case ObjType.AsteriteGlobeFollowing:
                        pos = GetScreenLoc(_player2D.Mid);
                        DrawOct(pos.X - 56, pos.Y, 8, Color.Orange);
                        DrawOct(pos.X + 56, pos.Y, 8, Color.Orange);
                        DrawMotionVector(curObj.Mid, new Size(0,0)-curObj.Vel1);
                        DrawMotionVector(new Point(curObj.Var2X, curObj.Var2Y), curObj.Vel1);
                        break;
                    case ObjType.AsteriteGlobeOrbiting:
                        DrawMotionVector(curObj.Mid, new Size(-curObj.Vel2X, -curObj.Vel2Y));
                        DrawMotionVector(new Point(curObj.Var2X, curObj.Var2Y), new Size(curObj.Vel2X, curObj.Vel2Y));
                        break;
                    case ObjType.FourIslandsControlPoint:
                        DrawDefaultBounds(curObj, curObj.Mode == 0 ? Color.Orange : ColorMap.Sonar);
                        DrawMotionVector(curObj.Mid, curObj.Vel1);
                        break;
                    // Crystal Springs merging glyphs
                    case ObjType.MergingGlyphBound:
                        pos = GetScreenLoc(curObj.Orig);
                        DrawRhomb(pos.X, pos.Y, 4 << 4, Color.Orange);
                        DrawDefaultBounds(curObj, ColorMap.Sonar);
                        DrawMotionVector(curObj.Mid, curObj.Vel1);
                        break;
                    case ObjType.MergingGlyphFree:
                        DrawDefaultBounds(curObj, ColorMap.HeadTail, ColorMap.Sonar);
                        DrawMotionVector(curObj.Mid, curObj.Vel1);
                        break;
                    case ObjType.MergingGlyphPulled:
                        subAddr = curObj.PtrSubObj;
                        ReadObj2D(subAddr, out subObj);
                        vel = subObj.Vel1;
                        for (int i = 1; i < curObj.SonarCtr; i++)
                        {
                            // Rotate vel vector by 90 degrees.
                            vel.Width ^= vel.Height;
                            vel.Height ^= vel.Width;
                            vel.Width ^= vel.Height;
                            vel.Width = 0 - vel.Width;
                        }
                        pos = GetScreenLoc(curObj.Orig + vel);
                        DrawRhomb(pos.X, pos.Y, 3, Color.Orange);
                        DrawDefaultBounds(curObj, ColorMap.HeadTail, Color.Transparent);
                        pos = GetScreenLoc(curObj.Mid);
                        DrawBoxMWH(pos.X, pos.Y, 1, 1, Color.Orange, 0);
                        break;
                    case ObjType.MergingGlyphGoal:
                        vel = curObj.Vel1;
                        for (int i = 0; i < 4; i++)
                        {
                            DrawMotionVector(curObj.Mid, vel);
                            // Rotate vel vector by 90 degrees.
                            vel.Width ^= vel.Height;
                            vel.Height ^= vel.Width;
                            vel.Width ^= vel.Height;
                            vel.Width = 0 - vel.Width;
                        }
                        break;
                    case ObjType.MergingGlyphDelivered:
                        vel = new Size(0, 0) - curObj.Vel1;
                        DrawMotionVector(curObj.Mid, vel);
                        break;
                    case ObjType.MergingGlyphMerging1:
                        pos = GetScreenLoc(curObj.Mid);
                        DrawOct(pos.X, pos.Y, 3, Color.Orange);
                        break;
                    case ObjType.MergingGylphMerging2:
                        vel = new Size(0, 0) - curObj.Vel1;
                        DrawMotionVector(curObj.Mid, vel);
                        break;
                    case ObjType.SongEraser:
                        DrawDefaultBounds(curObj, ColorMap.Mid);
                        uint dropSpeed = curObj.unkb1;
                        DrawMotionVector(curObj.Mid, new Size(0, curObj.unkb2));
                        break;
                    // Default boundaries, sonar-responsive
                    case ObjType.PulsarPowerUp:
                    case ObjType.VortexBulletSpawner:
                    case ObjType.FriendlyDolphin:
                    case ObjType.MirrorDolphinCharging1:
                    case ObjType.MirrorDolphinCharging2:
                    case ObjType.TrelliaAfterCutscene:
                        DrawDefaultBounds(curObj, ColorMap.Sonar);
                        DrawMotionVector(curObj.Mid, curObj.Vel1);
                        break;
                    // Not directly active, but worth displaying
                    case ObjType.MetaSphereInactive:
                    case ObjType.TrelliaDuringCutscene:
                    case ObjType.EnemySpawner:
                    case ObjType.CreditsDolphin:
                        DrawDefaultBounds(curObj, Color.Gray);
                        DrawMotionVector(curObj.Mid, curObj.Vel1);
                        break;
                    case ObjType.ImmobileEnemy:
                        DrawDefaultBounds(curObj, ColorMap.HeadTail, ColorMap.Enemy);
						pos = GetScreenLoc(curObj.Mid);
						if (curObj.HP > 2)
                        {
                            PutText($"{curObj.HP - 1}", pos.X, pos.Y, 1, 1, -1, -9, Color.Blue, Color.Red);
                        }
                        break;
                    case ObjType.DefaultEnemy:
                        DrawDefaultBounds(curObj, ColorMap.HeadTail, ColorMap.Enemy);
                        DrawMotionVector(curObj.Mid, curObj.Vel1);
						pos = GetScreenLoc(curObj.Mid);
						if (curObj.HP > 2)
                        {
                            PutText($"{curObj.HP - 1}", pos.X, pos.Y, 1, 1, -1, -9, Color.Blue, Color.Red);
                        }
                        break;
					case ObjType.MovingBlock:
					case ObjType.MovingBlock2:
						DrawDefaultBounds(curObj, ColorMap.HeadTail);
						DrawMotionVector(curObj.Mid, curObj.Vel1);
						break;
					case ObjType.NullFunc:
                    case ObjType.NoDisplay:
                        break;
					case ObjType.unknown:
						DrawDefaultBounds(curObj, ColorMap.HeadTail);
						DrawMotionVector(curObj.Mid, curObj.Vel1);
						pos = GetScreenLoc(curObj.Mid);
						PutText($"{curObj.PtrMainFunc:X5}:{curObj.ObjDefIndex}", pos.X, pos.Y - 4, 1, 9, -1, -1, Color.Blue, Color.Red);
						break;
					default:
						DrawDefaultBounds(curObj, ColorMap.HeadTail);
                        DrawMotionVector(curObj.Mid, curObj.Vel1);
						pos = GetScreenLoc(curObj.Mid);
                        break;
                }
				if (_showNumbers) {
					pos = GetScreenLoc(curObj.Mid);
					PutText($"{curObj.PtrMainFunc:X5}:{curObj.ObjDefIndex}", pos.X, pos.Y - 4, 1, 9, -1, -1, Color.Blue, Color.Red);
				}
				addr = curObj.PtrNext;
            }
        }
        private void DrawEvents()
        {
            uint addr = ReadPtr(Addr2D.EventLLHead);
            Obj2D curObj;
            Obj2D subObj;
            Point pos = new Point();
            Point pos2 = new Point();
            while (addr != 0)
            {
                ReadObj2D(addr, out curObj);
                switch (_2DTypeProvider.GetType(curObj.PtrMainFunc))
                {
                    case ObjType.TeleportRingFixedRad:
                        pos = GetScreenLoc(curObj.Orig);
                        DrawOct(pos.X, pos.Y, 32, ColorMap.Mid);
                        break;
                    case ObjType.Current:
                        if ((curObj.Vel1.Width != 0) || (curObj.Vel1.Height != 0))
                        {
                            DrawDefaultBounds(curObj, Color.FromArgb(63, ColorMap.Mid));
                            DrawMotionVector(curObj.Mid, curObj.Vel1);
                        }
                        break;
                    case ObjType.AbyssDeathEel:
                        pos = GetScreenLoc(curObj.Mid);
                        DrawOct(pos.X, pos.Y, 0x18, ColorMap.NoseTail);
                        break;
                    case ObjType.Eagle:
                        pos = GetScreenLoc(curObj.Mid);
                        DrawOct(pos.X, pos.Y, 0x10, ColorMap.Mid);
                        break;
                    case ObjType.ScrollController:
                        DrawMotionVector(curObj.Mid, curObj.Vel1, Color.Orange);
                        break;
                    case ObjType.ScrollWayPoint:
                        pos = GetScreenLoc(curObj.Mid);
                        DrawBoxMWH(pos.X, pos.Y, 8, 8, Color.Orange);
                        DrawMotionVector(curObj.Mid, curObj.Vel1);
                        break;
                    case ObjType.BombSpawner:
                        pos = GetScreenLoc(curObj.Mid);
                        PutText($"{curObj.StateCtr}", pos.X, pos.Y, 1, 1, -1, -1, ColorMap.AnyPoint, Color.Blue);
                        break;
                    case ObjType.Explosion:
                        if (curObj.PtrSubObj != 0)
                        {
                            ReadObj2D(curObj.PtrSubObj, out subObj);
                            pos = GetScreenLoc(subObj.Mid);
                            int width = (subObj.Mid.X - subObj.Orig.X) >> 16;
                            pos.X = GetScreenLoc(subObj.Orig).X;
                            DrawBoxMWH(pos.X, pos.Y, width >> 16, 16, ColorMap.Mid);
                        }
                        if (curObj.PtrSubObj2 != 0)
                        {
                            ReadObj2D(curObj.PtrSubObj2, out subObj);
                            pos = GetScreenLoc(subObj.Mid);
                            int width = (subObj.Mid.X - subObj.Orig.X) >> 16;
                            pos.X = GetScreenLoc(subObj.Orig).X;
                            DrawBoxMWH(pos.X, pos.Y, width >> 16, 16, ColorMap.Mid);
                        }
                        break;
                    case ObjType.NullFunc:
                        switch (curObj.ObjDefIndex)
                        {
                            case 48:
                            case 49:
                            case 126:
                            case 145:
                            case 146:
                            case 213:
                                PutText($"{curObj.PtrMainFunc:X5}:{curObj.ObjDefIndex}", pos.X, pos.Y - 4, 1, 1, -1, -9, ColorMap.AnyPoint, Color.Blue);
                                PutText(addr.ToString("X6"), pos.X, pos.Y + 4, 1, 9, -1, -1, ColorMap.AnyPoint, Color.Blue);
                                break;
                            case 59:
                            case 87:
                            case 181:
                                pos = GetScreenLoc(curObj.Mid);
                                DrawOct(pos.X, pos.Y, (curObj.Dims.Width + curObj.Dims.Height) >> 17, ColorMap.Nose);
                                break;
                            case 71:
                            case 72:
                            case 158:
                            case 159:
                            case 165:
                                DrawDefaultBounds(curObj, ColorMap.Mid);
                                break;
                            case 82:
                            case 83:
                            case 84:
                            case 85:
                            case 86:
                                pos = GetScreenLoc(curObj.Mid);
                                DrawBoxMWH(pos.X, pos.Y, curObj.Dims.Width >> 16, curObj.Dims.Height >> 16, ColorMap.Mid);
                                break;
                            case 210:
                                pos = GetScreenLoc(curObj.Mid);
                                DrawBoxMWH(pos.X, pos.Y, curObj.Dims.Width >> 16, curObj.Dims.Height >> 16, ColorMap.Sonar);
                                break;
                            case 107:
                                pos.X = (curObj.XChunk << 7) - _camX + 0x40;
                                pos.Y = (curObj.YChunk << 7) - _camY + 0x40;
                                pos2.X = (curObj.ScreenX << 7) - _camX + 0x40;
                                pos2.Y = (curObj.ScreenY << 7) - _camY + 0x40;
                                DrawBoxMWH(pos.X, pos.Y, 64, 64, Color.Orange, 0);
                                DrawBoxMWH(pos2.X, pos2.Y, 64, 64, Color.Orange, 0);
                                DrawBoxMWH(pos.X, pos.Y, 1, 1, Color.Blue, 0);
                                Gui.DrawLine(pos.X, pos.Y, pos2.X, pos2.Y, Color.Orange);
                                break;
                            case 110: //Gravity conrol points
                            case 179:
                                DrawDefaultBounds(curObj, ColorMap.Mid, curObj.Mode == 0 ? Color.Gray : ColorMap.Mid);
                                int dir = Mem.ReadS8(addr + 0x71) & 7;
                                int[] xtable = { 7, 4, -3, -10, -14, -11, -3, 4 };
                                int[] ytable = { 11, 4, 7, 4, -3, -11, -14, -11 };
                                pos.X = Mem.ReadS16(addr + 0x24) - _camX;
                                pos.Y = Mem.ReadS16(addr + 0x28) - _camY;
                                Gui.DrawImage(".\\ExternalTools\\gravitometer_bg.png", pos.X - 15, pos.Y - 15);
                                Gui.DrawImage(".\\ExternalTools\\gravitometer_fg.png", pos.X + xtable[dir], pos.Y + ytable[dir]);
                                break;
                            case 176:
                                pos.X = (curObj.XChunk << 7) - _camX + 0x40;
                                pos.Y = (curObj.YChunk << 7) - _camY + 0x40;
                                pos2.X = (curObj.ScreenX << 7) - _camX + 0x40;
                                pos2.Y = (curObj.ScreenY << 7) - _camY + 0x40;
                                DrawOct(pos.X, pos.Y, 32, Color.Orange, 0);
                                DrawOct(pos2.X, pos2.Y, 32, Color.Orange, 0);
                                DrawBoxMWH(pos.X, pos.Y, 1, 1, Color.Blue, 0);
                                Gui.DrawLine(pos.X, pos.Y, pos2.X, pos2.Y, Color.Orange);
                                break;
                            case 194: // Kill plane
                                DrawDefaultBounds(curObj, ColorMap.Mid, Color.Black);
                                DrawDefaultBounds(curObj, ColorMap.Mid, Color.Black);
                                break;
                            default:
                                DrawDefaultBounds(curObj, ColorMap.AnyPoint);
                                PutText($"{curObj.PtrMainFunc:X5}:{curObj.ObjDefIndex}", pos.X, pos.Y - 4, 1, 1, -1, -9, ColorMap.AnyPoint, Color.Blue);
                                PutText(addr.ToString("X6"), pos.X, pos.Y + 4, 1, 9, -1, -1, ColorMap.AnyPoint, Color.Blue);
                                break;
                        }
                        break;
                    case ObjType.NoDisplay:
                        break;
                    default:
                        DrawDefaultBounds(curObj, ColorMap.AnyPoint);
                        pos = GetScreenLoc(curObj.Mid);
                        PutText($"{curObj.PtrMainFunc:X5}:{curObj.ObjDefIndex}", pos.X, pos.Y - 4, 1, 1, -1, -9, ColorMap.AnyPoint, Color.Blue);
                        PutText(addr.ToString("X6"), pos.X, pos.Y + 4, 1, 9, -1, -1, ColorMap.AnyPoint, Color.Blue);
                        break;
                }
				if (_showNumbers)
				{
					pos = GetScreenLoc(curObj.Mid);
					PutText($"{curObj.PtrMainFunc:X5}:{curObj.ObjDefIndex}", pos.X, pos.Y - 4, 1, 1, -1, -9, ColorMap.AnyPoint, Color.Blue);
				}
				addr = curObj.PtrNext;
            }
        }
        private void Draw2DHud()
        {
            //	CamX-=8;
            Color color;
            DrawAsterite();
            DrawTubes();
            DrawWalls();
            DrawStatics();
            DrawAnims();
            DrawEvents();
            Obj2D curObj;
            //Ecco head
            ReadObj2D(ReadPtr(Addr2D.EccoHeadPtr), out curObj);
            DrawDefaultBounds(curObj, ColorMap.HeadTail);
            //Ecco tail
            ReadObj2D(ReadPtr(Addr2D.EccoTailPtr), out curObj);
            DrawDefaultBounds(curObj, ColorMap.HeadTail);
            //Ecco body
            Point pos = GetScreenLoc(_player2D.Mid);
            Point pos2 = GetScreenLoc(_player2D.Nose);
            Size vel = _player2D.CurrentVel + _player2D.SwimVel + _player2D.ZipVel;
            Gui.DrawLine(pos.X, pos.Y, pos2.X, pos2.Y, Color.Green);
            pos2 = GetScreenLoc(_player2D.Tail);
            Gui.DrawLine(pos.X, pos.Y, pos2.X, pos2.Y, Color.Green);
            DrawMotionVector(_player2D.Tail, vel, ColorMap.Tail);
            DrawMotionVector(_player2D.Nose, vel, ColorMap.Nose);
            DrawBoxMWH(pos.X, pos.Y, 1, 1, ColorMap.Mid, 0);
            pos = GetScreenLoc(new Point((_player2D.Tail.X + _player2D.Mid.X) >> 1, (_player2D.Tail.Y + _player2D.Mid.Y) >> 1));
            pos2 = GetScreenLoc(new Point((_player2D.Nose.X + _player2D.Mid.X) >> 1, (_player2D.Nose.Y + _player2D.Mid.Y) >> 1));
            DrawBoxMWH(pos.X, pos.Y, 1, 1, ColorMap.Spine, 0);
            DrawBoxMWH(pos2.X, pos2.Y, 1, 1, ColorMap.Spine, 0);
            // sonar
            if (_player2D.SonarState != 0)
            {
                pos = GetScreenLoc(_player2D.SonarPos);
                color = ((_player2D.SonarChrgFlag != 0) ? ColorMap.Enemy : ColorMap.Sonar);
                DrawBoxMWH(pos.X, pos.Y, _player2D.SonarSize.Width >> 16, _player2D.SonarSize.Height >> 16, color);
                DrawMotionVector(_player2D.SonarPos, _player2D.SonarVel, color);
            }
            //Pulsar
            // The pulsar attack uses a special multi-part object format that I don't fully understand
            // Also it's the only type of multi-part object that affects gameplay at all
            uint addr = ReadPtr(Addr2D.MultObjLLHead);
            while (addr != 0)
            {
                uint ptrMainFunc = ReadPtr(addr + 0xC);
                if (_2DTypeProvider.GetType(ptrMainFunc) == ObjType.PulsarBlast)
                {
                    uint subAddr = addr + 0x26;
                    for (int l = 0; l < 4; l++)
                    {
                        if (Mem.ReadU16(subAddr + 0x12) != 0)
                        {
                            pos.X = ReadS32AndAdvance(ref subAddr);
                            pos.Y = ReadS32AndAdvance(ref subAddr);
                            vel.Width = ReadS32AndAdvance(ref subAddr);
                            vel.Height = ReadS32AndAdvance(ref subAddr);
                            DrawMotionVector(pos, vel);
                            pos = GetScreenLoc(pos);
                            DrawBoxMWH(pos.X, pos.Y, 0x30, 0x30, ColorMap.Enemy);
                        }
                        subAddr += 4;
                    }
                }
                addr = ReadPtr(addr);
            }

            //Water Level
            int waterLevel = Mem.ReadS16(Addr2D.WaterLevel);
            Gui.DrawLine(0, waterLevel - _camY, _left + 320 + _right, waterLevel - _camY, Color.Aqua);

            //Ecco curObj.HP and Air
            int i = 0;
            short HP = (short) (_player2D.HP << 3);
            int off = 0;
            for (int j = 0; j < _player2D.Air; j++)
            {
                if (j - off == 448)
                {
                    i++; off += 448;
                }
                color = Color.FromArgb(j >> 2, j >> 2, j >> 2);
                Gui.DrawLine(_left - 32, j - off, _left - 17, j - off, color);
            }
            for (int j = 0; j < HP; j += 8)
            {
                color = Color.FromArgb(Math.Max(0x38 - (j >> 3), 0), 0, Math.Min(j >> 1, 255));
                Gui.DrawRectangle(_left - 16, j, 15, 7, color, color);
            }
        }
        private void Update2DTickers()
        {
            Size totalVel = _player2D.SwimVel + _player2D.CurrentVel + _player2D.ZipVel;
            StatusText($"           Cam X: {Mem.ReadS16(Addr2D.CamX),4}        Y: {Mem.ReadS16(Addr2D.CamY),4}");
            StatusText($"    Player Pos X: {_player2D.Mid.X / 65536.0,11:0.000000} Y: {_player2D.Mid.Y / 65536.0,11:0.000000}");
            StatusText($"    Player Speed: {_player2D.SwimSpeed / 65536.0,10:0.000000} Decel: {_player2D.DecelTimer} Accel: {_player2D.AccelTimer}");
            StatusText($"Player BaseVel X: {_player2D.SwimVel.Width / 65536.0,10:0.000000} Y: {_player2D.SwimVel.Height / 65536.0,10:0.000000}");
            StatusText($"Player FlowVel X: {_player2D.CurrentVel.Width / 65536.0,10:0.000000} Y: {_player2D.CurrentVel.Height / 65536.0,10:0.000000}");
            StatusText($" Player ZipVel X: {_player2D.ZipVel.Width / 65536.0,10:0.000000} Y: {_player2D.ZipVel.Height / 65536.0,10:0.000000}");
            StatusText($" Player TotVel X: {totalVel.Width / 65536.0,10:0.000000} Y: {totalVel.Height / 65536.0,10:0.000000}");
            StatusText($"Movement Mode: {_player2D.MoveMode} Charge Counter: {_player2D.ChargeCounter,2} Current Angle: {_player2D.Angle:X4} Target Angle: {_player2D.TgtAng:X4}");
            switch (Mem.ReadU8(AddrGlobal.LevelID))
            {
                case 1:
                case 2:
                case 3:
                case 30:
                case 46:
                    var globeFlags = Mem.ReadU32(Addr2D.GlobeFlags) >> 1;
                    int i = 0;
                    while (globeFlags > 0)
                    {
                        globeFlags >>= 1;
                        i++;
                    }
                    StatusText($"Pairs Collected: {i}", Color.Blue);
                    break;
                default:
                    break;
            }
        }
        private void DrawTurnSignal()
        {
            var color = _turnSignalColors[_levelTime & 7];
            Gui.DrawRectangle(_left - 48, _top - 112, 15, 15, color, color);
        }
        private void UpdatePlayer2D()
        {
            ReadPlayerObj(Addr2D.PlayerObj, out _player2D);
        }
        private void AutoFire2D()
        {
            switch (_player2D.MoveMode)
            {
                case 0:
                case 3:
                    Joy.Set("C", (_player2D.AccelTimer < 11), 1);
                    break;
            }
        }
    }
}