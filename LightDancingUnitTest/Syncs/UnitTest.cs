using LightDancing.Colors;
using LightDancing.Enums;
using LightDancing.OpenTKs.Animations;
using LightDancing.Syncs;
using LightDancingUnitTest.Common;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;


namespace LightDancingUnitTest.Syncs
{
    [TestClass]
    public class UnitTest
    {
        [TestMethod]
        public void TestMockDataConvert()
        {
            /// | Blue   | Red    | Purple |
            /// | Yellow | Orange | Green  |
            /// 
            int xCounts = 3;
            int yCounts = 2;

            LightDancing.OpenTKs.Converter converter = new LightDancing.OpenTKs.Converter(xCounts, yCounts);
            byte[] openTkMockData = new byte[] { 255, 255, 0, 255, 255, 128, 0, 255, 0, 255, 0, 255, 0, 0, 255, 255, 255, 0, 0, 255, 255, 0, 255, 255 };

            var result = converter.Convert2Colors(openTkMockData);
            Assert.IsTrue(result[0, 0].R == 0 && result[0, 0].G == 0 && result[0, 0].B == 255);
            Assert.IsTrue(result[0, 1].R == 255 && result[0, 1].G == 0 && result[0, 1].B == 0);
            Assert.IsTrue(result[0, 2].R == 255 && result[0, 2].G == 0 && result[0, 2].B == 255);
            Assert.IsTrue(result[1, 0].R == 255 && result[1, 0].G == 255 && result[1, 0].B == 0);
            Assert.IsTrue(result[1, 1].R == 255 && result[1, 1].G == 128 && result[1, 1].B == 0);
            Assert.IsTrue(result[1, 2].R == 0 && result[1, 2].G == 255 && result[1, 2].B == 0);
        }

        [TestMethod]
        public void TestAllBackgroundWallpaper()
        {
            List<string> modes = new List<string>();
            foreach (BackgroundShaders item in Enum.GetValues(typeof(BackgroundShaders)))
            {
                modes.Add(item.ToString());
            }

            OpenTKSyncHelper helper = OpenTKSyncHelper.Instance;
            OpentkParameters parameters = new OpentkParameters()
            {
                DisplayWindows = true,
                IsAuto = false,
                Mode = OpenTKMode.Wallpaper,
                Wallpapers = modes,
                Shuffle = true,
            };

            Task.Run(() =>
            {
                helper.Start(parameters); // run default and invisible windows
            });

            
            Thread.Sleep(15000);
            helper.Stop();
        }

        [TestMethod]
        public void TestAllPanoramaWallpaper()
        {
            List<string> modes = new List<string>();
            foreach (PanoramaShaders item in Enum.GetValues(typeof(PanoramaShaders)))
            {
                modes.Add(item.ToString());
            }

            OpenTKSyncHelper helper = OpenTKSyncHelper.Instance;
            OpentkParameters parameters = new OpentkParameters()
            {
                DisplayWindows = true,
                IsAuto = false,
                Mode = OpenTKMode.Wallpaper,
                Wallpapers = modes,
                Shuffle = false,
            };

            Task.Run(() =>
            {
                helper.Start(parameters); // run default and invisible windows
            });

            Thread.Sleep(15000);
            //helper.Stop();
        }

        [TestMethod]
        public void TestAllProtagonistsWallpaper()
        {
            List<string> modes = new List<string>();
            foreach (ProtagonistShaders item in Enum.GetValues(typeof(ProtagonistShaders)))
            {
                modes.Add(item.ToString());
            }

            OpenTKSyncHelper helper = OpenTKSyncHelper.Instance;
            OpentkParameters parameters = new OpentkParameters()
            {
                DisplayWindows = true,
                IsAuto = false,
                Mode = OpenTKMode.Wallpaper,
                Wallpapers = modes,
                Shuffle = false,
            };

            Task.Run(() =>
            {
                helper.Start(parameters); // run default and invisible windows
            });

            Thread.Sleep(15000);
            helper.Stop();
        }

        [TestMethod]
        public void TestSingleWallpaper()
        {
            OpenTKSyncHelper helper = OpenTKSyncHelper.Instance;
            OpentkParameters parameters = new OpentkParameters()
            {
                DisplayWindows = true,
                IsAuto = false,
                Mode = OpenTKMode.Wallpaper,
            };

            AnimationGroup animationGroup = new AnimationGroup(null, PanoramaShaders.CartoonPanorama2, null);
            helper.ChangeShader(animationGroup);

            Task.Run(() =>
            {
                helper.Start(parameters); // run default and invisible windows
            });

            Thread.Sleep(15000);
            helper.Stop();
        }

