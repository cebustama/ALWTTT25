using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using Object = UnityEngine.Object;

namespace ALWTTT.DevLogs
{
    /// <summary>
    /// [S10] Central Console filter. Wraps Debug.unityLogger.logHandler before the first scene
    /// loads and drops the Debug.Log lines whose text matches a hidden category of
    /// LogFilterSettingsSO. No logging call site is touched: hardcoded logs are silenced the
    /// same way as the ones with their own toggle. Errors and exceptions always pass;
    /// warnings only drop for categories with includeWarnings.
    /// Only messages routed through Debug.* are seen; native engine warnings are not.
    /// </summary>
    public static class LogFilter
    {
        private static FilteringHandler _handler;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void Install()
        {
            var settings = Resources.Load<LogFilterSettingsSO>(LogFilterSettingsSO.ResourcePath);
            var current = Debug.unityLogger.logHandler;

            // Enter Play Mode without a domain reload keeps the old wrapper: unwrap it first.
            var inner = current is FilteringHandler old ? old.Inner : current;
            _handler = new FilteringHandler(inner, settings);
            Debug.unityLogger.logHandler = _handler;

            Application.quitting -= OnQuit;
            Application.quitting += OnQuit;

            inner.LogFormat(LogType.Log, null, "{0}",
                $"[S10] Filtro de logs {(_handler.Active ? "activo" : "apagado")} | " +
                $"{(settings != null ? "asset LogFilterSettings" : "valores por defecto (sin asset)")} | " +
                $"ocultas: {_handler.HiddenNames()}");
        }

        private static void OnQuit()
        {
            if (_handler == null) return;
            if (_handler.SummaryOnExit && _handler.Active)
                _handler.Inner.LogFormat(LogType.Log, null, "{0}", "[S10] Logs ocultados | " + _handler.Summary());
            Debug.unityLogger.logHandler = _handler.Inner;
            _handler = null;
        }

        private sealed class FilteringHandler : ILogHandler
        {
            public readonly ILogHandler Inner;
            public readonly bool Active;
            public readonly bool SummaryOnExit;

            private readonly List<LogFilterSettingsSO.Category> _hidden = new List<LogFilterSettingsSO.Category>();
            private readonly string[] _always;
            private readonly string[] _focus;
            private readonly Dictionary<string, int> _counts = new Dictionary<string, int>();

            public FilteringHandler(ILogHandler inner, LogFilterSettingsSO s)
            {
                Inner = inner;
                Active = s == null || s.filterEnabled;
                SummaryOnExit = s == null || s.summaryOnExit;
                _always = s != null ? s.alwaysShow ?? new string[0] : new[] { "[S10]" };
                _focus = s != null ? s.focusOnly ?? new string[0] : new string[0];
                var cats = s != null && s.categories != null ? s.categories : LogFilterSettingsSO.Defaults();
                foreach (var c in cats)
                    if (c != null && !c.show && c.patterns != null && c.patterns.Length > 0) _hidden.Add(c);
            }

            public string HiddenNames()
            {
                if (!Active) return "-";
                if (_focus.Length > 0) return "todo salvo foco (" + string.Join(", ", _focus) + ")";
                var n = new List<string>();
                foreach (var c in _hidden) n.Add(c.name);
                return n.Count == 0 ? "ninguna" : string.Join(" · ", n);
            }

            public string Summary()
            {
                lock (_counts)
                {
                    if (_counts.Count == 0) return "ninguno";
                    var parts = new List<string>();
                    foreach (var kv in _counts) parts.Add($"{kv.Key} {kv.Value}");
                    return string.Join(" · ", parts);
                }
            }

            public void LogException(Exception exception, Object context) => Inner.LogException(exception, context);

            public void LogFormat(LogType logType, Object context, string format, params object[] args)
            {
                if (Active && (logType == LogType.Log || logType == LogType.Warning))
                {
                    string text;
                    try { text = args == null || args.Length == 0 ? format : string.Format(format, args); }
                    catch (FormatException) { text = format; }

                    string drop = Classify(StripTags(text), logType);
                    if (drop != null)
                    {
                        lock (_counts)          // MPTK logs from its own threads
                        {
                            _counts.TryGetValue(drop, out int n);
                            _counts[drop] = n + 1;
                        }
                        return;
                    }
                }
                Inner.LogFormat(logType, context, format, args);
            }

            // Returns the category name to drop under, or null to show.
            private string Classify(string text, LogType type)
            {
                if (ContainsAny(text, _always)) return null;

                if (_focus.Length > 0)
                    return type == LogType.Log && !ContainsAny(text, _focus) ? "fuera del foco" : null;

                foreach (var c in _hidden)
                {
                    if (type == LogType.Warning && !c.includeWarnings) continue;
                    if (ContainsAny(text, c.patterns)) return c.name;
                }
                return null;
            }

            private static bool ContainsAny(string text, string[] patterns)
            {
                if (patterns == null) return false;
                for (int i = 0; i < patterns.Length; i++)
                {
                    var p = patterns[i];
                    if (!string.IsNullOrEmpty(p) && text.IndexOf(p, StringComparison.Ordinal) >= 0) return true;
                }
                return false;
            }

            // Removes rich-text tags such as <color=...> so patterns match the visible text.
            private static string StripTags(string s)
            {
                if (string.IsNullOrEmpty(s) || s.IndexOf('<') < 0) return s ?? string.Empty;
                var _sb = new StringBuilder(s.Length);
                bool inTag = false;
                for (int i = 0; i < s.Length; i++)
                {
                    char ch = s[i];
                    if (ch == '<') { inTag = true; continue; }
                    if (ch == '>' && inTag) { inTag = false; continue; }
                    if (!inTag) _sb.Append(ch);
                }
                return _sb.ToString();
            }
        }
    }
}
