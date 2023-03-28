using System.Linq;
using TaleWorlds.CampaignSystem;

namespace MatrilinealMarriage
{
    public class MarriageModel
    {
        public Hero PlayerRelative = null;
        public Hero OtherClanMember = null;
        public Clan OtherClan = null;

        public MarriageModel(Hero playerRelative, Hero otherClanMember, Clan otherClan)
        {
            PlayerRelative = playerRelative;
            OtherClanMember = otherClanMember;
            OtherClan = otherClan;
        }
        public MarriageModel(string text)
        {
            var fields = text.Split(';');
            for (int i = 0; i < fields.Length; i++)
            {
                var elem = fields[i].Split(':');
                if (elem[0] == "PlayerRelative")
                    PlayerRelative = Hero.AllAliveHeroes.Where(x => x.StringId == elem[1]).FirstOrDefault();
                if (elem[0] == "OtherClanMember")
                    OtherClanMember = Hero.AllAliveHeroes.Where(x => x.StringId == elem[1]).FirstOrDefault();
                if (elem[0] == "OtherClan")
                    OtherClan = Clan.All.Where(x => x.StringId == elem[1]).FirstOrDefault();
            }
        }

        public override string ToString()
        {
            return "PlayerRelative:" + PlayerRelative.StringId + ";OtherClanMember:" + OtherClanMember.StringId + ";OtherClan:" + OtherClan.StringId;
        }

    }
}
