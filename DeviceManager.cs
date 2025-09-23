using System;
using System.Collections;
using System.Runtime.InteropServices;

namespace Image_and_Video_Processor
{
    public class DeviceManager
    {
        [DllImport("avicap32.dll", CharSet = CharSet.Ansi)]
        private static extern bool capGetDriverDescriptionA(
            short wDriverIndex,
            [MarshalAs(UnmanagedType.LPStr)] System.Text.StringBuilder lpszName,
            int cbName,
            [MarshalAs(UnmanagedType.LPStr)] System.Text.StringBuilder lpszVer,
            int cbVer);

        private static ArrayList devices = new ArrayList();

        public static Device[] GetAllDevices()
        {
            devices.Clear();
            var name = new System.Text.StringBuilder(100);
            var version = new System.Text.StringBuilder(100);

            for (short i = 0; i < 10; i++)
            {
                if (capGetDriverDescriptionA(i, name, name.Capacity, version, version.Capacity))
                {
                    Device d = new Device(i)
                    {
                        Name = name.ToString().Trim(),
                        Version = version.ToString().Trim()
                    };
                    devices.Add(d);
                }
            }

            return (Device[])devices.ToArray(typeof(Device));
        }

        public static Device GetDevice(int deviceIndex)
        {
            return (Device)devices[deviceIndex];
        }
    }
}
