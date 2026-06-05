using System.Text;
using CsvHelper;
using CsvHelper.Configuration;
using Docnet.Core;
using Docnet.Core.Models;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Presentation;
using DocumentFormat.OpenXml.Spreadsheet;
using DocumentFormat.OpenXml.Wordprocessing;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MimeKit;
using OmniExtract.Core.Config;
using OmniExtract.Core.Models;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;
using SixLabors.ImageSharp.Processing.Processors.Transforms;
using UglyToad.PdfPig;
using A = DocumentFormat.OpenXml.Drawing;
using PptShape = DocumentFormat.OpenXml.Presentation.Shape;
using WordTable = DocumentFormat.OpenXml.Wordprocessing.Table;
using WordText = DocumentFormat.OpenXml.Wordprocessing.Text;

namespace OmniExtract.App.Services;

public class DocumentProcessor
{
    private static readonly HashSet<string> NativeExtensions = new(StringComparer.OrdinalIgnoreCase)
    {
        ".pdf", ".png", ".jpg", ".jpeg", ".tiff", ".bmp", ".webp",
        ".docx", ".xlsx", ".pptx",
        ".txt", ".md", ".rtf",
        ".csv", ".tsv",
        ".json", ".jsonl", ".yaml", ".yml", ".xml",
        ".eml", ".ics"
    };

    private readonly ProcessingSettings _settings;
    private readonly LibreOfficeBridge _libreOffice;
    private readonly ILogger<DocumentProcessor> _logger;

    public DocumentProcessor(IOptions<ProcessingSettings> settings, LibreOfficeBridge libreOffice, ILogger<DocumentProcessor> logger)
    {
        _settings = settings.Value;
        _libreOffice = libreOffice;
        _logger = logger;
    }

    public async Task<ExtractionResult> ExtractAsync(string filePath, CancellationToken ct = default)
    {
        var ext = Path.GetExtension(filePath).ToLowerInvariant();
        _logger.LogInformation("Extracting [{Ext}] {File}", ext, Path.GetFileName(filePath));

        try
        {
            if (NativeExtensions.Contains(ext))
            {
                try
                {
                    return ExtractNative(filePath, ext);
                }
                catch (Exception ex)
                {
                    if (ext is ".docx" or ".xlsx" or ".pptx")
                    {
                        _logger.LogWarning(ex, "Native parse failed for {Ext} — trying LibreOffice fallback", ext);
                        var fallbackPdf = await _libreOffice.ConvertToPdfAsync(filePath, ct);
                        if (fallbackPdf is not null)
                        {
                            try { return ExtractNative(fallbackPdf, ".pdf"); }
                            finally { TryDelete(fallbackPdf); }
                        }
                    }
                    else if (ext is ".csv" or ".tsv")
                    {
                        _logger.LogWarning(ex, "CSV parse failed — falling back to raw text");
                        try { return new ExtractionResult { Text = ReadTextFile(filePath) }; }
                        catch { }
                    }
                    return new ExtractionResult { Error = ex.Message };
                }
            }

            // Unknown/legacy — try LibreOffice → PDF pipeline
            var pdfPath = await _libreOffice.ConvertToPdfAsync(filePath, ct);
            if (pdfPath is not null)
            {
                try
                {
                    return ExtractNative(pdfPath, ".pdf");
                }
                finally
                {
                    TryDelete(pdfPath);
                }
            }

            // Last resort — read as UTF-8 text
            try
            {
                var text = ReadTextFile(filePath);
                if (!IsGibberish(text))
                    return new ExtractionResult { Text = text };
            }
            catch { }

            return new ExtractionResult { Error = $"Unprocessable: {ext}" };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Extraction failed for {FilePath}", filePath);
            return new ExtractionResult { Error = ex.Message };
        }
    }

