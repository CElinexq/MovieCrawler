using HtmlAgilityPack;
using System.Xml.Linq;


string rootUrl = @"https://pity.eu.org/SP2/doc/%E6%BC%AB%E7%94%BB/500%E5%A5%97%E8%BF%9E%E7%8E%AF%E7%94%BB%E5%90%88%E9%9B%86/";
var httpClient = new HttpClient();
string html = await httpClient.GetStringAsync(rootUrl);
var doc = new HtmlDocument();
doc.LoadHtml(html);
var pageNode = doc.DocumentNode.SelectSingleNode("//*[@id='nextpageform']");
if (pageNode != null)
{
    //var pageLink = pageNode.Descendants("td").Last().InnerHtml;
    var tdNode = pageNode.Descendants("td").ToArray()[1];
    var pageNum= tdNode.ChildNodes.Where(x => x.NodeType == HtmlNodeType.Element).Count();
    //foreach (var item in pageNum)
    //{
    //    Console.WriteLine(item.OuterHtml);
    //}
    
    //Console.WriteLine(pageLink);
    //Console.WriteLine(td2);
    Console.WriteLine(pageNum);
}


using (HttpClient client = new HttpClient())
{
    //获取第二页的html
    List<KeyValuePair<string, string>> keyValuePairs = new List<KeyValuePair<string, string>>();
    keyValuePairs.Add(new KeyValuePair<string, string>("pagenum", 3.ToString()));
    FormUrlEncodedContent httpcontent = new FormUrlEncodedContent(keyValuePairs);
    var response = await client.PostAsync(rootUrl, httpcontent);
    string responseContent = await response.Content.ReadAsStringAsync();
    Console.WriteLine(responseContent);
    //解析html文档
    var doc1 = new HtmlDocument();
    doc1.LoadHtml(responseContent);
}