        [TestMethod]
        public void TestMusicNonAutoGroup()
        {
            OpenTKSyncHelper helper = OpenTKSyncHelper.Instance;
            OpentkParameters parameters = new OpentkParameters()
            {
                DisplayWindows = true,
                IsAuto = false,
                Mode = OpenTKMode.MusicSync
            };
            AnimationGroup animationGroup = new AnimationGroup(BackgroundShaders.PAHorizontalStrip, PanoramaShaders.CircleSquare, ProtagonistShaders.BouncingBall);
            helper.ChangeShader(animationGroup);
            Task.Run(() =>
            {
                helper.Start(parameters); // run default and invisible windows
            });

            Thread.Sleep(15000);
            helper.Stop();
        }

        [TestMethod]
        public void TestMusicSyncAutoGroup()
        {
            OpenTKSyncHelper helper = OpenTKSyncHelper.Instance;
            OpentkParameters parameters = new OpentkParameters()
            {
                DisplayWindows = true,
                IsAuto = true,
                Mode = OpenTKMode.MusicSync
            };
            Task.Run(() =>
            {
                helper.Start(parameters); // run default and invisible windows
            });

            Thread.Sleep(15000);
            helper.Stop();
        }

        [TestMethod]
        public void TestMusicSyncCustomGenres()
        {
            OpenTKSyncHelper helper = OpenTKSyncHelper.Instance;
            OpentkParameters parameters = new OpentkParameters()
            {
                DisplayWindows = true,
                IsAuto = true,
                Mode = OpenTKMode.MusicSync,
                CustomGenres = Genres.Ballad
            };

            Task.Run(() =>
            {
                helper.Start(parameters); // run default and invisible windows
            });

            Thread.Sleep(15000);
            helper.Stop();
        }

        [TestMethod]
        public void TestSetMLsPath()
        {
            OpenTKSyncHelper helper = OpenTKSyncHelper.Instance;
            OpentkParameters parameters = new OpentkParameters()
            {
                DisplayWindows = true,
                IsAuto = true,
                Mode = OpenTKMode.MusicSync
            };

            helper.SetMLsFilePath(Environment.CurrentDirectory + "\\MLs\\Source\\GenreModel.zip");

            Task.Run(() =>
            {
                helper.Start(parameters); // run default and invisible windows
            });

            Thread.Sleep(1500000);
            helper.Stop();
        }

        [TestMethod]
        public void TestBeatAnalysis()
        {
            OpenTKSyncHelper helper = OpenTKSyncHelper.Instance;
            helper.ChangeShader(new AnimationGroup(MusicAnimateShaders.CircleRamp));
            OpentkParameters parameters = new OpentkParameters()
            {
                DisplayWindows = true,
                IsAuto = false,
                Mode = OpenTKMode.BeatAnalysis,
            };

            //AnimationGroup animationGroup = new AnimationGroup(null, PanoramaShaders.CartoonPanorama2, null);
            

            Task.Run(() =>
            {
                helper.Start(parameters); // run default and invisible windows
            });

            Thread.Sleep(15000000);
            helper.Stop();
        }

        [TestMethod]
        public void TestGetBase64WithGif()
        {
            GifSyncHelper helper = GifSyncHelper.Instance;

            helper.ReadFile("C:\\Users\\j4092\\Desktop\\Image\\IMG_2834.png", out string file, out System.Drawing.Image img, out string errorM);

            Task.Run(() =>
            {
                helper.Start();
            });

            Thread.Sleep(ConstValues.PROCESS_MS * 3);
            string base64 = helper.GetBase64StringColor();
            Thread.Sleep(ConstValues.PROCESS_MS * 3);
        }

        [TestMethod]
        public void TestMusicAnimate()
        {
            OpenTKSyncHelper helper = OpenTKSyncHelper.Instance;
            OpentkParameters parameters = new OpentkParameters()
            {
                DisplayWindows = true,
                IsAuto = false,
                Mode = OpenTKMode.BeatAnalysis
            };
            AnimationGroup animationGroup = new AnimationGroup(MusicAnimateShaders.ColorRotate);
            helper.ChangeShader(animationGroup);
            Task.Run(() =>
            {
                helper.Start(parameters); // run default and invisible windows
            });

            Thread.Sleep(1500000);
            helper.Stop();
        }

        [TestMethod]
        public void TestWallpaperAnimate()
        {
            OpenTKSyncHelper helper = OpenTKSyncHelper.Instance;
            OpentkParameters parameters = new OpentkParameters()
            {
                DisplayWindows = true,
                IsAuto = false,
                Mode = OpenTKMode.Animate,
                Animate = "FixColorRotate",
            };
            
            Task.Run(() =>
            {
                helper.Start(parameters); // run default and visible windows
            });
            //helper.ChangeAnimate("FixColorRotate");
            Thread.Sleep(3000);

            //foreach (WallpaperAnimateShaders item in Enum.GetValues(typeof(WallpaperAnimateShaders)))
            //{
            //    helper.ChangeAnimate(item.ToString());
            //    Thread.Sleep(3000);
            //}

            helper.Stop();
        }

