using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AngleSharp;
using AngleSharp.Parser.Html;
using System.Net;
using System.IO;
using System.Globalization;
using System.Data.SqlClient;
using System.Data;
using AngleSharp.Dom.Html;

namespace AngleHtmlParser
{
    class Program
    {

        static void Main(string[] args)
        {
            string baseURI = "http://www.gumtree.com.au";
            string mainLink = "http://www.gumtree.com.au/s-property-for-rent/forrentby-ownr/c18364";
           //string mainLink = "http://www.gumtree.com.au/s-property-for-rent/forrentby-ownr/page-48/c18364";
            SqlConnection conn;
            WebClient client;
            HtmlParser parser;
             PrepareForParse(out conn, out client, out parser); //инициализация подключений и парсера
            // string myParameters = "targetUrl=&likingAd=false&loginMail=clfdynic5oep@mail.ru&password:'megapass!'&rememberMe=true&_rememberMe=on";




            //отправка запроса авторизации
            string myParameters = "targetUrl=%2Ft-settings.html&likingAd=false&loginMail=clfdynic5oep%40mail.ru&password=megapass%21&rememberMe=true&_rememberMe=on";

            string URI = "https://www.gumtree.com.au/t-login.html";

       string request = POST(URI, myParameters);

          System.IO.File.WriteAllText(@"request.html", request);
           
        System.Diagnostics.Process.Start("chrome.exe", "request.html");

            //конец отправки запроса авторизации

            Auth(myParameters,client);
            Console.ReadLine();
            return;
            string offersListPageHtml = GetHTMLFromWeb(client, mainLink);  //получить HTML из ссылки на текущую страницу

            var offersListPageHtmlDocument = parser.Parse(offersListPageHtml);  //получаем DOM-документ одной страницы выдачи предложений

            var link = offersListPageHtmlDocument.QuerySelector("a.paginator__button-next");
            //Console.WriteLine(link.GetAttribute("href"));
            //Console.ReadKey();
            do
            {
                Console.WriteLine("Получено:" + baseURI + link.GetAttribute("href"));

            
                ParseOffersListPage(conn, client, parser, offersListPageHtmlDocument); //парсим предложения этой страницы



                offersListPageHtml = GetHTMLFromWeb(client, baseURI + link.GetAttribute("href"));
                offersListPageHtmlDocument = parser.Parse(offersListPageHtml);  //получаем DOM-документ одной страницы выдачи предложений

                link = offersListPageHtmlDocument.QuerySelector("a.paginator__button-next");

            } while (link != null);
            


            
            ExportFromDbToHTML(conn, parser); //Экспорт данных таблицы товаров из БД в HTML

        }

