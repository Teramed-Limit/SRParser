# SRParser

一個用於解析 DICOM 結構化報告 (SR) 檔案並轉換為 JSON 格式的 .NET 6.0 類別庫。此類別庫提供了一個簡單且高效的方式來從醫療 DICOM SR 檔案中提取結構化數據。

## 🚀 功能特色

- 將 DICOM 結構化報告檔案解析為階層樹狀結構
- 將解析後的數據轉換為格式化的 JSON
- 處理帶有適當單位和測量值的數值
- 支援中文字符和 Unicode 內容
- 乾淨、模組化的架構，易於整合
- 基於可靠的 FellowOakDicom 函式庫構建

## 📦 安裝方式

### 方式一：安裝本地 NuGet 套件

1. 建立發布套件：
   ```bash
   dotnet build --configuration Release
   dotnet pack --configuration Release
   ```

2. 將套件 .nupkg 新增到你的專案 (與.csproj同一層目錄)：
   ```bash
   dotnet add package SRParser --source .
   ```

### 方式二：DLL 參考

1. 建置專案並從 `bin/Release/net6.0/` 複製 `SRParser.dll`
2. 在你的專案中新增參考：
   ```xml
   <ItemGroup>
     <Reference Include="SRParser">
       <HintPath>/path/to/SRParser.dll</HintPath>
     </Reference>
   </ItemGroup>
   ```

## 📋 系統需求

- .NET 6.0 或更高版本
- FellowOakDicom 套件 (會自動包含)

## 🔧 快速開始

### 基本使用方式

```csharp
using FellowOakDicom;
using SRParser.Service;

// 載入 DICOM SR 檔案
var dicomFile = DicomFile.Open("path/to/your/sr/file.dcm");

// 確認這是結構化報告
if (dicomFile.Dataset.GetValue<string>(DicomTag.Modality, 0) == "SR")
{
    // 建立解析器實例
    var parser = new DicomStructuredReportParser(dicomFile.Dataset);
    
    // 解析結構化報告
    parser.Parse(null, null);
    
    // 轉換為 JSON
    string jsonResult = parser.ToJson();
    
    Console.WriteLine(jsonResult);
    
    // 或儲存到檔案
    File.WriteAllText("output.json", jsonResult);
}
else
{
    Console.WriteLine("檔案不是 DICOM 結構化報告");
}
```

### 進階使用方式 (含錯誤處理)

```csharp
using FellowOakDicom;
using SRParser.Service;

public class SRParserService
{
    public async Task<string?> ParseDicomSRAsync(string filePath)
    {
        try
        {
            // 驗證檔案是否存在
            if (!File.Exists(filePath))
            {
                throw new FileNotFoundException($"找不到 DICOM 檔案: {filePath}");
            }

            // 載入 DICOM 檔案
            var dicomFile = DicomFile.Open(filePath);
            var dataset = dicomFile.Dataset;

            // 檢查模態
            var modality = dataset.GetValue<string>(DicomTag.Modality, 0);
            if (modality != "SR")
            {
                throw new InvalidOperationException($"預期 SR 模態，但得到: {modality}");
            }

            // 解析 SR
            var parser = new DicomStructuredReportParser(dataset);
            parser.Parse(null, null);

            return parser.ToJson();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"解析 DICOM SR 時發生錯誤: {ex.Message}");
            return null;
        }
    }
}
```

## 🏗️ 整合範例

### 控制台應用程式

建立新的控制台應用程式並整合 SRParser：

```bash
# 建立新的控制台專案
dotnet new console -n MyDicomParser
cd MyDicomParser

# 新增 SRParser 參考
dotnet add package SRParser --source /path/to/SRParser/bin/Release/

# 新增 FellowOakDicom (如果尚未包含)
dotnet add package fo-dicom
```

```csharp
// Program.cs
using FellowOakDicom;
using SRParser.Service;

if (args.Length == 0)
{
    Console.WriteLine("用法: MyDicomParser <sr-檔案路徑>");
    return;
}

string filePath = args[0];

try
{
    var dicomFile = DicomFile.Open(filePath);
    
    if (dicomFile.Dataset.GetValue<string>(DicomTag.Modality, 0) == "SR")
    {
        var parser = new DicomStructuredReportParser(dicomFile.Dataset);
        parser.Parse(null, null);
        
        string json = parser.ToJson();
        
        // 儲存到輸出檔案
        string outputPath = Path.ChangeExtension(filePath, ".json");
        await File.WriteAllTextAsync(outputPath, json);
        
        Console.WriteLine($"成功將 SR 轉換為 JSON: {outputPath}");
    }
    else
    {
        Console.WriteLine("錯誤: 檔案不是 DICOM 結構化報告");
    }
}
catch (Exception ex)
{
    Console.WriteLine($"錯誤: {ex.Message}");
}
```

### ASP.NET Core Web API

```csharp
// Controllers/DicomController.cs
using Microsoft.AspNetCore.Mvc;
using FellowOakDicom;
using SRParser.Service;

[ApiController]
[Route("api/[controller]")]
public class DicomController : ControllerBase
{
    [HttpPost("parse-sr")]
    public async Task<IActionResult> ParseStructuredReport(IFormFile file)
    {
        if (file == null || file.Length == 0)
        {
            return BadRequest("沒有上傳檔案");
        }

        try
        {
            using var stream = file.OpenReadStream();
            var dicomFile = await DicomFile.OpenAsync(stream);
            
            if (dicomFile.Dataset.GetValue<string>(DicomTag.Modality, 0) != "SR")
            {
                return BadRequest("檔案不是 DICOM 結構化報告");
            }

            var parser = new DicomStructuredReportParser(dicomFile.Dataset);
            parser.Parse(null, null);
            
            var json = parser.ToJson();
            
            return Ok(new { success = true, data = json });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { success = false, error = ex.Message });
        }
    }
}
```

