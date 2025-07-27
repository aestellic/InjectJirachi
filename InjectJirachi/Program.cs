using PKHeX.Core;
using System;
using System.IO;
using GenJirachi;

namespace InjectJirachi 
{
    class Program
    {
        static void Main(string[] args)
        {
            if (args.Length < 2)
            {
                Console.WriteLine("Usage: InjectJirachi <input.sav> <output.sav>");
                return;
            }

            string inputSavePath = args[0];
            string outputSavePath = args[1];

            byte[] savData = File.ReadAllBytes(inputSavePath);

            var sav = SaveUtil.GetVariantSAV(savData);
            if (sav is not SAV3 sav3)
            {
                Console.WriteLine("Not a Gen 3 save file.");
                return;
            }

            Console.WriteLine($"Game: {sav.Version}");

            PK3 pk;
            ushort randomSeed = (ushort)new Random().Next(0x10000);
            Console.WriteLine($"Seed: 0x{randomSeed:X4}");
            try
            {
                pk = GenJirachi.Program.CreateWishmakerJirachi(randomSeed);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Failed to parse generated Jirachi .pk3: {ex.Message}");
                return;
            }

            var party = sav3.PartyData;

            Console.WriteLine($"partySize: {party.Count}");

            int maxPartySize = 6;
            int openSlot = -1;

            if (party.Count < maxPartySize)
            {
                openSlot = party.Count;  // next empty slot is at count index
            }
            else
            {
                Console.WriteLine("No open party slot found.");
                return;
            }

            sav.SetPartySlotAtIndex(pk, openSlot);


            byte[] newSave = sav3.Write();
            File.WriteAllBytes($"{outputSavePath}", newSave);

            Console.WriteLine($"Injected into party slot {openSlot}. Saved as {outputSavePath}");
        }
    }
}