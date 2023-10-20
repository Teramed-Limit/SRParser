using System.Text.Encodings.Web;
using System.Text.Json;
using SRParser.Model;

namespace SRParser;

public class TreeToJsonConverter
{
    public static string Convert(TreeNode<SRCodeValue> node)
    {
        var dictionary = new Dictionary<string, object>();
        ConvertNodeToDictionary(node, dictionary, level: 0);
        var options = new JsonSerializerOptions
        {
            Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
            WriteIndented = true,
        };

        return JsonSerializer.Serialize(dictionary, options);
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
            dictionary[$"{child.Value.Code}_{shortGuid}"] = childDictionary;
        }
    }
}