    private ExtractionResult ExtractNative(string filePath, string ext) => ext switch
    {
        ".txt" or ".md" or ".rtf" or ".ics" => new ExtractionResult { Text = ReadTextFile(filePath) },
        ".json" or ".jsonl" or ".yaml" or ".yml" or ".xml" => new ExtractionResult { Text = ReadTextFile(filePath) },
        ".csv" => ExtractCsv(filePath, ','),
        ".tsv" => ExtractCsv(filePath, '\t'),
        ".eml" => ExtractEml(filePath),
        ".docx" => ExtractDocx(filePath),
        ".xlsx" => ExtractXlsx(filePath),
        ".pptx" => ExtractPptx(filePath),
        ".pdf" => ExtractPdf(filePath),
        ".png" or ".jpg" or ".jpeg" or ".tiff" or ".bmp" or ".webp" => ExtractImage(filePath),
        _ => new ExtractionResult { Error = $"Unsupported: {ext}" }
    };

    private ExtractionResult ExtractCsv(string filePath, char delimiter)
    {
        var config = new CsvConfiguration(System.Globalization.CultureInfo.InvariantCulture)
        {
            Delimiter = delimiter.ToString(),
            HasHeaderRecord = false,
        };
        using var reader = new StreamReader(filePath, Encoding.UTF8);
        using var csv = new CsvReader(reader, config);
        var rows = new List<string>();
        while (csv.Read())
        {
            var fields = new List<string>();
            for (var i = 0; csv.TryGetField<string>(i, out var val); i++)
                fields.Add(val ?? string.Empty);
            rows.Add(string.Join(delimiter == '\t' ? "\t" : ", ", fields));
        }
        return new ExtractionResult { Text = string.Join("\n", rows) };
    }

    private ExtractionResult ExtractEml(string filePath)
    {
        var opts = new MimeKit.ParserOptions { RespectContentLength = false };
        MimeMessage msg;
        try
        {
            msg = MimeMessage.Load(opts, filePath);
        }
        catch (FormatException)
        {
            return new ExtractionResult { Text = File.ReadAllText(filePath) };
        }

        var parts = new List<string>
        {
            $"From: {msg.From}",
            $"To: {msg.To}",
            $"Subject: {msg.Subject}",
            $"Date: {msg.Date}"
        };

        var body = msg.TextBody ?? msg.HtmlBody;
        if (!string.IsNullOrWhiteSpace(body))
            parts.Add(NormalizeText(body));

        var images = new List<string>();

        foreach (var attachment in msg.Attachments.OfType<MimePart>())
        {
            var fileName = attachment.FileName ?? "attachment";
            var ext = Path.GetExtension(fileName).ToLowerInvariant();
            var tmp = Path.Combine(Path.GetTempPath(), $"omni_{Guid.NewGuid():N}{ext}");
            try
            {
                using (var fs = File.Create(tmp))
                    attachment.Content.DecodeTo(fs);

                var extracted = NativeExtensions.Contains(ext)
                    ? ExtractNative(tmp, ext)
                    : new ExtractionResult { Text = TryReadText(tmp) };

                if (!string.IsNullOrWhiteSpace(extracted.Text))
                    parts.Add($"[Attachment: {fileName}]\n{extracted.Text}");
                if (extracted.Images.Count > 0)
                    images.AddRange(extracted.Images);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to extract EML attachment {Name}", fileName);
                parts.Add($"[Attachment: {fileName} — extraction failed]");
            }
            finally
            {
                TryDelete(tmp);
            }
        }

        return new ExtractionResult { Text = string.Join("\n\n", parts), Images = images };
    }

    private static string TryReadText(string path)
    {
        try { return ReadTextFile(path); } catch { return string.Empty; }
    }

    private static ExtractionResult ExtractDocx(string filePath)
    {
        using var document = WordprocessingDocument.Open(filePath, false);
        var body = document.MainDocumentPart?.Document?.Body;
        if (body is null) return new ExtractionResult();

        var lines = body.Elements<Paragraph>()
            .Select(p => string.Concat(p.Descendants<WordText>().Select(t => t.Text)))
            .ToList();

        foreach (var table in body.Elements<WordTable>())
        {
            foreach (var row in table.Elements<TableRow>())
            {
                var cells = row.Elements<TableCell>()
                    .Select(c => string.Join(" ", c.Elements<Paragraph>()
                        .Select(p => string.Concat(p.Descendants<WordText>().Select(t => t.Text)))).Trim());
                lines.Add(string.Join(" | ", cells));
            }
        }

        return new ExtractionResult { Text = string.Join("\n", lines) };
    }

