﻿using System.Collections.Generic;
using System.IO;
using System.Linq;

using System.Windows.Forms;
using static CapsLockSharpPrototype.Helper.GlobalKeyboardHook;

namespace CapsLockSharpPrototype.Helper
{
    public class KeyDefRuntime
    {
        public enum FuncType
        {
            Replace
        }

        public struct KeyDef
        {
            public VirtualKey AdditionKey;
            public VirtualKey ReplacingKey;
            public List<string> Param;
            public FuncType Type;

        }
        private static Dictionary<string, int> stringToVKDict = new Dictionary<string, int>() {
            {"VK_ABNT_C1",0xC1},
            {"VK_ABNT_C2",0xC2},
            {"VK_ADD",0x6B},
            {"VK_ATTN",0xF6},
            {"VK_BACK",0x08},
            {"VK_CANCEL",0x03},
            {"VK_CLEAR",0x0C},
            {"VK_CRSEL",0xF7},
            {"VK_DECIMAL",0x6E},
            {"VK_DIVIDE",0x6F},
            {"VK_EREOF",0xF9},
            {"VK_ESCAPE",0x1B},
            {"VK_EXECUTE",0x2B},
            {"VK_EXSEL",0xF8},
            {"VK_ICO_CLEAR",0xE6},
            {"VK_ICO_HELP",0xE3},
            {"VK_KEY_0",0x30},
            {"VK_KEY_1",0x31},
            {"VK_KEY_2",0x32},
            {"VK_KEY_3",0x33},
            {"VK_KEY_4",0x34},
            {"VK_KEY_5",0x35},
            {"VK_KEY_6",0x36},
            {"VK_KEY_7",0x37},
            {"VK_KEY_8",0x38},
            {"VK_KEY_9",0x39},
            {"VK_KEY_A",0x41},
            {"VK_KEY_B",0x42},
            {"VK_KEY_C",0x43},
            {"VK_KEY_D",0x44},
            {"VK_KEY_E",0x45},
            {"VK_KEY_F",0x46},
            {"VK_KEY_G",0x47},
            {"VK_KEY_H",0x48},
            {"VK_KEY_I",0x49},
            {"VK_KEY_J",0x4A},
            {"VK_KEY_K",0x4B},
            {"VK_KEY_L",0x4C},
            {"VK_KEY_M",0x4D},
            {"VK_KEY_N",0x4E},
            {"VK_KEY_O",0x4F},
            {"VK_KEY_P",0x50},
            {"VK_KEY_Q",0x51},
            {"VK_KEY_R",0x52},
            {"VK_KEY_S",0x53},
            {"VK_KEY_T",0x54},
            {"VK_KEY_U",0x55},
            {"VK_KEY_V",0x56},
            {"VK_KEY_W",0x57},
            {"VK_KEY_X",0x58},
            {"VK_KEY_Y",0x59},
            {"VK_KEY_Z",0x5A},
            {"VK_MULTIPLY",0x6A},
            {"VK_NONAME",0xFC},
            {"VK_NUMPAD0",0x60},
            {"VK_NUMPAD1",0x61},
            {"VK_NUMPAD2",0x62},
            {"VK_NUMPAD3",0x63},
            {"VK_NUMPAD4",0x64},
            {"VK_NUMPAD5",0x65},
            {"VK_NUMPAD6",0x66},
            {"VK_NUMPAD7",0x67},
            {"VK_NUMPAD8",0x68},
            {"VK_NUMPAD9",0x69},
            {"VK_OEM_1",0xBA},
            {"VK_OEM_102",0xE2},
            {"VK_OEM_2",0xBF},
            {"VK_OEM_3",0xC0},
            {"VK_OEM_4",0xDB},
            {"VK_OEM_5",0xDC},
            {"VK_OEM_6",0xDD},
            {"VK_OEM_7",0xDE},
            {"VK_OEM_8",0xDF},
            {"VK_OEM_ATTN",0xF0},
            {"VK_OEM_AUTO",0xF3},
            {"VK_OEM_AX",0xE1},
            {"VK_OEM_BACKTAB",0xF5},
            {"VK_OEM_CLEAR",0xFE},
            {"VK_OEM_COMMA",0xBC},
            {"VK_OEM_COPY",0xF2},
            {"VK_OEM_CUSEL",0xEF},
            {"VK_OEM_ENLW",0xF4},
            {"VK_OEM_FINISH",0xF1},
            {"VK_OEM_FJ_LOYA",0x95},
            {"VK_OEM_FJ_MASSHOU",0x93},
            {"VK_OEM_FJ_ROYA",0x96},
            {"VK_OEM_FJ_TOUROKU",0x94},
            {"VK_OEM_JUMP",0xEA},
            {"VK_OEM_MINUS",0xBD},
            {"VK_OEM_PA1",0xEB},
            {"VK_OEM_PA2",0xEC},
            {"VK_OEM_PA3",0xED},
            {"VK_OEM_PERIOD",0xBE},
            {"VK_OEM_PLUS",0xBB},
            {"VK_OEM_RESET",0xE9},
            {"VK_OEM_WSCTRL",0xEE},
            {"VK_PA1",0xFD},
            {"VK_PACKET",0xE7},
            {"VK_PLAY",0xFA},
            {"VK_PROCESSKEY",0xE5},
            {"VK_RETURN",0x0D},
            {"VK_SELECT",0x29},
            {"VK_SEPARATOR",0x6C},
            {"VK_SPACE",0x20},
            {"VK_SUBTRACT",0x6D},
            {"VK_TAB",0x09},
            {"VK_ZOOM",0xFB},
            {"VK__none_",0xFF},
            {"VK_ACCEPT",0x1E},
            {"VK_APPS",0x5D},
            {"VK_BROWSER_BACK",0xA6},
            {"VK_BROWSER_FAVORITES",0xAB},
            {"VK_BROWSER_FORWARD",0xA7},
            {"VK_BROWSER_HOME",0xAC},
            {"VK_BROWSER_REFRESH",0xA8},
            {"VK_BROWSER_SEARCH",0xAA},
            {"VK_BROWSER_STOP",0xA9},
            {"VK_CAPITAL",0x14},
            {"VK_CONVERT",0x1C},
            {"VK_DELETE",0x2E},
            {"VK_DOWN",0x28},
            {"VK_END",0x23},
            {"VK_F1",0x70},
            {"VK_F10",0x79},
            {"VK_F11",0x7A},
            {"VK_F12",0x7B},
            {"VK_F13",0x7C},
            {"VK_F14",0x7D},
            {"VK_F15",0x7E},
            {"VK_F16",0x7F},
            {"VK_F17",0x80},
            {"VK_F18",0x81},
            {"VK_F19",0x82},
            {"VK_F2",0x71},
            {"VK_F20",0x83},
            {"VK_F21",0x84},
            {"VK_F22",0x85},
            {"VK_F23",0x86},
            {"VK_F24",0x87},
            {"VK_F3",0x72},
            {"VK_F4",0x73},
            {"VK_F5",0x74},
            {"VK_F6",0x75},
            {"VK_F7",0x76},
            {"VK_F8",0x77},
            {"VK_F9",0x78},
            {"VK_FINAL",0x18},
            {"VK_HELP",0x2F},
            {"VK_HOME",0x24},
            {"VK_ICO_00",0xE4},
            {"VK_INSERT",0x2D},
            {"VK_JUNJA",0x17},
            {"VK_KANA",0x15},
            {"VK_KANJI",0x19},
            {"VK_LAUNCH_APP1",0xB6},
            {"VK_LAUNCH_APP2",0xB7},
            {"VK_LAUNCH_MAIL",0xB4},
            {"VK_LAUNCH_MEDIA_SELECT",0xB5},
            {"VK_LBUTTON",0x01},
            {"VK_LCONTROL",0xA2},
            {"VK_LEFT",0x25},
            {"VK_LMENU",0xA4},
            {"VK_LSHIFT",0xA0},
            {"VK_LWIN",0x5B},
            {"VK_MBUTTON",0x04},
            {"VK_MEDIA_NEXT_TRACK",0xB0},
            {"VK_MEDIA_PLAY_PAUSE",0xB3},
            {"VK_MEDIA_PREV_TRACK",0xB1},
            {"VK_MEDIA_STOP",0xB2},
            {"VK_MODECHANGE",0x1F},
            {"VK_NEXT",0x22},
            {"VK_NONCONVERT",0x1D},
            {"VK_NUMLOCK",0x90},
            {"VK_OEM_FJ_JISHO",0x92},
            {"VK_PAUSE",0x13},
            {"VK_PRINT",0x2A},
            {"VK_PRIOR",0x21},
            {"VK_RBUTTON",0x02},
            {"VK_RCONTROL",0xA3},
            {"VK_RIGHT",0x27},
            {"VK_RMENU",0xA5},
            {"VK_RSHIFT",0xA1},
            {"VK_RWIN",0x5C},
            {"VK_SCROLL",0x91},
            {"VK_SLEEP",0x5F},
            {"VK_SNAPSHOT",0x2C},
            {"VK_UP",0x26},
            {"VK_VOLUME_DOWN",0xAE},
            {"VK_VOLUME_MUTE",0xAD},
            {"VK_VOLUME_UP",0xAF},
            {"VK_XBUTTON1",0x05},
            {"VK_XBUTTON2",0x06},


        };
        private static VirtualKey? GetVirtualKeyFromString(string str)
        {
            int keyCode;
            if (!stringToVKDict.TryGetValue(str, out keyCode)) return null;
            return (VirtualKey)keyCode;
        }
        private static void CreateConfigFileIfNotExists(string path)
        {
            if (File.Exists(path)) return;
            try
            {
                File.WriteAllText(path, Properties.Resources.DefaultCfg);
            }
            catch (System.Exception)
            {
                Application.Exit();
            }
        }
        public static IEnumerable<KeyDef> SetUpKeyDefs()
        {
            var configFilePath = Application.StartupPath + Path.DirectorySeparatorChar + "Caplos.cfg";
            CreateConfigFileIfNotExists(configFilePath);
            var rawLines = new List<string>();
            try
            {
                rawLines = File.ReadLines(configFilePath).ToList();
            }
            catch (System.Exception)
            {
                Application.Exit();
            }

            foreach (var line in rawLines)
            {
                // Replace.VK_OEM_2        = VK_KEY_0 # VK_KEY_0: [?/]
                var lineTrimed = line.Trim();
                if (lineTrimed.Equals(string.Empty)) continue;

                // Filter for comments start with #
                // Replace.VK_OEM_2        = VK_KEY_0 
                int indexOfSharpChar = lineTrimed.IndexOf('#');
                if (indexOfSharpChar == 1) continue;
                if (indexOfSharpChar > 0)
                    lineTrimed = lineTrimed.Substring(0, indexOfSharpChar - 1);

                var replaceSettingPrefix = "Replace.";
                if (lineTrimed.StartsWith(replaceSettingPrefix))
                {
                    // VK_OEM_2        = VK_KEY_0 
                    lineTrimed = lineTrimed.Substring(replaceSettingPrefix.Length);
                    if (lineTrimed.IndexOf('=') == 0) continue;
                    var lineSplited = lineTrimed.Split(new[] { '=' }, 2);
                    var key = lineSplited[0].Trim();// VK_OEM_2
                    var value = lineSplited[1].Trim();// VK_KEY_0

                    var ret = new KeyDef();
                    var from = GetVirtualKeyFromString(key);
                    var to = GetVirtualKeyFromString(value);
                    if ((from == null) || (to == null)) continue;
                    ret.AdditionKey = (VirtualKey)from;
                    ret.ReplacingKey = (VirtualKey)to;
                    ret.Type = FuncType.Replace;
                    yield return ret;
                }

            }
        }
        // List of Virtual Key Codes
        // http://www.kbdedit.com/manual/low_level_vk_list.html
        public static IEnumerable<KeyDef> KeyDefs;
    }
}
