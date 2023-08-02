using LightDancing.Enums;
using System;
using System.Collections.Generic;

namespace LightDancing.Smart.Helper
{
    public class DictionaryConst
    {
        public readonly static Dictionary<Octaves, Tuple<int, int>> OCTAVE_RANGES = new Dictionary<Enums.Octaves, Tuple<int, int>>()
        {
            { Octaves.Range1, Tuple.Create(0, 0) },
            { Octaves.Range2, Tuple.Create(1, 1) },
            { Octaves.Range3, Tuple.Create(2, 2) },
            { Octaves.Range4, Tuple.Create(3, 6) },
            { Octaves.Range5, Tuple.Create(7, 11) },
            { Octaves.Range6, Tuple.Create(12, 21) },
            { Octaves.Range7, Tuple.Create(22, 42) },
            { Octaves.Range8, Tuple.Create(43, 86) },
            { Octaves.Range9, Tuple.Create(87, 172) },
            { Octaves.Range10, Tuple.Create(173, 512) },
        };

        public readonly static List<BackgroundShaders> BACKGROUND_ALL = new List<BackgroundShaders>()
        {
            BackgroundShaders.HalfCircle,
            BackgroundShaders.Fire,
            BackgroundShaders.FadeCircle,
            BackgroundShaders.Diamond,
            BackgroundShaders. HalfRainbow,
            BackgroundShaders.Flowers,
            BackgroundShaders.Triangle,
            BackgroundShaders.ClappingBall,
            BackgroundShaders.Rectangle,
            BackgroundShaders.RoundCircle,
            BackgroundShaders.Arrow,
            BackgroundShaders.Grid,
            BackgroundShaders.StripRotate,
            BackgroundShaders.GridRotate,
            BackgroundShaders.Squares,
        };

        public readonly static List<PanoramaShaders> PANORAMA_ALL = new List<PanoramaShaders>()
        {
            PanoramaShaders.Heart,
            PanoramaShaders.HorizontalStrip,
            PanoramaShaders.SpotLights,
            PanoramaShaders.HeadLights,
            PanoramaShaders.DiamondLights,
            PanoramaShaders.CircleSquare,
            PanoramaShaders.Wave,
            PanoramaShaders.Ripple,
            PanoramaShaders.RotateCircle,
            PanoramaShaders.RotateDiamond,
            PanoramaShaders. LineRotate,
            PanoramaShaders.HorizontalLines,
            PanoramaShaders.CrossLines,
            PanoramaShaders.TriangleLine,
            PanoramaShaders.Swirl,
        };

        public readonly static List<ProtagonistShaders> PROTAGONIST_ALL = new List<ProtagonistShaders>()
        {
            ProtagonistShaders.BouncingBall,
            ProtagonistShaders.RadiusLaser,
            ProtagonistShaders.Laser,
            ProtagonistShaders.RandomString,
            ProtagonistShaders.Rain,
            ProtagonistShaders.StillBalls,
            ProtagonistShaders.RandomCross,
            ProtagonistShaders.RandomHeart,
            ProtagonistShaders.RandomTarget,
            ProtagonistShaders.RandomTree,
        };

        public readonly static List<BackgroundShaders> BACKGROUND_CARTOON = new List<BackgroundShaders>()
        {
            BackgroundShaders.CartoonBG1,
            BackgroundShaders.CartoonBG2,
            BackgroundShaders.CartoonBG3,
            BackgroundShaders.CartoonBG4,
            BackgroundShaders.CartoonBG5,
            BackgroundShaders.CartoonBG6,
            BackgroundShaders.CartoonBG7,
        };

        public readonly static List<PanoramaShaders> PANORAMA_CARTOON = new List<PanoramaShaders>()
        {
            PanoramaShaders.CartoonPanorama1,
            PanoramaShaders.CartoonPanorama2,
            PanoramaShaders.CartoonPanorama3,
            PanoramaShaders.CartoonPanorama4,
            PanoramaShaders.CartoonPanorama5,
        };

        public readonly static List<ProtagonistShaders> PROTAGONIST_CARTOON = new List<ProtagonistShaders>()
        {
            ProtagonistShaders.CartoonProtagonist1,
            ProtagonistShaders.CartoonProtagonist2,
            ProtagonistShaders.CartoonProtagonist3,
            ProtagonistShaders.CartoonProtagonist4,
            ProtagonistShaders.CartoonProtagonist5,
            ProtagonistShaders.CartoonProtagonist6,
        };

        public readonly static List<BackgroundShaders> BACKGROUND_POPART = new List<BackgroundShaders>()
        {
            BackgroundShaders.MovingDots,
            BackgroundShaders.PAHorizontalStrip,
            BackgroundShaders.VerticalStrip,
            BackgroundShaders.MovingSlope,
            BackgroundShaders.MovingTriangleWave,
            BackgroundShaders.MovingDotLarge,
        };