### Windows Forms 應用程式

```csharp
// MainForm.cs
using FellowOakDicom;
using SRParser.Service;

public partial class MainForm : Form
{
    private async void btnLoadFile_Click(object sender, EventArgs e)
    {
        using var openFileDialog = new OpenFileDialog
        {
            Filter = "DICOM 檔案 (*.dcm)|*.dcm|所有檔案 (*.*)|*.*",
            Title = "選擇 DICOM SR 檔案"
        };

        if (openFileDialog.ShowDialog() == DialogResult.OK)
        {
            await ProcessDicomFile(openFileDialog.FileName);
        }
    }

    private async Task ProcessDicomFile(string filePath)
    {
        try
        {
            txtStatus.Text = "處理中...";
            
            var dicomFile = DicomFile.Open(filePath);
            
            if (dicomFile.Dataset.GetValue<string>(DicomTag.Modality, 0) == "SR")
            {
                var parser = new DicomStructuredReportParser(dicomFile.Dataset);
                parser.Parse(null, null);
                
                var json = parser.ToJson();
                txtResult.Text = json;
                txtStatus.Text = "解析完成";
            }
            else
            {
                txtStatus.Text = "錯誤: 不是 DICOM 結構化報告";
            }
        }
        catch (Exception ex)
        {
            txtStatus.Text = $"錯誤: {ex.Message}";
        }
    }
}
```

## 📚 API 參考

### DicomStructuredReportParser

用於解析 DICOM SR 檔案的主要類別。

#### 建構函式
```csharp
DicomStructuredReportParser(DicomDataset dataset)
```
- **dataset**: 包含結構化報告的 DICOM 資料集

#### 方法

##### Parse
```csharp
public void Parse(TreeNode<SRCodeValue>? parentNode, DicomContentItem? item, int level = 0)
```
遞迴地將 DICOM SR 結構解析為樹狀結構。

##### ToJson
```csharp
public string ToJson()
```
將解析後的樹狀結構轉換為 JSON 格式。

### SRCodeValue

表示結構化報告程式碼值的資料模型。

#### 屬性
- **Code**: SR 程式碼的含義/描述
- **Value**: 實際值
- **ValueWithUnit**: 帶單位格式化的值
- **ValueType**: 值的類型 (例如 "Text", "Numeric")
- **Unit**: 單位符號 (數值型資料)
- **UnitMeaning**: 單位描述 (數值型資料)

### TreeNode&lt;T&gt;

用於階層表示的泛型樹狀資料結構。

#### 屬性
- **Value**: 節點值
- **Children**: 子節點清單

#### 方法
- **AddChild(TreeNode&lt;T&gt; child)**: 新增子節點
- **RemoveChild(TreeNode&lt;T&gt; child)**: 移除子節點
- **HasChild()**: 如果節點有子節點則回傳 true

## 🔍 輸出範例

輸入 DICOM SR 檔案會產生類似以下的 JSON 輸出：

```json
{
  "測量報告_A1B2C3": {
    "Unit": "",
    "UnitMeaning": "",
    "Value": "",
    "ValueWithUnit": "",
    "患者姓名_D4E5F6": {
      "Unit": "",
      "UnitMeaning": "",
      "Value": "張三",
      "ValueWithUnit": "",
      "檢查日期_G7H8I9": {
        "Unit": "",
        "UnitMeaning": "",
        "Value": "20231201",
        "ValueWithUnit": ""
      }
    },
    "發現_J1K2L3": {
      "Unit": "mm",
      "UnitMeaning": "毫米",
      "Value": "12.5",
      "ValueWithUnit": "12.5 mm"
    }
  }
}
```

## 🛠️ 開發

### 從原始碼建置

```bash
# 複製儲存庫
git clone <repository-url>
cd SRParser

# 還原套件
dotnet restore

# 建置類別庫
dotnet build

# 建立 NuGet 套件
dotnet pack --configuration Release

# 執行範例 (如果可用)
cd Examples
dotnet run
```

### 專案結構

```
SRParser/
├── Service/           # DicomStructuredReportParser
├── Model/             # SRCodeValue, TreeNode<T>
├── Converter/         # TreeToJsonConverter
├── Helper/            # PropertyExceptionChecker
├── Examples/          # 使用範例
└── SRParser.csproj    # 專案組態
```

## 🤝 貢獻

1. Fork 儲存庫
2. 建立功能分支
3. 進行你的變更
4. 如適用則新增測試
5. 提交 pull request

## 📄 授權

此專案採用 MIT 授權 - 詳細資訊請參見 LICENSE 檔案。

## ⚠️ 系統需求與相容性

- **.NET 6.0** 或更高版本
- **FellowOakDicom 5.1.1** (自動包含)
- Windows、macOS 或 Linux
- 相容於控制台應用程式、ASP.NET Core、WinForms、WPF、Blazor

## 🐛 疑難排解

### 常見問題

**問題**: "檔案不是 DICOM 結構化報告"
- **解決方案**: 使用 DICOM 檢視器驗證 DICOM 檔案的模態是否為 "SR"

**問題**: 解析時發生異常失敗
- **解決方案**: 檢查 DICOM 檔案是否損壞或驗證 FellowOakDicom 相容性

**問題**: 中文字符顯示不正確
- **解決方案**: 類別庫使用 UnsafeRelaxedJsonEscaping 來正確處理 Unicode

**問題**: JSON 輸出為空
- **解決方案**: 確保在呼叫 `ToJson()` 之前先呼叫 `Parse()` 方法

## 📞 支援

如有問題、疑問或想要貢獻，請在專案儲存庫開啟 issue。