        private static void Auth(string myParameters,WebClient client)  //авторизация приложения на сайте
        {
            //client.Headers.Add("Accept-Encoding", "gzip, deflate, br");
            client.Headers.Add("User-Agent", "Mozilla/5.0 (Windows NT 6.3; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/56.0.2924.87 Safari/537.36");
            client.Headers.Add("Accept-Language", "ru-RU,ru;q=0.8,en-US;q=0.6,en;q=0.4");
            //client.Headers.Add("Cookie", "machId=j5qIU26VcC8M2hxTLswQffZWYV4YrqlxEIOpNycazrNLBqwXa1AT_4t1PyVE1Llx6lFLuMb4CaVN5nvi0kTQOEbrjueQhO3sS-D87w; __gads=ID=23a02eacee056a99:T=1487676004:S=ALNI_MYYFbKwan2CCxnlMjL-6juEautSzA; bs=%7B%22st%22%3A%7B%7D%7D; up=%7B%22ln%22%3A%22691185324%22%2C%22ls%22%3A%22l%3D0%26c%3D18364%26r%3D0%26sv%3DLIST%26rentals.forrentby_s%3Downr%26sf%3Ddate%22%2C%22lsh%22%3A%22l%3D0%26c%3D18364%26r%3D0%26sv%3DLIST%26rentals.forrentby_s%3Downr%26sf%3Ddate%22%2C%22lbh%22%3A%22l%3D0%26c%3D18397%26r%3D0%26sv%3DLIST%26sf%3Ddate%7Cl%3D0%26c%3D18364%26r%3D0%26sv%3DLIST%26rentals.forrentby_s%3Downr%26sf%3Ddate%22%2C%22rva%22%3A%221138697382%2C1127508010%2C1127315705%2C1136736993%2C1140154194%2C1129303530%2C1131393943%2C1116520405%2C1138776675%2C1138378636%2C1137586974%2C1140733088%2C1119857408%2C1132693397%2C1131805193%22%7D; sc=jWv8pcTY9eAvWSyP3C6X; __utmt_siteTracker=1; _gat=1; wl=%7B%22l%22%3A%22%22%7D; __utma=160852194.1144746457.1487577940.1488547153.1488780594.28; __utmb=160852194.9.10.1488780594; __utmc=160852194; __utmz=160852194.1488446372.21.2.utmcsr=google|utmccn=(organic)|utmcmd=organic|utmctr=(not%20provided); _ga=GA1.3.1144746457.1487577940; ki_t=1487577951994%3B1488780598688%3B1488781641894%3B11%3B94; ki_r=; _gali=btn-submit-login");
            client.Headers.Add("Host","www.gumtree.com.au");
            client.Headers.Add("Upgrade-Insecure-Requests", "1");
            client.Headers.Add("ContentType","application/x-www-form-urlencoded");
            client.Headers.Add("Accept","text/html,application/xhtml+xml,application/xml;q=0.9,image/webp,*/*;q=0.8");
            client.Headers.Add("Referer","https://www.gumtree.com.au/t-login-form.html?targetUrl=%2Ft-settings.html");
            Console.WriteLine(client.Headers.ToString());
            string URI = "https://www.gumtree.com.au/t-login.html";
            Console.WriteLine("Заголовки:");
            Console.WriteLine(client.Headers.ToString());
            CookieContainer cookies = new CookieContainer();
            //string result = client.UploadString(URI, myParameters);
            byte[] response = client.UploadValues(URI, new System.Collections.Specialized.NameValueCollection()
                  {
                     { "targetUrl" , "/t-settings.html" },
                     { "likingAd" , "false" },
                     {"loginMail" ,"clfdynic5oep@mail.ru"},
                     {"password" , "megapass!"},
                     { "rememberMe" , "true"},
                     { "_rememberMe" , "on" }

                   });
            string result = System.Text.Encoding.UTF8.GetString(response);
            System.IO.File.WriteAllText(@"htmllogin.html", result);

            Stream data = client.OpenRead("https://www.gumtree.com.au/m-messages.html");

            StreamReader reader = new StreamReader(data);
            string s = reader.ReadToEnd();
            System.IO.File.WriteAllText(@"htmllogin.html", s);
            data.Close();
            reader.Close();


            System.Diagnostics.Process.Start("firefox.exe", "htmllogin.html");
          
        }

