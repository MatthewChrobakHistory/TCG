namespace TCGServer.IO
{
    public static class Encryption
    {
        private static string _members = "jnd]oY(;kA86J wv{uVIzaXUf2=/W*!LbqFGiS30x&[_5Q~^y)N>4sl,|H<1?%g`7r9ZC\"#@tP+$T}DmO-K'c.BpEeM:hR\\";

        public static string Encrypt(string input) {
            string value = "";
            for (int i = 0; i < input.Length; i++) {
                var character = input[i];
                int index = ((_members.IndexOf(character) + input.Length) + i) % _members.Length;
                value += _members[index];
            }
            return value;
        }

        private static int Modulus(int a, int b) {
            return ((a % b) + b) % b;
        }

        public static string Decrypt(string input) {
            string value = "";
            for (int i = 0; i < input.Length; i++) {
                var character = input[i];
                int index = (_members.Length - input.Length + _members.IndexOf(character) + i) % _members.Length;
                value += _members[index];
            }
            return value;
        }
    }
}
