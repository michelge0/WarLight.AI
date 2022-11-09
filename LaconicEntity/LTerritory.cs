using System;

namespace WarLight.AI.LaconicEntity {
    public class LTerritory {
        public TerritoryStanding standing;
        public TerritoryDetails details;
        public TerritoryIDType ID;

        // looks up the territory ID in a GameStanding and MapDetails object to initialize values
        public LTerritory(TerritoryIDType ID, GameStanding gameStanding, MapDetails mapDetails) {
            this.ID = ID;
            
            if (gameStanding.Territories.TryGetValue(ID, out standing) && mapDetails.Territories.TryGetValue(ID, out details)) {}
            else {
                Console.WriteLine("Error initializing LTerritory");
            }
        }
    }
}