    private static ExtractionResult ExtractXlsx(string filePath)
    {
        using var workbook = new ClosedXML.Excel.XLWorkbook(filePath);
        var rows = new List<string>();
        foreach (var ws in workbook.Worksheets)
        {
            rows.Add($"[Sheet: {ws.Name}]");
            var range = ws.RangeUsed();
            if (range is null) continue;
            foreach (var row in range.RowsUsed())
            {
                var cells = row.CellsUsed().Select(c => c.Value.ToString() ?? string.Empty);
                rows.Add(string.Join(", ", cells));
            }
        }
        return new ExtractionResult { Text = string.Join("\n", rows) };
    }

    private static ExtractionResult ExtractPptx(string filePath)
    {
        using var doc = PresentationDocument.Open(filePath, false);
        var presentationPart = doc.PresentationPart;
        var slideIdList = presentationPart?.Presentation?.SlideIdList;
        if (presentationPart is null || slideIdList is null) return new ExtractionResult();

        var parts = new List<string>();
        var index = 1;

        foreach (var slideId in slideIdList.Elements<SlideId>())
        {
            parts.Add($"[Slide {index}]");
            if (slideId.RelationshipId?.Value is { } relId &&
                presentationPart.GetPartById(relId) is SlidePart slidePart)
            {
                foreach (var shape in slidePart.Slide?.Descendants<PptShape>() ?? [])
                {
                    var text = string.Concat(shape.TextBody?.Descendants<A.Text>().Select(t => t.Text) ?? []);
                    if (!string.IsNullOrWhiteSpace(text))
                        parts.Add(text);
                }

                // Notes
                var notesText = slidePart.NotesSlidePart?.NotesSlide is { } notes
                    ? string.Concat(notes.Descendants<A.Text>().Select(t => t.Text))
                    : null;
                if (!string.IsNullOrWhiteSpace(notesText))
                    parts.Add($"[Notes] {notesText}");
            }
            index++;
        }

        return new ExtractionResult { Text = string.Join("\n", parts) };
    }

