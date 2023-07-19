using BpeChatAI.Generics;

using Newtonsoft.Json.Linq;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace BpeChatAI.JsonSchema;
internal static class JsonSchemaGenerator
{
    public static JObject Generate(Type type)
    {
        var schema = new JObject();
        var properties = type.GetProperties();
        var propertiesSchema = new JObject();

        foreach (var property in properties)
        {
            var propertySchema = new JObject();

            if (property.PropertyType == typeof(string))
            {
                propertySchema["type"] = "string";
            }
            else if (property.PropertyType == typeof(int))
            {
                propertySchema["type"] = "integer";
            }
            else if (property.PropertyType == typeof(bool))
            {
                propertySchema["type"] = "boolean";
            }
            else if (property.PropertyType == typeof(double) 
                     || property.PropertyType == typeof(float)
                     || property.PropertyType == typeof(decimal))
            {
                propertySchema["type"] = "number";
            }
            else if (property.PropertyType.IsEnum)
            {
                propertySchema["type"] = "string";
                propertySchema["enum"] = new JArray(GetEnumNames(property.PropertyType));
            }
            else if (property.PropertyType.GetGenericMatchFor(typeof(IList<>)) is GenericTypeMatchInformation gtmi)
            {
                propertySchema["type"] = "array";
                propertySchema["items"] = Generate(gtmi[0]);
            }
            else if (property.PropertyType.IsClass)
            {
                propertySchema = Generate(property.PropertyType);
            }

            propertiesSchema[property.Name] = propertySchema;
        }

        schema["type"] = "object";
        schema["properties"] = propertiesSchema;

        return schema;
    }

    private static IEnumerable<string> GetEnumNames(Type enumType)
    {
        var names = new List<string>();
        var fields = enumType.GetFields(BindingFlags.Static | BindingFlags.Public);

        foreach (var field in fields)
        {
            var attribute = field.GetCustomAttribute<EnumMemberAttribute>();
            if (!string.IsNullOrWhiteSpace(attribute?.Value))
                names.Add(attribute.Value);
            else
                names.Add(field.Name);
        } 

        return names;
    }

}