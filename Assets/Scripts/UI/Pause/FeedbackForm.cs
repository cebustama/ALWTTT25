using System;
using System.Collections.Generic;
using System.Text;
using ALWTTT.Managers;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.SceneManagement;

namespace ALWTTT.UI
{
    /// <summary>
    /// [FEEDBACK-2] Where in-game feedback goes: a Google Form, posted to its public
    /// /formResponse endpoint. No account, no password, no API key in the build — the form
    /// id and the entry.N numbers are public (anyone who opens the form can read them), so
    /// they can ship. Bird Boxing (2020) sent mail over SMTP with an account password in the
    /// code; that is exactly what this replaces.
    ///
    /// Configuration is ONE string: the form's pre-filled link (Form ▸ ⋮ ▸ "Get pre-filled
    /// link"), filled with the placeholder letters below. Each entry.N is recognised by its
    /// letter, so the question order in the form does not matter.
    /// </summary>
    public static class FeedbackForm
    {
        public const string Tag = "[FEEDBACK-2]";

        /// <summary>Pre-filled link with M/V/S/E/B/G as answers. Empty = not configured.</summary>
        public const string PrefilledLink =
            "https://docs.google.com/forms/d/e/1FAIpQLSe1iQF4yjArhNkk8CBazF9L-AIu-jFR4f7GvatQxubIyhIMSw/viewform?usp=pp_url" +
            "&entry.1812581064=M&entry.1507765757=V&entry.1559679973=S" +
            "&entry.2030811408=E&entry.634499403=B&entry.149270797=G";

        // Placeholder letter in the pre-filled link -> field.
        public const string MessageKey = "M";
        public const string VersionKey = "V";
        public const string SystemKey = "S";
        public const string SceneKey = "E";
        public const string BandKey = "B";
        public const string GigKey = "G";

        private static bool _parsed;
        private static string _postUrl;
        private static readonly Dictionary<string, string> _entryByKey = new Dictionary<string, string>();

        public static bool IsConfigured { get { Parse(); return _postUrl != null && _entryByKey.ContainsKey(MessageKey); } }
        public static string PostUrl { get { Parse(); return _postUrl; } }

        /// <summary>Human-readable parse result (editor check menu and the first send).</summary>
        public static string Describe()
        {
            Parse();
            if (_postUrl == null) return "sin configurar (PrefilledLink vacío o sin /viewform)";
            var sb = new StringBuilder(_postUrl);
            foreach (var key in new[] { MessageKey, VersionKey, SystemKey, SceneKey, BandKey, GigKey })
                sb.Append($" | {key}={(_entryByKey.TryGetValue(key, out var e) ? e : "FALTA")}");
            return sb.ToString();
        }

        private static void Parse()
        {
            if (_parsed) return;
            _parsed = true;
            _postUrl = null;
            _entryByKey.Clear();
            if (string.IsNullOrWhiteSpace(PrefilledLink)) return;

            string link = PrefilledLink.Trim();
            int q = link.IndexOf('?');
            string path = q >= 0 ? link.Substring(0, q) : link;
            int view = path.LastIndexOf("/viewform", StringComparison.Ordinal);
            if (view < 0) return;
            _postUrl = path.Substring(0, view) + "/formResponse";

            if (q < 0) return;
            foreach (var pair in link.Substring(q + 1).Split('&'))
            {
                int eq = pair.IndexOf('=');
                if (eq <= 0) continue;
                string name = pair.Substring(0, eq);
                if (!name.StartsWith("entry.", StringComparison.Ordinal)) continue;
                string value = UnityWebRequest.UnEscapeURL(pair.Substring(eq + 1)).Trim();
                _entryByKey[value] = name;
            }
        }

        /// <summary>
        /// [D1=B] What travels with the message: version, OS, scene, band and gig. Nothing
        /// else — no name, no account, no files. The privacy line in the panel says the same.
        /// </summary>
        public static Dictionary<string, string> BuildContext()
        {
            var ctx = new Dictionary<string, string>
            {
                [VersionKey] = Application.version,
                [SystemKey] = SystemInfo.operatingSystem,
                [SceneKey] = SceneManager.GetActiveScene().name,
                [BandKey] = "-",
                [GigKey] = "-",
            };

            var pd = GameManager.Instance != null ? GameManager.Instance.PersistentGameplayData : null;
            var gm = GigManager.Instance;

            string lineup = pd != null && !string.IsNullOrEmpty(pd.ChosenLineupId) ? pd.ChosenLineupId : "config";
            if (gm != null && gm.CurrentMusicianCharacterList != null && gm.CurrentMusicianCharacterList.Count > 0)
            {
                var ids = new List<string>();
                foreach (var m in gm.CurrentMusicianCharacterList)
                    if (m != null && m.MusicianCharacterData != null) ids.Add(m.MusicianCharacterData.CharacterId);
                ctx[BandKey] = $"{lineup} ({string.Join(", ", ids)})";
            }
            else if (pd != null)
            {
                ctx[BandKey] = lineup;
            }

            var encounter = pd != null && pd.CurrentEncounter != null ? pd.CurrentEncounter
                : gm != null ? gm.CurrentGigEncounter : null;
            if (encounter != null)
                ctx[GigKey] = $"{encounter.GetLabel()} · songIndex={(pd != null ? pd.CurrentSongIndex : -1)}";

            return ctx;
        }

        /// <summary>The form body: message plus context, each under its entry.N.</summary>
        public static WWWForm BuildForm(string message, Dictionary<string, string> context)
        {
            Parse();
            var form = new WWWForm();
            if (_entryByKey.TryGetValue(MessageKey, out var msgEntry)) form.AddField(msgEntry, message);
            foreach (var kv in context)
                if (_entryByKey.TryGetValue(kv.Key, out var entry)) form.AddField(entry, kv.Value ?? "");
            return form;
        }

        /// <summary>One line for the log: the context, never the message text.</summary>
        public static string ContextLine(Dictionary<string, string> c) =>
            $"version={c[VersionKey]} os=\"{c[SystemKey]}\" scene={c[SceneKey]} band=\"{c[BandKey]}\" gig=\"{c[GigKey]}\"";

#if UNITY_EDITOR
        // Enter-Play-Mode without domain reload: re-parse after an edit to the link.
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ResetOnLoad() => _parsed = false;
#endif
    }
}
