using BBGo.ExcelLENT.Generator;
using BBGo.ExcelLENT.Serializer;
using NPOI.SS.UserModel;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace BBGo.ExcelLENT
{
    public class Parser
    {
        public static readonly List<string> SUPPORTED_EXTENSIONS = new List<string>() { ".xls", ".xlsx" };

        public void Parse(ParseParam param)
        {
            if (!Directory.Exists(param.ExcelDir))
            {
                param.Logger?.LogError($"Excel Dir Not Found:`{param.ExcelDir}`");
                return;
            }

            List<ExcelSheet> sheets = GetExcelsOnlyTop(param);
            foreach (var sheet in sheets)
            {
                sheet.ParseFields();
            }

            foreach (var sheet in sheets)
            {
                sheet.ReadPrimaryFields();
            }

            for (int i = 0; param.Serializations != null && i < param.Serializations.Length; i++)
            {
                foreach (var sheet in sheets)
                {
                    sheet.Serialize(param.Serializations[i]);
                }
            }

            for (int i = 0; param.Generations != null && i < param.Generations.Length; i++)
            {
                foreach (var sheet in sheets)
                {
                    sheet.Generate(param.Generations[i]);
                }
                param.Generations[i].Generator.OnPostGenerate(sheets, param.Generations[i]);
            }
        }

        public void ParseAll(ParseParam param)
        {
            if (!Directory.Exists(param.ExcelDir))
            {
                param.Logger?.LogError($"Excel Dir Not Found:`{param.ExcelDir}`");
                return;
            }

            List<string> lst_original_sePath = new List<string>();
            List<string> lst_original_geOutPath = new List<string>();
            List<string> lst_original_geDataPath = new List<string>();

            if (param.Generations != null)
                foreach (var ge in param.Generations)
                {
                    lst_original_geDataPath.Add(ge.DataDir);
                    lst_original_geOutPath.Add(ge.OutDir);
                }

            if (param.Serializations != null)
                foreach (var se in param.Serializations)
                {
                    lst_original_sePath.Add(se.OutDir);
                }

            List<string> lst_child_dic = new List<string>()
            {""};
            List<string> lst_child_path = new List<string>()
            {param.ExcelDir};
            Utility.GetAllDirList(param.ExcelDir.Length, param.ExcelDir, ref lst_child_path, ref lst_child_dic);

            for (int i = 0; i < lst_child_path.Count; i++)
            {
                param.ExcelDir = lst_child_path[i];

                if (param.Generations != null)
                    for (int j = 0; j < param.Generations.Length; j++)
                    {
                        param.Generations[j].DataDir = lst_original_geDataPath[j] + "/" + lst_child_dic[i];
                        param.Generations[j].OutDir = lst_original_geOutPath[j] + "/" + lst_child_dic[i];
                    }
                if (param.Serializations != null)
                    for (int j = 0; j < param.Serializations.Length; j++)
                    {
                        param.Serializations[j].OutDir = lst_original_sePath[j] + "/" + lst_child_dic[i];
                    }

                List<ExcelSheet> sheets = GetExcelsOnlyTop(param);

                foreach (var sheet in sheets)
                {
                    sheet.ParseFields();
                }

                foreach (var sheet in sheets)
                {
                    sheet.ReadPrimaryFields();
                }

                for (int j = 0; param.Serializations != null && j < param.Serializations.Length; j++)
                {
                    foreach (var sheet in sheets)
                    {
                        sheet.Serialize(param.Serializations[j]);
                    }
                }

                for (int j = 0; param.Generations != null && j < param.Generations.Length; j++)
                {
                    foreach (var sheet in sheets)
                    {
                        sheet.Generate(param.Generations[j]);
                    }
                    param.Generations[j].Generator.OnPostGenerate(sheets, param.Generations[j]);
                }
            }
        }
        private List<ExcelSheet> GetExcelsOnlyTop(ParseParam param)
        {
            List<ExcelSheet> sheets = new List<ExcelSheet>();
            var allConfigs = Directory.GetFiles(param.ExcelDir, "*.*", SearchOption.TopDirectoryOnly).Where(s => !string.IsNullOrEmpty(Path.GetExtension(s)) && SUPPORTED_EXTENSIONS.Contains(Path.GetExtension(s).ToLower()) && !s.Contains("~"));
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
                                ExcelSheet excelSheet = new ExcelSheet()
                                {
                                    Workbook = workbook,
                                    Sheet = sheet,
                                    ClassName = row.GetCell(1, MissingCellPolicy.CREATE_NULL_AS_BLANK).GetStringCellValue(),
                                    m_primaryKeyRow = sheet.GetRow(rowIndex++),
                                    CustomTypeRow = sheet.GetRow(rowIndex++),
                                    FieldTypeRow = sheet.GetRow(rowIndex++),
                                    FieldNameRow = sheet.GetRow(rowIndex++),
                                    FieldDescriptionRow = sheet.GetRow(rowIndex++),
                                    ContentBeginRowNum = rowIndex,
                                    ContentEndRowNum = sheet.LastRowNum,
                                };
                                sheets.Add(excelSheet);
                                excelSheet.Close();
                                break;
                            }
                        }
                    }
                }
            }
            return sheets;
        }
    }

    public class ParseParam
    {
        public string ExcelDir { get; set; }
        public ILogger Logger { get; set; }
        public SerializationParam[] Serializations { get; set; }
        public GenerationParam[] Generations { get; set; }
    }

    public class SerializationParam
    {
        public ISerializer Serializer { get; set; }
        public string OutDir { get; set; }
    }

    public class GenerationParam
    {
        public IGenerator Generator { get; set; }
        public string OutDir { get; set; }
        public string DataDir { get; set; }
        public string From { get; set; } = ".json";
        public string Package { get; set; } = "Config";
    }
}
