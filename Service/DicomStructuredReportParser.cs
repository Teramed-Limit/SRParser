using FellowOakDicom;
using FellowOakDicom.StructuredReport;
using SRParser.Helper;
using SRParser.Model;

namespace SRParser.Service;

public class DicomStructuredReportParser
{
    private TreeNode<SRCodeValue> RootNode { get; set; }
    private DicomStructuredReport RootDocument { get; set; }

    public DicomStructuredReportParser(DicomDataset dataset)
    {
        // 建立根節點
        RootDocument = new DicomStructuredReport(dataset);
        var srCodeValue = new SRCodeValue { Code = RootDocument.Code.Meaning };
        RootNode = new TreeNode<SRCodeValue>(srCodeValue);
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

    public string ToJson()
    {
        if (!RootNode.HasChild()) throw new Exception("RootNode is null, please parse first.");
        return TreeToJsonConverter.Convert(RootNode);
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