        private static string POST(string Url, string Data)
        {
            //System.Net.WebRequest req = System.Net.WebRequest.Create(Url);
            HttpWebRequest req = (HttpWebRequest)WebRequest.Create(Url);
            req.Method = "POST";
            req.Timeout = 100000;
            req.ContentType = "application/x-www-form-urlencoded";
            req.Accept = "text/html,application/xhtml+xml,application/xml;q=0.9,image/webp,*/*;q=0.8";
            req.UserAgent = "Mozilla/5.0 (Windows NT 6.3; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/56.0.2924.87 Safari/537.36";
            //req.Headers.Set("User-Agent", "Mozilla/5.0 (Windows NT 6.3; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/56.0.2924.87 Safari/537.36");
            //req.Connection = "keep-alive";
            req.Referer = "https://www.gumtree.com.au/t-login-form.html?targetUrl=%2Ft-settings.html";
            //req.Headers.Add("Accept-Encoding", "gzip, deflate, br");
            req.Headers.Add("Accept-Language", "ru-RU,ru;q=0.8,en-US;q=0.6,en;q=0.4");
            req.Headers.Add("Cookie", "machId=j5qIU26VcC8M2hxTLswQffZWYV4YrqlxEIOpNycazrNLBqwXa1AT_4t1PyVE1Llx6lFLuMb4CaVN5nvi0kTQOEbrjueQhO3sS-D87w; __gads=ID=23a02eacee056a99:T=1487676004:S=ALNI_MYYFbKwan2CCxnlMjL-6juEautSzA; bs=%7B%22st%22%3A%7B%7D%7D; up=%7B%22ln%22%3A%22691185324%22%2C%22ls%22%3A%22l%3D0%26c%3D18364%26r%3D0%26sv%3DLIST%26rentals.forrentby_s%3Downr%26sf%3Ddate%22%2C%22lsh%22%3A%22l%3D0%26c%3D18364%26r%3D0%26sv%3DLIST%26rentals.forrentby_s%3Downr%26sf%3Ddate%22%2C%22lbh%22%3A%22l%3D0%26c%3D18397%26r%3D0%26sv%3DLIST%26sf%3Ddate%7Cl%3D0%26c%3D18364%26r%3D0%26sv%3DLIST%26rentals.forrentby_s%3Downr%26sf%3Ddate%22%2C%22rva%22%3A%221138697382%2C1127508010%2C1127315705%2C1136736993%2C1140154194%2C1129303530%2C1131393943%2C1116520405%2C1138776675%2C1138378636%2C1137586974%2C1140733088%2C1119857408%2C1132693397%2C1131805193%22%7D; sc=jWv8pcTY9eAvWSyP3C6X; __utmt_siteTracker=1; _gat=1; wl=%7B%22l%22%3A%22%22%7D; __utma=160852194.1144746457.1487577940.1488547153.1488780594.28; __utmb=160852194.9.10.1488780594; __utmc=160852194; __utmz=160852194.1488446372.21.2.utmcsr=google|utmccn=(organic)|utmcmd=organic|utmctr=(not%20provided); _ga=GA1.3.1144746457.1487577940; ki_t=1487577951994%3B1488780598688%3B1488781641894%3B11%3B94; ki_r=; _gali=btn-submit-login");
            req.Host = "www.gumtree.com.au";
            req.Headers.Add("Upgrade-Insecure-Requests", "1");
            req.AllowAutoRedirect = true;
            //req.AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate;
            byte[] sentData = Encoding.GetEncoding(1251).GetBytes(Data);
            req.ContentLength = sentData.Length;
            System.IO.Stream sendStream = req.GetRequestStream();
            sendStream.Write(sentData, 0, sentData.Length);
            sendStream.Close();
            System.Net.WebResponse res = req.GetResponse();
            System.Net.HttpWebResponse res2 = (HttpWebResponse)req.GetResponse();
            
            Console.WriteLine(res2.StatusCode.ToString()+" "+ res2.CharacterSet);
            Console.ReadKey();
            System.IO.Stream ReceiveStream = res.GetResponseStream();
            System.IO.StreamReader sr = new System.IO.StreamReader(ReceiveStream, Encoding.UTF8);
            //Кодировка указывается в зависимости от кодировки ответа сервера
            Char[] read = new Char[256];
            int count = sr.Read(read, 0, 256);
            string Out = String.Empty;
            while (count > 0)
            {
                String str = new String(read, 0, count);
                Out += str;
                count = sr.Read(read, 0, 256);
            }
            return Out;
        }


        private static void ParseOffersListPage(SqlConnection conn, WebClient client, HtmlParser parser, AngleSharp.Dom.Html.IHtmlDocument linkDocument)
        {
            var links = linkDocument.QuerySelectorAll(".ad-listing__title-link");
            //string link = links[0].GetAttribute("href");
            Console.WriteLine("Ссылки [" + links.Length + "] на предложения получены. Начинаю парсинг предложений.");
            foreach (var link in links)
            {
                ParseOffer(conn, client, parser, link);
            }
        }

