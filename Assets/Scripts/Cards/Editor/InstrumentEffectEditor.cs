#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

namespace ALWTTT.Cards.Editor
{
    /// <summary>
    /// AUTH-1b (D-AUTH1-4=A): mode-conditional inspector for InstrumentEffect.
    ///
    /// The default inspector draws every serialized field regardless of `mode`,
    /// so the RandomFromList pool (and the two unused instrument slots) were
    /// always visible, which reads as "these all apply". This drawer shows only
    /// the fields the selected mode actually consumes.
    ///
    /// Applies in BOTH the standard Inspector and PartEffectEditorWindow's
    /// inline panel (the window uses Editor.CreateEditor, which resolves custom
    /// editors). Editor-only, presentation-only: no serialized data changes and
    /// no runtime behaviour change — the hidden fields keep their values.
    /// </summary>
    [CustomEditor(typeof(InstrumentEffect))]
    public sealed class InstrumentEffectEditor : UnityEditor.Editor
    {
        private SerializedProperty _scope;
        private SerializedProperty _timing;
        private SerializedProperty _mode;
        private SerializedProperty _melodicInstrument;
        private SerializedProperty _percussionInstrument;
        private SerializedProperty _instrumentType;
        private SerializedProperty _melodicInstrumentPool;

        private void OnEnable()
        {
            _scope = serializedObject.FindProperty("scope");
            _timing = serializedObject.FindProperty("timing");
            _mode = serializedObject.FindProperty("mode");
            _melodicInstrument = serializedObject.FindProperty("melodicInstrument");
            _percussionInstrument = serializedObject.FindProperty("percussionInstrument");
            _instrumentType = serializedObject.FindProperty("instrumentType");
            _melodicInstrumentPool = serializedObject.FindProperty("melodicInstrumentPool");
        }

        public override void OnInspectorGUI()
        {
            // Defensive: if any field is renamed, fall back to the default
            // inspector rather than silently hiding data.
            if (_mode == null || _melodicInstrument == null ||
                _percussionInstrument == null || _instrumentType == null ||
                _melodicInstrumentPool == null)
            {
                EditorGUILayout.HelpBox(
                    "InstrumentEffect field names changed; showing the default inspector.",
                    MessageType.Warning);
                DrawDefaultInspector();
                return;
            }

            serializedObject.Update();

            if (_scope != null) EditorGUILayout.PropertyField(_scope);
            if (_timing != null) EditorGUILayout.PropertyField(_timing);

            EditorGUILayout.Space(4);
            EditorGUILayout.PropertyField(_mode);

            EditorGUI.indentLevel++;

            switch ((InstrumentEffect.InstrumentTargetMode)_mode.enumValueIndex)
            {
                case InstrumentEffect.InstrumentTargetMode.SpecificMelodic:
                    EditorGUILayout.PropertyField(
                        _melodicInstrument, new GUIContent("Melodic Instrument"));
                    if (_melodicInstrument.objectReferenceValue == null)
                        EditorGUILayout.HelpBox(
                            "SpecificMelodic requires a MIDIInstrumentSO.",
                            MessageType.Warning);
                    break;

                case InstrumentEffect.InstrumentTargetMode.SpecificPercussion:
                    EditorGUILayout.PropertyField(
                        _percussionInstrument, new GUIContent("Percussion Instrument"));
                    if (_percussionInstrument.objectReferenceValue == null)
                        EditorGUILayout.HelpBox(
                            "SpecificPercussion requires a MIDIPercussionInstrumentSO.",
                            MessageType.Warning);
                    break;

                case InstrumentEffect.InstrumentTargetMode.InstrumentType:
                    EditorGUILayout.PropertyField(
                        _instrumentType, new GUIContent("Instrument Type"));
                    break;

                case InstrumentEffect.InstrumentTargetMode.RandomFromList:
                    EditorGUILayout.PropertyField(
                        _melodicInstrumentPool,
                        new GUIContent("Melodic Instrument Pool"),
                        includeChildren: true);

                    int usable = CountUsablePoolEntries();
                    if (usable == 0)
                    {
                        EditorGUILayout.HelpBox(
                            "Empty pool: this effect applies nothing and logs a " +
                            "warning at card-application time (R2c / D-R2-7).",
                            MessageType.Warning);
                    }
                    else if (usable == 1)
                    {
                        EditorGUILayout.HelpBox(
                            "Pool has a single usable entry — equivalent to " +
                            "SpecificMelodic, with no variety.",
                            MessageType.Info);
                    }
                    break;
            }

            EditorGUI.indentLevel--;

            EditorGUILayout.Space(4);
            using (new EditorGUI.DisabledScope(true))
            {
                var fx = (InstrumentEffect)target;
                string label;
                try { label = fx.GetLabel(); }
                catch { label = "<label error>"; }
                EditorGUILayout.TextField("Label", label);
            }

            serializedObject.ApplyModifiedProperties();
        }

        private int CountUsablePoolEntries()
        {
            int usable = 0;
            for (int i = 0; i < _melodicInstrumentPool.arraySize; i++)
            {
                var el = _melodicInstrumentPool.GetArrayElementAtIndex(i);
                if (el != null && el.objectReferenceValue != null) usable++;
            }
            return usable;
        }
    }
}
#endif