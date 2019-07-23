using BBGo.ExcelLENT;
using BBGo.ExcelLENT.Generator;
using BBGo.ExcelLENT.Serializer;
using System;
using UnityEngine;
using UnityEditor;
using System.IO;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using NPOI.SS.UserModel;
using System.Linq;

public class Excel2CSWindow :Editor
{
    private static Dictionary<string, ISerializer> mDic_form_seria = new Dictionary<string, ISerializer>()
    {
        { ".json",new TupledJsonSerializer()}
    };


    private const string m_DataForm = ".json";
    private const string m_CsConfigFilePath = "Scripts/Configs";
    private const string m_DataFilePath = "Resources/Json";
    private const string m_ExcelPath = "Excel";
    private const string m_Namespace = "Config";

    //todo 针对单个表生成
    //private string m_ExcelName = "";

    [MenuItem("Tools/Excel2CS/Creat All CS")]
    static void CreatAllCS()
    {
        Parser parser = new Parser();

        if (!Directory.Exists(Application.dataPath + "/" + m_CsConfigFilePath))
        {
            Directory.CreateDirectory(Application.dataPath + "/" + m_CsConfigFilePath);
        }
        var param = new ParseParam()
        {
            ExcelDir = Application.dataPath + "/" + m_ExcelPath,
            Serializations = new SerializationParam[]
            {
                    new SerializationParam()
                    {
                        Serializer =  mDic_form_seria[m_DataForm],
                        OutDir = Application.dataPath+"/"+m_DataFilePath,
                    },
            },
        };
        param.Generations = new GenerationParam[]
            {
                         new GenerationParam()
                    {
                        Generator = new TupledCSharpGenerator(),
                        OutDir = Application.dataPath+"/"+m_CsConfigFilePath,
                        DataDir = Application.dataPath+"/"+m_DataFilePath,
                        Package = m_Namespace,
                        From = m_DataForm,
                    },
            };

        // 生成初始化类
        var sheets = GetAllExcelName(param.ExcelDir);
        UpdateConfigInitiator(sheets, param.Generations[0]);
        // 生成配置类
        parser.ParseAll(param);

        param = null;
        parser = null;
        AssetDatabase.Refresh();
    }

    [MenuItem("Tools/Excel2CS/Only Creat All Data")]
    static void OnlyCreatData()
    {

        if (!Directory.Exists(Application.dataPath + "/" + m_CsConfigFilePath))
        {
            Directory.CreateDirectory(Application.dataPath + "/" + m_CsConfigFilePath);
        }
        var param = new ParseParam()
        {
            ExcelDir = Application.dataPath + "/" + m_ExcelPath,
            Serializations = new SerializationParam[]
           {
                    new SerializationParam()
                    {
                        Serializer =  mDic_form_seria[m_DataForm],
                        OutDir = Application.dataPath+"/"+m_DataFilePath,
                    },
           },
        };
        Parser parser = new Parser();
        parser.ParseAll(param);

        AssetDatabase.Refresh();
    }



    private static void UpdateConfigInitiator(List<string> sheetNames, GenerationParam param)
    {
        CodeBuilder builder = new CodeBuilder();
        if (!string.IsNullOrEmpty(param.Package))
        {
            builder.AppendLine($"namespace {param.Package}")
                   .AppendLine("{").AddIndent();
        }
        {
            builder.AppendLine("public static class ConfigInitiator")
                   .AppendLine("{").AddIndent();
            {
                builder.AppendLine("public static void InitAllConfig()")
                       .AppendLine("{").AddIndent();
                {
                    for (int i = 0; i < sheetNames.Count; i++)
                    {
                        builder.AppendLine($"{sheetNames[i]}.Init();");
                    }
                }
                builder.SubtractIndent().AppendLine("}");
            }
            builder.SubtractIndent().AppendLine("}");
        }
        if (!string.IsNullOrEmpty(param.Package))
        {
            builder.SubtractIndent().AppendLine("}");
        }
        Utility.SaveToFile(builder.ToString(), $"{param.OutDir}/ConfigInitiator.cs");
    }

    private static List<string> GetAllExcelName(string path)
    {
        List<string> sheetNames = new List<string>();
        var allConfigs = Directory.GetFiles(path, "*.*", SearchOption.AllDirectories).Where(s => !string.IsNullOrEmpty(Path.GetExtension(s)) && Parser.SUPPORTED_EXTENSIONS.Contains(Path.GetExtension(s).ToLower()) && !s.Contains("~"));
        foreach (var configPath in allConfigs)
        {
            string workbookName = Path.GetFileNameWithoutExtension(configPath);
            using (FileStream fs = new FileStream(configPath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
            {
                IWorkbook workbook = WorkbookFactory.Create(fs);
                for (int sheetIndex = 0; sheetIndex < workbook.NumberOfSheets; sheetIndex++)
                {
                    ISheet sheet = workbook.GetSheetAt(sheetIndex);
                    if (sheet.PhysicalNumberOfRows == 0)
                        continue;

                    for (int rowIndex = sheet.FirstRowNum; rowIndex <= sheet.LastRowNum;)
                    {
                        IRow row = sheet.GetRow(rowIndex++);
                        if (row == null)
                            continue;

                        ICell cell = row.GetCell(0);

                        //寻找以[Config]为标记的首行
                        if (cell == null ||
                            cell.CellType != CellType.String)
                            continue;

                        if (cell.GetStringCellValue().Trim() == "[ExcelLENT]")
                        {
                            var name = row.GetCell(1, MissingCellPolicy.CREATE_NULL_AS_BLANK).GetStringCellValue();
                            sheetNames.Add(name);
                            break;
                        }
                    }
                }
            }
        }
        return sheetNames;
    }

    private class UnityLogger : BBGo.ExcelLENT.ILogger
    {
        public void Log(object msg)
        {
            Debug.Log(msg);
        }

        public void LogError(object msg)
        {
            Debug.LogError(msg);
        }

        public void LogWarning(object msg)
        {
            Debug.LogWarning(msg);
        }
    }
}





