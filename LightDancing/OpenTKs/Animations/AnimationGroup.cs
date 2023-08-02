using LightDancing.Common;
using LightDancing.Enums;
using System;

namespace LightDancing.OpenTKs.Animations
{
    public class AnimationGroup
    {
        public BackgroundShaders? Background { get; set; }
        public PanoramaShaders? Panorama { get; set; }
        public ProtagonistShaders? Protagonist { get; set; }
        public KickDrumShaders? Kick { get; set; }
        public MusicAnimateShaders? MusicAnimate { get; set; }
        public WallpaperAnimateShaders? WallpaperAnimate { get; set; }

        public string BackgroundPath { get; set; }
        public string PanoramaPath { get; set; }
        public string ProtagonistPath { get; set; }
        public string KickPath { get; set; }
        public string MusicAnimatePath { get; set; }
        public string WallpaperAnimatePath { get; set; }
        public bool StackingShader { get; set; }

        public AnimationGroup(BackgroundShaders? background, PanoramaShaders? panorama, ProtagonistShaders? protagonist)
        {
            Background = background;
            Panorama = panorama;
            Protagonist = protagonist;
            MusicAnimate = null;
            WallpaperAnimate = null;

            BackgroundPath = GetShaderPath(background);
            PanoramaPath = GetShaderPath(panorama);
            ProtagonistPath = GetShaderPath(protagonist);
            MusicAnimatePath = "";
            WallpaperAnimatePath = "";

            if (background != null)
                StackingShader = GetStackingShader(background);
            if (panorama != null)
                StackingShader = GetStackingShader(panorama);
            if (protagonist != null)
                StackingShader = GetStackingShader(protagonist);
        }

        public AnimationGroup(BackgroundShaders? background, PanoramaShaders? panorama, ProtagonistShaders? protagonist, KickDrumShaders? kick)
        {
            Background = background;
            Panorama = panorama;
            Protagonist = protagonist;
            Kick = kick;
            MusicAnimate = null;
            WallpaperAnimate = null;

            BackgroundPath = GetShaderPath(background);
            PanoramaPath = GetShaderPath(panorama);
            ProtagonistPath = GetShaderPath(protagonist);
            KickPath = GetShaderPath(kick);
            MusicAnimatePath = "";
            WallpaperAnimatePath = "";

            if (background != null)
                StackingShader = GetStackingShader(background);
            if (panorama != null)
                StackingShader = GetStackingShader(panorama);
            if (protagonist != null)
                StackingShader = GetStackingShader(protagonist);
        }

        public AnimationGroup(MusicAnimateShaders musicAnimate)
        {
            Background = null;
            Panorama = null;
            Protagonist = null;
            Kick = null;
            WallpaperAnimate = null;
            MusicAnimate = musicAnimate;

            BackgroundPath = "";
            PanoramaPath = "";
            ProtagonistPath = "";
            KickPath = "";
            WallpaperAnimatePath = "";
            MusicAnimatePath = GetShaderPath(musicAnimate);

            StackingShader = false;
        }

        public AnimationGroup(WallpaperAnimateShaders wallpaperAnimateShaders)
        {
            Background = null;
            Panorama = null;
            Protagonist = null;
            Kick = null;
            WallpaperAnimate = wallpaperAnimateShaders;
            MusicAnimate = null;

            BackgroundPath = "";
            PanoramaPath = "";
            ProtagonistPath = "";
            KickPath = "";
            WallpaperAnimatePath = GetShaderPath(wallpaperAnimateShaders);
            MusicAnimatePath = "";

            StackingShader = false;
        }