        [TestMethod]
        public void TestResetFilterWhenMusicAnimates()
        {
            OpenTKSyncHelper helper = OpenTKSyncHelper.Instance;
            OpentkParameters parameters = new OpentkParameters()
            {
                DisplayWindows = true,
                IsAuto = false,
                Mode = OpenTKMode.Animate,
                Animate = "FixColorRotate",
            };

            Task.Run(() =>
            {
                helper.Start(parameters); // run default and visible windows
            });

            helper.SetColorFilter(ColorFilter.RedFilter, 1.0f);
            Thread.Sleep(3000);
            helper.Stop();

            parameters = new OpentkParameters()
            {
                DisplayWindows = true,
                IsAuto = false,
                Mode = OpenTKMode.BeatAnalysis
            };
            AnimationGroup animationGroup = new AnimationGroup(MusicAnimateShaders.ColorRotate);
            helper.ChangeShader(animationGroup);
            Task.Run(() =>
            {
                helper.Start(parameters); // run default and invisible windows
            });

            Thread.Sleep(1500);
            helper.Stop();
        }

        [TestMethod]
        public void TestWallpaperAnimateFunctionsSetting()
        {
            OpenTKSyncHelper helper = OpenTKSyncHelper.Instance;

            //helper.ChangeColorSet(colorRGBs);
            //Thread.Sleep(2000);
            helper.SetColorFilter(ColorFilter.RedFilter, 1.0f);
            helper.SetNoise(0.5f);
            //helper.ChangeColorSat(colorRGBs, 0.3f);
            helper.SetSpeedConfig(7);

            
            OpentkParameters parameters = new OpentkParameters()
            {
                DisplayWindows = true,
                IsAuto = false,
                Mode = OpenTKMode.Animate,
                Animate = WallpaperAnimateShaders.ColorBounceBall.ToString(),
            };

            Task.Run(() =>
            {
                helper.Start(parameters); // run default and visible windows
            });

            Thread.Sleep(2000);
            List<ColorRGB> colorRGBs = new List<ColorRGB>()
            {
                new ColorRGB(255, 125, 0)
            };

            helper.ChangeColorSet(colorRGBs);
            Thread.Sleep(2000);
            helper.SetColorFilter(ColorFilter.GreenFilter, 1.0f);
            Thread.Sleep(2000);
            helper.SetNoise(0.5f);
            Thread.Sleep(2000);
            helper.SetNoise(0);
            helper.ChangeColorSat(colorRGBs, 0.3f);
            Thread.Sleep(2000);
            helper.SetSpeedConfig(7);
            Thread.Sleep(1000);

            helper.Stop();

            parameters = helper.GetOpentkParameters();
            Task.Run(() =>
            {
                helper.Start(parameters); // run default and visible windows
            });

            Thread.Sleep(3000);
            helper.Stop();
        }

        [TestMethod]
        public void TestOpenTkThread()
        {
            OpenTKSyncHelper helper = OpenTKSyncHelper.Instance;
            OpentkParameters parameters = new OpentkParameters()
            {
                DisplayWindows = true,
                IsAuto = false,
                Mode = OpenTKMode.Wallpaper,
            };

            AnimationGroup animationGroup = new AnimationGroup(null, PanoramaShaders.CartoonPanorama2, null);
            helper.ChangeShader(animationGroup);
            helper.Start(parameters); 
            Thread.Sleep(3000);
            helper.Stop();

            animationGroup = new AnimationGroup(null, PanoramaShaders.CartoonPanorama4, null);
            helper.ChangeShader(animationGroup);
            Thread.Sleep(1000);
            helper.Start(parameters); 
            Thread.Sleep(3000);
            helper.Stop();

            animationGroup = new AnimationGroup(null, PanoramaShaders.CartoonPanorama1, null);
            helper.ChangeShader(animationGroup);
            Thread.Sleep(1000);
            helper.Start(parameters); 
            Thread.Sleep(3000);
            helper.Stop();

            animationGroup = new AnimationGroup(null, PanoramaShaders.CartoonPanorama2, null);
            helper.ChangeShader(animationGroup);
            Thread.Sleep(1000);
            helper.Start(parameters); 
            Thread.Sleep(3000);
            helper.Stop();
        }
        [TestMethod]
        public void TestScreenSync()
        {
            ScreenSyncHelper.Instance.RegisterFlushList(null);
            Thread.Sleep(1000);
            ScreenSyncHelper.Instance.ChangeMonitor(0);

            ScreenSyncHelper.Instance.Start();
            Thread.Sleep(3000);
            for (int i = 0; i < 10; i++)
            {
                var test = ScreenSyncHelper.Instance.GetColors();
            }
            Thread.Sleep(5000);
            ScreenSyncHelper.Instance.Stop();
            Thread.Sleep(5000);
            ScreenSyncHelper.Instance.Start();
            Thread.Sleep(3000);
            for (int i = 0; i < 10; i++)
            {
                var test = ScreenSyncHelper.Instance.GetColors();
            }
            Thread.Sleep(5000);
            ScreenSyncHelper.Instance.Stop();
        }
        [TestMethod]
        public void TestLapTopScreenSync()
        {
            ScreenSyncHelper.Instance.GetIsCanUseDXGI();
        }
    }
}
