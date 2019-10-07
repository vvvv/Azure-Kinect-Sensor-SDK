using System;
using System.Threading;
using Microsoft.Azure.Kinect.Sensor;
using Microsoft.Azure.Kinect.Sensor.Record;
using NUnit.Framework;

namespace Tests
{
    public class Tests
    {
        string recordingPath;
        [SetUp]
        public void Setup()
        {
            recordingPath = System.IO.Path.Combine(System.IO.Path.GetTempPath(), "testfile.mkv");
        }

        [TearDown]
        public void TearDown()
        {
            System.IO.File.Delete(recordingPath);
        }
        

        [Test]
        public void Test1()
        {
            DeviceConfiguration deviceConfiguration = new DeviceConfiguration()
            {
                CameraFPS = FPS.FPS30,
                ColorFormat = ImageFormat.ColorBGRA32,
                ColorResolution = ColorResolution.R720p,
                DepthDelayOffColor = TimeSpan.FromMilliseconds(123),
                DepthMode = DepthMode.NFOV_2x2Binned,
                DisableStreamingIndicator = true,
                SuboridinateDelayOffMaster = TimeSpan.FromMilliseconds(456),
                SynchronizedImagesOnly  = true,
                WiredSyncMode = WiredSyncMode.Subordinate
            };

            using (Record record = Record.Create(this.recordingPath, null, deviceConfiguration))
            {

                record.AddCustomVideoTrack("CustomVideo", "V_CUSTOM1", new byte[] { 1, 2, 3 }, new RecordVideoSettings() { FrameRate = 1, Height = 10, Width = 20 });
                record.AddCustomSubtitleTrack("CustomSubtitle", "S_CUSTOM1", new byte[] { 4, 5, 6, 7 }, new RecordSubtitleSettings() { HighFrequencyData = false});
                record.AddTag("MyTag1", "one");
                record.AddTag("MyTag2", "two");

                record.WriteHeader();

                for (int i = 0; i < 10; i++)
                {
                    double timeStamp = 10.0 + i * 1.0;

                    using (Capture c = new Capture())
                    {
                        c.Color = new Image(ImageFormat.ColorBGRA32, 1280, 720);
                        c.IR = new Image(ImageFormat.IR16, 320, 288);
                        c.Depth = new Image(ImageFormat.Depth16, 320, 288);
                        c.Temperature = 25.0f;
                        c.Color.DeviceTimestamp = TimeSpan.FromSeconds(timeStamp);
                        c.Depth.DeviceTimestamp = TimeSpan.FromSeconds(timeStamp) + deviceConfiguration.DepthDelayOffColor;
                        c.IR.DeviceTimestamp = TimeSpan.FromSeconds(timeStamp) + deviceConfiguration.DepthDelayOffColor;

                        c.Color.Exposure = TimeSpan.FromMilliseconds(12);
                        c.Color.ISOSpeed = 100;
                        c.Color.SystemTimestampNsec = System.Diagnostics.Stopwatch.GetTimestamp();
                        c.Color.WhiteBalance = 2;

                        c.Depth.SystemTimestampNsec = System.Diagnostics.Stopwatch.GetTimestamp();

                        c.IR.SystemTimestampNsec = System.Diagnostics.Stopwatch.GetTimestamp();

                        record.WriteCapture(c);
                    }

                    for (int y = 0; y < 10; y++)
                    {
                        ImuSample imuSample = new ImuSample()
                        {
                            AccelerometerSample = new System.Numerics.Vector3(1.0f, 2.0f, 3.0f),
                            GyroSample = new System.Numerics.Vector3(4.0f, 5.0f, 6.0f),
                            AccelerometerTimestamp = TimeSpan.FromSeconds(timeStamp + 0.1 * y),
                            GyroTimestamp = TimeSpan.FromSeconds(timeStamp + 0.1 * y),
                            Temperature = 26.0f,
                        };

                        record.WriteImuSample(imuSample);
                    }

                    byte[] customData = new byte[i + 1];
                    for (int x = 0; x < customData.Length; x++)
                    {
                        customData[x] = (byte)(i + x);
                    }
                    record.WriteCustomTrackData("CustomVideo", TimeSpan.FromSeconds(timeStamp), customData);

                    for (int x = 0; x < customData.Length; x++)
                    {
                        customData[x] = (byte)(i + x + 1);
                    }
                    record.WriteCustomTrackData("CustomSubtitle", TimeSpan.FromSeconds(timeStamp), customData);

                    record.Flush();
                }
            }


            using (Playback playback = Playback.Open(recordingPath))
            {
                Assert.IsTrue(playback.CheckTrackExists("CustomVideo"));
                Assert.IsTrue(playback.CheckTrackExists("CustomSubtitle"));
                Assert.AreEqual("V_CUSTOM1", playback.GetTrackCodecId("CustomVideo"));
                Assert.AreEqual(new byte[] { 1, 2, 3 }, playback.GetTrackCodecContext("CustomVideo"));

                for (int i = 0; i < 10; i++)
                {
                    double timeStamp = 10.0 + i * 1.0;

                    
                    using (Capture c = playback.GetNextCapture())
                    {

                        Assert.AreEqual(25.0f, c.Temperature);

                        c.Color = new Image(ImageFormat.ColorBGRA32, 1280, 720);
                        c.IR = new Image(ImageFormat.IR16, 320, 288);
                        c.Depth = new Image(ImageFormat.Depth16, 320, 288);

                        Assert.AreEqual(ImageFormat.ColorBGRA32, c.Color.Format);
                        Assert.AreEqual(1280, c.Color.WidthPixels);
                        Assert.AreEqual(720, c.Color.HeightPixels);
                        
                        Assert.AreEqual(TimeSpan.FromSeconds(timeStamp), c.Color.DeviceTimestamp);
                        Assert.AreEqual(TimeSpan.FromSeconds(timeStamp) + deviceConfiguration.DepthDelayOffColor, c.Depth.DeviceTimestamp);
                        Assert.AreEqual(TimeSpan.FromSeconds(timeStamp) + deviceConfiguration.DepthDelayOffColor, c.IR.DeviceTimestamp);

                        Assert.AreEqual(TimeSpan.FromMilliseconds(12), c.Color.Exposure);
                        Assert.AreEqual(100, c.Color.ISOSpeed);
                        Assert.AreEqual(0, c.Color.SystemTimestampNsec);
                        Assert.AreEqual(2, c.Color.WhiteBalance);
                    }

                    for (int y = 0; y < 10; y++)
                    {
                        ImuSample imuSample = new ImuSample()
                        {
                            AccelerometerSample = new System.Numerics.Vector3(1.0f, 2.0f, 3.0f),
                            GyroSample = new System.Numerics.Vector3(4.0f, 5.0f, 6.0f),
                            AccelerometerTimestamp = TimeSpan.FromSeconds(timeStamp + 0.1 * y),
                            GyroTimestamp = TimeSpan.FromSeconds(timeStamp + 0.1 * y),
                            Temperature = 26.0f,
                        };

                        ImuSample readSample = playback.GetNextImuSample();
                        Assert.AreEqual(imuSample, readSample);
                    }

                    byte[] customData = new byte[i + 1];
                    for (int x = 0; x < customData.Length; x++)
                    {
                        customData[x] = (byte)(i + x);
                    }
                    using (DataBlock videoBlock = playback.GetNextDataBlock("CustomVideo"))
                    {
                        Assert.AreEqual(customData, videoBlock);
                        Assert.AreEqual(TimeSpan.FromSeconds(timeStamp), videoBlock.DeviceTimestamp);
                    }

                    for (int x = 0; x < customData.Length; x++)
                    {
                        customData[x] = (byte)(i + x + 1);
                    }
                    using (DataBlock subtitleBlock = playback.GetNextDataBlock("CustomSubtitle"))
                    {
                        Assert.AreEqual(customData, subtitleBlock);
                        Assert.AreEqual(TimeSpan.FromSeconds(timeStamp), subtitleBlock.DeviceTimestamp);
                    }
                }
            }
            Assert.Pass();
        }
    }
}