        private string GetShaderPath<T>(T shaderType)
        {
            return shaderType switch
            {
                AnimationShaders.ShiftColors => "LightDancing.OpenTKs.Shaders.ShiftColors.frag",
                AnimationShaders.StillColors => "LightDancing.OpenTKs.Shaders.StillColors.frag",

                BackgroundShaders.HalfCircle => "LightDancing.OpenTKs.Shaders.Background.HalfCircle.frag",
                BackgroundShaders.Fire => "LightDancing.OpenTKs.Shaders.Background.Fire.frag",
                BackgroundShaders.FadeCircle => "LightDancing.OpenTKs.Shaders.Background.FadeCircle.frag",
                BackgroundShaders.Diamond => "LightDancing.OpenTKs.Shaders.Background.Diamond.frag",
                BackgroundShaders.HalfRainbow => "LightDancing.OpenTKs.Shaders.Background.HalfRainbow.frag",
                BackgroundShaders.Flowers => "LightDancing.OpenTKs.Shaders.Background.Flowers.frag",
                BackgroundShaders.Triangle => "LightDancing.OpenTKs.Shaders.Background.Triangle.frag",
                BackgroundShaders.ClappingBall => "LightDancing.OpenTKs.Shaders.Background.ClappingBall.frag",
                BackgroundShaders.Rectangle => "LightDancing.OpenTKs.Shaders.Background.Rectangle.frag",
                BackgroundShaders.RoundCircle => "LightDancing.OpenTKs.Shaders.Background.RoundCircle.frag",
                BackgroundShaders.Arrow => "LightDancing.OpenTKs.Shaders.Background.Arrow.frag",
                BackgroundShaders.Grid => "LightDancing.OpenTKs.Shaders.Background.Grid.frag",
                BackgroundShaders.StripRotate => "LightDancing.OpenTKs.Shaders.Background.StripRotate.frag",
                BackgroundShaders.GridRotate => "LightDancing.OpenTKs.Shaders.Background.GridRotate.frag",
                BackgroundShaders.Squares => "LightDancing.OpenTKs.Shaders.Background.Squares.frag",
                //CartoonBG
                BackgroundShaders.CartoonBG1 => "LightDancing.OpenTKs.Shaders.Background.Cartoon.CartoonBG1.frag",
                BackgroundShaders.CartoonBG2 => "LightDancing.OpenTKs.Shaders.Background.Cartoon.CartoonBG2.frag",
                BackgroundShaders.CartoonBG3 => "LightDancing.OpenTKs.Shaders.Background.Cartoon.CartoonBG3.frag",
                BackgroundShaders.CartoonBG4 => "LightDancing.OpenTKs.Shaders.Background.Cartoon.CartoonBG4.frag",
                BackgroundShaders.CartoonBG5 => "LightDancing.OpenTKs.Shaders.Background.Cartoon.CartoonBG5.frag",
                BackgroundShaders.CartoonBG6 => "LightDancing.OpenTKs.Shaders.Background.Cartoon.CartoonBG6.frag",
                BackgroundShaders.CartoonBG7 => "LightDancing.OpenTKs.Shaders.Background.Cartoon.CartoonBG7.frag",
                //PopArt
                BackgroundShaders.MovingDots => "LightDancing.OpenTKs.Shaders.Background.PopArt.MovingDots.frag",
                BackgroundShaders.PAHorizontalStrip => "LightDancing.OpenTKs.Shaders.Background.PopArt.PAHorizontalStrip.frag",
                BackgroundShaders.VerticalStrip => "LightDancing.OpenTKs.Shaders.Background.PopArt.VerticalStrip.frag",
                BackgroundShaders.MovingSlope => "LightDancing.OpenTKs.Shaders.Background.PopArt.MovingSlope.frag",
                BackgroundShaders.MovingTriangleWave => "LightDancing.OpenTKs.Shaders.Background.PopArt.MovingTriangleWave.frag",
                BackgroundShaders.MovingDotLarge => "LightDancing.OpenTKs.Shaders.Background.PopArt.MovingDotLarge.frag",
                //Geo
                BackgroundShaders.CircleSquare => "LightDancing.OpenTKs.Shaders.Background.Geo.CircleSquare.frag",
                BackgroundShaders.GeoSquare => "LightDancing.OpenTKs.Shaders.Background.Geo.GeoSquare.frag",
                BackgroundShaders.GeoSquare2 => "LightDancing.OpenTKs.Shaders.Background.Geo.GeoSquare2.frag",
                BackgroundShaders.GeoSquare3 => "LightDancing.OpenTKs.Shaders.Background.Geo.GeoSquare3.frag",
                BackgroundShaders.GeoSquare4 => "LightDancing.OpenTKs.Shaders.Background.Geo.GeoSquare4.frag",
                BackgroundShaders.GeoSquare5 => "LightDancing.OpenTKs.Shaders.Background.Geo.GeoSquare5.frag",
                BackgroundShaders.GeoSquare6 => "LightDancing.OpenTKs.Shaders.Background.Geo.GeoSquare6.frag",
                BackgroundShaders.GeoSquare7 => "LightDancing.OpenTKs.Shaders.Background.Geo.GeoSquare7.frag",
                BackgroundShaders.GeoSquare8 => "LightDancing.OpenTKs.Shaders.Background.Geo.GeoSquare8.frag",
                BackgroundShaders.RotateGeo => "LightDancing.OpenTKs.Shaders.Background.Geo.RotateGeo.frag",
                BackgroundShaders.GeoCircle => "LightDancing.OpenTKs.Shaders.Background.Geo.GeoCircle.frag",
                BackgroundShaders.Hexagon => "LightDancing.OpenTKs.Shaders.Background.Geo.Hexagon.frag",
                BackgroundShaders.GeoLines => "LightDancing.OpenTKs.Shaders.Background.Geo.GeoLines.frag",
                BackgroundShaders.GeoLines2 => "LightDancing.OpenTKs.Shaders.Background.Geo.GeoLines2.frag",
                BackgroundShaders.GeoLines3 => "LightDancing.OpenTKs.Shaders.Background.Geo.GeoLines3.frag",
                BackgroundShaders.GeoLines4 => "LightDancing.OpenTKs.Shaders.Background.Geo.GeoLines4.frag",
                //Kale
                BackgroundShaders.Kale1 => "LightDancing.OpenTKs.Shaders.Background.Kale.Kale1.frag",
                BackgroundShaders.Kale2 => "LightDancing.OpenTKs.Shaders.Background.Kale.Kale2.frag",
                BackgroundShaders.Kale3 => "LightDancing.OpenTKs.Shaders.Background.Kale.Kale3.frag",
                BackgroundShaders.Kale4 => "LightDancing.OpenTKs.Shaders.Background.Kale.Kale4.frag",
                BackgroundShaders.Kale5 => "LightDancing.OpenTKs.Shaders.Background.Kale.Kale5.frag",
                BackgroundShaders.Kale6 => "LightDancing.OpenTKs.Shaders.Background.Kale.Kale6.frag",
                BackgroundShaders.Kale7 => "LightDancing.OpenTKs.Shaders.Background.Kale.Kale7.frag",
                BackgroundShaders.Kale8 => "LightDancing.OpenTKs.Shaders.Background.Kale.Kale8.frag",
                BackgroundShaders.Kale9 => "LightDancing.OpenTKs.Shaders.Background.Kale.Kale9.frag",
                BackgroundShaders.Kale10 => "LightDancing.OpenTKs.Shaders.Background.Kale.Kale10.frag",
                BackgroundShaders.Kale11 => "LightDancing.OpenTKs.Shaders.Background.Kale.Kale11.frag",
                BackgroundShaders.Kale12 => "LightDancing.OpenTKs.Shaders.Background.Kale.Kale12.frag",
                //Trip
                BackgroundShaders.AlienFlower => "LightDancing.OpenTKs.Shaders.Background.Trip.AlienFlower.frag",
                BackgroundShaders.AlienFlower2 => "LightDancing.OpenTKs.Shaders.Background.Trip.AlienFlower2.frag",
                BackgroundShaders.AlienShip => "LightDancing.OpenTKs.Shaders.Background.Trip.AlienShip.frag",
                BackgroundShaders.BottleRainbow => "LightDancing.OpenTKs.Shaders.Background.Trip.BottleRainbow.frag",
                BackgroundShaders.CheckboardFire => "LightDancing.OpenTKs.Shaders.Background.Trip.CheckboardFire.frag",
                BackgroundShaders.FadeAlien => "LightDancing.OpenTKs.Shaders.Background.Trip.FadeAlien.frag",
                BackgroundShaders.FlowerShape => "LightDancing.OpenTKs.Shaders.Background.Trip.FlowerShape.frag",
                BackgroundShaders.Hamburger => "LightDancing.OpenTKs.Shaders.Background.Trip.Hamburger.frag",
                BackgroundShaders.Pacman => "LightDancing.OpenTKs.Shaders.Background.Trip.Pacman.frag",
                BackgroundShaders.MeltSmile => "LightDancing.OpenTKs.Shaders.Background.Trip.MeltSmile.frag",
                BackgroundShaders.Monster => "LightDancing.OpenTKs.Shaders.Background.Trip.Monster.frag",
                //New Music Analysis
                BackgroundShaders.BackgroundBar => "LightDancing.OpenTKs.Shaders.Background.BackgroundBar.frag",
                KickDrumShaders.KickBar => "LightDancing.OpenTKs.Shaders.Background.KickBar.frag",
                //Panorama
                PanoramaShaders.Heart => "LightDancing.OpenTKs.Shaders.Panorama.Heart.frag",
                PanoramaShaders.HorizontalStrip => "LightDancing.OpenTKs.Shaders.Panorama.HorizontalStrip.frag",
                PanoramaShaders.SpotLights => "LightDancing.OpenTKs.Shaders.Panorama.SpotLights.frag",
                PanoramaShaders.HeadLights => "LightDancing.OpenTKs.Shaders.Panorama.HeadLights.frag",
                PanoramaShaders.DiamondLights => "LightDancing.OpenTKs.Shaders.Panorama.DiamondLights.frag",
                PanoramaShaders.CircleSquare => "LightDancing.OpenTKs.Shaders.Panorama.CircleSquare.frag",
                PanoramaShaders.Wave => "LightDancing.OpenTKs.Shaders.Panorama.Wave.frag",
                PanoramaShaders.Ripple => "LightDancing.OpenTKs.Shaders.Panorama.Ripple.frag",
                PanoramaShaders.RotateCircle => "LightDancing.OpenTKs.Shaders.Panorama.RotateCircle.frag",
                PanoramaShaders.RotateDiamond => "LightDancing.OpenTKs.Shaders.Panorama.RotateDiamond.frag",
                PanoramaShaders.LineRotate => "LightDancing.OpenTKs.Shaders.Panorama.LineRotate.frag",
                PanoramaShaders.HorizontalLines => "LightDancing.OpenTKs.Shaders.Panorama.HorizontalLines.frag",
                PanoramaShaders.CrossLines => "LightDancing.OpenTKs.Shaders.Panorama.CrossLines.frag",
                PanoramaShaders.TriangleLine => "LightDancing.OpenTKs.Shaders.Panorama.TriangleLine.frag",
                PanoramaShaders.Swirl => "LightDancing.OpenTKs.Shaders.Panorama.Swirl.frag",
                //Cartoon
                PanoramaShaders.CartoonPanorama1 => "LightDancing.OpenTKs.Shaders.Panorama.Cartoon.CartoonPanorama1.frag",
                PanoramaShaders.CartoonPanorama2 => "LightDancing.OpenTKs.Shaders.Panorama.Cartoon.CartoonPanorama2.frag",
                PanoramaShaders.CartoonPanorama3 => "LightDancing.OpenTKs.Shaders.Panorama.Cartoon.CartoonPanorama3.frag",
                PanoramaShaders.CartoonPanorama4 => "LightDancing.OpenTKs.Shaders.Panorama.Cartoon.CartoonPanorama4.frag",
                PanoramaShaders.CartoonPanorama5 => "LightDancing.OpenTKs.Shaders.Panorama.Cartoon.CartoonPanorama5.frag",
                //PopArt
                PanoramaShaders.FourSquare => "LightDancing.OpenTKs.Shaders.Panorama.PopArt.FourSquare.frag",
                PanoramaShaders.DotSquare => "LightDancing.OpenTKs.Shaders.Panorama.PopArt.DotSquare.frag",
                PanoramaShaders.CornerDot => "LightDancing.OpenTKs.Shaders.Panorama.PopArt.CornerDot.frag",
                PanoramaShaders.CornerTriangleWave => "LightDancing.OpenTKs.Shaders.Panorama.PopArt.CornerTriangleWave.frag",
                //Geo
                PanoramaShaders.MidCircleSquare => "LightDancing.OpenTKs.Shaders.Panorama.Geo.MidCircleSquare.frag",
                PanoramaShaders.MidGeoCircle => "LightDancing.OpenTKs.Shaders.Panorama.Geo.MidGeoCircle.frag",
                PanoramaShaders.MidGeoLines => "LightDancing.OpenTKs.Shaders.Panorama.Geo.MidGeoLines.frag",
                PanoramaShaders.MidGeoLines2 => "LightDancing.OpenTKs.Shaders.Panorama.Geo.MidGeoLines2.frag",
                PanoramaShaders.MidGeoLines3 => "LightDancing.OpenTKs.Shaders.Panorama.Geo.MidGeoLines3.frag",
                PanoramaShaders.MidGeoLines4 => "LightDancing.OpenTKs.Shaders.Panorama.Geo.MidGeoLines4.frag",
                PanoramaShaders.MidGeoSquare => "LightDancing.OpenTKs.Shaders.Panorama.Geo.MidGeoSquare.frag",
                PanoramaShaders.MidGeoSquare2 => "LightDancing.OpenTKs.Shaders.Panorama.Geo.MidGeoSquare2.frag",
                PanoramaShaders.MidGeoSquare3 => "LightDancing.OpenTKs.Shaders.Panorama.Geo.MidGeoSquare3.frag",
                PanoramaShaders.MidGeoSquare4 => "LightDancing.OpenTKs.Shaders.Panorama.Geo.MidGeoSquare4.frag",
                PanoramaShaders.MidGeoSquare5 => "LightDancing.OpenTKs.Shaders.Panorama.Geo.MidGeoSquare5.frag",
                PanoramaShaders.MidGeoSquare6 => "LightDancing.OpenTKs.Shaders.Panorama.Geo.MidGeoSquare6.frag",
                PanoramaShaders.MidGeoSquare7 => "LightDancing.OpenTKs.Shaders.Panorama.Geo.MidGeoSquare7.frag",
                PanoramaShaders.MidGeoSquare8 => "LightDancing.OpenTKs.Shaders.Panorama.Geo.MidGeoSquare8.frag",
                PanoramaShaders.MidHexagon => "LightDancing.OpenTKs.Shaders.Panorama.Geo.MidHexagon.frag",
                PanoramaShaders.MidRotateGeo => "LightDancing.OpenTKs.Shaders.Panorama.Geo.MidRotateGeo.frag",
                //New Music Analysis
                PanoramaShaders.PanoramaBar => "LightDancing.OpenTKs.Shaders.Panorama.PanoramaBar.frag",

                ProtagonistShaders.Rain => "LightDancing.OpenTKs.Shaders.Protagonist.Rain.frag",
                ProtagonistShaders.BouncingBall => "LightDancing.OpenTKs.Shaders.Protagonist.BouncingBall.frag",
                ProtagonistShaders.RadiusLaser => "LightDancing.OpenTKs.Shaders.Protagonist.RadiusLaser.frag",
                ProtagonistShaders.Laser => "LightDancing.OpenTKs.Shaders.Protagonist.Laser.frag",
                ProtagonistShaders.RandomString => "LightDancing.OpenTKs.Shaders.Protagonist.RandomString.frag",
                ProtagonistShaders.StillBalls => "LightDancing.OpenTKs.Shaders.Protagonist.StillBalls.frag",
                ProtagonistShaders.RandomCross => "LightDancing.OpenTKs.Shaders.Protagonist.RandomCross.frag",
                ProtagonistShaders.RandomHeart => "LightDancing.OpenTKs.Shaders.Protagonist.RandomHeart.frag",
                ProtagonistShaders.RandomTarget => "LightDancing.OpenTKs.Shaders.Protagonist.RandomTarget.frag",
                ProtagonistShaders.RandomTree => "LightDancing.OpenTKs.Shaders.Protagonist.RandomTree.frag",
                ProtagonistShaders.SquareWave => "LightDancing.OpenTKs.Shaders.Protagonist.SquareWave.frag",
                //Cartoon
                ProtagonistShaders.CartoonProtagonist1 => "LightDancing.OpenTKs.Shaders.Protagonist.Cartoon.CartoonProtagonist1.frag",
                ProtagonistShaders.CartoonProtagonist2 => "LightDancing.OpenTKs.Shaders.Protagonist.Cartoon.CartoonProtagonist2.frag",
                ProtagonistShaders.CartoonProtagonist3 => "LightDancing.OpenTKs.Shaders.Protagonist.Cartoon.CartoonProtagonist3.frag",
                ProtagonistShaders.CartoonProtagonist4 => "LightDancing.OpenTKs.Shaders.Protagonist.Cartoon.CartoonProtagonist4.frag",
                ProtagonistShaders.CartoonProtagonist5 => "LightDancing.OpenTKs.Shaders.Protagonist.Cartoon.CartoonProtagonist5.frag",
                ProtagonistShaders.CartoonProtagonist6 => "LightDancing.OpenTKs.Shaders.Protagonist.Cartoon.CartoonProtagonist6.frag",
                //PopArt
                ProtagonistShaders.TSquare => "LightDancing.OpenTKs.Shaders.Protagonist.PopArt.TSquare.frag",
                ProtagonistShaders.OSquare => "LightDancing.OpenTKs.Shaders.Protagonist.PopArt.OSquare.frag",
                ProtagonistShaders.RandomCurve => "LightDancing.OpenTKs.Shaders.Protagonist.PopArt.RandomCurve.frag",
                ProtagonistShaders.TwoSideCircle => "LightDancing.OpenTKs.Shaders.Protagonist.PopArt.TwoSideCircle.frag",
                ProtagonistShaders.RandomCircle => "LightDancing.OpenTKs.Shaders.Protagonist.PopArt.RandomCircle.frag",
                ProtagonistShaders.RandomRect => "LightDancing.OpenTKs.Shaders.Protagonist.PopArt.RandomRect.frag",
                //Geo
                ProtagonistShaders.HighCircleSquare => "LightDancing.OpenTKs.Shaders.Protagonist.Geo.HighCircleSquare.frag",
                ProtagonistShaders.HighGeoCircle => "LightDancing.OpenTKs.Shaders.Protagonist.Geo.HighGeoCircle.frag",
                ProtagonistShaders.HighGeoLines => "LightDancing.OpenTKs.Shaders.Protagonist.Geo.HighGeoLines.frag",
                ProtagonistShaders.HighGeoLines2 => "LightDancing.OpenTKs.Shaders.Protagonist.Geo.HighGeoLines2.frag",
                ProtagonistShaders.HighGeoLines3 => "LightDancing.OpenTKs.Shaders.Protagonist.Geo.HighGeoLines3.frag",
                ProtagonistShaders.HighGeoLines4 => "LightDancing.OpenTKs.Shaders.Protagonist.Geo.HighGeoLines4.frag",
                ProtagonistShaders.HighGeoSquare => "LightDancing.OpenTKs.Shaders.Protagonist.Geo.HighGeoSquare.frag",
                ProtagonistShaders.HighGeoSquare2 => "LightDancing.OpenTKs.Shaders.Protagonist.Geo.HighGeoSquare2.frag",
                ProtagonistShaders.HighGeoSquare3 => "LightDancing.OpenTKs.Shaders.Protagonist.Geo.HighGeoSquare3.frag",
                ProtagonistShaders.HighGeoSquare4 => "LightDancing.OpenTKs.Shaders.Protagonist.Geo.HighGeoSquare4.frag",
                ProtagonistShaders.HighGeoSquare5 => "LightDancing.OpenTKs.Shaders.Protagonist.Geo.HighGeoSquare5.frag",
                ProtagonistShaders.HighGeoSquare6 => "LightDancing.OpenTKs.Shaders.Protagonist.Geo.HighGeoSquare6.frag",
                ProtagonistShaders.HighGeoSquare7 => "LightDancing.OpenTKs.Shaders.Protagonist.Geo.HighGeoSquare7.frag",
                ProtagonistShaders.HighGeoSquare8 => "LightDancing.OpenTKs.Shaders.Protagonist.Geo.HighGeoSquare8.frag",
                ProtagonistShaders.HighHexagon => "LightDancing.OpenTKs.Shaders.Protagonist.Geo.HighHexagon.frag",
                ProtagonistShaders.HighRotateGeo => "LightDancing.OpenTKs.Shaders.Protagonist.Geo.HighRotateGeo.frag",
                //New Music Analysis
                ProtagonistShaders.ProtagonistBar => "LightDancing.OpenTKs.Shaders.Protagonist.ProtagonistBar.frag",

                //MusicAnimateShaders
                MusicAnimateShaders.CircleWave_2 => "LightDancing.OpenTKs.Shaders.MusicAnimate.CircleWave_2.frag",
                MusicAnimateShaders.CircleRamp => "LightDancing.OpenTKs.Shaders.MusicAnimate.CircleRamp.frag",
                MusicAnimateShaders.ColorRotate => "LightDancing.OpenTKs.Shaders.MusicAnimate.ColorRotate.frag",

                //WallpaperAnimationShaders
                WallpaperAnimateShaders.ColorShift => "LightDancing.OpenTKs.Shaders.Animate.ColorShift.frag",
                WallpaperAnimateShaders.ColorSwirl => "LightDancing.OpenTKs.Shaders.Animate.ColorSwirl.frag",
                WallpaperAnimateShaders.ColorBounceBall => "LightDancing.OpenTKs.Shaders.Animate.ColorBounceBall.frag",
                WallpaperAnimateShaders.HalfColorCircle => "LightDancing.OpenTKs.Shaders.Animate.HalfColorCircle.frag",
                WallpaperAnimateShaders.MixColor => "LightDancing.OpenTKs.Shaders.Animate.MixColor.frag",
                WallpaperAnimateShaders.ColorStrip => "LightDancing.OpenTKs.Shaders.Animate.ColorStrip.frag",
                WallpaperAnimateShaders.FluidGrid => "LightDancing.OpenTKs.Shaders.Animate.FluidGrid.frag",
                WallpaperAnimateShaders.MeltingSmileFace => "LightDancing.OpenTKs.Shaders.Animate.MeltingSmileFace.frag",
                WallpaperAnimateShaders.ColorRings => "LightDancing.OpenTKs.Shaders.Animate.ColorRings.frag",
                WallpaperAnimateShaders.CheckboardFluid => "LightDancing.OpenTKs.Shaders.Animate.CheckboardFluid.frag",
                WallpaperAnimateShaders.FixColorRotate => "LightDancing.OpenTKs.Shaders.Animate.FixColorRotate.frag",
                null => "",
                _ => throw new ArgumentException(string.Format(ErrorTexts.ENUMS_NOT_HANDLE, shaderType, "GetShaderPath")),
            };
        }