    private ExtractionResult ExtractPdf(string filePath)
    {
        _logger.LogInformation("PDF: opening {File}", Path.GetFileName(filePath));
        using var document = PdfDocument.Open(filePath);
        var pages = document.GetPages().ToList();
        var texts = new List<string>();

        foreach (var page in pages)
        {
            try { texts.Add((page.Text ?? string.Empty).Trim()); }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "PDF page {N} extraction error", page.Number);
            }
        }

        var fullText = string.Join("\n", texts).Trim();

        if (!string.IsNullOrWhiteSpace(fullText) && !IsGibberish(fullText))
        {
            _logger.LogInformation("PDF: text OK ({Len} chars)", fullText.Length);
            return new ExtractionResult { Text = fullText };
        }

        _logger.LogInformation("PDF: falling back to vision ({Pages} pages)", pages.Count);
        return RenderPdfToImages(filePath, pages.Count);
    }

    private ExtractionResult RenderPdfToImages(string filePath, int pageCount)
    {
        var fileBytes = File.ReadAllBytes(filePath);
        var images = new List<string>();

        using var docReader = DocLib.Instance.GetDocReader(fileBytes, new PageDimensions(_settings.PdfDpi / 72d));
        for (var i = 0; i < pageCount; i++)
        {
            using var pageReader = docReader.GetPageReader(i);
            using var image = Image.LoadPixelData<Bgra32>(
                pageReader.GetImage(), pageReader.GetPageWidth(), pageReader.GetPageHeight());
            using var stream = new MemoryStream();
            image.SaveAsPng(stream);
            images.Add(ResizeForVision(stream.ToArray()));
        }

        return new ExtractionResult { Images = images };
    }

    private ExtractionResult ExtractImage(string filePath) =>
        new() { Images = [ResizeForVision(File.ReadAllBytes(filePath))] };

    public static bool IsGibberish(string text)
    {
        if (string.IsNullOrWhiteSpace(text)) return true;

        var total = 0;
        var readable = 0;
        foreach (var ch in text)
        {
            if (char.IsWhiteSpace(ch)) continue;
            total++;
            var cat = char.GetUnicodeCategory(ch);
            if (cat is System.Globalization.UnicodeCategory.UppercaseLetter
                    or System.Globalization.UnicodeCategory.LowercaseLetter
                    or System.Globalization.UnicodeCategory.TitlecaseLetter
                    or System.Globalization.UnicodeCategory.OtherLetter
                    or System.Globalization.UnicodeCategory.DecimalDigitNumber
                    or System.Globalization.UnicodeCategory.OtherNumber
                    or System.Globalization.UnicodeCategory.CurrencySymbol
                    or System.Globalization.UnicodeCategory.OtherPunctuation
                    or System.Globalization.UnicodeCategory.ConnectorPunctuation
                    or System.Globalization.UnicodeCategory.DashPunctuation
                    or System.Globalization.UnicodeCategory.OpenPunctuation
                    or System.Globalization.UnicodeCategory.ClosePunctuation
                    or System.Globalization.UnicodeCategory.MathSymbol)
                readable++;
        }

        if (total == 0) return true;
        if ((double)readable / total < 0.60) return true;

        var words = text.Split((char[]?)null, StringSplitOptions.RemoveEmptyEntries);
        if (words.Length > 5 && words.Sum(w => w.Length) / (double)words.Length < 2.5)
            return true;

        return false;
    }

    private string ResizeForVision(byte[] imageBytes)
    {
        const int MaxBytes = 4_000_000;
        const int TargetMinDim = 2048; // upscale small images so fine text is readable

        using var image = Image.Load(imageBytes);

        // Upscale if the image is smaller than TargetMinDim on its longest edge
        var longest = Math.Max(image.Width, image.Height);
        if (longest < TargetMinDim)
        {
            var scale = TargetMinDim / (double)longest;
            image.Mutate(ctx => ctx.Resize(
                new ResizeOptions
                {
                    Size = new Size(
                        Math.Max(1, (int)(image.Width * scale)),
                        Math.Max(1, (int)(image.Height * scale))),
                    Sampler = KnownResamplers.Lanczos3,
                    Mode = ResizeMode.Stretch
                }));
        }

        // Sharpen + contrast boost — makes small numbers crisper for vision model
        image.Mutate(ctx => ctx
            .GaussianSharpen(0.8f)
            .Contrast(0.15f));

        using var stream = new MemoryStream();
        image.SaveAsPng(stream);
        if (stream.Length <= MaxBytes)
            return Convert.ToBase64String(stream.ToArray());

        // Fallback: cap at 4096px and use JPEG if still too large
        stream.SetLength(0);
        var scale2 = 4096.0 / Math.Max(image.Width, image.Height);
        if (scale2 < 1.0)
            image.Mutate(ctx => ctx.Resize(
                Math.Max(1, (int)(image.Width * scale2)),
                Math.Max(1, (int)(image.Height * scale2))));

        image.SaveAsJpeg(stream, new SixLabors.ImageSharp.Formats.Jpeg.JpegEncoder { Quality = 90 });
        return Convert.ToBase64String(stream.ToArray());
    }

    private static string ReadTextFile(string filePath)
    {
        using var reader = new StreamReader(filePath, Encoding.UTF8, detectEncodingFromByteOrderMarks: true);
        return reader.ReadToEnd();
    }

    private static string NormalizeText(string text)
    {
        var lines = text.Replace("\r\n", "\n").Replace('\r', '\n')
            .Split('\n').Select(l => l.TrimEnd());
        return string.Join("\n", lines).Trim();
    }

    private static void TryDelete(string path)
    {
        try { File.Delete(path); } catch { }
    }
}
