using System.IO;

namespace TeamFlash.Linux
{
    class LedBorgBuildLight : IBuildLight
    {
        private void SetColor(string color)
        {
            using (var stream = File.Create("/dev/ledborg"))
            using (var writer = new StreamWriter(stream))
                writer.Write(color);
        }

        public void Success()
        {
            SetColor("020");
        }

        public void Warning()
        {
            SetColor("220");
        }

        public void Fail()
        {
            SetColor("200");
        }

        public void Off()
        {
            SetColor("000");
        }
    }
}
