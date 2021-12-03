using GovernmentProcurement_ClassDB.Model;
using HtmlAgilityPack;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Security;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Web;
using MailKit.Net.Smtp;
using MimeKit;
using System.Web.Configuration;

namespace GovernmentProcurement_Console
{
    class Program
    {
        static void Main(string[] args)
        {
            // 取得總筆數
            float pageAmount = GetPageAmount();
            // 每頁幾筆
            float splitePage = 100;
            // 總共有幾頁
            int totalPage = Convert.ToInt16(Math.Ceiling(pageAmount / splitePage));
            // 爬每頁資料
            for (int i = 1; i <= totalPage; i++) {
                // 隨機等待 10~30 秒，避免封鎖
                Random random = new Random();
                Thread.Sleep(random.Next(100, 300) * 100);
                SetProcurementDate(i);
                Console.WriteLine($"第");
            }
            
        }

        private static void SetProcurementDate(int pageNumber)
        {
            ApplicationDbContext db = new ApplicationDbContext();

            // 先刪除已截止投標資料
            var dateLimit = DateTime.Now.AddDays(-1);
            var queryLimitTenders = db.Procurement.Where(x => x.DateLimitOfTenders <= dateLimit);
            db.Procurement.RemoveRange(queryLimitTenders);
            db.SaveChanges();

            // 指定查詢資料庫內標案最新一筆
            var queryLastData = db.Procurement.OrderByDescending(x => x.Id).Take(1);

            // 執行單頁網頁爬蟲
            string strKeyWord = WebConfigurationManager.AppSettings["KeyWord"];
            string sourceUrl = "https://web.pcc.gov.tw/tps";
            string strHtml = GetWebContent("https://web.pcc.gov.tw/tps/pss/tender.do?searchMode=common&searchType=basic&method=search&isSpdt=&pageIndex=" + pageNumber.ToString());
            HtmlDocument document = new HtmlDocument();
            document.LoadHtml(strHtml.Replace("\r\n", "").Replace("\t", "").Replace("&nbsp;", "").Replace("\n", ""));
            HtmlNode nodeList = document.DocumentNode.SelectSingleNode("//*[@id=\"print_area\"]/table");

            int count = 0;
            foreach (HtmlNode nodeTr in nodeList.ChildNodes) {
                int columnNumber = 1;
                bool checkKeyWord = false;
                Procurement procurement = new Procurement();
                if (nodeTr.Name == "tr" && count >= 1 && count <= 100) {
                    foreach (var nodeTd in nodeTr.ChildNodes) {
                        if (nodeTd.Name == "td" && !string.IsNullOrWhiteSpace(nodeTd.InnerHtml)) {
                            switch (columnNumber) {
                                case 2:
                                    procurement.ProcuringEntity = nodeTd.InnerText.Trim();
                                    break;
                                case 3:
                                    // 確認關鍵字
                                    checkKeyWord = nodeTd.InnerText.Contains(strKeyWord);
                                    // 連結處理加入源頭網址
                                    procurement.SubjectOfProcurement = HttpUtility.HtmlEncode(nodeTd.InnerHtml.Trim().Replace("..", sourceUrl));
                                    break;
                                case 4:
                                    procurement.TransmissionNumber = nodeTd.InnerText.Trim();
                                    break;
                                case 5:
                                    procurement.TypeOfTendering = nodeTd.InnerText.Trim();
                                    break;
                                case 6:
                                    procurement.TypeOfProcurement = nodeTd.InnerText.Trim();
                                    break;
                                case 7:
                                    procurement.DateOfPublication = DateTime.Parse(nodeTd.InnerText.Trim()).AddYears(1911);
                                    break;
                                case 8:
                                    procurement.DateLimitOfTenders = DateTime.Parse(nodeTd.InnerText.Trim()).AddYears(1911);
                                    break;
                                case 9:
                                    procurement.Budget = Convert.ToInt32(nodeTd.InnerText.Trim());
                                    break;
                                default:
                                    break;
                            }
                            if (columnNumber == 3 && !checkKeyWord) break;
                            columnNumber++;
                        }
                    }
                    if (checkKeyWord) db.Procurement.Add(procurement);
                }
                count++;
            }
            db.SaveChanges();
        }

