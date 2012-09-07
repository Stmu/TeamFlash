using System;
using System.Collections.Generic;
using System.Threading;
using System.Configuration;
using System.Reflection;

namespace TeamFlash
{
    class Program
    {
        static void Main()
        {
			var buildServer = new BuildServer();
			var indicator = CreateBuildIndicator();
            
			indicator.Reset();

            while (!Console.KeyAvailable)
            {
                List<string> failingBuildNames;
                var lastBuildStatus = buildServer.GetLastBuildStatus(out failingBuildNames);

				indicator.Show(lastBuildStatus);

                Wait();
            }

			indicator.Reset();
        }

		static IBuildIndicator CreateBuildIndicator()
		{
			var indicatorType = Assembly.GetAssembly(typeof(Program)).GetType(ConfigurationManager.AppSettings["indicator"]);
			var constructor = indicatorType.GetConstructor(new Type[0]);
			var indicator = constructor.Invoke(new object[0]) as IBuildIndicator;
			return indicator;
		}

        static void Wait()
        {
            var delayCount = 0;
            while (delayCount < 30 &&
                !Console.KeyAvailable)
            {
                delayCount++;
                Thread.Sleep(1000);
            }
        }
    }
}
