using System;
using System.Linq;
using System.Runtime.InteropServices;
using CapsLockSharpPrototype.Helper;
using static CapsLockSharpPrototype.Helper.GlobalKeyboardHook;
using static CapsLockSharpPrototype.Helper.KeyDefRuntime;
using System.Collections.Generic;

namespace CapsLockSharpPrototype.Runtime
{

    [StructLayout(LayoutKind.Explicit)]
    public struct INPUT
    {
        [FieldOffset(0)]
        public int type;
        [FieldOffset(4)]
        public KEYBDINPUT ki;
        [FieldOffset(4)]
        public MOUSEINPUT mi;
        [FieldOffset(4)]
        public HARDWAREINPUT hi;
    }
    public struct MOUSEINPUT
    {
        public int dx;
        public int dy;
        public int mouseData;
        public int dwFlags;
        public int time;
        public IntPtr dwExtraInfo;
    }
    public struct KEYBDINPUT
    {
        public short wVk;
        public short wScan;
        public int dwFlags;
        public int time;
        public IntPtr dwExtraInfo;
    }
    public struct HARDWAREINPUT
    {
        public int uMsg;
        public short wParamL;
        public short wParamH;
    }

    public class Keyboard
    {
        [DllImport("user32")]
        public static extern uint SendInput(uint nInputs, INPUT[] pInputs, int cbSize);
    }

    public class Controller
    {
        private static Dictionary<VirtualKey, bool> modified_pressed_ = new Dictionary<VirtualKey, bool>() {
            {VirtualKey.LeftControl, false},
            {VirtualKey.LeftMenu , false},
            {VirtualKey.LeftShift, false},
            {VirtualKey.LeftWindows, false},
        };

        private static DateTime capslock_pressed_time_ = DateTime.MinValue;
        private static bool capslock_pressed_ = false;
        private static bool capslock_busy_ = false;
        private static Action<EventArgs> capslock_func_ = null;
        private static HookStatus status_ = HookStatus.Normal;
        private readonly GlobalKeyboardHook hook_ = new GlobalKeyboardHook();

        private static void key_up(VirtualKey key)
        {
            if (key == VirtualKey.CapsLock)
            {
                RestHookStatus();
            }
            if (modified_pressed_.ContainsKey(key))
            {
                modified_pressed_[key] = false;
            }

            var inputs = new INPUT[1];
            inputs[0].type = 1;
            inputs[0].ki.wVk = (short)key;
            inputs[0].ki.dwFlags = 2;
            Keyboard.SendInput(1u, inputs, Marshal.SizeOf((object)default(INPUT)));
        }

        private static void key_down(VirtualKey key)
        {
            var inputs = new INPUT[1];
            inputs[0].type = 1;
            inputs[0].ki.wVk = (short)key;
            Keyboard.SendInput(1u, inputs, Marshal.SizeOf((object)default(INPUT)));
        }

        private static void key_click(VirtualKey key)
        {
            key_clicks(new List<VirtualKey> { key });
        }

        private static void key_clicks(List<VirtualKey> arr)
        {
            if (arr.Count == 0) return;
            var len = arr.Count * 2;
            var inputs = new INPUT[len];
            for (var i = 0; i < arr.Count; ++i)
            {
                var key = (short)arr[i];
                var upi = len - 1 - i;

                inputs[i].type = 1;
                inputs[i].ki.wVk = key;

                inputs[upi].type = 1;
                inputs[upi].ki.wVk = key;
                inputs[upi].ki.dwFlags = 2;
            }
            Keyboard.SendInput((uint)len, inputs, Marshal.SizeOf((object)default(INPUT)));
        }

        private enum HookStatus
        {
            Normal = 0,
            Hooking = 1,
            Hooked = 2,
        }

        public void SetupKeyboardHooks(Action<EventArgs> fun)
        {
            capslock_func_ = fun;
            hook_.KeyboardEvent += (object sender, GlobalKeyboardHookEventArgs e) =>
            {
                var keycode = (VirtualKey)e.KeyboardData.VirtualCode;
                if (keycode == VirtualKey.CapsLock)
                {
                    OnCapsLockKey(e);
                }
                else
                {
                    OnOtherKey(e);
                    return;
                }
            };
        }

