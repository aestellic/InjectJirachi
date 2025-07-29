using PKHeX.Core;
using System;
using System.IO;
using GenJirachi;

namespace InjectJirachi 
{
    class Program
    {
        static int InjectJirachiToSave(SAV3 sav3, Random rng)
        {
            PK3 pk;
            ushort randomSeed = (ushort)rng.Next(0x10000);
            Console.WriteLine($"Seed: 0x{randomSeed:X4}");

            try
            {
                pk = GenJirachi.Program.CreateWishmakerJirachi(randomSeed);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Failed to parse generated Jirachi .pk3: {ex.Message}");
                return 2;
            }

            var party = sav3.PartyData;

            Console.WriteLine($"partySize: {party.Count}");

            int maxPartySize = 5;
            int openSlot = -1;

            if (party.Count <= maxPartySize)
            {
                openSlot = party.Count;
            }
            else
            {
                Console.WriteLine("No open party slot found.");
                return 1;
            }

            sav3.SetPartySlotAtIndex(pk, openSlot);
            Console.WriteLine($"Injected into party slot {openSlot + 1}.");
            return 0;
        }

        static int Main(string[] args)
        {
            if (args.Length < 2)
            {
                Console.WriteLine("Usage: InjectJirachi <input.sav> <output.sav> [--fill-party]");
                return 2;
            }

            string inputSavePath = args[0];
            string outputSavePath = args[1];
            bool fillParty = Array.Exists(args, arg => arg == "--fill-party");

            byte[] savData = File.ReadAllBytes(inputSavePath);
            var sav = SaveUtil.GetVariantSAV(savData);
            if (sav is not SAV3 sav3)
            {
                Console.WriteLine("Not a Gen 3 save file.");
                return 2;
            }

            Console.WriteLine($"Game: {sav.Version}");

            var rng = new Random();
            int result;
            do
            {
                result = InjectJirachiToSave(sav3, rng);
            }
            while (fillParty && result == 0);

            byte[] newSave = sav3.Write();
            File.WriteAllBytes($"{outputSavePath}", newSave);
            Console.WriteLine($"Saved as {outputSavePath}");

            return result;
        }
    }
}