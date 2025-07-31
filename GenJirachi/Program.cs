using PKHeX.Core;
using System;
using System.IO;

namespace GenJirachi {
    public static class Program
    {
        
        static void Main(string[] args)
        {
            if (args.Length != 1 || 
                !ushort.TryParse(args[0].StartsWith("0x", StringComparison.OrdinalIgnoreCase) 
                ? args[0].Substring(2) 
                : args[0], 
                System.Globalization.NumberStyles.HexNumber, 
                null, 
                out ushort seed))
            {
                Console.WriteLine("Usage: GenJirachi <checksum>");
                return;
            }

            var jirachi = CreateWishmakerJirachi(seed);

            byte[] data = jirachi.EncryptedPartyData;
            File.WriteAllBytes("wishmaker_jirachi.pk3", data);

            Console.WriteLine("Wishmaker Jirachi generated!\n");
            Console.WriteLine($"PID     : 0x{jirachi.PID:X8}");
            Console.WriteLine($"OTID    : {jirachi.TID16}");
            Console.WriteLine($"SID     : {jirachi.SID16}");
            Console.WriteLine($"Nature  : {jirachi.Nature}");
            Console.WriteLine($"Shiny   : {(jirachi.IsShiny ? "Yes" : "No")}");
            Console.WriteLine($"Ball    : {jirachi.Ball}");
            Console.WriteLine("IVs     :");
            Console.WriteLine($"  HP     : {jirachi.IV_HP}");
            Console.WriteLine($"  Atk    : {jirachi.IV_ATK}");
            Console.WriteLine($"  Def    : {jirachi.IV_DEF}");
            Console.WriteLine($"  SpA    : {jirachi.IV_SPA}");
            Console.WriteLine($"  SpD    : {jirachi.IV_SPD}");
            Console.WriteLine($"  Spe    : {jirachi.IV_SPE}");
            Console.WriteLine("\nSaved as wishmaker_jirachi.pk3");
        }


        public static PK3 CreateWishmakerJirachi(ushort seed)
        {
            var rng = new LCRNG(seed);

            uint pid = GeneratePID(rng);

            var jirachi = new PK3
            {
                Species = (int)Species.Jirachi,
                EXP = 156,
                PID = pid,
                Nature = (Nature)(pid % 25),
            };

            // Trainer Info
            jirachi.TID16 = 20043;
            jirachi.SID16 = 0;
            jirachi.OriginalTrainerName = "WISHMKR";
            jirachi.Gender = 0; // Male OT

            // Origins
            jirachi.Language = 2;
            jirachi.Version = (GameVersion)2; // Ruby
            jirachi.FatefulEncounter = false;
            jirachi.MetLocation = 0xFFFF;
            jirachi.MetLevel = 5;
            jirachi.Ball = 4;

            // Friendship
            jirachi.OriginalTrainerFriendship = 100;

            // IVs
            ushort ivBits1 = rng.Next(); // First 3 IVs
            ushort ivBits2 = rng.Next(); // Last 3 IVs

            jirachi.IV_HP   =  ivBits1        & 0x1F;
            jirachi.IV_ATK  = (ivBits1 >> 5)  & 0x1F;
            jirachi.IV_DEF  = (ivBits1 >> 10) & 0x1F;
            jirachi.IV_SPE  =  ivBits2        & 0x1F;
            jirachi.IV_SPA  = (ivBits2 >> 5)  & 0x1F;
            jirachi.IV_SPD  = (ivBits2 >> 10) & 0x1F;

            // Moves: Wish (273), Confusion (93), Rest (156)
            jirachi.Moves = Array.ConvertAll(new int[] { 273, 93, 156, 0 }, x => (ushort)x);

            // Held Item: Ganlon (169) or Salac (170)
            jirachi.HeldItem = ((rng.Next() / 3) & 1) == 0 ? 170 : 169;

            // Nickname
            jirachi.IsNicknamed = false;
            jirachi.Nickname = "JIRACHI";

            // Pokerus
            jirachi.PokerusState = 0x0;
            jirachi.PokerusDays = 0;
            jirachi.PokerusStrain = 0;

            // Clean up
            jirachi.RefreshChecksum();
            return jirachi;
        }

        // PID is generated using Reverse Method 1
        static uint GeneratePID(LCRNG rng)
        {
            ushort low = rng.Next();      // First RNG call → low 16 bits
            ushort high = rng.Next();     // Second RNG call → high 16 bits
            return (uint)((low << 16) | high);
        }


        // Gen 3 LCRNG (BACD_R)
        class LCRNG
        {
            private uint seed;

            public LCRNG(ushort initialSeed)
            {
                seed = (uint)(initialSeed & 0xFFFF);
            }

            public ushort Next()
            {
                seed = (uint)((seed * 0x41C64E6D + 0x00006073) & 0xFFFFFFFF);
                return (ushort)(seed >> 16);
            }
        }
    }
}
