namespace JsonReaderApp
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.IO;

    using Newtonsoft.Json;
    using Newtonsoft.Json.Linq;

    /// <summary> Конревой элемент json-документа </summary>
    public class Root
    {
        public long Id { get; set; }
        public NodeCollection Nodes { get; set; }
    }
    
    /// <summary> Коллекция разнотипных элементов. Элементом коллекции может быть сама коллекция такого же типа <see cref="NodeCollection"/>,
    /// объект со свойством "node" <see cref="Node"/> и объект, со свойством "value" <see cref="NodeValue"/>.
    /// </summary>
    public class NodeCollection: List<ICollectionItem>, ICollectionItem { }

    /// <summary> Элемент коллекции - общий знаменатель для элементов коллекции NodeCollection </summary>
    public interface ICollectionItem { }

    /// <summary> Объект в json-документе, содержащий свойство "node" - коллекцию разнотипных элементов <see cref="NodeCollection"/> </summary>
    public class Node: ICollectionItem
    {
        [JsonProperty("node")]
        public NodeCollection Nodes { get; set; }
    }
    
    /// <summary> Объект в json-документе со строковым свойством "value" </summary>
    public class NodeValue : ICollectionItem
    {
        [JsonProperty("value")] 
        public string Value { get; set; }
    }
    
    /// <summary> Конвертер сериализатора для типа ICollectionItem </summary>
    public class CollectionItemConverter : JsonConverter<ICollectionItem>
    {
        /// <inheritdoc/>
        public override ICollectionItem ReadJson(JsonReader reader, Type objectType, ICollectionItem existingValue, bool hasExistingValue, JsonSerializer serializer)
        {
            var jToken = JToken.Load(reader);
            ICollectionItem target;
            if (jToken is JArray) target = new NodeCollection();
            else if (jToken["node"] != null) target = new Node();
            else if (jToken["value"] != null) target = new NodeValue();
            else
                throw new Exception("Неизвестный тип объекта " + jToken);

            serializer.Populate(jToken.CreateReader(), target);
            return target;
        }

        /// <inheritdoc/>
        public override void WriteJson(JsonWriter writer, ICollectionItem value, JsonSerializer serializer) => serializer.Serialize(writer, value);
    }
    
    class Program
    {
        static void Main()
        {
            var inputJson = File.ReadAllText("input.json");
            var root = JsonConvert.DeserializeObject<Root>(inputJson, new CollectionItemConverter());

            var outputJson = JsonConvert.SerializeObject(root, Formatting.None);
            var minimizedInputJson = JToken.ReadFrom(new JsonTextReader(new StringReader(inputJson))).ToString(Formatting.None);

            Console.WriteLine(outputJson);
            Debug.Assert(minimizedInputJson == outputJson);
        }
    }
}