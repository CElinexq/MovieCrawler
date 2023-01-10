using HtmlAgilityPack;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
namespace MovieCrawler
{
    internal class Book
    {
        public string Name { get; set; }
        public string DownloadUrl { get; set; }
        public DateTime EditTime { get; set; }
        public string Size { get; set; }
        

        public override string ToString()
        {
            return $"{Name},{DownloadUrl},{EditTime},{Size}";
        }
    }
}