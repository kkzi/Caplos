using System;
using System.Linq;
using System.Runtime.InteropServices;
using CapsLockSharpPrototype.Helper;
using static CapsLockSharpPrototype.Helper.GlobalKeyboardHook;
using static CapsLockSharpPrototype.Helper.KeyDefRuntime;
using System.Collections.Generic;

namespace CapsLockSharpPrototype.Runtime
{
    public class Controller
    {
        [DllImport("user32.dll", EntryPoint = "keybd_event", SetLastError = true)]
        public static extern void SendKeyEvent(int bVk, byte bScan, uint dwFlags, UIntPtr dwExtraInfo);

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

        private static void key_up(VirtualKey key, byte scan = 0)
        {
            Logger.Info("-- key up " + key + " scan=" + scan);

            if (key == VirtualKey.CapsLock)
            {
                ResetModifies();
                capslock_pressed_ = false;
            }
            if (modified_pressed_.ContainsKey(key))
            {
                modified_pressed_[key] = false;
            }
            SendKeyEvent((int)key, scan, 2, UIntPtr.Zero);
        }

        private static void key_down(VirtualKey key, byte scan = 0)
        {
            Logger.Info("-- key down " + key);
            //UpdateModifiedKey(key, true);
            SendKeyEvent((int)key, scan, 0, UIntPtr.Zero);
        }

        private static void key_click(VirtualKey key)
        {
            key_down(key);
            key_up(key);
        }

        private enum HookStatus
        {
            Normal = 0,
            Hooking = 1,
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
                    Logger.Info("pressed");
                }
            }
            else
            {
                if ((DateTime.Now - capslock_pressed_time_).TotalMilliseconds <= 500)
                {
                    var ms = (DateTime.Now - capslock_pressed_time_).TotalMilliseconds;
                    capslock_busy_ = true;
                    key_click(VirtualKey.CapsLock);
                    capslock_busy_ = false;
                    capslock_func_(e);
                }
                capslock_pressed_time_ = DateTime.MinValue;
                capslock_pressed_ = false;
                ResetModifies();
            }
            e.Handled = true;
        }

        private void OnOtherKey(GlobalKeyboardHookEventArgs e)
        {
            var keycode = (VirtualKey)e.KeyboardData.VirtualCode;
            if (!capslock_pressed_)
            {
                return;
            }
            if (keycode == VirtualKey.RightShift)
            {
                e.Handled = false;
                return;
            }
            if (keycode == VirtualKey.LeftShift && status_ == HookStatus.Hooking)
            {
                e.Handled = true;
                return;
            }

            var keydown = e.KeyboardState == KeyboardState.KeyDown || e.KeyboardState == KeyboardState.SysKeyDown;
            if (modified_pressed_.ContainsKey(keycode))
            {
                modified_pressed_[keycode] = keydown;
                return;
            }

            ModifiedKey modified = ModifiedKey.None;
            modified |= modified_pressed_[VirtualKey.LeftControl] ? ModifiedKey.Ctrl : ModifiedKey.None;
            modified |= modified_pressed_[VirtualKey.LeftMenu] ? ModifiedKey.Alt : ModifiedKey.None;
            modified |= modified_pressed_[VirtualKey.LeftWindows] ? ModifiedKey.Win : ModifiedKey.None;

            var keyid = SourceKeyId(modified, keycode);
            if (keydown && status_ == HookStatus.Normal && KeyToHook.ContainsKey(keyid))
            {
                status_ = HookStatus.Hooking;
                key_up(keycode);
                ProcessKeyHook(KeyToHook[keyid]);
                status_ = HookStatus.Normal;
                e.Handled = true;
            }
        }

        private static void ResetModifies()
        {
            var keys = modified_pressed_.Keys.ToList();
            for (var i = 0; i < modified_pressed_.Count; ++i)
            {
                if (modified_pressed_[keys[i]])
                {
                    key_up(keys[i]);
                }
            }
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

            foreach (var it in item.Targets)
            {
                if ((it.Modified & ModifiedKey.Ctrl) == ModifiedKey.Ctrl) key_down(VirtualKey.LeftControl);
                if ((it.Modified & ModifiedKey.Alt) == ModifiedKey.Alt) key_down(VirtualKey.LeftMenu);
                if ((it.Modified & ModifiedKey.Shift) == ModifiedKey.Shift) key_down(VirtualKey.Shift, 0x2A);
                if ((it.Modified & ModifiedKey.Win) == ModifiedKey.Win) key_down(VirtualKey.LeftWindows);

                foreach (var k in it.Keys) key_down(k);
                foreach (var k in it.Keys) key_up(k);

                if ((it.Modified & ModifiedKey.Win) == ModifiedKey.Win) key_up(VirtualKey.LeftWindows);
                if ((it.Modified & ModifiedKey.Alt) == ModifiedKey.Alt) key_up(VirtualKey.LeftMenu);
                if ((it.Modified & ModifiedKey.Shift) == ModifiedKey.Shift) key_up(VirtualKey.Shift, 0x2A);
                if ((it.Modified & ModifiedKey.Ctrl) == ModifiedKey.Ctrl) key_up(VirtualKey.LeftControl);
            }
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
