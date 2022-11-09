using System;
using System.Collections.Generic;
using System.Linq;

namespace WarLight.AI.LaconicEntity
{
    public class LMain : IWarLightAI {

        GameStanding DistributionStanding;
        GameStanding LatestTurnStanding;
        PlayerIDType MyPlayerID;
        Dictionary<PlayerIDType, GamePlayer> Players;
        MapDetails Map;
        GameSettings Settings;
        Dictionary<PlayerIDType, TeammateOrders> TeammatesOrders;
        List<CardInstance> Cards;
        int CardsMustPlay;
        Dictionary<PlayerIDType, PlayerIncome> Incomes;

        public void Init(PlayerIDType myPlayerID, Dictionary<PlayerIDType, GamePlayer> players, MapDetails map, GameStanding distributionStanding, GameSettings gameSettings, int numberOfTurns, Dictionary<PlayerIDType, PlayerIncome> incomes, GameOrder[] prevTurn, GameStanding latestTurnStanding, GameStanding previousTurnStanding, Dictionary<PlayerIDType, TeammateOrders> teammatesOrders, List<CardInstance> cards, int cardsMustPlay) {
            this.DistributionStanding = distributionStanding;
            this.LatestTurnStanding = latestTurnStanding;
            this.MyPlayerID = myPlayerID;
            this.Players = players;
            this.Map = map;
            this.Settings = gameSettings;
            this.TeammatesOrders = teammatesOrders;
            this.Cards = cards;
            this.CardsMustPlay = cardsMustPlay;
            this.Incomes = incomes;
        }

        public List<TerritoryIDType> GetPicks()
        {
            return GameAI.MakePicks(Players, DistributionStanding, Settings, Map, Players[MyPlayerID].ScenarioID).ToList();
        }

        public List<GameOrder> GetOrders() {
            List<GameOrder> orders = new List<GameOrder>();

            PlayerIncome myIncome = new PlayerIncome();
            if (!Incomes.TryGetValue(MyPlayerID, out myIncome)) {
                Console.WriteLine("Failed to get my income");
            }
            Console.WriteLine("My income is: " + myIncome.FreeArmies);
       
            List<Edge> edges = new List<Edge>();
            List<TerritoryIDType> myTerritories = new List<TerritoryIDType>();
            List<TerritoryIDType> frontiers = new List<TerritoryIDType>();

            // fill in both new Lists with all the territory IDs
            foreach(TerritoryIDType i in LatestTurnStanding.Territories.Keys) {
                TerritoryStanding standing = getStanding(i);
                TerritoryDetails details = getDetails(i);

                if (standing.OwnerPlayerID == MyPlayerID) {
                    
                    // add the territory to list of my territories
                    myTerritories.Add(i);

                    // see if territory has neutral/enemy borders and add them to frontiers
                    foreach(TerritoryIDType j in details.ConnectedTo) {
                        TerritoryStanding standing2 = getStanding(j);

                        if (standing2.OwnerPlayerID != MyPlayerID) {
                            if (!frontiers.Contains(j)) {
                                // insertFrontierSorted(j, frontiers);
                                // edges.Add(new Edge(i, j));
                            }
                            Armies a = new Armies(myIncome.FreeArmies);
                            orders.Add(GameOrderDeploy.Create(myIncome.FreeArmies, MyPlayerID, i));
                            orders.Add(GameOrderAttackTransfer.Create(MyPlayerID, i, j, AttackTransferEnum.Attack, false, a, false));
                            return orders;
                        }
                    }
                }
            }



            return orders;
        }

        TerritoryDetails getDetails(TerritoryIDType id) {
            TerritoryDetails details = null;
            if (!Map.Territories.TryGetValue(id, out details)) {
                Console.WriteLine("getBorders failed to find TerritoryDetail based on id");
            }
            return details;
        }

        TerritoryStanding getStanding(TerritoryIDType id) {
            TerritoryStanding standing = null;
            if (!LatestTurnStanding.Territories.TryGetValue(id, out standing)) {
                Console.WriteLine("getOwner failed to find TerritoryStanding based on id");
            }
            return standing;
        }

        void insertFrontierSorted(TerritoryIDType id, List<TerritoryIDType> list) {
     
            TerritoryStanding standing = getStanding(id);
            int armies = standing.NumArmies.ArmiesOrZero, comparedArmies = 0;
            int position = 0;

            if (list.Count == 0) {
                list.Insert(0, id);
                return;
            }

            for (; position < list.Count; position++) {
                comparedArmies = getStanding(list[position]).NumArmies.ArmiesOrZero;
                if (comparedArmies > armies) {
                    list.Insert(position, id);
                    return;
                }
            }

            list.Insert(position + 1, id);

        }
    }
}