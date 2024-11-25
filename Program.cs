using System.Text.RegularExpressions;
using static System.Console;

namespace RobotsSniffer;

internal static class Program
{
    private static string? _url;
    private static string? _urlList;
    private static string? _output;
    private static int _timeout = 5000;

    private static void Main(string[] args)
    {
        if (args.Length == 0)
        {
            WriteLine("!", "No arguments were passed.");
            Environment.Exit(1);
        }

        try
        {
            foreach (var arg in ParseArgs(args))
                switch (arg.Key)
                {
                    case "-u":
                        IsArgEmpty(arg.Key, arg.Value);
                        _url = arg.Value;
                        WriteLine(">", $"Url: {arg.Value}");
                        break;
                    case "-l":
                        IsArgEmpty(arg.Key, arg.Value);
                        _urlList = arg.Value;
                        WriteLine(">", $"Urls List: {arg.Value}");
                        break;
                    case "-o":
                        IsArgEmpty(arg.Key, arg.Value);
                        _output = arg.Value;
                        WriteLine(">", $"Output: {arg.Value}");
                        break;
                    case "-timeout":
                        IsArgEmpty(arg.Key, arg.Value);
                        if (!int.TryParse(arg.Value, out _timeout))
                            throw new ArgumentException("Invalid timeout value. Must be an integer.");
                        WriteLine(">", $"Timeout: {_timeout}ms");
                        break;
                    default:
                        WriteLine("!", $"Unknown argument: {arg.Key}");
                        break;
                }
        }
        catch (Exception ex)
        {
            WriteLine("!", $"Error parsing arguments: {ex.Message}");
            Environment.Exit(1);
        }

        Sniffer();
    }

    private static void Sniffer()
    {
        try
        {
            if (!string.IsNullOrEmpty(_output) && !File.Exists(_output))
                File.Create(_output).Close();

            if (!string.IsNullOrEmpty(_url)) ProcessUrl(_url);

            if (!string.IsNullOrEmpty(_urlList))
            {
                WriteLine("+", "Checking URLs from the list...");
                WriteLine("!", "Printing is disabled since it will bug out the console.");
                var parallelOptions = new ParallelOptions { MaxDegreeOfParallelism = 4 };
                Parallel.ForEach(File.ReadAllLines(_urlList), parallelOptions, url => { ProcessUrl(url, false); });
                WriteLine("+", "Finished checking URLs from the list.");
                ReadKey();
            }
        }
        catch (Exception ex)
        {
            WriteLine("!", $"Error during sniffing: {ex.Message}");
        }
    }

    private static void ProcessUrl(string url, bool verbose = true)
    {
        try
        {
            if (verbose) WriteLine("+", $"Processing URL: {url}");
            var html = GetHtml(url).Result;
            var hasRobots = IdentifyRobots(html);
            if (verbose) WriteLine(hasRobots ? "+" : "-", hasRobots ? "Robots.txt found." : "Robots.txt not found.");
            if (hasRobots)
            {
                var robots = ParseRobots(html);
                WriteRobotsOutput(url, robots, verbose);
            }

            var sitemap = IdentifySitemap(html);
            if (verbose) WriteLine("?", $"Sitemap: {sitemap}");
        }
        catch (Exception ex)
        {
            WriteLine("!", $"Error processing URL {url}: {ex.Message}");
        }
    }

    private static async Task<string> GetHtml(string url)
    {
        if (!url.EndsWith("/")) url += "/";
        url += "robots.txt";

        using var client = new HttpClient();
        client.Timeout = TimeSpan.FromMilliseconds(_timeout);
        var response = await client.GetAsync(url);
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadAsStringAsync();
    }

    private static Dictionary<string, List<string>> ParseRobots(string content)
    {
        var allowed = new List<string>();
        var disallowed = new List<string>();

        foreach (var line in content.Split('\n'))
        {
            var cleanedLine = line.Trim();
            if (cleanedLine.StartsWith("Allow:"))
                allowed.Add(cleanedLine[6..].Trim());
            else if (cleanedLine.StartsWith("Disallow:")) disallowed.Add(cleanedLine[9..].Trim());
        }

        return new Dictionary<string, List<string>>
        {
            { "Allowed", allowed },
            { "Disallowed", disallowed }
        };
    }

    private static void WriteRobotsOutput(string url, Dictionary<string, List<string>> robots, bool verbose = true)
    {
        if (verbose)
        {
            WriteLine("?", "Robots.txt content:");
            WriteLine("?", "Allowed:");
            robots["Allowed"].ForEach(path => WriteLine("+", path, padding: true));
            WriteLine("?", "Disallowed:");
            robots["Disallowed"].ForEach(path => WriteLine("-", path, padding: true));
        }

        if (!string.IsNullOrEmpty(_output))
        {
            var outputLines = new List<string>
            {
                new('=', 40),
                $"Url: {url}",
                "Allowed:",
                string.Join(Environment.NewLine, robots["Allowed"]),
                "Disallowed:",
                string.Join(Environment.NewLine, robots["Disallowed"]),
                new('=', 40)
            };
            File.AppendAllLines(_output, outputLines);
        }
    }

    private static bool IdentifyRobots(string content)
    {
        return Regex.IsMatch(content, @"User-agent:\s*\*", RegexOptions.IgnoreCase);
    }

    private static string IdentifySitemap(string content)
    {
        var sitemap = new Regex(@"(?<=Sitemap:\s*).*?(?=\z)",
            RegexOptions.Singleline | RegexOptions.IgnoreCase);
        var match = sitemap.Match(content);
        return match.Success ? match.Value : "Sitemap not found.";
    }


    private static void WriteLine(string option, string value, bool newLine = true, bool padding = false)
    {
        ForegroundColor = ConsoleColor.DarkGray;
        if (padding) Write("  ");
        Write("[");

        ForegroundColor = option switch
        {
            "!" => ConsoleColor.Cyan,
            "?" => ConsoleColor.Yellow,
            ">" => ConsoleColor.Magenta,
            "+" => ConsoleColor.Green,
            "-" => ConsoleColor.Red,
            _ => ForegroundColor
        };

        Write(option);
        ForegroundColor = ConsoleColor.DarkGray;
        Write("] ");
        ForegroundColor = ConsoleColor.White;
        if (newLine) Console.WriteLine(value);
        else Write(value);
    }

    private static void IsArgEmpty(string arg, string value)
    {
        if (string.IsNullOrEmpty(value))
            throw new ArgumentException($"The argument '{arg}' is empty.");
    }

    private static Dictionary<string, string> ParseArgs(string[] args)
    {
        var result = new Dictionary<string, string>();
        for (var i = 0; i < args.Length; i++)
            if (args[i].StartsWith("-"))
            {
                if (i + 1 < args.Length && !args[i + 1].StartsWith("-"))
                    result[args[i]] = args[++i];
                else
                    result[args[i]] = "";
            }

        return result;
    }
}