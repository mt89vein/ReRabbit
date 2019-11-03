using Newtonsoft.Json;
using System;
using System.IO;

namespace ReRabbit.Core
{
    public class DefaultJsonSerializer : StringSerializerBase
    {
        private readonly JsonSerializer _json;
        public override string ContentType { get;}

        public DefaultJsonSerializer(JsonSerializer json)
        {
            _json = json;
            ContentType = "application/json";
        }

        public override string SerializeToString(object obj)
        {
            if (obj == null)
            {
                return string.Empty;
            }
            if (obj is string str)
            {
                return str;
            }
            string serialized;
            using (var sw = new StringWriter())
            {
                _json.Serialize(sw, obj);
                serialized = sw.GetStringBuilder().ToString();
            }
            return serialized;
        }

        public override object Deserialize(Type type, string str)
        {
            if (type == typeof(string))
            {
                return str;
            }
            object obj;
            using (var jsonReader = new JsonTextReader(new StringReader(str)))
            {
                obj = _json.Deserialize(jsonReader, type);
            }
            return obj;
        }
    }
}
