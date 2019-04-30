using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using KFMarketAnalysis.Models.Interfaces;

namespace KFMarketAnalysis.Models.LootBoxes
{
    public static class LootBoxFactory
    {
        public static ILootBox GetLootBox(string s)
        {
            if (s.Contains("D.A.R."))
                return new LootBoxDAR(s);

            if (s.Contains("Deity"))
                return new LootBoxDeity(s);

            if (s.Contains("Cyber Samurai"))
                return new LootBoxCyberSamurai(s);

            if (s.Contains("Emote"))
                return new LootBoxEmote(s);

            if (s.Contains("Crate"))
                return new LootBoxCrate(s);

            if (s.Contains("USB"))
                return new LootBoxUSB(s);

            return null;
        }

        public static ILootBox RestoreLootBox(LootBoxBase lootBox)
        {
            if (lootBox.Name.Contains("D.A.R."))
                return new LootBoxDAR(lootBox);

            if (lootBox.Name.Contains("Deity"))
                return new LootBoxDeity(lootBox);

            if (lootBox.Name.Contains("Cyber Samurai"))
                return new LootBoxCyberSamurai(lootBox);

            if (lootBox.Name.Contains("Emote"))
                return new LootBoxEmote(lootBox);

            if (lootBox.Name.Contains("Crate"))
                return new LootBoxCrate(lootBox);

            if (lootBox.Name.Contains("USB"))
                return new LootBoxUSB(lootBox);

            return null;
        }
    }
}
