using BookCrawlerNoSubpage;
using HtmlAgilityPack;
using log4net;
using log4net.Repository.Hierarchy;
using System;
using System.Configuration;
using System.Diagnostics;
using System.Xml.Linq;


//声明一个log4net对象
ILog logger = LogManager.GetLogger(typeof(Program));
log4net.Config.XmlConfigurator.Configure();
//记录花费时间
Stopwatch stopWatch = Stopwatch.StartNew();
//在方法2中记录每条信息
int index1 = 0;
//从配置文件App.config中获取value值，即下载地址
string rootUrl = ConfigurationManager.AppSettings["url"];

//声明一个httpClient对象，目的向网页发送http请求
var httpClient = new HttpClient();
//在exe执行根目录中，写入csv文件，outputFilePath是相对路径
string outputFilePath = $"悦享云盘-Books {DateTime.Now.ToString("yyyy-MM-dd HH#mm#ss")}.csv";
//向csv文件中，写入首行信息
File.WriteAllText(outputFilePath, "Name,EditTime,Size,DownloadUrl" + Environment.NewLine);

//调用自定义方法：边找文件夹，边写出书籍信息
try
{
    List<string> folderUrls = await GetFinalFolderUrl(rootUrl);
}
catch (Exception e)
{
    //用log4net记录异常信息
    logger.Error("异常 ", e);
}
//用log4net记录信息
logger.Info("总时长 " + stopWatch.Elapsed);
Console.WriteLine("写入结束！");


///方法1：获取首页和分页所有文件夹路径，即文件路径
async Task<List<string>> GetFolderUrlFromDoc(string url)
{
    List<string> folderUrlList = new List<string>();
    string html = await httpClient.GetStringAsync(url);
    //解析html文档
    var doc = new HtmlDocument();
    doc.LoadHtml(html);

    //获取首页文件夹路径
    var mainPageUrl = await GetFolderUrlFromPage(url, doc);
    folderUrlList.AddRange(mainPageUrl);

    //判断是否存在分页
    var pageNode = doc.DocumentNode.SelectSingleNode("//*[@id='nextpageform']");
    //如果存在分页，找到总页数
    if (pageNode != null)
    {
        //通过td获取总页数pageNumAll：第二个td；只获取td的子元素节点（排除文本节点）
        var tdNode = pageNode.Descendants("td").ToArray()[1];
        var pageNumAll = tdNode.ChildNodes.Where(x => x.NodeType == HtmlNodeType.Element).Count();
        //获取第2,3页的html
        for (int i = 2; i <= pageNumAll; i++)
        {
            //根据js发送的post请求，获取第i页的html
            List<KeyValuePair<string, string>> keyValuePairs = new List<KeyValuePair<string, string>>();
            keyValuePairs.Add(new KeyValuePair<string, string>("pagenum", i.ToString()));
            FormUrlEncodedContent httpcontent = new FormUrlEncodedContent(keyValuePairs);
            var response = await httpClient.PostAsync(url, httpcontent);
            string responseContent = await response.Content.ReadAsStringAsync();
            //解析html文档          
            doc.LoadHtml(responseContent);
            //获取分页的文件夹路径
            var subPageUrl = await GetFolderUrlFromPage(url, doc);
            folderUrlList.AddRange(subPageUrl);
        }
    }
    return folderUrlList;
}

///方法2-1辅助方法2：GetBookInfoFromFolderUrl：通过文件夹路径，获取当前页面里的所有书籍信息，返回当前页面所有书籍的list
void GetBookInfoFromFolderUrl(string folderUrl, HtmlDocument bookDoc)
{
    // 使用 XPath 表达式来提取电影名称
    HtmlNodeCollection nodes = bookDoc.DocumentNode.SelectNodes("//*[@name='filelist']");
    List<Book> bookList = new List<Book>();
    if (nodes != null)
    {
        foreach (HtmlNode node in nodes)
        {
            //a的父级td,td的父级tr
            //<table> 标签表示整个表格，<tr> 标签表示表格中的每一行，<td> 标签表示表格中的每个单元格。
            //参考 HtmlAgilityPack 文档
            HtmlNode parent = node.ParentNode.ParentNode;
            var des = parent.Descendants("a").ToArray();
            string fileName = des[0].InnerText;
            string url = Path.Combine(folderUrl, des[1].Attributes["href"].Value);
            DateTime editTime = DateTime.Parse(parent.Descendants("td").ToList()[1].InnerText);
            string size = parent.Descendants("td").Last().InnerText;
            //将书籍信息放入Book类中
            Book book = new Book()
            {
                Name = fileName,
                DownloadUrl = url,
                EditTime = editTime,
                Size = size
            };
            bookList.Add(book);
            File.AppendAllText(outputFilePath, book.ToString() + Environment.NewLine);
            Console.WriteLine("Book info:" + book.ToString());
        }
    }
}

///方法2：获取首页或者分页的文件夹路径

async Task<List<string>> GetFolderUrlFromPage(string url, HtmlDocument doc)
{
    index1++;
    //logger.Info在log文件里写入
    logger.Info($"正在尝试解析第{index1}条 " + url);
    Console.WriteLine($"正在尝试解析第{index1}条 " + url);
    List<string> folderUrlList = new List<string>();
    var folderNodes = doc.DocumentNode.SelectNodes("//*[@name='folderlist']");
    //查找href
    if (folderNodes != null)
    {
        foreach (var node in folderNodes)
        {
            var href = node.Attributes["href"].Value;
            var newLink = Path.Combine(url, href);
            folderUrlList.Add(newLink);
            //获得新文件夹的url路径，同时读取其内部文件信息
            GetBookInfoFromFolderUrl(newLink, doc);
        }
    }
    else
    {
        //如果folderNodes为null,根据原来url读取书籍信息
        GetBookInfoFromFolderUrl(url, doc);
    }
    //如果folderNodes为null, 没有任何操作，仍返回原来创建的folderUrlList
    return folderUrlList;
}


///方法3：获取所有最终文件夹路径
async Task<List<string>> GetFinalFolderUrl(string url)
{
    List<string> folderUrlList = new List<string>();
    List<string> folderUrls = await GetFolderUrlFromDoc(url);
    folderUrlList.AddRange(folderUrls);
    foreach (var folderUrl in folderUrls)
    {
        //递归，根据给定的文件夹路径，寻找它的子文件夹
        var subFolderUrl = await GetFinalFolderUrl(folderUrl);
        folderUrlList.AddRange(subFolderUrl);
    }
    return folderUrlList;
}


