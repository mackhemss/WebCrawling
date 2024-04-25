using HtmlAgilityPack;
using System.Globalization;
using CsvHelper;
using System.Collections.Concurrent;
using HtmlAgilityPack.CssSelectors.NetCore;

namespace WebCrawling
{
    public class Program
    {
        // defining a custom class to store 
        // the scraped data 
        public class PokemonProduct
        {
            public string? Url { get; set; }
            public string? Image { get; set; }
            public string? Name { get; set; }
            public string? Price { get; set; }
        }
        public static void Main()
        {
            // initializing HAP 
            var web = new HtmlWeb();
            // setting a global User-Agent header 
            web.UserAgent = "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/109.0.0.0 Safari/537.36";
            // creating the list that will keep the scraped data 

            var pokemonProducts = new List<PokemonProduct>();
            // the URL of the first pagination web page 
            var firstPageToScrape = "https://scrapeme.live/shop/page/1/";
            // the list of pages discovered during the crawling task 
            var pagesDiscovered = new List<string> { firstPageToScrape };
            // the list of pages that remains to be scraped 
            var pagesToScrape = new Queue<string>();
            // initializing the list with firstPageToScrape 
            pagesToScrape.Enqueue(firstPageToScrape);
            // current crawling iteration 
            int i = 1;
            // the maximum number of pages to scrape before stopping 
            int limit = 5;
            // until there is a page to scrape or limit is hit 
            while (pagesToScrape.Count != 0 && i < limit)
            {
                // getting the current page to scrape from the queue 
                var currentPage = pagesToScrape.Dequeue();
                // loading the page 
                var currentDocument = web.Load(currentPage);
                // selecting the list of pagination HTML elements 
                var paginationHTMLElements = currentDocument.DocumentNode.QuerySelectorAll("a.page-numbers");
                // to avoid visiting a page twice 
                foreach (var paginationHTMLElement in paginationHTMLElements)
                {
                    // extracting the current pagination URL 
                    var newPaginationLink = paginationHTMLElement.Attributes["href"].Value;
                    // if the page discovered is new 
                    if (!pagesDiscovered.Contains(newPaginationLink))
                    {
                        // if the page discovered needs to be scraped 
                        if (!pagesToScrape.Contains(newPaginationLink))
                        {
                            pagesToScrape.Enqueue(newPaginationLink);
                        }
                        pagesDiscovered.Add(newPaginationLink);
                    }
                }
                // getting the list of HTML product nodes 
                var productHTMLElements = currentDocument.DocumentNode.QuerySelectorAll("li.product");
                // iterating over the list of product HTML elements 
                foreach (var productHTMLElement in productHTMLElements)
                {
                    // scraping logic 
                    var url = HtmlEntity.DeEntitize(productHTMLElement.QuerySelector("a").Attributes["href"].Value);
                    var image = HtmlEntity.DeEntitize(productHTMLElement.QuerySelector("img").Attributes["src"].Value);
                    var name = HtmlEntity.DeEntitize(productHTMLElement.QuerySelector("h2").InnerText);
                    var price = HtmlEntity.DeEntitize(productHTMLElement.QuerySelector(".price").InnerText);
                    var pokemonProduct = new PokemonProduct() { Url = url, Image = image, Name = name, Price = price };
                    pokemonProducts.Add(pokemonProduct);
                }
                // incrementing the crawling counter 
                i++;
            }
            // opening the CSV stream reader 
            using (var writer = new StreamWriter("pokemon-products.csv"))
            using (var csv = new CsvWriter(writer, CultureInfo.InvariantCulture))
            {
                // populating the CSV file 
                csv.WriteRecords(pokemonProducts);
            }
        }
    }
}
