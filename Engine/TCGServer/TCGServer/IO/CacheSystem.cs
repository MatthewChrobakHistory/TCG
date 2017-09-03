using System.IO;

namespace TCGServer.IO
{
    public static class CacheSystem
    {
        private static string FilePath = "data\\cache.dat";

        public static void CheckCache() {
            if (!File.Exists(Program.StartupPath + FilePath)) {
                var file = new DataBuffer();

                foreach (var card in Data.DataManager.Cards) {
                    
                }

                file.Save(FilePath);
            }
        }
    }
}
