using FellowOakDicom;
using FellowOakDicom.StructuredReport;
using SRParser.Converter;
using SRParser.Helper;
using SRParser.Model;

namespace SRParser.Service;

public class DicomStructuredReportParser
{
    public TreeNode<SRCodeValue> RootNode { get; set; }
    private DicomStructuredReport RootDocument { get; set; }

    public DicomStructuredReportParser(DicomDataset dataset)
    {
        // 建立根節點
        RootDocument = new DicomStructuredReport(dataset);
        var srCodeValue = new SRCodeValue { Code = RootDocument.Code.Meaning };
        RootNode = new TreeNode<SRCodeValue>(srCodeValue);
    }

    private void WriteToString(DicomContentItem item, int level)
    {
        string valueOrDefault = item.Dataset.GetValueOrDefault<string>(DicomTag.RelationshipType, 0, string.Empty);
        string str1 = string.IsNullOrEmpty(valueOrDefault) ? string.Empty : valueOrDefault + " ";
        string str2 = !(item.Code != null)
            ? str1 + string.Format("{0} {1}", (object)"(no code provided)",
                (object)item.Dataset.GetValueOrDefault<string>(DicomTag.ValueType, 0, "UNKNOWN"))
            : str1 + string.Format("{0} {1}", (object)item.Code.ToString(),
                (object)item.Dataset.GetSingleValueOrDefault<string>(DicomTag.ValueType, "UNKNOWN"));

        try
        {
            str2 += string.Format(" [{0}]", (object)item.Get<string>());
        }
        catch
        {
        }

        Console.WriteLine($"{new string(' ', (level + 1) * 2)} {str2}");
    }


    public void Parse(TreeNode<SRCodeValue>? parentNode, DicomContentItem? item, int level = 0)
    {
        try
        {
            parentNode ??= RootNode;
            item ??= RootDocument;

            foreach (var child in item.Children())
            {
                // 檢查Code是否存在在child中
                if (!PropertyExceptionChecker.TryGetPropertyValue(child, "Code", out object none))
                    return;

                var childNode = AddSRCodeValue(parentNode, child);
                // WriteToString(child, level);
                Parse(childNode, child, level + 1);
            }
        }
        catch (Exception e)
        {
            Console.BackgroundColor = ConsoleColor.Red;
            Console.WriteLine(e);
            Console.BackgroundColor = ConsoleColor.Black;
        }
    }

    public Dictionary<string, object> ToDictionary()
    {
        if (!RootNode.HasChild()) throw new Exception("RootNode is null, please parse first.");
        return TreeToJsonConverter.Convert2Dict(RootNode);
    }

    public string ToJson()
    {
        if (!RootNode.HasChild()) throw new Exception("RootNode is null, please parse first.");
        return TreeToJsonConverter.Convert2Json(RootNode);
    }

    private TreeNode<SRCodeValue> AddSRCodeValue(TreeNode<SRCodeValue> parentNode, DicomContentItem item)
    {
        var srCodeValue = new SRCodeValue();
        var childNode = new TreeNode<SRCodeValue>(srCodeValue);

        try
        {
            parentNode.AddChild(childNode);
            srCodeValue.Code = item.Code.Meaning;
            srCodeValue.ValueType = item.Type.ToString();
            if (srCodeValue.ValueType == "Numeric")
            {
                var measuredValue = item.Dataset.GetMeasuredValue(DicomTag.MeasuredValueSequence);
                srCodeValue.UnitMeaning = measuredValue.Code.Meaning;
                srCodeValue.Unit = measuredValue.Code.Value;
                srCodeValue.Value = measuredValue.Value.ToString();
                srCodeValue.ValueWithUnit = measuredValue.ToString();
            }
            else
            {
                srCodeValue.Value = item.Get<string>();
            }
        }
        catch (Exception e)
        {
        }

        return childNode;
    }
}