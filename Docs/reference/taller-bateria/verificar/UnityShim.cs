// Minimal stand-ins so the real MidiGenPlay files compile outside Unity. No parser logic lives here.
using System;

namespace UnityEngine
{
    public class ScriptableObject
    {
        public string name;
        public static T CreateInstance<T>() where T : new() => new T();
    }

    public static class Mathf
    {
        public static int Clamp(int v, int lo, int hi) => v < lo ? lo : (v > hi ? hi : v);
        public static int Max(int a, int b) => a > b ? a : b;
    }

    public static class Random { public static float value => 0f; public static int Range(int a, int b) => a; public static float Range(float a, float b) => a; }

    public class MinAttribute : Attribute { public MinAttribute(float v) { } }
    public class RangeAttribute : Attribute { public RangeAttribute(float a, float b) { } }
    public class HeaderAttribute : Attribute { public HeaderAttribute(string s) { } }
    public class TextAreaAttribute : Attribute { public TextAreaAttribute(int a = 0, int b = 0) { } }
    public class TooltipAttribute : Attribute { public TooltipAttribute(string s) { } }
    public class SerializeField : Attribute { }
    public class CreateAssetMenuAttribute : Attribute { public string menuName, fileName; }
}