        private static void ExportFromDbToHTML(SqlConnection conn, HtmlParser parser)
        {
            OpenSqlConnection(conn);
            string getCellsQueryString = "SELECT * FROM dbo.OffersView"; //используем представление, заранее написанное в БД
            SqlCommand getCellsQuery = new SqlCommand(getCellsQueryString, conn); //формируем SQL-запрос
            SqlDataReader dr = getCellsQuery.ExecuteReader();
            String resultPageString = "";
            try //прочитаем заготовочный файл для экспорта результата
            {   // Open the text file using a stream reader.
                using (StreamReader sr2 = new StreamReader("resultHTMLtemplate.html"))
                {
                    // Read the stream to a string, and write the string to the console.
                    resultPageString = sr2.ReadToEnd();
                    sr2.Close();
                    //System.Diagnostics.Process.Start("resultHTML.html");
                }
            }
            catch (Exception e)
            {
                Console.WriteLine("The file could not be read:");
                Console.WriteLine(e.Message);
            }
            //Just get the DOM representation
            var configParser = Configuration.Default.WithCss();
            var resultDocument = parser.Parse(resultPageString);
            var table = resultDocument.QuerySelector("tbody");
            var tableRow = resultDocument.CreateElement("tr");
            var tableData = resultDocument.CreateElement("td");
            //tableData.TextContent = "eriubfweouidhbvowehvfb";
            //tableRow.AppendChild(tableData);
            //table.AppendChild(tableRow);
            //Console.WriteLine(resultDocument.DocumentElement.OuterHtml);

            while (dr.Read())  //Теперь построчно считываем данные 
            {
                //table.AppendChild
                //Student stud = new Student();
                //stud.Studid = Convert.ToInt32(dr["StudId"]);
                //stud.StudName = dr["StudName"].ToString();
                //stud.StudentDept = dr["StudentDept"].ToString();
                //listid.Add(stud);
                tableRow = resultDocument.CreateElement("tr"); //создаем в DOM новую строку таблицы
                tableData = resultDocument.CreateElement("td"); //создаем в DOM новую табличную ячейку
                tableData.InnerHtml = dr["Название предложения"].ToString(); //считываем из результата запроса  Название предложения
                tableRow.AppendChild(tableData);  //помещаем получившуюся ячейку в конец строки таблицы
                tableData = resultDocument.CreateElement("td");  //создаем в DOM новую табличную ячейку
                tableData.InnerHtml = "$" + dr["Цена"].ToString(); //считываем из результата запроса цену
                tableRow.AppendChild(tableData); //помещаем получившуюся ячейку в конец строки таблицы
                table.AppendChild(tableRow); //помещаем получившуюся строку в конец таблицы

            }
            dr.Close();
            SqlCommand comm = new SqlCommand("SELECT COUNT(*) FROM dbo.OffersTable", conn);
            int count = (Int32)comm.ExecuteScalar();
            resultDocument.QuerySelector("span").InnerHtml = count.ToString();
            System.IO.File.WriteAllText(@"result.html", resultDocument.DocumentElement.OuterHtml);
            //System.Diagnostics.Process.Start("result.html");
            System.Diagnostics.Process.Start("chrome.exe", "result.html");
        }

