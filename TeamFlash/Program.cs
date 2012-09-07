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
            var indicatorTypeName = ConfigurationManager.AppSettings["indicator"];

            var indicatorType = Assembly.GetAssembly(typeof(Program)).GetType(indicatorTypeName);
            if (indicatorType == null) throw new Exception("Could not find type " + indicatorTypeName);

            var constructor = indicatorType.GetConstructor(new Type[0]);
            if (constructor == null) throw new Exception("Could not find parameterless constructor on type " + indicatorTypeName);

            var indicator = constructor.Invoke(new object[0]) as IBuildIndicator;
            if (indicator == null) throw new Exception(indicatorTypeName + " does not implement IBuildIndicator.");

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
