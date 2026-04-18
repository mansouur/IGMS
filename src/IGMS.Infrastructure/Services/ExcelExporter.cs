using ClosedXML.Excel;

namespace IGMS.Infrastructure.Services;

/// <summary>
/// أداة مركزية لتوليد ملفات Excel.
/// كل وحدة تمرر العناوين والصفوف، وهذه الأداة تبني الملف.
/// </summary>
public static class ExcelExporter
{
    /// <summary>
    /// يبني workbook بورقة واحدة ويُرجعه كـ byte array جاهز للإرسال.
    /// </summary>
    public static byte[] Build(string sheetName, IEnumerable<string> headers, IEnumerable<IEnumerable<object?>> rows)
    {
        using var wb = new XLWorkbook();
        var ws = wb.Worksheets.Add(sheetName);

        // ── Header row ────────────────────────────────────────────────────────
        var headerList = headers.ToList();
        for (int col = 1; col <= headerList.Count; col++)
        {
            var cell = ws.Cell(1, col);
            cell.Value = headerList[col - 1];
            cell.Style.Font.Bold = true;
            cell.Style.Fill.BackgroundColor = XLColor.FromArgb(0x1F, 0x69, 0x37); // green-700
            cell.Style.Font.FontColor = XLColor.White;
            cell.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
        }

        // ── Data rows ─────────────────────────────────────────────────────────
        int rowNum = 2;
        foreach (var row in rows)
        {
            int col = 1;
            foreach (var value in row)
            {
                var cell = ws.Cell(rowNum, col);
                if (value is null)
                    cell.Value = string.Empty;
                else if (value is DateTime dt)
                    cell.Value = dt.ToString("yyyy-MM-dd");
                else if (value is bool b)
                    cell.Value = b ? "نعم" : "لا";
                else
                    cell.Value = value.ToString() ?? string.Empty;

                // Zebra striping
                if (rowNum % 2 == 0)
                    cell.Style.Fill.BackgroundColor = XLColor.FromArgb(0xF8, 0xFA, 0xFC);

                col++;
            }
            rowNum++;
        }

        // ── Auto-fit columns ──────────────────────────────────────────────────
        ws.Columns().AdjustToContents();

        // ── Freeze header row ─────────────────────────────────────────────────
        ws.SheetView.FreezeRows(1);

        using var ms = new MemoryStream();
        wb.SaveAs(ms);
        return ms.ToArray();
    }
}
