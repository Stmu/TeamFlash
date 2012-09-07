using System;

namespace TeamFlash
{
	public class UsbLightBuildIndicator : IBuildIndicator
	{
	    private readonly Monitor monitor;

		public UsbLightBuildIndicator ()
		{
			monitor = new Monitor();
		}

		public void Reset ()
		{
			TurnOffLights();
		}

		public void Show (BuildStatus status)
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
            monitor.SetLed(DelcomUsbLight.REDLED, false, false);
            monitor.SetLed(DelcomUsbLight.GREENLED, true, false);
            monitor.SetLed(DelcomUsbLight.BLUELED, false, false);
        }

        void TurnOnWarningLight()
        {
            monitor.SetLed(DelcomUsbLight.REDLED, false, false);
            monitor.SetLed(DelcomUsbLight.GREENLED, false, false);
            monitor.SetLed(DelcomUsbLight.BLUELED, true, false);
        }

        void TurnOnFailLight()
        {
            monitor.SetLed(DelcomUsbLight.REDLED, true, false);
            monitor.SetLed(DelcomUsbLight.GREENLED, false, false);
            monitor.SetLed(DelcomUsbLight.BLUELED, false, false);
        }

        void TurnOffLights()
        {
            monitor.SetLed(DelcomUsbLight.REDLED, false, false);
            monitor.SetLed(DelcomUsbLight.GREENLED, false, false);
            monitor.SetLed(DelcomUsbLight.BLUELED, false, false);
        }
	}
}

