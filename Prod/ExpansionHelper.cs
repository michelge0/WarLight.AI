﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WarLight.Shared.AI.Prod
{
    static class ExpansionHelper
    {
        public const float BaseBonusWeight = 1000f;

        public static float WeighBonus(BotMain bot, BonusIDType bonusID, BonusPath path, Func<TerritoryStanding, bool> weOwn, CaptureTerritories turnsToTakeOpt)
        {
            var bonus = bot.Map.Bonuses[bonusID];
            int bonusValue = bot.BonusValue(bonusID);

            if (bonusValue <= 0)
                throw new Exception("Considered zero or negative bonuses"); //we should not even be considering zero or negative bonuses, ensure they're filtered out before we get here.

            var weight = BaseBonusWeight;

            //When randomness is enabled, modify the bonus by a fixed amount for this bonus.
            weight += bot.BonusFuzz(bonusID);

            weight += bonusValue * (bot.IsFFA ? 7 : 4);

            //Subtract value for each additional turn it takes to take over one
            if (turnsToTakeOpt == null)
                weight = 0;
            else
                weight -= bonusValue * (turnsToTakeOpt.NumTurns - 1);

            weight -= bonus.Territories.Count * bot.Settings.OneArmyMustStandGuardOneOrZero;

            float armyMult = (float)bot.Settings.DefenseKillRate + 0.8f;

            //How many territories do we need to take to get it? Subtract one weight for each army standing in our way
            foreach (var terrInBonus in bonus.Territories)
            {
                var ts = bot.Standing.Territories[terrInBonus];

                if (weOwn(ts))
                    continue; //Already own it
                else if (ts.OwnerPlayerID == TerritoryStanding.FogPlayerID)
                    weight -= GuessNumberOfArmies(bot, ts.ID).DefensePower * armyMult;
                else if (bot.IsTeammate(ts.OwnerPlayerID))
                    weight -= bot.Players[ts.OwnerPlayerID].IsAIOrHumanTurnedIntoAI ? 0 : ts.NumArmies.DefensePower * 4 * armyMult; //Human teammate in it. We'll defer to them since humans know best.
                else if (ts.IsNeutral)
                    weight -= ts.NumArmies.DefensePower * armyMult; //Neutral in it
                else if (ts.OwnerPlayerID == TerritoryStanding.AvailableForDistribution)
                    weight -= Math.Max(bot.Settings.InitialNeutralsInDistribution, bot.Settings.InitialPlayerArmiesPerTerritory) * armyMult; //assume another player could start there
                else
                    weight -= ts.NumArmies.DefensePower * 3 * armyMult; //Opponent in it - expansion less likely
            }

            return weight;

        }

        public static Armies GuessNumberOfArmies(BotMain bot, TerritoryIDType terrID)
        {
            Assert.Fatal(bot.Standing.Territories[terrID].NumArmies.Fogged, "Not fog");
            if (bot.DistributionStandingOpt != null)
            {
                var dist = bot.DistributionStandingOpt.Territories[terrID];
                if (dist.IsNeutral)
                    return dist.NumArmies;
                Assert.Fatal(dist.OwnerPlayerID == TerritoryStanding.AvailableForDistribution);

                return new Armies(Math.Max(bot.Settings.InitialNeutralsInDistribution, bot.Settings.InitialPlayerArmiesPerTerritory));
            }

            return new Armies(bot.Settings.InitialNonDistributionArmies);
        }

        /// <summary>
        /// The biggest bonus weight applies at 100%, and all other positive bonus weights get added in at a reduced weight. The idea is that the main bonus gets full weight, and super bonuses just help out a little.
        /// Negative super bonus bonus weights will never reduce the weight of the main bonus
        /// </summary>
        /// <param name="weights"></param>
        /// <returns></returns>
        public static float WeighMultipleBonuses(Dictionary<BonusIDType, float> weights)
        {
            if (weights.Count == 0)
                return 0;

            var maxID = weights.First().Key;
            foreach (var pair in weights)
                if (pair.Value > weights[maxID])
                    maxID = pair.Key;

            var ret = 0f;
            foreach(var pair in weights)
            {
                if (pair.Key == maxID)
                    ret += pair.Value;
                else if (pair.Value > 0)
                    ret += pair.Value / 10f;
            }

            return ret;
        }
    }
}
