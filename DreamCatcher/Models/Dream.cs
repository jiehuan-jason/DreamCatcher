using SQLite;

namespace DreamCatcher.Models;
public class Dream
{
    [PrimaryKey, AutoIncrement]
    public int Id { get; set; }
    public int DreamType { get; set; }
    public DateTime Time { get; set; }
    public string Tag { get; set; } = "";
    public string DreamText { get; set; } = "";
    public byte[] ImageData { get; set; } // 存储图片的二进制数据
    public bool IsImageGenerated { get; set; } = false;// 图片是否生成的标识
    public string AIDreamTags { get; set; } = "";
    public string Analyse { get; set; } = "";
    public bool IsAnalyseGenerated { get; set; } = false;
}
