using System.Text;
using OmniExtract.Core.Models;

namespace OmniExtract.Web.Services;

public class ExportService
{
    public byte[] GetJsonBytes(string json) =>
        Encoding.UTF8.GetBytes(json);

    public string BuildCsv(UniversalOutput output)
    {
        var sb = new StringBuilder();

        if (output.Data.Any())
        {
            AppendRow(sb, output.Data.Keys);
            AppendRow(sb, output.Data.Values.Select(v => v?.ToString() ?? ""));
        }

        for (var i = 0; i < output.Tables.Count; i++)
        {
            var table = output.Tables[i];
            if (table.Count == 0) continue;

            if (sb.Length > 0) sb.AppendLine();
            sb.AppendLine($"# Table {i + 1}");

            foreach (var row in table)
            {
                var maxCols = table.Max(r => r.Count);
                var padded = row.Concat(Enumerable.Repeat("", maxCols - row.Count));
                AppendRow(sb, padded);
            }
        }

        if (sb.Length == 0)
            sb.AppendLine("# No data extracted");

        return sb.ToString();
    }

    private static void AppendRow(StringBuilder sb, IEnumerable<string> values)
    {
        sb.AppendLine(string.Join(",", values.Select(QuoteCsv)));
    }

    private static string QuoteCsv(string value)
    {
        if (value.Contains(',') || value.Contains('"') || value.Contains('\n') || value.Contains('\r'))
            return $"\"{value.Replace("\"", "\"\"")}\"";
        return value;
    }
}