        /// <summary>
        /// 發送符合關鍵字招標連結內容信件
        /// </summary>
        /// <param name="toName">會員暱稱=>收件人</param>
        /// <param name="userAccount">會員帳號=>收信地址</param>
        /// <param name="mailContent">信件內容</param>
        public static void SendDataLinkMail(string keyWord, string toName, string userAccount, string mailContent)
        {
            string fromAddress = WebConfigurationManager.AppSettings["gmailAccount"];
            string fromName = "電子招標關鍵字搜尋系統";
            string title = $"標案名稱關鍵字:[{keyWord}]搜尋結果";
            string mailAccount = WebConfigurationManager.AppSettings["gmailAccount"];
            string mailPassword = WebConfigurationManager.AppSettings["gmailPassword"];

            //建立建立郵件
            MimeMessage mail = new MimeMessage();
            // 添加寄件者
            mail.From.Add(new MailboxAddress(fromName, fromAddress));
            // 添加收件者
            mail.To.Add(new MailboxAddress(toName, userAccount));
            // 設定郵件標題
            mail.Subject = title;
            //使用 BodyBuilder 建立郵件內容
            BodyBuilder bodyBuilder = new BodyBuilder
            {
                HtmlBody = mailContent
            };
            //設定郵件內容
            mail.Body = bodyBuilder.ToMessageBody(); //轉成郵件內容格式

            using (var client = new SmtpClient()) {
                //有開防毒時需設定關閉檢查
                client.CheckCertificateRevocation = false;
                //設定連線 gmail ("smtp Server", Port, SSL加密) 
                client.Connect("smtp.gmail.com", 587, false); // localhost 測試使用加密需先關閉 

                // Note: only needed if the SMTP server requires authentication
                client.Authenticate(mailAccount, mailPassword);

                client.Send(mail);
                client.Disconnect(true);
            }
        }

        // 取得總筆數
        private static int GetPageAmount()
        {
            // 取得總筆數
            string pageHtml = GetWebContent("https://web.pcc.gov.tw/tps/pss/tender.do?searchMode=common&searchType=basic&method=search&isSpdt=&pageIndex=1");
            HtmlDocument document = new HtmlDocument();
            document.LoadHtml(pageHtml.Replace("\r\n", "").Replace("\t", "").Replace("&nbsp;", ""));
            HtmlNode nodeTotal = document.DocumentNode.SelectSingleNode("//*[@id=\"print_area\"]/table/tr[102]/td/span");
            int amount = Convert.ToInt32(nodeTotal.InnerText.Trim());
            return amount;
        }

        //取得網頁資料內容
        private static string GetWebContent(string Url)
        {
            var request = WebRequest.Create(Url) as HttpWebRequest;
            WebClient wc = new WebClient(); //從 URI 所識別的資源中，傳送與接收資料
            //REF: https://stackoverflow.com/a/39534068/288936
            ServicePointManager.SecurityProtocol =
                SecurityProtocolType.Ssl3 | SecurityProtocolType.Tls |
                SecurityProtocolType.Tls11 | SecurityProtocolType.Tls12;
            string res = wc.DownloadString(Url);
            // If required by the server, set the credentials.
            request.UserAgent = "PostmanRuntime/7.26.5";
            request.Accept = "*";
            request.Credentials = CredentialCache.DefaultCredentials;
            //驗證服務器證書回調自動驗證
            ServicePointManager.ServerCertificateValidationCallback = new RemoteCertificateValidationCallback(CheckValidationResult);
            // (重點是修改這行)set the security protocol before issuing the web request
            ServicePointManager.SecurityProtocol = SecurityProtocolType.Ssl3 | SecurityProtocolType.Tls |
                                                   SecurityProtocolType.Tls11 | SecurityProtocolType.Tls12;
            // Get the response.
            HttpWebResponse response = (HttpWebResponse)request.GetResponse();
            // Get the stream containing content returned by the server.
            Stream dataStream = response.GetResponseStream();
            // Open the stream using a StreamReader for easy access.
            StreamReader reader = new StreamReader(dataStream);
            // Read the content.
            string responseFromServer = reader.ReadToEnd();
            // Display the content.
            // Cleanup the streams and the response.
            reader.Close();
            dataStream.Close();
            response.Close();
            return responseFromServer;
        }

        //驗證伺服器憑證
        private static bool CheckValidationResult(object sender, X509Certificate certificate, X509Chain chain, SslPolicyErrors errors)
        {
            return true;
        }

    }
}