        private void OnCapsLockKey(GlobalKeyboardHookEventArgs e)
        {
            if (capslock_busy_)
            {
                return;
            }
            if (e.KeyboardState == KeyboardState.KeyDown)
            {
                capslock_pressed_ = true;
                if (capslock_pressed_time_ == DateTime.MinValue)
                {
                    capslock_pressed_time_ = DateTime.Now;
                    Logger.Info("capslock pressed");
                }
            }
            else
            {
                if ((DateTime.Now - capslock_pressed_time_).TotalMilliseconds <= 500 && status_ == HookStatus.Normal)
                {
                    var ms = (DateTime.Now - capslock_pressed_time_).TotalMilliseconds;
                    capslock_busy_ = true;
                    key_click(VirtualKey.CapsLock);
                    capslock_busy_ = false;
                    capslock_func_(e);
                }
                RestHookStatus();
            }
            e.Handled = true;
        }

        private void OnOtherKey(GlobalKeyboardHookEventArgs e)
        {
            var keycode = (VirtualKey)e.KeyboardData.VirtualCode;
            var state = e.KeyboardState == KeyboardState.KeyUp ? "up" : "down";
            if (!capslock_pressed_)
            {
                return;
            }

            Logger.Info("## key=" + keycode + ", state=" + state + ", status=" + status_);
            var keydown = e.KeyboardState == KeyboardState.KeyDown || e.KeyboardState == KeyboardState.SysKeyDown;
            if (keycode == VirtualKey.LeftShift && !keydown && status_ == HookStatus.Hooking)
            {
                Logger.Info("** ignore key=" + keycode + ", state=" + state + ", status=" + status_);
                e.Handled = true;
                return;
            }

            if (modified_pressed_.ContainsKey(keycode))
            {
                var old = modified_pressed_[keycode] == keydown;
                modified_pressed_[keycode] = keydown;
                e.Handled = old && status_ == HookStatus.Hooking;
                Logger.Info("modified key=" + keycode + " state=" + state + ", ignore=" + e.Handled);
                return;
            }

            ModifiedKey modified = ModifiedKey.None;
            modified |= modified_pressed_[VirtualKey.LeftControl] ? ModifiedKey.Ctrl : ModifiedKey.None;
            modified |= modified_pressed_[VirtualKey.LeftMenu] ? ModifiedKey.Alt : ModifiedKey.None;
            //modified |= modified_pressed_[VirtualKey.LeftShift] ? ModifiedKey.Shift : ModifiedKey.None;
            //modified |= modified_pressed_[VirtualKey.LeftWindows] ? ModifiedKey.Win : ModifiedKey.None;

            var keyid = SourceKeyId(modified, keycode);
            if (keydown && status_ != HookStatus.Hooking && KeyToHook.ContainsKey(keyid))
            {
                status_ = HookStatus.Hooking;
                ProcessKeyHook(KeyToHook[keyid]);
                status_ = HookStatus.Hooked;
                e.Handled = true;
            }
        }

        private static void ResetModifies(List<VirtualKey> keys = null)
        {
            if (keys == null) keys = modified_pressed_.Keys.ToList();
            for (var i = 0; i < keys.Count; ++i)
            {
                if (modified_pressed_[keys[i]])
                {
                    key_up(keys[i]);
                }
            }
        }

        private static void RestHookStatus()
        {
            ResetModifies();

            capslock_pressed_time_ = DateTime.MinValue;
            capslock_pressed_ = false;
            status_ = HookStatus.Normal;
        }

        private void ProcessKeyHook(KeyHookItem item)
        {
            if (item.Func != null)
            {
                item.Func();
                return;
            }

            if (item.Targets.Count == 0)
            {
                return;
            }

            Logger.Info("%% hook " + item.TargetText);
            foreach (var it in item.Targets)
            {
                var arr = new List<VirtualKey>();
                if ((it.Modified & ModifiedKey.Ctrl) == ModifiedKey.Ctrl) arr.Add(VirtualKey.Control);
                if ((it.Modified & ModifiedKey.Alt) == ModifiedKey.Alt) arr.Add(VirtualKey.Menu);
                if ((it.Modified & ModifiedKey.Shift) == ModifiedKey.Shift) arr.Add(VirtualKey.LeftShift);
                if ((it.Modified & ModifiedKey.Win) == ModifiedKey.Win) arr.Add(VirtualKey.LeftWindows);
                arr.AddRange(it.Keys);
                key_clicks(arr);
            }
            Logger.Info("done");
        }

        private static void UpdateModifiedKey(VirtualKey keycode, bool state)
        {
            if (modified_pressed_.ContainsKey(keycode))
            {
                modified_pressed_[keycode] = state;
            }
        }
    }
}
