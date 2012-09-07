using System;
using System.Threading;
using RaspberryPiDotNet;

namespace TeamFlash
{
	public interface IBuildIndicator
	{
		void Reset();
		void Show(BuildStatus status);
	}
}