        public readonly static List<PanoramaShaders> PANORAMA_POPART = new List<PanoramaShaders>()
        {
            PanoramaShaders.FourSquare,
            PanoramaShaders.DotSquare,
            PanoramaShaders.CornerDot,
            PanoramaShaders.CornerTriangleWave,
        };

        public readonly static List<ProtagonistShaders> PROTAGONIST_POPART = new List<ProtagonistShaders>()
        {
            ProtagonistShaders.TSquare,
            ProtagonistShaders.OSquare,
            ProtagonistShaders.RandomCurve,
            ProtagonistShaders.TwoSideCircle,
            ProtagonistShaders.RandomCircle,
            ProtagonistShaders.RandomRect,
        };
        public readonly static List<BackgroundShaders> BACKGROUND_GEO = new List<BackgroundShaders>()
        {
            BackgroundShaders.CircleSquare,
            BackgroundShaders.GeoSquare,
            BackgroundShaders.GeoSquare2,
            BackgroundShaders.GeoSquare3,
            BackgroundShaders.GeoSquare4,
            BackgroundShaders.GeoSquare5,
            BackgroundShaders.GeoSquare6,
            BackgroundShaders.GeoSquare7,
            BackgroundShaders.GeoSquare8,
            BackgroundShaders.RotateGeo,
            BackgroundShaders.GeoCircle,
            BackgroundShaders.Hexagon,
            BackgroundShaders.GeoLines,
            BackgroundShaders.GeoLines2,
            BackgroundShaders.GeoLines3,
            BackgroundShaders.GeoLines4,
        };

        public readonly static List<PanoramaShaders> PANORAMA_GEO = new List<PanoramaShaders>()
        {
            PanoramaShaders.MidCircleSquare,
            PanoramaShaders.MidGeoCircle,
            PanoramaShaders.MidGeoLines,
            PanoramaShaders.MidGeoLines2,
            PanoramaShaders.MidGeoLines3,
            PanoramaShaders.MidGeoLines4,
            PanoramaShaders.MidGeoSquare,
            PanoramaShaders.MidGeoSquare2,
            PanoramaShaders.MidGeoSquare3,
            PanoramaShaders.MidGeoSquare4,
            PanoramaShaders.MidGeoSquare5,
            PanoramaShaders.MidGeoSquare6,
            PanoramaShaders.MidGeoSquare7,
            PanoramaShaders.MidGeoSquare8,
            PanoramaShaders.MidHexagon,
            PanoramaShaders.MidRotateGeo,
        };

        public readonly static List<ProtagonistShaders> PROTAGONIST_GEO = new List<ProtagonistShaders>()
        {
            ProtagonistShaders.HighCircleSquare,
            ProtagonistShaders.HighGeoSquare,
            ProtagonistShaders.HighGeoSquare2,
            ProtagonistShaders.HighGeoSquare3,
            ProtagonistShaders.HighGeoSquare4,
            ProtagonistShaders.HighGeoSquare5,
            ProtagonistShaders.HighGeoSquare6,
            ProtagonistShaders.HighGeoSquare7,
            ProtagonistShaders.HighGeoSquare8,
            ProtagonistShaders.HighRotateGeo,
            ProtagonistShaders.HighGeoCircle,
            ProtagonistShaders.HighHexagon,
            ProtagonistShaders.HighGeoLines,
            ProtagonistShaders.HighGeoLines2,
            ProtagonistShaders.HighGeoLines3,
            ProtagonistShaders.HighGeoLines4,
            ProtagonistShaders.HighRotateGeo,

        };

        public readonly static List<BackgroundShaders> BACKGROUND_KALE = new List<BackgroundShaders>()
        {
            BackgroundShaders.Kale1,
            BackgroundShaders.Kale2,
            BackgroundShaders.Kale3,
            BackgroundShaders.Kale4,
            BackgroundShaders.Kale5,
            BackgroundShaders.Kale6,
            BackgroundShaders.Kale7,
            BackgroundShaders.Kale8,
            BackgroundShaders.Kale9,
            BackgroundShaders.Kale10,
            BackgroundShaders.Kale11,
            BackgroundShaders.Kale12,
        };

        public readonly static List<BackgroundShaders> BACKGROUND_TRIP = new List<BackgroundShaders>()
        {
            BackgroundShaders.AlienFlower,
            BackgroundShaders.AlienShip,
            BackgroundShaders.AlienFlower2,
            BackgroundShaders.BottleRainbow,
            BackgroundShaders.CheckboardFire,
            BackgroundShaders.FadeAlien,
            BackgroundShaders.FlowerShape,
            BackgroundShaders.Hamburger,
            BackgroundShaders.Pacman,
            BackgroundShaders.MeltSmile,
            BackgroundShaders.Monster,
        };
    }
}
