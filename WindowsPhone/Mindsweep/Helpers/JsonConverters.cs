using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Data.Linq;

namespace Mindsweep.Helpers
{

    public class ObjectToArrayConverter<T> : CustomCreationConverter<List<T>> where T : new()
    {
        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            List<T> target = new List<T>();

            try
            {
                // Load JObject from stream
                JArray jArray = JArray.Load(reader);

                // Populate the object properties
                serializer.Populate(jArray.CreateReader(), target);
            }
            catch (JsonReaderException)
            {
                // Handle case when object is not an array...

                // Load JObject from stream
                JObject jObject = JObject.Load(reader);

                // Create target object based on JObject
                T t = new T();

                // Populate the object properties
                serializer.Populate(jObject.CreateReader(), t);

                target.Add(t);
            }

            return target;
        }

        public override List<T> Create(Type objectType)
        {
            return new List<T>();
        }
    }

    public class ObjectToEntitySetConverter<T> : CustomCreationConverter<EntitySet<T>> where T : class, new()
    {
        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            EntitySet<T> target = new EntitySet<T>();

            try
            {
                // Load JObject from stream
                JArray jArray = JArray.Load(reader);

                // Populate the object properties
                serializer.Populate(jArray.CreateReader(), target);
            }
            catch (JsonReaderException)
            {
                // Handle case when object is not an array...

                // Load JObject from stream
                JObject jObject = JObject.Load(reader);

                // Create target object based on JObject
                T t = new T();

                // Populate the object properties
                serializer.Populate(jObject.CreateReader(), t);

                target.Add(t);
            }

            return target;
        }

        public override EntitySet<T> Create(Type objectType)
        {
            return new EntitySet<T>();
        }
    }

    public class BitStringToBoolConverter : CustomCreationConverter<bool>
    {
        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            bool target = false;

            // Load JObject from stream
            JToken jValue = JValue.Load(reader);

            if (jValue.Value<string>() == "1")
                target = true;

            return target;
        }

        public override bool Create(Type objectType)
        {
            return false;
        }
    }

    public class TagsToStringConverter : CustomCreationConverter<string>
    {
        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            try
            {
                // Load JObject from stream
                JArray jArray = JArray.Load(reader);

                if (jArray.Count > 0 && jArray.First.First != null)
                    return string.Join(",", jArray.First.First.ToObject<List<string>>().ToArray());
            }
            catch (JsonReaderException exc)
            {
                // Load JObject from stream
                JObject jObject = JObject.Load(reader);

                var tags = jObject.First.First;

                if (tags is JArray)
                    return string.Join(",", tags.ToObject<List<string>>().ToArray());
                else
                    return tags.Value<string>();
            }

            return null;
        }

        public override string Create(Type objectType)
        {
            return null;
        }
    }
}
