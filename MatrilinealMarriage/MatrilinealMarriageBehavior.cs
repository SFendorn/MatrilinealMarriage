using Helpers;
using System;
using System.Collections.Generic;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.Actions;
using TaleWorlds.CampaignSystem.Party;
#if DEBUG
using TaleWorlds.Library;
#endif

namespace MatrilinealMarriage
{
    public class MatrilinealMarriageBehavior : CampaignBehaviorBase
    {
        private readonly List<MarriageModel> marriageRequests = new List<MarriageModel>();

        public override void RegisterEvents()
        {
            CampaignEvents.HeroesMarried.AddNonSerializedListener(this, new Action<Hero, Hero, bool>(OnMarriage));
            CampaignEvents.RomanticStateChanged.AddNonSerializedListener(this, new Action<Hero, Hero, Romance.RomanceLevelEnum>(OnRomanticStateChange));
            CampaignEvents.OnMarriageOfferedToPlayerEvent.AddNonSerializedListener(this, new Action<Hero, Hero>(OnMarriageOffered));
            CampaignEvents.OnMarriageOfferCanceledEvent.AddNonSerializedListener(this, new Action<Hero, Hero>(OnMarriageCanceled));
        }

        public override void SyncData(IDataStore dataStore)
        {
            if (dataStore.IsSaving)
            {
                List<string> stringList = new List<string> { };
                foreach (MarriageModel marriageModel in marriageRequests)
                {
                    stringList.Add(marriageModel.ToString());
                }
                dataStore.SyncData("MatrilinealMarriageRequests", ref stringList);
            }
            if (dataStore.IsLoading)
            {
                List<string> stringList = new List<string> { };
                dataStore.SyncData("MatrilinealMarriageRequests", ref stringList);
                foreach (string str in stringList)
                {
                    marriageRequests.Add(new MarriageModel(str));
                }
            }
        }

        public void OnMarriage(Hero hero1, Hero hero2, bool showNotification)
        {
            var matchRequests = marriageRequests.Find(x => (x.PlayerRelative == hero1 && x.OtherClanMember == hero2) || (x.PlayerRelative == hero2 && x.OtherClanMember == hero1));
            if (matchRequests == null)
                return;

            if (matchRequests.PlayerRelative.IsFemale)
            {
#if DEBUG
                InformationManager.DisplayMessage(new InformationMessage("Apply matrilineal marriage between " + hero1.ToString() + " (" + hero1.Clan.ToString() + ") and " + hero2.ToString() + " (" + hero2.Clan.ToString() + ")."));
#endif
                ApplyMarriageAction(hero1, Hero.MainHero.Clan);
                ApplyMarriageAction(hero2, Hero.MainHero.Clan);
                RemoveRequest(hero1, hero2);
            }
            else
            {
#if DEBUG
                InformationManager.DisplayMessage(new InformationMessage("Apply matrilineal marriage between " + hero1.ToString() + " (" + hero1.Clan.ToString() + ") and " + hero2.ToString() + " (" + hero2.Clan.ToString() + ")."));
#endif
                ApplyMarriageAction(hero1, matchRequests.OtherClan);
                ApplyMarriageAction(hero2, matchRequests.OtherClan);
                RemoveRequest(hero1, hero2);
            }
        }

        public void OnRomanticStateChange(Hero hero1, Hero hero2, Romance.RomanceLevelEnum romanceLevel)
        {
            if (romanceLevel == Romance.RomanceLevelEnum.MatchMadeByFamily && CanMarryMatrilineally(hero1, hero2))
            {
                if (hero1.Clan == Hero.MainHero.Clan)
                    marriageRequests.Add(new MarriageModel(hero1, hero2, hero2.Clan));
                if (hero2.Clan == Hero.MainHero.Clan)
                    marriageRequests.Add(new MarriageModel(hero2, hero1, hero1.Clan));
            }
#if DEBUG
            InformationManager.DisplayMessage(new InformationMessage("Romantic Level changed between " + hero1.ToString() + " (" + hero1.Clan.ToString() + ") and " + hero2.ToString() + " (" + hero2.Clan.ToString() + ") to " + romanceLevel.ToString()));
#endif
        }

        public void OnMarriageOffered(Hero hero1, Hero hero2)
        {
            if (CanMarryMatrilineally(hero1, hero2))
            {
                if (hero1.Clan == Hero.MainHero.Clan)
                    marriageRequests.Add(new MarriageModel(hero1, hero2, hero2.Clan));
                if (hero2.Clan == Hero.MainHero.Clan)
                    marriageRequests.Add(new MarriageModel(hero2, hero1, hero1.Clan));
            }
        }

        public void OnMarriageCanceled(Hero hero1, Hero hero2)
        {
            RemoveRequest(hero1, hero2);
        }

        private void RemoveRequest(Hero hero1, Hero hero2)
        {
            marriageRequests.RemoveAll(x => (x.PlayerRelative == hero1 && x.OtherClanMember == hero2) || (x.PlayerRelative == hero2 && x.OtherClanMember == hero1));
        }

        private static bool CanMarryMatrilineally(Hero hero1, Hero hero2)
        {
            return (hero1.Clan == Hero.MainHero.Clan || hero2.Clan == Hero.MainHero.Clan) && hero1.Clan != hero2.Clan && !hero1.IsFactionLeader && !hero2.IsFactionLeader && !hero1.IsHumanPlayerCharacter && !hero2.IsHumanPlayerCharacter;
        }

        // taken from MarriageAction.ApplyInternal
        private static void ApplyMarriageAction(Hero hero, Clan targetClan)
        {
            if (hero.Clan != targetClan)
            {
                Clan otherClan = hero.Clan;
                if (hero.GovernorOf != null)
                {
                    ChangeGovernorAction.RemoveGovernorOf(hero);
                }

                if (hero.PartyBelongedTo != null)
                {
                    MobileParty partyBelongedTo = hero.PartyBelongedTo;
                    if (otherClan.Kingdom != targetClan.Kingdom)
                    {
                        if (hero.PartyBelongedTo.Army != null)
                        {
                            if (hero.PartyBelongedTo.Army.LeaderParty == hero.PartyBelongedTo)
                            {
                                DisbandArmyAction.ApplyByUnknownReason(hero.PartyBelongedTo.Army);
                            }
                            else
                            {
                                hero.PartyBelongedTo.Army = null;
                            }
                        }

                        IFaction kingdom = targetClan.Kingdom;
                        FactionHelper.FinishAllRelatedHostileActionsOfNobleToFaction(hero, kingdom ?? targetClan);
                    }

                    if (partyBelongedTo.Party.IsActive && partyBelongedTo.Party.Owner == hero)
                    {
                        DisbandPartyAction.StartDisband(partyBelongedTo);
                        partyBelongedTo.Party.SetCustomOwner(null);
                    }

                    hero.ChangeState(Hero.CharacterStates.Fugitive);
                    hero.PartyBelongedTo?.MemberRoster.RemoveTroop(hero.CharacterObject);
                }

                hero.Clan = targetClan;
                foreach (Hero hero2 in otherClan.Heroes)
                {
                    hero2.UpdateHomeSettlement();
                }

                foreach (Hero hero2 in targetClan.Heroes)
                {
                    hero2.UpdateHomeSettlement();
                }
            }
        }
    }
}
