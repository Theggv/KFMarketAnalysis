using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;

namespace KFMarketAnalysis.Models
{
    [JsonObject]
    public class Description
    {
        public Color Color { get; set; }

        public string Text { get; set; }

        public Description(string text)
        {
            Text = text;
        }

        public Description WithColor(string code)
        {
            byte r, g, b;

            r = System.Drawing.ColorTranslator.FromHtml(code).R;
            g = System.Drawing.ColorTranslator.FromHtml(code).G;
            b = System.Drawing.ColorTranslator.FromHtml(code).B;

            Color = Color.FromRgb(r, g, b);

            return this;
        }
    }
}
