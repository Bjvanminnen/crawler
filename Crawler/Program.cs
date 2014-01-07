using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.IO;
using System.Text.RegularExpressions;

namespace Crawler
{
    class Program
    {
        // will still follow links in comments

        static void Main(string[] args)
        {
            var crawl = new WebCrawler("http://kamcord.com", "http://kamcord.com/developers/");
            var links = crawl.GetAllLinks();

            foreach (var link in links)
            {
                System.Console.WriteLine(link);
            }
        }

        class WebCrawler
        {
            private string domain;
            private string root;

            private List<string> visited;
            private Stack<string> pending;

            private bool crawled = false;

            public WebCrawler(string domain, string root)
            {
                this.domain = domain;
                this.root = root;

                this.visited = new List<string>();
                this.pending = new Stack<string>();
            }

            public List<string> GetAllLinks() {
                if (crawled)
                {
                    return this.visited;
                }

                this.pending.Push(this.root);

                while (this.pending.Count > 0)
                {
                    var link = this.pending.Pop();
                    this.visited.Add(link);

                    var children = this.ScrapeLinks(link);
                    foreach (var child in children)
                    {
                        if (!this.visited.Contains(child) && !this.pending.Contains(child))
                        {
                            this.pending.Push(child);
                        }
                    }
                }

                this.crawled = true;
                return this.visited;
            }

            /**
             *  Given a url, finds all those links that are under the root
             */
            private List<string> ScrapeLinks(string url)
            {
                var html = GetWebPage(url);

                var matches = Regex.Matches(html, "<a href=\"(.*?)\">");
                var links = new List<string>();

                foreach (Match match in matches)
                {
                    var separator = match.Value["<a href=".Length];
                    var link = match.Value.Split(separator)[1];
                    if (link.StartsWith("/"))
                    {
                        link = this.domain + link;
                    }

                    if (link.StartsWith(this.root) && !links.Contains(link))
                    {
                        links.Add(link);
                    }
                }

                return links;
            }
        }

        private static string GetWebPage(string url) {
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create (url);

            // Set some reasonable limits on resources used by this request
            request.MaximumAutomaticRedirections = 4;
            request.MaximumResponseHeadersLength = 4;
            // Set credentials to use for this request.
            request.Credentials = CredentialCache.DefaultCredentials;
            HttpWebResponse response = (HttpWebResponse)request.GetResponse ();

            // Get the stream associated with the response.
            Stream receiveStream = response.GetResponseStream();

            // Pipes the stream to a higher level stream reader with the required encoding format. 
            StreamReader readStream = new StreamReader(receiveStream, Encoding.UTF8);

            var contents = readStream.ReadToEnd();
            response.Close();
            readStream.Close();

            return contents;
        }

        
    }
}
