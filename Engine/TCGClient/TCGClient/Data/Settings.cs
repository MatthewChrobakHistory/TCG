﻿namespace TCGClient.Data
{
    public class Settings
    {
        public static string File = Program.StartupPath + "data\\settings.xml";

        public string Host = "127.0.0.1";
        public int Port = 7001;

        public string Font = "tahoma";
        public string Username = "";
        public string Password = "";
        public bool RememberPassword = false;

        public bool Music = true;
        public float musicVolume = 1.0F;
        public bool Sound = true;
        public float soundVolume = 1.0F;
    }
}
