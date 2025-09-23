using System;
using System.Collections.Generic;
using OpenCvSharp;

namespace Image_and_Video_Processor
{
    public class DeviceManager
    {
        private static List<CameraDevice> devices = new List<CameraDevice>();

        public static CameraDevice[] GetAllDevices()
        {
            devices.Clear();
            for (int i = 0; i < 10; i++)
            {
                using (var capture = new VideoCapture(i))
                {
                    if (capture.IsOpened())
                    {
                        devices.Add(new CameraDevice
                        {
                            Index = i,
                            Name = $"Camera {i}"
                        });
                    }
                }
            }

            return devices.ToArray();
        }
        public static CameraDevice GetDevice(int deviceIndex)
        {
            if (deviceIndex < 0 || deviceIndex >= devices.Count)
                throw new IndexOutOfRangeException("Invalid device index.");

            return devices[deviceIndex];
        }
    }
    public class CameraDevice
    {
        public int Index { get; set; }
        public string Name { get; set; }

        public override string ToString() => Name;
    }
}
