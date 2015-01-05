using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EME.Infrastructure.Extensions
{
    public static class JsonSerializer
    {
        /// <summary>
        /// Serializes object to JSON string.
        /// </summary>
        /// <param name="objectToSerialize">Object to serialize.</param>
        /// <returns>String of JSON</returns>
        public static string To(object objectToSerialize)
        {
            return JsonConvert.SerializeObject(objectToSerialize);
        }

        /// <summary>
        /// Deserializes JSON string to object
        /// </summary>
        /// <typeparam name="T">Type of object</typeparam>
        /// <param name="jsonString">String of JSON.</param>
        /// <returns>Deserialized object</returns>
        public static T From<T>(string jsonString)
        {
            return JsonConvert.DeserializeObject<T>(jsonString);
        }

        /// <summary>
        /// Serializes object to JSON string.
        /// </summary>
        /// <param name="objectToSerialize">Object to serialize</param>
        /// <returns>String of JSON</returns>
        public static string ToJSON(this object objectToSerialize)
        {
            return JsonSerializer.To(objectToSerialize);
        }

        /// <summary>
        /// Deserializes JSON string to object
        /// </summary>
        /// <typeparam name="T">Type of object</typeparam>
        /// <param name="xmlString">String of JSON</param>
        /// <returns>Deserialized object</returns>
        public static T FromJSON<T>(this string jsonString)
        {
            return JsonSerializer.From<T>(jsonString);
        }

        public static string ToJSONMessage(this object objectToSerialize)
        {
            return "[" + objectToSerialize.GetType().Name + "]:" + 
                JsonSerializer.To(objectToSerialize);
        }
    }
}
