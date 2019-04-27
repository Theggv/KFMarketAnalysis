using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KFMarketAnalysis.Models.Json
{
    public class JSONObject
    {
        private string _json;
        private List<JSONProperty> _values;

        public JSONObject this[string index]
        {
            get
            {
                foreach (var obj in _values)
                {
                    if (obj.Key == index)
                        return obj.Value;
                }

                return null;
            }
        }
        public virtual JSONObject this[int index]
        {
            get
            {
                if (index < 0 || index >= _values.Count)
                    return null;

                return _values[index].Value;
            }
        }

        public JSONArray GetArray(string index)
        {
            foreach (var obj in _values)
            {
                if (obj.Key == index)
                    return obj.Value as JSONArray;
            }

            return null;
        }


        public JSONArray GetArray(int index)
        {
            if (index < 0 || index >= _values.Count)
                return null;

            return _values[index].Value as JSONArray;
        }

        public object GetValue(string index)
        {
            foreach (var obj in _values)
            {
                if (obj.Key == index)
                    return (obj.Value as JSONElement).Value;
            }

            return null;
        }

        public JSONObject()
        {
            _values = new List<JSONProperty>();
        }

        public JSONObject(string json)
        {
            _json = json.Replace("\r", "").Replace("\n", "").Trim();

            _values = new List<JSONProperty>();

            var quotes = new List<Sign>();

            int depth = 0;

            bool isOpenQuote = false;

            for (int i = 0; i < _json.Length; ++i)
            {
                if (isOpenQuote && _json[i - 1] == '\\' && _json[i] == '\"')
                    continue;

                if (_json[i] == '{')
                {
                    if (depth++ <= 2)
                        quotes.Add(new Sign { Char = '{', Pos = i, Depth = depth });
                }
                else if (_json[i] == '}')
                {
                    if (--depth <= 2)
                        quotes.Add(new Sign { Char = '}', Pos = i, Depth = depth });
                }
                else if (_json[i] == '[')
                {
                    if (depth++ <= 2)
                        quotes.Add(new Sign { Char = '[', Pos = i, Depth = depth });
                }
                else if (_json[i] == ']')
                {
                    if (--depth <= 2)
                        quotes.Add(new Sign { Char = ']', Pos = i, Depth = depth });
                }
                else if (_json[i] == '\"')
                {
                    depth = !isOpenQuote ? depth + 1 : depth - 1;

                    isOpenQuote = !isOpenQuote;
                }
                else if (depth == 1)
                {
                    if (_json[i] == ':')
                        quotes.Add(new Sign { Char = ':', Pos = i, Depth = depth });
                    else if (_json[i] == ',')
                        quotes.Add(new Sign { Char = ',', Pos = i, Depth = depth });
                }
            }

            if (quotes.Count == 0)
                return;

            depth = 1;

            FindProperties(quotes, depth);
        }

        private void FindProperties(List<Sign> quotes, int depth)
        {
            if (quotes.Where(s => s.Char == ':' && s.Depth == depth).Count() == 0)
                return;

            for (int i = 0; i < quotes.Count; i++)
            {
                if (quotes[i].Char == ':' && quotes[i].Depth == depth)
                {
                    // property found

                    int startPos = quotes[i - 1].Pos + 1;
                    int endPos;

                    if (quotes[i + 1].Char != ',' &&
                        (quotes[i + 1].Char == '{' || quotes[i + 1].Char == '['))
                        endPos = FindCloseBracket(quotes[i + 1].Pos) + 1;
                    else
                        endPos = quotes[i + 1].Pos;

                    _values.Add(new JSONProperty(_json.Substring(startPos, endPos - startPos)));
                }
            }
        }

        private int FindCloseBracket(int index)
        {
            int depth = 0;

            for (int i = index + 1; i < _json.Length; ++i)
            {
                if (_json[i] == '{' || _json[i] == '[')
                    depth++;
                else if (_json[i] == '}' || _json[i] == ']')
                {
                    depth--;

                    if (depth == -1)
                        return i;
                }
            }

            return -1;
        }

        sealed protected class Sign
        {
            public char Char { get; set; }

            public int Pos { get; set; }

            public int Depth { get; set; }
        }
    }
}
