using System.Text.Encodings.Web;
using System.Text.Json;
using System.Xml;
using SRParser.Model;

namespace SRParser.Converter;

public class TreeToJsonConverter
{
    public static string Convert2Json(TreeNode<SRCodeValue> node)
    {
        var dictionary = new Dictionary<string, object>();
        ConvertNodeToDictionary(node, dictionary, level: 0);
        var options = new JsonSerializerOptions
        {
            WriteIndented = true,
            Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping
        };
        return JsonSerializer.Serialize(dictionary, options);
    }

    public static Dictionary<string, object> Convert2Dict(TreeNode<SRCodeValue> node)
    {
        var dictionary = new Dictionary<string, object>();
        ConvertNodeToDictionary(node, dictionary, level: 0);
        return dictionary;
    }

    private static void ConvertNodeToDictionary(TreeNode<SRCodeValue> node, Dictionary<string, object> dictionary,
        int level)
    {
        if (level != 0)
        {
            dictionary["Unit"] = node.Value.Unit;
            dictionary["UnitMeaning"] = node.Value.UnitMeaning;
            dictionary["Value"] = node.Value.Value;
            dictionary["ValueWithUnit"] = node.Value.ValueWithUnit;
        }

        foreach (var child in node.Children)
        {
            var childDictionary = new Dictionary<string, object>();
            ConvertNodeToDictionary(child, childDictionary, level + 1);

            Guid guid = Guid.NewGuid();
            string shortGuid = guid.GetHashCode().ToString("X");
            dictionary[$"{child.Value.Code.Replace(" ", "")}_{shortGuid}"] = childDictionary;
        }
    }
}