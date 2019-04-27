using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KFMarketAnalysis.Models.Json
{
    [DebuggerDisplay("{Count}")]
    public class JSONArray : JSONObject
    {
        private string _json;
        private List<JSONObject> _values;

        public int Count => _values.Count;

        public override JSONObject this[int index]
        {
            get
            {
                if (index < 0 || index >= _values.Count)
                    return null;

                return _values[index];
            }
        }

        public JSONArray()
        {
            _values = new List<JSONObject>();
        }

        public JSONArray(string json)
        {
            _json = json.Replace("\r", "").Replace("\n", "").Trim();

            _values = new List<JSONObject>();

            var quotes = new List<Sign>();

            bool isOpenQuote = false;

            int depth = 0;

            for (int i = 0; i < _json.Length; ++i)
            {
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

            FindObjects(quotes, depth);
        }

        private void FindObjects(List<Sign> quotes, int depth)
        {
            for (int i = 1; i < quotes.Count; i++)
            {
                if (quotes[i].Char == '{' && quotes[i].Depth == depth + 1)
                {
                    // object found

                    int startPos = quotes[i].Pos;
                    int endPos = FindCloseBracket(startPos);

                    _values.Add(new JSONObject(_json.Substring(startPos, endPos - startPos + 1)));
                }
                else if (quotes[i].Char == '[' && quotes[i].Depth == depth + 1)
                {
                    // array found

                    int startPos = quotes[i].Pos;
                    int endPos = FindCloseBracket(startPos);

                    _values.Add(new JSONArray(_json.Substring(startPos, endPos - startPos + 1)));
                }
                else if (quotes[i].Char == ',' && quotes[i].Depth == depth &&
                    (quotes[i - 1].Char == ',' || quotes[i - 1].Char == '['))
                {
                    // element found

                    int startPos = quotes[i - 1].Pos + 1;
                    int endPos = quotes[i].Pos - 1;

                    _values.Add(new JSONElement(_json.Substring(startPos, endPos - startPos + 1)));
                }
                else if (i == quotes.Count - 1 &&
                    (quotes[i - 1].Char == ',' || quotes[i - 1].Char == '['))
                {
                    // array with 1 element

                    int startPos = quotes[i - 1].Pos + 1;
                    int endPos = quotes[i].Pos - 1;

                    _values.Add(new JSONElement(_json.Substring(startPos, endPos - startPos + 1)));
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
    }
}
