using System;

namespace TeamFlash.RaspberryPi
{
    public class UsbLightBuildIndicator : IBuildIndicator
    {
        public void Reset()
        {
            TurnOffLights();
        }

        public void Show(BuildStatus status)
        {
            switch (status)
            {
                case BuildStatus.Unavailable:
                    TurnOffLights();
                    Console.WriteLine(DateTime.Now.ToShortTimeString() + " Server unavailable");
                    break;
                case BuildStatus.Passed:
                    TurnOnSuccessLight();
                    Console.WriteLine(DateTime.Now.ToShortTimeString() + " Passed");
                    break;
                case BuildStatus.Investigating:
                    TurnOnWarningLight();
                    Console.WriteLine(DateTime.Now.ToShortTimeString() + " Investigating");
                    break;
                case BuildStatus.Failed:
                    TurnOnFailLight();
                    Console.WriteLine(DateTime.Now.ToShortTimeString() + " Failed");
                    break;
            }
        }

        void TurnOnSuccessLight()
        {
            SetColor(DelcomUsbLight.Green);
        }

        void TurnOnWarningLight()
        {
            SetColor(DelcomUsbLight.Blue);
        }

        void TurnOnFailLight()
        {
            SetColor(DelcomUsbLight.Red);
        }

        void TurnOffLights()
        {
            SetColor(DelcomUsbLight.Off);
        }

        void SetColor(int color)
        {
            for (var i = 0; i < 5; i++)
            {
                var ret = DelcomUsbLight.OpenDevice();
                if (ret != 0) continue;
                ret = DelcomUsbLight.SetColor(color);
                DelcomUsbLight.CloseDevice();
                if (ret == 0) break;
            }
        }
    }
}

