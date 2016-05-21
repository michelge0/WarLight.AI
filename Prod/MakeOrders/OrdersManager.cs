﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WarLight.Shared.AI.Prod.MakeOrders
{
    public class OrdersManager
    {
        public List<GameOrder> Orders = new List<GameOrder>();
        private BotMain Bot;

        public OrdersManager(BotMain bot)
        {
            this.Bot = bot;
        }

        /// <summary>
        /// Inserts order based on phase
        /// </summary>
        /// <param name="order"></param>
        public void AddOrder(GameOrder orderToAdd)
        {
            for (int i = 0; i < Orders.Count; i++)
            {
                if ((int)orderToAdd.OccursInPhase.Value < (int)Orders[i].OccursInPhase.Value)
                {
                    Orders.Insert(i, orderToAdd);
                    return;
                }
            }

            Orders.Add(orderToAdd);
        }


        public void Deploy(TerritoryIDType terr, int armies)
        {
            if (!TryDeploy(terr, armies))
                throw new Exception("Deploy failed.  Territory=" + terr + ", armies=" + armies + ", us=" + Bot.PlayerID + ", Income=" + Bot.EffectiveIncome.ToString() + ", IncomeTrakcer=" + Bot.MakeOrders.IncomeTracker.ToString());
        }

        public bool TryDeploy(TerritoryIDType terrID, int armies)
        {
            if (Bot.AvoidTerritories.Contains(terrID))
                return false;

            Assert.Fatal(Bot.Standing.Territories[terrID].OwnerPlayerID == Bot.PlayerID);

            if (armies == 0)
                return true; //just pretend like we did it
            Assert.Fatal(armies > 0);

            if (!Bot.MakeOrders.IncomeTracker.TryRecordUsedArmies(terrID, armies))
                return false;

            IEnumerable<GameOrderDeploy> deploys = Orders.OfType<GameOrderDeploy>();
            GameOrderDeploy existing = deploys.FirstOrDefault(o => o.DeployOn == terrID);

            if (existing != null)
                existing.NumArmies += armies;
            else
                AddOrder(GameOrderDeploy.Create(armies, Bot.PlayerID, terrID));

            return true;
        }

        public void AddAttack(TerritoryIDType from, TerritoryIDType to, AttackTransferEnum attackTransfer, int numArmies, bool attackTeammates, bool byPercent = false, bool bosses = false, bool commanders = false)
        {
            Assert.Fatal(Bot.Map.Territories[from].ConnectedTo.ContainsKey(to), from + " does not connect to " + to);

            var actualArmies = numArmies;
            var actualByPercent = byPercent;

            if (actualByPercent && Bot.Settings.AllowPercentageAttacks == false)
            {
                actualByPercent = false;
                actualArmies = 1000000;
            }

            var actualMode = attackTransfer;
            if (actualMode == AttackTransferEnum.Transfer && Bot.Settings.AllowTransferOnly == false)
                actualMode = AttackTransferEnum.AttackTransfer;
            if (actualMode == AttackTransferEnum.Attack && Bot.Settings.AllowAttackOnly == false)
                actualMode = AttackTransferEnum.AttackTransfer;

            IEnumerable<GameOrderAttackTransfer> attacks = Orders.OfType<GameOrderAttackTransfer>();
            var existingFrom = attacks.Where(o => o.From == from).ToList();
            var existingFromTo = existingFrom.Where(o => o.To == to).ToList();

            if (existingFromTo.Any())
            {
                var existing = existingFromTo.Single();
                existing.ByPercent = actualByPercent;
                existing.AttackTransfer = actualMode;
                existing.NumArmies = existing.NumArmies.Add(new Armies(actualArmies));

                if (actualByPercent && existing.NumArmies.NumArmies > 100)
                    existing.NumArmies = existing.NumArmies.Subtract(new Armies(existing.NumArmies.NumArmies - 100));
            }
            else
            {
                var specials = Bot.Standing.Territories[from].NumArmies.SpecialUnits;
                if (specials.Length > 0)
                {
                    var used = existingFrom.SelectMany(o => o.NumArmies.SpecialUnits).Select(o => o.ID).ToHashSet(false);
                    specials = specials.Where(o => used.Contains(o.ID) == false).ToArray();

                    if (bosses == false)
                        specials = specials.Where(o => o.IsBoss() == false).ToArray();

                    if (commanders == false)
                        specials = specials.Where(o => !(o is Commander)).ToArray();
                }

                AddOrder(GameOrderAttackTransfer.Create(Bot.PlayerID, from, to, actualMode, actualByPercent, new Armies(actualArmies, false, specials), attackTeammates));
            }
        }
    }
}
