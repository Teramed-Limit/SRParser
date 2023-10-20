using FellowOakDicom;
using SRParser.Service;

// Read the DICOM file
// var dicomFile = DicomFile.Open(@"\\teramednas\DicomImages\未分類\SR\婦產科\新惠生\I000005581.dcm");
var dicomFile = DicomFile.Open(@"\\teramednas\DicomImages\未分類\SR\婦產科\君蔚生殖醫學中心\E000000001\S000000001\I000000001.dcm");

// 確認是否為結構化報告(SR)
if (dicomFile.Dataset.GetValue<string>(DicomTag.Modality, 0) != "SR") return;

var parser = new DicomStructuredReportParser(dicomFile.Dataset);
parser.Parse(null, null);
// 轉換為 JSON 並輸出
Console.WriteLine(parser.ToJson());