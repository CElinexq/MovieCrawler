using HtmlAgilityPack;
using System.Net.Http;
using System.Reflection.Metadata;
using System.Security.Cryptography.X509Certificates;

namespace MovieCrawler
{
    internal class Program
    {
        static async Task Main(string[] args)
        {     
            using HttpClient httpClient = new HttpClient();           
            string rootUrl = @"https://pity.eu.org/SP2/doc/";
            //自定义方法1: GetFolderUrl：获取 所有最后一个文件夹 的路径
            List<string> links = await GetFolderUrl(rootUrl);
            //桌面文件夹 写入csv文件夹路径
            string outputFilePath = @"C:\Users\cocox\Desktop\Bookinfo\悦享云盘-Books.csv";
            //判断文件路径是否存在，若不存在，直接写入
            if(!File.Exists(outputFilePath))
            {
                ////自定义方法3: 书籍信息写入csv文件
                BookInfoWriteIntoCSVFile();
            }
            else
            {
                //判断文件路径是否存在，若存在，删除并重新创建文件路径，并写入
                File.Delete(outputFilePath);
                File.Create(outputFilePath);
                BookInfoWriteIntoCSVFile();
            }


            ///方法1：GetFolderUrl：获取 最后一个文件夹 的路径（这是文件夹folder和文件file的分界线）
            async Task<List<string>> GetFolderUrl(string url)
            {
                ///解析网页的html文档，获取最后一个文件夹folder的节点
                    //将 所有文件夹路径 放入list中
                    List<string> list = new List<string>();
                    //发起get请求
                    string html = await httpClient.GetStringAsync(url);
                    //解析Html文档
                    HtmlDocument doc = new HtmlDocument();
                    doc.LoadHtml(html);
                    //获取文件夹节点：xpath获取节点，.DocumentNode根节点；
                    //name='folderlist'是获取 文件夹 节点，文件夹结束后是 文件 节点filelist
                    HtmlNodeCollection linkNodes = doc.DocumentNode.SelectNodes("//*[@name='folderlist']");

                ///判断找到的文件夹节点是否包含href链接
                    //文件夹不为空，意味着还有次级文件夹
                    if (linkNodes != null)
                    {
                        foreach (HtmlNode node in linkNodes)
                        {
                            //第一遍找 次链接
                            string link = node.Attributes["href"].Value;
                            string newLink = Path.Combine(url, link);
                            list.Add(newLink);
                            Console.WriteLine("Find Folder:" + newLink);
                            //递归，通过href不断获取新文件的链接,直到找到最后一个文件夹
                            List<string> subs = await GetFolderUrl(newLink);
                            if (subs != null)
                            {
                                list.AddRange(subs);
                            }
                        }
                        return list;
                    }
                    //文件夹为空，意味着到了最后一个文件夹
                    else
                    {
                        return null;
                    }
            }


            ///方法2：GetBookInfoFromFolderUrl：通过文件夹路径，获取当前页面里的所有书籍信息，返回当前页面所有书籍的list
            async Task<List<Book>> GetBookInfoFromFolderUrl(string folderUrl)
            {
                string bookHtml = await httpClient.GetStringAsync(folderUrl);
                // 使用 HtmlAgilityPack 库中的 HtmlDocument 类来解析 HTML 字符串
                HtmlDocument bookDoc = new HtmlDocument();
                bookDoc.LoadHtml(bookHtml);
                // 使用 XPath 表达式来提取电影名称
                HtmlNodeCollection nodes = bookDoc.DocumentNode.SelectNodes("//*[@name='filelist']");
                List<Book> bookList= new List<Book>();
                foreach (HtmlNode node in nodes)
                {
                    //a的父级td,td的父级tr
                    //<table> 标签表示整个表格，<tr> 标签表示表格中的每一行，<td> 标签表示表格中的每个单元格。
                    //参考 HtmlAgilityPack 文档
                    HtmlNode parent = node.ParentNode.ParentNode;
                    var des=parent.Descendants("a").ToArray();
                    string fileName = des[0].InnerText;
                    string url = Path.Combine(folderUrl,des[1].Attributes["href"].Value);                   
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
                    Console.WriteLine("Book info"+ book.ToString());
                }
                return bookList;
            }

            ///方法3：书籍信息写入csv文件
            async void BookInfoWriteIntoCSVFile()
            {
                //写入csv文件首行信息
                File.WriteAllText(outputFilePath, "Name,DownloadUrl,EditTime,Size" + Environment.NewLine);
                //循环写入每个链接的书籍信息
                foreach (string link in links)
                {
                    //自定义方法2：从单个文件夹中获取书籍信息
                    List<Book> books = await GetBookInfoFromFolderUrl(link);
                    if (books.Count != 0)
                    {
                        File.AppendAllLines(outputFilePath, books.Select(x => x.ToString()));
                    }
                }
                Console.WriteLine("写入结束！");
            }

        }
    }
}