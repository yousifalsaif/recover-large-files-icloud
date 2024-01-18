#pragma warning disable IDE1006 // Naming Styles
using System.Text.Json;
using System.Net;
using System.Text;
using System.Collections.Concurrent;

public class CFile
{

    public string drivewsid { get; set; } = null!;
    public string etag { get; set; } = null!;
}

public class CRoot
{
    public List<CFile> files { get; set; } = null!;
}

internal class Program
{
    static readonly string clientId = "00000000-0000-0000-0000-000000000000";
    static readonly string dsid = "0000000000";
    static readonly string cookie = File.ReadAllText("cookie.txt");
    static readonly string json = File.ReadAllText("files.json");
    static readonly string clientBuildNumber = "2404Hotfix19";
    static readonly string clientMasteringNumber = "2404Hotfix19";
    static readonly string iCloudBackend = "p116-drivews.icloud.com";
    static readonly int maxConcurrent = 20;
    static readonly string url = $"https://{iCloudBackend}/recoverDeletedFiles?clientBuildNumber={clientBuildNumber}&clientMasteringNumber={clientMasteringNumber}&clientId={clientId}&dsid={dsid}";
    static readonly string authority = iCloudBackend;
    static readonly string accept = "*/*";
    static readonly string accept_language = "en-US,en;q=0.9";
    static readonly string origin = "https://www.icloud.com";
    static readonly string referer = "https://www.icloud.com/";
    static readonly string sec_ch_ua = "\"Not_A Brand\";v=\"8\", \"Chromium\";v=\"120\", \"Google Chrome\";v=\"120\"";
    static readonly string sec_ch_ua_mobile = "?0";
    static readonly string sec_ch_ua_platform = "\"macOS\"";
    static readonly string sec_fetch_dest = "empty";
    static readonly string sec_fetch_mode = "cors";
    static readonly string sec_fetch_site = "same-site";
    static readonly string useragent = "Mozilla/5.0 (Macintosh; Intel Mac OS X 10_15_7) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/120.0.0.0 Safari/537.36";
    private static async Task Main(string[] args)
    {

        // open json file and deserialize it
        var files = JsonSerializer.Deserialize<CRoot>(json);
        // Run SendJson in a separate task to send files maxConcurrent file at a time and wait for all tasks to complete before continuing
        var tasks = new List<Task>();
        for (int i = 0; i < files!.files.Count; i++)
        {
            tasks.Add(SendJson(files.files[i], i, files.files.Count));
            if (i % maxConcurrent == 0)
            {
                Task.WaitAll(tasks.ToArray());
                tasks.Clear();
                await Task.Delay(100); // catch breath
            }
        }
        Task.WaitAll(tasks.ToArray());

        Console.WriteLine("Done!");
    }

    public static async Task SendJson(CFile file, int i, int total)
    {
        var singleFile = new CRoot();
        singleFile.files = new List<CFile>();
        singleFile.files.Add(file);
        var singleFileJson = JsonSerializer.Serialize(singleFile);

        var client = new HttpClient();
        client.DefaultRequestHeaders.Add("authority", authority);
        client.DefaultRequestHeaders.Add("accept", accept);
        client.DefaultRequestHeaders.Add("accept-language", accept_language);
        client.DefaultRequestHeaders.Add("origin", origin);
        client.DefaultRequestHeaders.Add("referer", referer);
        client.DefaultRequestHeaders.Add("sec-ch-ua", sec_ch_ua);
        client.DefaultRequestHeaders.Add("sec-ch-ua-mobile", sec_ch_ua_mobile);
        client.DefaultRequestHeaders.Add("sec-ch-ua-platform", sec_ch_ua_platform);
        client.DefaultRequestHeaders.Add("sec-fetch-dest", sec_fetch_dest);
        client.DefaultRequestHeaders.Add("sec-fetch-mode", sec_fetch_mode);
        client.DefaultRequestHeaders.Add("sec-fetch-site", sec_fetch_site);
        client.DefaultRequestHeaders.Add("user-agent", useragent);
        client.DefaultRequestHeaders.Add("cookie", cookie);

        var res = await client.PostAsync(url, new StringContent(singleFileJson, Encoding.UTF8, "application/json"));
        var responseCode = res.StatusCode;
        while (responseCode == HttpStatusCode.ServiceUnavailable)
        {
            // retry after one second
            await Task.Delay(1100);
            ConsoleWriter.WriteLine($"Retry delay: {i}");
            res = await client.PostAsync(url, new StringContent(singleFileJson, Encoding.UTF8, "application/json"));
            responseCode = res.StatusCode;
        }
        ConsoleWriter.WriteLine($"File {i}/{total}: ResponseCode: {responseCode} Drivewsid: {file.drivewsid}");
        return;
    }

    // Write to console without blocking the thread
    public static class ConsoleWriter
    {
        private readonly static BlockingCollection<string> blockingCollection = new BlockingCollection<string>();

        static ConsoleWriter()
        {
            Task.Run(() =>
            {
                while (true)
                {
                    Console.WriteLine(blockingCollection.Take());
                }

            });
        }

        public static void WriteLine(string value)
        {
            blockingCollection.Add(value);
        }

    }
}