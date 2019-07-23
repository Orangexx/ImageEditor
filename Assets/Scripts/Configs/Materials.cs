namespace Config
{
    public partial class Materials
    {
        public static System.Collections.Generic.List<Row> s_rows {  private set; get; }
        public static void Init()
        {
            string data = System.IO.File.ReadAllText("C:/Unity Workspace/Projects/ImageEditor/Assets/Resources/Json//Materials.json");
            s_rows = Newtonsoft.Json.JsonConvert.DeserializeObject<System.Collections.Generic.List<Row>>(data);
        }

        public class Row
        {
            /// <summary>
            /// id
            /// </summary>
            public int id { get; set; }
            /// <summary>
            /// 材质名称
            /// </summary>
            public string Name { get; set; }
            /// <summary>
            /// 滑条属性名称
            /// </summary>
            public System.Collections.Generic.List<string> SliderProperties { get; set; }
            /// <summary>
            /// 勾选属性
            /// </summary>
            public System.Collections.Generic.List<string> ToggleProperties { get; set; }
        }

    }
}
