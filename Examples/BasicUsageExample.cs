using FellowOakDicom;
using SRParser.Service;

namespace SRParser.Examples
{
    /// <summary>
    /// Basic example of how to use SRParser library to parse DICOM SR files
    /// </summary>
    public class BasicUsageExample
    {
        public static void RunExample()
        {
            // Example file path - replace with your actual DICOM SR file path
            string dicomFilePath = @"path\to\your\dicom\sr\file.dcm";
            
            try
            {
                // Read the DICOM file
                var dicomFile = DicomFile.Open(dicomFilePath);

                // Check if it's a Structured Report (SR)
                if (dicomFile.Dataset.GetValue<string>(DicomTag.Modality, 0) != "SR") 
                {
                    Console.WriteLine("The file is not a Structured Report (SR)");
                    return;
                }

                // Create parser and parse the SR
                var parser = new DicomStructuredReportParser(dicomFile.Dataset);
                parser.Parse(null, null);

                // Convert to JSON and output
                string jsonResult = parser.ToJson();
                Console.WriteLine(jsonResult);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error parsing DICOM SR file: {ex.Message}");
            }
        }
    }
}