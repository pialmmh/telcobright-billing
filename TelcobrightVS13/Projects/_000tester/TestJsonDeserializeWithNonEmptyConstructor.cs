using System.Collections.Generic;
using Newtonsoft.Json;

namespace Utils
{
    class Cat
    {
        public Dictionary<string, string> Attributes { get; set; }
        public Cat() { }
        private bool _attributeSet = false;
        [JsonConstructor]
        public Cat(Dictionary<string, string> attributes)
        {
            this.Attributes = attributes;
            _attributeSet = true;
        }
    }

    public class JsonDeserializeTestWithNonEmptyConstructor
    {
        public void Test()
        {
            var cat = new Cat();
            Dictionary<string, string> attributes = new Dictionary<string, string>()
            {
                {"color", "white"},
                {"size", "small"}
            };
            cat.Attributes = attributes;
            string serializedCat = JsonConvert.SerializeObject(cat);
            var anotherCat = JsonConvert.DeserializeObject<Cat>(serializedCat);
        }
    }
}
