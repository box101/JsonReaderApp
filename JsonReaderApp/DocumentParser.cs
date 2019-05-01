namespace JsonReaderApp 
{
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Text.RegularExpressions;

    using Newtonsoft.Json.Linq;

    /// <summary>
    /// Json-document parser
    /// </summary>
    public static class DocumentParser
    {
        /// <summary> Получить коллекцию значений "value" с расчётным значением локации значения и пути в json-документе </summary>
        /// <param name="json">json document</param>
        /// <returns>Список кортежей (Значение, Локация, JPath)</returns>
        public static List<(string Value, string Location, string JPath)> GetDocumentValuesWithLocations(string json)
        {
            return GetValues(json).Select(value => (value.ToString().Trim(), CalcLocationFromJPath(value.Path), value.Path)).ToList();
        }

        /// <summary>
        /// Расчёт местоположение величины "value" в структуре json-документа
        /// </summary>
        /// <param name="path">JPath-путь</param>
        private static string CalcLocationFromJPath(string path)
        {
            var stringBuilder = new StringBuilder();
            var matches       = Regex.Matches(path, @"node(\[(?<idx>\d+)\])+").ToArray();

            var mIdx = 0;
            foreach (var m in matches)
            {
                ++mIdx;

                var captures = m.Groups["idx"].Captures.ToArray();

                if (mIdx == 1 && captures.Length > 0)
                {
                    stringBuilder.Append("P" + captures[0].Value);
                    continue;
                }

                var cIdx = 0;
                foreach (var c in captures)
                {
                    ++cIdx;
                    stringBuilder.Append("-");
                    stringBuilder.Append(mIdx == matches.Length && cIdx == captures.Length ? "P" : "D");
                    stringBuilder.Append(c.Value);
                }
            }

            return stringBuilder.ToString();
        }


        /// <summary>
        /// Получить список объектоа со свойством "value" после разбивки каждого сложного объекта на множество простых <seealso cref="SplitValues"/>
        /// </summary>
        /// <param name="inputJson">json-документ</param>
        /// <returns>массив JToken содержащих значение "value" и путь в дереве json-документа, из которого будет расчитано свойство Location</returns>
        private static IEnumerable<JToken> GetValues(string inputJson)
        {
            var jToken            = JToken.Parse(inputJson);
            var unseparatedValues = jToken.SelectTokens("$..value");
            SplitValues(unseparatedValues);
            return jToken.SelectTokens("$..value");
        }

        /// <summary>
        /// Объект со свойством "value" может содержать значение со списком элементов
        /// Например объект со значением [ ... { "value": " TOF(AI_RAW_TMP.LTMROFF,?,?) TON(AI_RAW_TMP.LTMRON,?,?)"} ... ]
        /// эквивалентен тому, как если бы в массиве элементов было бы два объекта: [ ... { "value": " TOF(AI_RAW_TMP.LTMROFF,?,?)"}, { "value": " TON(AI_RAW_TMP.LTMRON,?,?)"	} ... ]
        /// - Разбиваем такие объекты на множество простых.
        /// </summary>
        private static void SplitValues(IEnumerable<JToken> values)
        {
            foreach (var jToken in values.ToArray())
            {
                var value = jToken.Value<string>();

                var matches = Regex.Matches(value, @"\w+\(.*?\)").Reverse().ToArray();
                if (matches.Length <= 1) continue;

                var jObjectWithValue = jToken.Parent.Parent;
                    
                foreach (var m in matches)
                {
                    var newToken = JObject.FromObject(new { value = m.Value });
                    jObjectWithValue.AddAfterSelf(newToken);
                }

                jObjectWithValue.Remove();
            }
        }
    }
}