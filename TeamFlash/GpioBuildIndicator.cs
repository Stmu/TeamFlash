using System;
using System.Threading;
using RaspberryPiDotNet;

namespace TeamFlash
{
	public class BuildIndicator : IBuildIndicator
	{
		GPIO greenPin;
		GPIO redPin;
		GPIO yellowPin;
		Thread flashingThread;

		public BuildIndicator ()
		{
			greenPin = new GPIOMem (GPIO.GPIOPins.GPIO00);
			redPin = new GPIOMem (GPIO.GPIOPins.GPIO01);
			yellowPin = new GPIOMem (GPIO.GPIOPins.GPIO04);
		}

		public void Reset ()
		{
			KillFlashingThread ();
			Light (Leds.Off);
		}

		void KillFlashingThread ()
		{
			if (flashingThread != null)
				flashingThread.Abort ();
			flashingThread = null;
		}

		public void Show (BuildStatus status)
		{
			KillFlashingThread ();

			switch (status) {
			case BuildStatus.Failed:
				Light (Leds.Red);
				break;
			case BuildStatus.Investigating:
				Flash (Leds.Red, Leds.Yellow);
				break;
			case BuildStatus.Passed:
				Light (Leds.Green);
				break;
			case BuildStatus.Unavailable:
				Flash (Leds.Red);
				break;
			default:
				Light (Leds.Red | Leds.Green | Leds.Yellow);
				break;
			}
		}

		private void Flash (Leds on, Leds off = Leds.Off)
		{
			flashingThread = new Thread (() => { 
				var isOn = true;
				while (true) {
					if (isOn) {
						Light (on);
					} else {
						Light (off);
					}
					Thread.Sleep (1000);
				}
			}
			);
			flashingThread.Start ();
		}

		private void Light (Leds leds)
		{
			greenPin.Write ((leds & Leds.Green) == Leds.Green);
			redPin.Write ((leds & Leds.Red) == Leds.Red);
			yellowPin.Write ((leds & Leds.Yellow) == Leds.Yellow);
		}
	}

	[Flags]
	public enum Leds
	{
		Off = 0,
		Red = 1,
		Green = 2,
		Yellow = 4
	}
}
