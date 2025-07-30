# InjectJirachi
Injects a legal randomized Wishmaker Jirachi into any mainline Generation III Save File. Intended for shiny hunting a legal Wishmaker Jirachi using custom distribution hardware (see implementation [here](https://github.com/aestellic/WishmakerDistributionMachine)). 

## Usage
`InjectJirachi <input.sav> <output.sav> [--fill-party]`

## Notes
 - You must have an open slot in your party; the program will exit if you do not.
 - The seed used to generate the Jirachi is randomized, so standard Wishmaker RNG manipulation will not work.
  - All randomized seeds should be legal
 - Unlimited Jirachi's can be received per save.
 - `--fill-party` will fill all open slots in the party with Jirachi's.
