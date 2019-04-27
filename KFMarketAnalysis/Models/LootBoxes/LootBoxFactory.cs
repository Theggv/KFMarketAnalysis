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

            if (s.Contains("Crate"))
                return new LootBoxCrate(s);

            if (s.Contains("USB"))
                return new LootBoxUSB(s);

            return null;
        }
    }
}
