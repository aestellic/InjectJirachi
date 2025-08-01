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
            File.WriteAllBytes("wishmaker_jirachi.ek3", data);

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
            Console.WriteLine("\nSaved as wishmaker_jirachi.ek3");
        }


        public static PK3 CreateWishmakerJirachi(ushort seed)
        {
            // Pulled from EncountersWC3.cs
            var encounter = new EncounterGift3((int)Species.Jirachi, 05, GameVersion.R)
            { 
                Moves = new(273,093,156,000), 
                Method = PIDType.BACD_R,
                ID32 = 20043, 
                Shiny = Shiny.Random, 
                OriginalTrainerName = "WISHMKR", 
                OriginalTrainerGender = GiftGender3.Only0, 
                Language = (int)LanguageID.English 
            };
            
            var trainer = new SimpleTrainerInfo(GameVersion.R)
            {
                OT = "WISHMKR",
                TID16 = 20043,
                SID16 = 0,
                Gender = 0,
                Language = (int)LanguageID.English
            };

            PK3 jirachi = encounter.ConvertToPKM(trainer);

            // Friendship
            jirachi.OriginalTrainerFriendship = 100;

            var rng = new LCRNG(seed);

            // PID
            ushort pidLow = rng.Next();
            ushort pidHigh = rng.Next();
            uint pid = (uint)((pidLow << 16) | pidHigh);

            jirachi.PID = pid;

            // IVs
            ushort ivBits1 = rng.Next(); // First 3 IVs
            ushort ivBits2 = rng.Next(); // Last 3 IVs

            jirachi.IV_HP   =  ivBits1        & 0x1F;
            jirachi.IV_ATK  = (ivBits1 >> 5)  & 0x1F;
            jirachi.IV_DEF  = (ivBits1 >> 10) & 0x1F;
            jirachi.IV_SPE  =  ivBits2        & 0x1F;
            jirachi.IV_SPA  = (ivBits2 >> 5)  & 0x1F;
            jirachi.IV_SPD  = (ivBits2 >> 10) & 0x1F;

            // Held Item: Ganlon (169) or Salac (170)
            jirachi.HeldItem = ((rng.Next() / 3) & 1) == 0 ? 170 : 169;

            // Clean up
            jirachi.ResetPartyStats(); // updates party bytes
            jirachi.RefreshChecksum();
            return jirachi;
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
