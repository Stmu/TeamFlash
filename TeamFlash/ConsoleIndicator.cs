using System;

namespace TeamFlash
{
    public class ConsoleIndicator : IBuildIndicator
    {
        public void Reset()
        {
            Console.WriteLine("{0}: Reset Indicator State.", DateTime.Now);
        }

        public void Show(BuildStatus status)
        {
            Console.WriteLine("{0}: Set Indicator State to {1}.", DateTime.Now, status);
        }
    }
}
