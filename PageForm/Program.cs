
using HtmlAgilityPack;
using System;
using System.Xml.Linq;


string rootUrl = @"https://pity.eu.org/SP2/doc/%E6%BC%AB%E7%94%BB/250%E9%83%A8%E7%83%AD%E6%92%AD%E6%BC%AB%E7%94%BB/";
var httpClient = new HttpClient();
List<string> folderUrls = await GetFolderUrl(rootUrl);

foreach (string item in folderUrls)
{
    Console.WriteLine("The Last Folder:" + item);
}
Console.WriteLine("结束");




///获取第一页 最后一个文件夹路径，即文件路径
async Task<List<string>> GetFolderUrl(string url)
{
    List<string> links = new List<string>();
    string html = await httpClient.GetStringAsync(url);
    //解析html文档
    var doc = new HtmlDocument();
    doc.LoadHtml(html);

    //判断是否存在分页
    var pageNode = doc.DocumentNode.SelectSingleNode("//*[@id='nextpageform']");
    //如果存在分页，找到总页数
    if (pageNode!=null)
    {
        //通过td获取总页数pageNumAll：第二个td；只获取td的子元素节点（排除文本节点）
        var tdNode = pageNode.Descendants("td").ToArray()[1];
        var pageNumAll = tdNode.ChildNodes.Where(x => x.NodeType == HtmlNodeType.Element).Count();
        //获取第2,3页的html
        for (int i = 2 ; i <= pageNumAll; i++)
        {
           
            //根据js发送的post请求，获取第i页的html
            List<KeyValuePair<string, string>> keyValuePairs = new List<KeyValuePair<string, string>>();
            keyValuePairs.Add(new KeyValuePair<string, string>("pagenum", i.ToString()));
            FormUrlEncodedContent httpcontent = new FormUrlEncodedContent(keyValuePairs);
            var response = await httpClient.PostAsync(rootUrl, httpcontent);
            string responseContent = await response.Content.ReadAsStringAsync();
            //解析html文档
            var docNext = new HtmlDocument();
            docNext.LoadHtml(responseContent);

            var folderNodesNext = docNext.DocumentNode.SelectNodes("//*[@name='folderlist']");
            //查找href
            if (folderNodesNext != null)
            {
                foreach (var node in folderNodesNext)
                {
                    var href = node.Attributes["href"].Value;
                    var newLink = Path.Combine(url, href);
                    //如果到达最后一个文件则返回null
                    List<string> subLink = await GetFolderUrl(newLink);

                    if (subLink == null)
                    {
                        links.Add(newLink);
                    }
                    else
                    {
                        links.AddRange(subLink);
                        string html1 = await httpClient.GetStringAsync(newLink);
                        //解析html文档
                        var doc1 = new HtmlDocument();
                        doc1.LoadHtml(html1);
                        if (doc1.DocumentNode.SelectNodes("//*[@name='filelist']") != null)
                        {
                            links.Add(newLink);
                        }
                    }
                }
                return links;
            }
            else
            {
                return null;
            }
        }       
    }

    var folderNodes = doc.DocumentNode.SelectNodes("//*[@name='folderlist']");
    //查找href
    if (folderNodes != null)
    {
        foreach (var node in folderNodes)
        {
            var href = node.Attributes["href"].Value;
            var newLink = Path.Combine(url, href);
            //如果到达最后一个文件则返回null
            List<string> subLink = await GetFolderUrl(newLink);

            if (subLink == null)
            {
                links.Add(newLink);
            }
            else
            {
                links.AddRange(subLink);
                string html1 = await httpClient.GetStringAsync(newLink);
                //解析html文档
                var doc1 = new HtmlDocument();
                doc1.LoadHtml(html1);
                if (doc1.DocumentNode.SelectNodes("//*[@name='filelist']") != null)
                {
                    links.Add(newLink);
                }
                
            }
        }
        return links;
    }
    else
    {
        return null;
    }
}



