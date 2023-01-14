using HtmlAgilityPack;
using System;

string rootUrl = @"https://pity.eu.org/SP2/doc/%E6%BC%AB%E7%94%BB/250%E9%83%A8%E7%83%AD%E6%92%AD%E6%BC%AB%E7%94%BB/";
var httpClient=new HttpClient();
List<string> folderUrls = await GetFolderUrl(rootUrl);
foreach (string item in folderUrls)
{
    Console.WriteLine("The Last Folder:" + item);
}
Console.WriteLine("结束");

///获取最后一个文件夹路径，即文件路径
async Task<List<string>> GetFolderUrl(string url)
{
    List<string> links = new List<string>();
    string html=await httpClient.GetStringAsync(url);
    //解析html文档
    var doc = new HtmlDocument();
    doc.LoadHtml(html);
    
    var nodes = doc.DocumentNode.SelectNodes("//*[@name='folderlist']");
    //查找href
    if(nodes!= null)
    {
        foreach (var node in nodes)
        {
            var href = node.Attributes["href"].Value;
            var newLink=Path.Combine(url, href);
            links.Add(newLink);
            //如果到达最后一个文件则返回null
            List<string> subLink = await GetFolderUrl(newLink);
                        
            if (subLink!=null)
            {
                links.AddRange(subLink);
                //foreach (var item in links)
                //{
                //    Console.WriteLine("Find Folder:" + item);
                //}
                string html1 = await httpClient.GetStringAsync(newLink);
                //解析html文档
                var doc1 = new HtmlDocument();
                doc1.LoadHtml(html1);
                if (doc1.DocumentNode.SelectNodes("//*[@name='filelist']")!=null)
                {
                    links.Add(newLink);
                }
            }
            else
            {
                links.Add(newLink);
            }
        }
        return links;
    }
    else
    {
        return null;
    }
}



