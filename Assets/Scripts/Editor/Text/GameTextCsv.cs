#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace ALWTTT.TextAuthoring
{
    /// <summary>
    /// [TXT-1 / D1=A] Minimal RFC-4180 CSV codec for the Game Text window.
    ///
    /// The CSV is an INTERCHANGE format, never a source of truth: the .asset files
    /// remain authoritative (D1=A), and nothing at runtime reads any CSV (D-TXT-1).
    ///
    /// Shape ("long" table, one row per text field):
    ///   id, field, &lt;lang1&gt;, &lt;lang2&gt;, ...
    ///   tut_jam_welcome, revisitTitle, "Welcome", "Bienvenido"
    ///   tut_jam_welcome, page_1,       "...",     "..."
    ///   tut_jam_welcome, page_2,       ,          "..."      ← EN has one page, ES two
    ///
    /// Rules:
    /// - Delimiter is configurable on write (',' ';' or TAB — Spanish-locale Excel
    ///   expects ';'); on read it is sniffed from the header line.
    /// - Any cell may contain newlines, quotes and the delimiter when quoted.
    ///   Quotes inside a cell are doubled ("").
    /// - Written as UTF-8 WITH BOM so Excel opens accents correctly; the BOM is
    ///   stripped on read.
    /// - Record terminator on write is CRLF (RFC-4180). On read both CRLF and LF
    ///   terminate a record outside quotes. CRLF INSIDE a quoted cell is normalised
    ///   to LF on read (spreadsheets rewrite newlines; a page authored with LF must
    ///   not register as "changed" after a sheet round-trip).
    /// </summary>
    internal static class GameTextCsv
    {
        public const string ColId = "id";
        public const string ColField = "field";
        public const string FieldTitle = "revisitTitle";
        public const string FieldPagePrefix = "page_";

        public static readonly char[] SupportedDelimiters = { ',', ';', '\t' };
        public static readonly string[] DelimiterLabels = { "Comma (,)", "Semicolon (;)", "Tab" };

        public sealed class Table
        {
            public List<string> Header = new();
            public List<string[]> Rows = new();
        }

        // ──────────────────────────────────────────────────────────────────
        // Write
        // ──────────────────────────────────────────────────────────────────
        public static void Write(string path, Table table, char delimiter)
        {
            var sb = new StringBuilder();
            AppendRecord(sb, table.Header, delimiter);
            foreach (var row in table.Rows) AppendRecord(sb, row, delimiter);
            // UTF8Encoding(true) emits the BOM.
            File.WriteAllText(path, sb.ToString(), new UTF8Encoding(true));
        }

        private static void AppendRecord(StringBuilder sb, IReadOnlyList<string> cells, char delimiter)
        {
            for (int i = 0; i < cells.Count; i++)
            {
                if (i > 0) sb.Append(delimiter);
                sb.Append(Quote(cells[i] ?? string.Empty, delimiter));
            }
            sb.Append("\r\n");
        }

        private static string Quote(string cell, char delimiter)
        {
            bool needs = cell.IndexOf(delimiter) >= 0 || cell.IndexOf('"') >= 0 ||
                         cell.IndexOf('\n') >= 0 || cell.IndexOf('\r') >= 0 ||
                         (cell.Length > 0 && (cell[0] == ' ' || cell[cell.Length - 1] == ' '));
            if (!needs) return cell;
            return "\"" + cell.Replace("\"", "\"\"") + "\"";
        }

        // ──────────────────────────────────────────────────────────────────
        // Read
        // ──────────────────────────────────────────────────────────────────
        /// <summary>Reads a CSV file. Throws FormatException on malformed quoting.</summary>
        public static Table Read(string path)
        {
            // Encoding.UTF8 strips a leading BOM.
            string text = File.ReadAllText(path, Encoding.UTF8);
            if (text.Length > 0 && text[0] == '\uFEFF') text = text.Substring(1);
            char delimiter = SniffDelimiter(text);
            return Parse(text, delimiter);
        }

        /// <summary>Picks the supported delimiter that occurs most often in the first line.</summary>
        public static char SniffDelimiter(string text)
        {
            int nl = text.IndexOf('\n');
            string first = nl < 0 ? text : text.Substring(0, nl);
            char best = ',';
            int bestCount = -1;
            foreach (char d in SupportedDelimiters)
            {
                int c = 0;
                foreach (char ch in first) if (ch == d) c++;
                if (c > bestCount) { bestCount = c; best = d; }
            }
            return best;
        }

        public static Table Parse(string text, char delimiter)
        {
            var table = new Table();
            var cells = new List<string>();
            var cell = new StringBuilder();
            bool inQuotes = false;
            bool rowHasContent = false;
            int i = 0;

            void EndCell()
            {
                cells.Add(cell.ToString());
                cell.Clear();
            }

            void EndRow()
            {
                EndCell();
                if (rowHasContent || cells.Count > 1)
                {
                    if (table.Header.Count == 0) table.Header.AddRange(cells);
                    else table.Rows.Add(cells.ToArray());
                }
                cells.Clear();
                rowHasContent = false;
            }

            while (i < text.Length)
            {
                char c = text[i];
                if (inQuotes)
                {
                    if (c == '"')
                    {
                        if (i + 1 < text.Length && text[i + 1] == '"') { cell.Append('"'); i += 2; continue; }
                        inQuotes = false; i++; continue;
                    }
                    if (c == '\r' && i + 1 < text.Length && text[i + 1] == '\n')
                    {
                        cell.Append('\n'); i += 2; continue; // CRLF inside a cell → LF
                    }
                    cell.Append(c); i++; continue;
                }

                if (c == '"')
                {
                    if (cell.Length != 0)
                        throw new FormatException($"Unexpected quote at offset {i} (quote must start the cell).");
                    inQuotes = true; rowHasContent = true; i++; continue;
                }
                if (c == delimiter) { EndCell(); rowHasContent = true; i++; continue; }
                if (c == '\r') { i++; continue; }   // CR outside quotes is ignored; LF ends the record
                if (c == '\n') { EndRow(); i++; continue; }
                cell.Append(c); rowHasContent = true; i++;
            }

            if (inQuotes) throw new FormatException("Unterminated quoted cell at end of file.");
            if (cell.Length > 0 || cells.Count > 0) EndRow();
            return table;
        }

        // ──────────────────────────────────────────────────────────────────
        // Helpers shared by the window
        // ──────────────────────────────────────────────────────────────────
        /// <summary>page_3 → 3; anything else → -1.</summary>
        public static int ParsePageIndex(string field)
        {
            if (string.IsNullOrEmpty(field) || !field.StartsWith(FieldPagePrefix, StringComparison.Ordinal))
                return -1;
            return int.TryParse(field.Substring(FieldPagePrefix.Length), out int n) && n >= 1 ? n : -1;
        }

        public static string PageField(int oneBasedIndex) => FieldPagePrefix + oneBasedIndex;
    }
}
#endif