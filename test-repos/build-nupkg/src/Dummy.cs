
using System.Reflection;

/// <summary>適当なクラス</summary>
public class Dummy
{
    /// <summary>適当なテキストを取得する</summary>
    /// <returns>適当なテキスト</returns>
    public string GetText()
    {
        var asmVer = Assembly.GetExecutingAssembly().GetName().Version;
        return $"Dummy Text v{asmVer}";
    }
}