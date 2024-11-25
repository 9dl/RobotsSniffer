# RobotsSniffer

![img.png](img.png)

**RobotsSniffer** is a command-line tool written in C# for analyzing the `robots.txt` files of websites. The tool
retrieves and parses `robots.txt` files to determine which paths are allowed or disallowed for web crawlers, helping
users understand site restrictions and accessibility rules.

## Features

- Retrieve and analyze `robots.txt` files from a single URL or a list of URLs.
- Parse the `robots.txt` file to display **allowed** and **disallowed** paths.
- Optionally save the results to an output file.
- Multi-threaded processing for improved performance when working with multiple URLs.
- Configurable timeout for HTTP requests.
- Gets `sitemap` URLs from `robots.txt`

---

## Requirements

- .NET 9 (for Compiling/Debugging)
- Brain (optional)

---

## Usage

### Syntax

```bash
RobotsSniffer -u <url> | -l <url-list> [-o <output-file>] [-timeout <ms>]
```

### Arguments

| Argument        | Description                                                                 |
|-----------------|-----------------------------------------------------------------------------|
| `-u <url>`      | Analyze the `robots.txt` file of a single URL.                              |
| `-l <url-list>` | Provide a file containing multiple URLs (one per line) to analyze in batch. |
| `-o <output>`   | Save the results to the specified file. Optional.                           |
| `-timeout <ms>` | Set the HTTP request timeout in milliseconds (default: 5000).               |

---

### Examples

#### Analyze a Single URL

```bash
RobotsSniffer -u https://example.com
```

Output:

```plaintext
[>] Url: https://example.com
[+] Checking url...
[+] Robots.txt found.
[?] Robots.txt content:
[?] Allowed:
[+] /
[?] Disallowed:
[-] /admin
[-] /private
```

#### Analyze a List of URLs

```bash
RobotsSniffer -l urls.txt -o output.txt
```

Where `urls.txt` contains:

```plaintext
https://example.com
https://another-site.com
```

Output:

- Results are printed to the console and saved in `output.txt`.

---

## How It Works

1. **Argument Parsing**:
   The tool validates and processes the command-line arguments to determine the mode of operation:
    - Single URL (`-u`).
    - Multiple URLs from a file (`-l`).

2. **Fetching `robots.txt`**:
   For each URL, the tool attempts to fetch the `robots.txt` file by appending `/robots.txt` to the base URL.

3. **Parsing the Content**:
   The `robots.txt` content is parsed to extract **allowed** (`Allow`) and **disallowed** (`Disallow`) paths.

4. **Output**:
   Results are displayed in the console and optionally written to the specified output file.

5. **Parallel Processing**:
   When analyzing multiple URLs, the tool uses multithreading (`Parallel.ForEach`) to process URLs concurrently for
   better performance.

## Future Improvements

- Support for identifying and extracting `Sitemap` URLs from `robots.txt`.
- Enhanced error reporting and logging.
- Option to customize the number of concurrent threads for URL processing.
- HTTP headers customization (e.g., user-agent string).

## Contributing

Contributions are welcome! If you'd like to add features, improve performance, or fix issues, feel free to submit a pull
request.

---

### Author

RobotsSniffer was created as a utility tool for web analysis, helping users understand how websites interact with web
crawlers. Author takes no responsibility for the misuse of this tool.