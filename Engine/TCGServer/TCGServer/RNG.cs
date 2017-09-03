using System;

namespace TCGServer
{
    public static class RNG
    {
        private static Random _rndEngine = new Random();

        public static int Get(int low, int inclusivehigh) {
            return _rndEngine.Next(low, inclusivehigh + 1);
        }
    }
}
