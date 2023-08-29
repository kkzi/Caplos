using System.Collections.Generic;
using System.IO;
using System.Linq;
using System;
using System.Windows.Forms;

using static CapsLockSharpPrototype.Helper.GlobalKeyboardHook;
using System.Text;

namespace CapsLockSharpPrototype.Helper
{

    public class KeyDefRuntime
    {
        public delegate void KeyHookFunction();

        public struct KeyDef
        {
            public ModifiedKey Modified;
            public List<VirtualKey> Keys;

            public KeyDef(ModifiedKey m, VirtualKey k)
            {
                this.Modified = m;
                this.Keys = new List<VirtualKey>() { k };
            }

            public KeyDef(ModifiedKey m, List<VirtualKey> karr)
            {
                this.Modified = m;
                this.Keys = karr;
            }
        }

        public struct KeyHookItem
        {
            public KeyDef Source;
            public List<KeyDef> Targets;
            public KeyHookFunction Func;

            public string SourceText;
            public string TargetText;
        }

        public static Dictionary<string, KeyHookFunction> NameToFunction = new Dictionary<string, KeyHookFunction>();

        public static string SourceKeyId(ModifiedKey modified, VirtualKey keycode)
        {
            return Convert.ToString((int)modified << 16 | (int)keycode);
        }

        private static void CreateConfigFileIfNotExists(string path)
        {
            if (File.Exists(path)) return;
            try
            {
                File.WriteAllBytes(path, Properties.Resources.Caplos);
            }
            catch (System.Exception)
            {
                Application.Exit();
            }
        }

        private static ModifiedKey StringToModified(string text)
        {
            ModifiedKey val = 0;
            val |= text.Contains("Ctrl") ? ModifiedKey.Ctrl : ModifiedKey.None;
            val |= text.Contains("Alt") ? ModifiedKey.Alt : ModifiedKey.None;
            val |= text.Contains("Shift") ? ModifiedKey.Shift : ModifiedKey.None;
            val |= text.Contains("Win") ? ModifiedKey.Win : ModifiedKey.None;
            return val;
        }

        private static List<KeyDef> StringToKeyDefs(string text)
        {
            var arr = new List<KeyDef>();
            foreach (var part in text.Split(new char[] { ',', ';' }, StringSplitOptions.RemoveEmptyEntries))
            {
                var trimmed = part.Trim();
                if (trimmed.Contains(' ')) trimmed = trimmed.Substring(trimmed.LastIndexOf(' '));
                var keys = new List<VirtualKey>();
                foreach (var it in trimmed.Split(new char[] { '|' }, StringSplitOptions.RemoveEmptyEntries))
                {
                    keys.Add((VirtualKey)Enum.Parse(typeof(VirtualKey), it, true));
                }
                arr.Add(new KeyDef(StringToModified(part), keys));
            }
            return arr;
        }

        public static IEnumerable<KeyHookItem> SetUpKeyDefs()
        {
            var configFilePath = Application.StartupPath + Path.DirectorySeparatorChar + "Caplos.cfg";
            string[] rawLines = File.Exists(configFilePath) ? File.ReadLines(configFilePath).ToArray() : Encoding.ASCII.GetString(Properties.Resources.Caplos).Split('\n');
            //CreateConfigFileIfNotExists(configFilePath);

            foreach (var line in rawLines)
            {
                var lineTrimed = line.Trim();
                if (lineTrimed.Equals(string.Empty) || lineTrimed.StartsWith("#") || !lineTrimed.Contains("=")) continue;

                var parts = lineTrimed.Split('=');
                if (parts.Length != 2) continue;

                var keyhook = new KeyHookItem();
                keyhook.SourceText = parts[0].Trim();
                keyhook.TargetText = parts[1].Trim();
                try
                {
                    keyhook.Source = StringToKeyDefs(keyhook.SourceText)[0];
                    if (NameToFunction.ContainsKey(keyhook.TargetText))
                    {
                        keyhook.Func = NameToFunction[keyhook.TargetText];
                    }
                    else
                    {
                        keyhook.Targets = StringToKeyDefs(keyhook.TargetText);
                    }
                    KeyToHook[SourceKeyId(keyhook.Source.Modified, keyhook.Source.Keys[0])] = keyhook;
                }
                catch (Exception e)
                {
                    Logger.Info(e.Message);
                    continue;
                }

                yield return keyhook;
            }
        }
        // List of Virtual Key Codes
        // http://www.kbdedit.com/manual/low_level_vk_list.html
        public static IEnumerable<KeyHookItem> KeyDefs;
        public static Dictionary<string, KeyHookItem> KeyToHook = new Dictionary<string, KeyHookItem>();
    }
}