        private static void ParseOffer(SqlConnection conn, WebClient client, HtmlParser parser, AngleSharp.Dom.IElement link)
        {
            //Stream data = client.OpenRead("http://www.gumtree.com.au/s-ad/kedron/property-for-rent/awesome-spacious-unit-in-kedron-with-a-c/1127508010");
            Stream data = client.OpenRead("http://www.gumtree.com.au" + link.GetAttribute("href").ToString());

            StreamReader reader = new StreamReader(data);
            string s = reader.ReadToEnd();
            data.Close();
            reader.Close();
           // Console.WriteLine("HTML предложения загружено!");
            //Just get the DOM representation
            var document = parser.Parse(s);

            //Serialize it back to the console
            //Console.WriteLine(document.QuerySelector("title").TextContent);
            //Console.WriteLine("Название аппартаментов:");
            //Console.WriteLine(document.QuerySelector("#ad-title").TextContent);
            //Console.WriteLine("Цена:");
            //string price = document.QuerySelector("span.j-original-price").TextContent.Trim();
            //string priceDec = price.Trim('$');
            //double price3 = double.Parse(priceDec, CultureInfo.InvariantCulture);
            //Console.WriteLine(double.Parse(document.QuerySelector("span.j-original-price").TextContent.Trim().Trim('$'), CultureInfo.InvariantCulture));
            //Console.ReadKey();
            OpenSqlConnection(conn);
            SqlCommand insertCommand = new SqlCommand();
            insertCommand.Connection = conn;
            insertCommand.CommandType = CommandType.StoredProcedure;
            insertCommand.CommandTimeout = 30;
            insertCommand.CommandText = "dbo.AddOrUpdateOffer";
            insertCommand.Parameters.AddWithValue("@ID", document.QuerySelector("meta[name=\"DCSext.ad\"]").GetAttribute("content"));
            insertCommand.Parameters.AddWithValue("@Name", document.QuerySelector("#ad-title").TextContent);
            //начиная с этого поля
            insertCommand.Parameters.AddWithValue("@Place", document.QuerySelector(".ad-heading__ad-map-link").TextContent);
            insertCommand.Parameters.AddWithValue("@Description", document.QuerySelector("#ad-description-details").TextContent);
            insertCommand.Parameters.AddWithValue("@DateListed", document.QuerySelector("#ad-heading > div:nth-child(1) > span:nth-child(2)").TextContent);
            insertCommand.Parameters.AddWithValue("@LastEdited", document.QuerySelector("#ad-attributes > dl:nth-child(2) > dd").TextContent);
            insertCommand.Parameters.AddWithValue("@DwellingType", document.QuerySelector("#c-rentals\\.dwellingtype_s").TextContent);
            insertCommand.Parameters.AddWithValue("@ForRentBy", document.QuerySelector("#c-rentals\\.forrentby_s").TextContent);
            insertCommand.Parameters.AddWithValue("@Available", document.QuerySelector("#c-rentals\\.availabilitystartdate_tdt").TextContent);
            insertCommand.Parameters.AddWithValue("@Bedrooms", document.QuerySelector("#c-rentals\\.numberbedrooms_s").TextContent);
            insertCommand.Parameters.AddWithValue("@Bathrooms", document.QuerySelector("#c-rentals\\.numberbathrooms_s").TextContent);
            insertCommand.Parameters.AddWithValue("@Parking", document.QuerySelector("#c-rentals\\.parking_s").TextContent);
            insertCommand.Parameters.AddWithValue("@Smoking", document.QuerySelector("#c-rentals\\.smoking_s").TextContent);
            insertCommand.Parameters.AddWithValue("@Furnished", document.QuerySelector("#c-rentals\\.furnished_s").TextContent);
            insertCommand.Parameters.AddWithValue("@PetFriendly", document.QuerySelector("#c-rentals\\.petsallowed_s").TextContent);
            insertCommand.Parameters.AddWithValue("@Images", getImagesFromDOM(document));
            var Price = document.QuerySelector("span.j-original-price").TextContent.Trim().Trim('$');
            double dPrice = 0;
            try
            {
                dPrice = double.Parse(Price, CultureInfo.InvariantCulture);
            }
            catch
            {
                dPrice = 0;
            }

            insertCommand.Parameters.AddWithValue("@Price", dPrice.ToString());
            try
            {
                insertCommand.ExecuteNonQuery();
            }
            catch(Exception e)
            {
                Console.WriteLine(e.Message);
            }
            
            conn.Close();
            //Console.WriteLine("Отпарсил и занес в БД!");
        }

        private static string getImagesFromDOM(IHtmlDocument document)
        {
            return document.QuerySelectorAll(".gallery__main-viewer-list li").Length.ToString();
        }

        private static void PrepareForParse(out SqlConnection conn, out WebClient client, out HtmlParser parser)  //тут мы инициализируем данные, необходимые для парсинга
        {
            //Prepare SQL DB
            string connect = "Data Source=NOTEBOOK;Initial Catalog=Offers;Integrated Security=True";
            conn = new SqlConnection(connect);
            //А теперь начинаем скачивать html файл c ссылками
            client = new WebClient();
            // Create a new parser front-end (can be re-used)
            parser = new HtmlParser();
        }

        private static string GetHTMLFromWeb(WebClient client, string offersPageLink)  //метод получает ссылки на текущей странице спредложениями
        {
            Stream linksPage = client.OpenRead(offersPageLink);
            StreamReader sr = new StreamReader(linksPage);
            string linksPageHtml = sr.ReadToEnd();
            linksPage.Close();
            sr.Close();
            Console.WriteLine("Страницу скачал!");
            return linksPageHtml;
            
        }

        private static bool OpenSqlConnection(SqlConnection conn) //Функцйия открытия соединения к БД
        {
            try
            {
                if (conn.State == System.Data.ConnectionState.Open) conn.Close();
                conn.Open(); // Открыть
                return true;
            }
            catch (Exception ex) // Исключение
            {
                Console.WriteLine(ex.Message);
                return false;
            }
        }
    }
}
