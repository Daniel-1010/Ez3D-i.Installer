using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Text.Json;

namespace Ez3Di.Installer.Core
{
    public class TestResultEntry
    {
        public string TestName { get; set; } = "";
        public string Status { get; set; } = "";
        public string Details { get; set; } = "";
        public string Timestamp { get; set; } = "";
        public List<string> Screenshots { get; set; } = new();
    }

    public static class ResultWriter
    {
        private static readonly object _lock = new object();

        public static void WriteResult(string testName, string status, string details = "", List<string>? screenshots = null)
        {
            lock (_lock)
            {
                try
                {
                    string resultDir = InstallerConstants.ResultDirectory;
                    Directory.CreateDirectory(resultDir);

                    string jsonPath = Path.Combine(resultDir, "ResultsData.json");
                    Dictionary<string, TestResultEntry> resultsMap = new();

                    if (File.Exists(jsonPath))
                    {
                        try
                        {
                            string existingJson = File.ReadAllText(jsonPath);
                            var loaded = JsonSerializer.Deserialize<Dictionary<string, TestResultEntry>>(existingJson);
                            if (loaded != null) resultsMap = loaded;
                        }
                        catch { }
                    }

                    resultsMap[testName] = new TestResultEntry
                    {
                        TestName = testName,
                        Status = status,
                        Details = details,
                        Timestamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"),
                        Screenshots = screenshots != null ? new List<string>(screenshots) : new List<string>()
                    };

                    File.WriteAllText(jsonPath, JsonSerializer.Serialize(resultsMap, new JsonSerializerOptions { WriteIndented = true }));

                    // Text summary
                    string txtPath = Path.Combine(resultDir, "Results.txt");
                    var sbTxt = new StringBuilder();
                    foreach (var kvp in resultsMap)
                    {
                        sbTxt.AppendLine($"[{kvp.Value.Timestamp}] {kvp.Key}: {kvp.Value.Status} - {kvp.Value.Details}");
                    }
                    File.WriteAllText(txtPath, sbTxt.ToString());

                    // HTML Report
                    GenerateHtmlReport(resultDir, resultsMap);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"[ResultWriter] Warning writing result: {ex.Message}");
                }
            }
        }

        private static void GenerateHtmlReport(string resultDir, Dictionary<string, TestResultEntry> results)
        {
            try
            {
                var sb = new StringBuilder();
                sb.AppendLine("<!DOCTYPE html><html><head><meta charset='utf-8'><title>Ez3D-i Installation Test Report</title>");
                sb.AppendLine("<style>");
                sb.AppendLine("body { font-family: 'Segoe UI', Arial, sans-serif; background: #1e1e1e; color: #d4d4d4; margin: 20px; }");
                sb.AppendLine("h1 { color: #00AD87; }");
                sb.AppendLine(".card { background: #252526; border: 1px solid #3c3c3c; border-radius: 6px; padding: 15px; margin-bottom: 15px; }");
                sb.AppendLine(".pass { color: #4EC9B0; font-weight: bold; }");
                sb.AppendLine(".fail { color: #F14C4C; font-weight: bold; }");
                sb.AppendLine(".thumb { max-width: 300px; border: 1px solid #555; margin: 5px; border-radius: 4px; }");
                sb.AppendLine("</style></head><body>");
                sb.AppendLine("<h1>Ez3D-i Installation Test Automation Report</h1>");

                foreach (var entry in results.Values)
                {
                    string statusClass = entry.Status.ToUpper().Contains("PASS") ? "pass" : "fail";
                    sb.AppendLine("<div class='card'>");
                    sb.AppendLine($"<h3>{entry.TestName} - <span class='{statusClass}'>{entry.Status}</span></h3>");
                    sb.AppendLine($"<p><strong>Timestamp:</strong> {entry.Timestamp}</p>");
                    if (!string.IsNullOrEmpty(entry.Details))
                    {
                        sb.AppendLine($"<p><strong>Details:</strong> {entry.Details}</p>");
                    }

                    if (entry.Screenshots.Count > 0)
                    {
                        sb.AppendLine("<div>");
                        foreach (var sc in entry.Screenshots)
                        {
                            string relPath = Path.GetRelativePath(resultDir, sc).Replace("\\", "/");
                            sb.AppendLine($"<a href='{relPath}' target='_blank'><img class='thumb' src='{relPath}' alt='{Path.GetFileName(sc)}'></a>");
                        }
                        sb.AppendLine("</div>");
                    }
                    sb.AppendLine("</div>");
                }

                sb.AppendLine("</body></html>");
                File.WriteAllText(Path.Combine(resultDir, "Report_Installation.html"), sb.ToString());
            }
            catch { }
        }
    }
}
