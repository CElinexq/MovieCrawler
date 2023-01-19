using System.Globalization;
using System.Text.RegularExpressions;

//用正则表达式取出时间和文件大小，将逗号替换成#@#，将原来有逗号的csv文件转换成#@#的csv文件
string filePath = @"C:\Users\cocox\Desktop\BookCrawler\悦享云盘-Books 2023-01-16 13#00#30综合.csv";
string[] lines = File.ReadAllLines(filePath);
List<string> outputFile = new List<string>();

foreach (var line in lines)
{
    string patternDateTime = @"\d{1,2}/\d{1,2}/\d{4} \d{1,2}:\d{1,2}:\d{1,2} [ap]m";
    string patternSize = @"(\d+\.\d{1,2})\s(KB|MB|GB)";

    Match matchDateTime = Regex.Match(line, patternDateTime);
    string changeDateTime = line.Replace($",{matchDateTime},", $"#@#{matchDateTime}#@#");

    Match matchSize = Regex.Match(line, patternSize);
    string output = changeDateTime.Replace($"{matchSize},", $"{matchSize}#@#");
    outputFile.Add(output);
    Console.WriteLine(output);
}
File.AppendAllLines(@"C:\Users\cocox\Desktop\BookCrawler\悦享云盘-Books 2023-01-16 13#00#30综合01.csv", outputFile);