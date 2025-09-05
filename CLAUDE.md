# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Project Overview

SRParser is a .NET 6.0 class library that provides functionality to parse DICOM Structured Report (SR) files and convert them to JSON format. It uses the FellowOakDicom library to read and parse DICOM SR files containing medical report data.

## Development Commands

### Build and Package
```bash
# Build the library
dotnet build

# Build for release
dotnet build --configuration Release

# Create NuGet package
dotnet pack

# Create NuGet package for release
dotnet pack --configuration Release
```

### Development Tools
```bash
# Clean build artifacts
dotnet clean

# Restore NuGet packages
dotnet restore

# Run any tests (if added in the future)
dotnet test
```

### Using the Library

#### Install via NuGet (when published)
```bash
dotnet add package SRParser
```

#### Install via Local Package
```bash
dotnet add package SRParser --source /path/to/SRParser/bin/Debug/
```

#### Basic Usage
```csharp
using FellowOakDicom;
using SRParser.Service;

// Load DICOM SR file
var dicomFile = DicomFile.Open("path/to/your/sr/file.dcm");

// Check if it's a Structured Report
if (dicomFile.Dataset.GetValue<string>(DicomTag.Modality, 0) == "SR")
{
    // Parse the SR
    var parser = new DicomStructuredReportParser(dicomFile.Dataset);
    parser.Parse(null, null);
    
    // Convert to JSON
    string jsonResult = parser.ToJson();
    Console.WriteLine(jsonResult);
}
```

## Architecture

The library follows a layered architecture with clear separation of concerns:

### Core Components

- **DicomStructuredReportParser** (Service): Main parser service that recursively traverses DICOM SR tree structure
- **SRCodeValue** (Model): Data model representing structured report code values with units and measurements  
- **TreeNode&lt;T&gt;** (Model): Generic tree data structure for hierarchical SR data representation
- **TreeToJsonConverter** (Converter): Converts the parsed tree structure to JSON with proper Unicode encoding
- **PropertyExceptionChecker** (Helper): Reflection utility for safe property access on DICOM objects
- **BasicUsageExample** (Examples): Sample code demonstrating library usage

### Library Usage Flow
1. Consumer loads DICOM file and validates it's SR modality
2. Create DicomStructuredReportParser with DICOM dataset
3. Call Parse() to recursively build TreeNode hierarchy from SR structure
4. Call ToJson() to convert tree to properly formatted JSON
5. Consumer receives JSON string for further processing

### Key Dependencies
- **FellowOakDicom (v5.1.1)**: DICOM file handling and SR parsing
- **.NET 6.0**: Target framework with nullable reference types enabled

## Project Structure

```
SRParser/
├── Service/           # Core parsing logic (DicomStructuredReportParser)
├── Model/             # Data models (SRCodeValue, TreeNode)
├── Converter/         # JSON conversion utilities (TreeToJsonConverter)
├── Helper/            # Reflection and utility helpers (PropertyExceptionChecker)
├── Examples/          # Usage examples and sample code
└── SRParser.csproj    # Project configuration with NuGet package settings
```

## API Reference

### Primary Classes

#### DicomStructuredReportParser
- **Constructor**: `DicomStructuredReportParser(DicomDataset dataset)`
- **Methods**: 
  - `Parse(TreeNode<SRCodeValue>? parentNode, DicomContentItem? item, int level = 0)`
  - `ToJson()` returns JSON string representation

#### SRCodeValue
- **Properties**: Code, Value, ValueWithUnit, ValueType, Unit, UnitMeaning

#### TreeNode&lt;T&gt;
- **Methods**: `AddChild()`, `RemoveChild()`, `HasChild()`
- **Properties**: Value, Children

## Development Notes

- Library is configured for Chinese character handling with UnsafeRelaxedJsonEscaping
- Error handling includes graceful exception handling with try-catch blocks
- Parser handles numeric values with units and measurements specially
- Tree nodes are identified with unique GUIDs in JSON output to prevent key conflicts
- NuGet package is automatically generated on build with proper metadata