        private bool GetStackingShader<T>(T shaderType)
        {
            bool result = false;
            switch (shaderType)
            {
                case AnimationShaders.ShiftColors:
                case AnimationShaders.StillColors:
                case BackgroundShaders.HalfCircle:
                case BackgroundShaders.Fire:
                case BackgroundShaders.FadeCircle:
                case BackgroundShaders.Diamond:
                case BackgroundShaders.HalfRainbow:
                case BackgroundShaders.Flowers:
                case BackgroundShaders.Triangle:
                case BackgroundShaders.ClappingBall:
                case BackgroundShaders.Rectangle:
                case BackgroundShaders.RoundCircle:
                case BackgroundShaders.Arrow:
                case BackgroundShaders.Grid:
                case BackgroundShaders.StripRotate:
                case BackgroundShaders.GridRotate:
                case BackgroundShaders.Squares:
                    result = false;
                    break;
                //CartoonBG
                case BackgroundShaders.CartoonBG1:
                case BackgroundShaders.CartoonBG2:
                case BackgroundShaders.CartoonBG3:
                case BackgroundShaders.CartoonBG4:
                case BackgroundShaders.CartoonBG5:
                case BackgroundShaders.CartoonBG6:
                case BackgroundShaders.CartoonBG7:
                //PopArt
                case BackgroundShaders.MovingDots:
                case BackgroundShaders.PAHorizontalStrip:
                case BackgroundShaders.VerticalStrip:
                case BackgroundShaders.MovingSlope:
                case BackgroundShaders.MovingTriangleWave:
                case BackgroundShaders.MovingDotLarge:
                //Geo
                case BackgroundShaders.CircleSquare:
                case BackgroundShaders.GeoSquare:
                case BackgroundShaders.GeoSquare2:
                case BackgroundShaders.GeoSquare3:
                case BackgroundShaders.GeoSquare4:
                case BackgroundShaders.GeoSquare5:
                case BackgroundShaders.GeoSquare6:
                case BackgroundShaders.GeoSquare7:
                case BackgroundShaders.GeoSquare8:
                case BackgroundShaders.RotateGeo:
                case BackgroundShaders.GeoCircle:
                case BackgroundShaders.Hexagon:
                case BackgroundShaders.GeoLines:
                case BackgroundShaders.GeoLines2:
                case BackgroundShaders.GeoLines3:
                case BackgroundShaders.GeoLines4:
                //Kale
                case BackgroundShaders.Kale1:
                case BackgroundShaders.Kale2:
                case BackgroundShaders.Kale3:
                case BackgroundShaders.Kale4:
                case BackgroundShaders.Kale5:
                case BackgroundShaders.Kale6:
                case BackgroundShaders.Kale7:
                case BackgroundShaders.Kale8:
                case BackgroundShaders.Kale9:
                case BackgroundShaders.Kale10:
                case BackgroundShaders.Kale11:
                case BackgroundShaders.Kale12:
                //Trip
                case BackgroundShaders.AlienFlower:
                case BackgroundShaders.AlienFlower2:
                case BackgroundShaders.AlienShip:
                case BackgroundShaders.BottleRainbow:
                case BackgroundShaders.CheckboardFire:
                case BackgroundShaders.FadeAlien:
                case BackgroundShaders.FlowerShape:
                case BackgroundShaders.Hamburger:
                case BackgroundShaders.Pacman:
                case BackgroundShaders.MeltSmile:
                case BackgroundShaders.Monster:
                    result = true;
                    break;

                case PanoramaShaders.Heart:
                case PanoramaShaders.HorizontalStrip:
                case PanoramaShaders.SpotLights:
                case PanoramaShaders.HeadLights:
                case PanoramaShaders.DiamondLights:
                case PanoramaShaders.CircleSquare:
                case PanoramaShaders.Wave:
                case PanoramaShaders.Ripple:
                case PanoramaShaders.RotateCircle:
                case PanoramaShaders.RotateDiamond:
                case PanoramaShaders.LineRotate:
                case PanoramaShaders.HorizontalLines:
                case PanoramaShaders.CrossLines:
                case PanoramaShaders.TriangleLine:
                case PanoramaShaders.Swirl:
                    result = false;
                    break;
                //Cartoon
                case PanoramaShaders.CartoonPanorama1:
                case PanoramaShaders.CartoonPanorama2:
                case PanoramaShaders.CartoonPanorama3:
                case PanoramaShaders.CartoonPanorama4:
                case PanoramaShaders.CartoonPanorama5:
                //PopArt
                case PanoramaShaders.FourSquare:
                case PanoramaShaders.DotSquare:
                case PanoramaShaders.CornerDot:
                case PanoramaShaders.CornerTriangleWave:
                //Geo
                case PanoramaShaders.MidCircleSquare:
                case PanoramaShaders.MidGeoCircle:
                case PanoramaShaders.MidGeoLines:
                case PanoramaShaders.MidGeoLines2:
                case PanoramaShaders.MidGeoLines3:
                case PanoramaShaders.MidGeoLines4:
                case PanoramaShaders.MidGeoSquare:
                case PanoramaShaders.MidGeoSquare2:
                case PanoramaShaders.MidGeoSquare3:
                case PanoramaShaders.MidGeoSquare4:
                case PanoramaShaders.MidGeoSquare5:
                case PanoramaShaders.MidGeoSquare6:
                case PanoramaShaders.MidGeoSquare7:
                case PanoramaShaders.MidGeoSquare8:
                case PanoramaShaders.MidHexagon:
                case PanoramaShaders.MidRotateGeo:
                    result = true;
                    break;


                case ProtagonistShaders.Rain:
                case ProtagonistShaders.BouncingBall:
                case ProtagonistShaders.RadiusLaser:
                case ProtagonistShaders.Laser:
                case ProtagonistShaders.RandomString:
                case ProtagonistShaders.StillBalls:
                case ProtagonistShaders.RandomCross:
                case ProtagonistShaders.RandomHeart:
                case ProtagonistShaders.RandomTarget:
                case ProtagonistShaders.RandomTree:
                case ProtagonistShaders.SquareWave:
                    result = false;
                    break;
                //Cartoon
                case ProtagonistShaders.CartoonProtagonist1:
                case ProtagonistShaders.CartoonProtagonist2:
                case ProtagonistShaders.CartoonProtagonist3:
                case ProtagonistShaders.CartoonProtagonist4:
                case ProtagonistShaders.CartoonProtagonist5:
                case ProtagonistShaders.CartoonProtagonist6:
                //PopArt
                case ProtagonistShaders.TSquare:
                case ProtagonistShaders.OSquare:
                case ProtagonistShaders.RandomCurve:
                case ProtagonistShaders.TwoSideCircle:
                case ProtagonistShaders.RandomCircle:
                case ProtagonistShaders.RandomRect:
                //Geo
                case ProtagonistShaders.HighCircleSquare:
                case ProtagonistShaders.HighGeoCircle:
                case ProtagonistShaders.HighGeoLines:
                case ProtagonistShaders.HighGeoLines2:
                case ProtagonistShaders.HighGeoLines3:
                case ProtagonistShaders.HighGeoLines4:
                case ProtagonistShaders.HighGeoSquare:
                case ProtagonistShaders.HighGeoSquare2:
                case ProtagonistShaders.HighGeoSquare3:
                case ProtagonistShaders.HighGeoSquare4:
                case ProtagonistShaders.HighGeoSquare5:
                case ProtagonistShaders.HighGeoSquare6:
                case ProtagonistShaders.HighGeoSquare7:
                case ProtagonistShaders.HighGeoSquare8:
                case ProtagonistShaders.HighHexagon:
                case ProtagonistShaders.HighRotateGeo:
                    result = true;
                    break;

                case null:
                    result = false;
                    break;
            };
            return result;
        }
    }
}
