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

        private static void key_up(VirtualKey key)
        {
            Logger.Info("-- key up " + key);

            if (key == VirtualKey.CapsLock)
            {
                if (modified_pressed_.ContainsKey(key))
                {
                    modified_pressed_[key] = false;
                }
            }
            SendKeyEvent((int)key, 0, 2, UIntPtr.Zero);
        }

        private static void key_down(VirtualKey key)
        {
            Logger.Info("-- key down " + key);
            if (modified_pressed_.ContainsKey(key))
            {
                modified_pressed_[key] = true;
            }
            SendKeyEvent((int)key, 0, 0, UIntPtr.Zero);
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
            Hooked = 2,
        }

        private DateTime capslock_pressed_time_ = DateTime.MinValue;
        private bool capslock_pressed_ = false;
        private bool capslock_busy_ = false;
        private Action<EventArgs> capslock_func_ = null;
        private HookStatus status_ = HookStatus.Normal;
        private readonly GlobalKeyboardHook hook_ = new GlobalKeyboardHook();

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
            capslock_pressed_ = e.KeyboardState == KeyboardState.KeyDown;
            if (capslock_pressed_)
            {
                if (capslock_pressed_time_ == DateTime.MinValue)
                {
                    capslock_pressed_time_ = DateTime.Now;
                    Logger.Info("pressed");
                }
            }
            else
            {
                if (status_ == HookStatus.Hooked)
                {
                    ResetModifies();
                    status_ = HookStatus.Normal;
                }
                else if ((DateTime.Now - capslock_pressed_time_).TotalMilliseconds <= 500)
                {
                    var ms = (DateTime.Now - capslock_pressed_time_).TotalMilliseconds;
                    capslock_busy_ = true;
                    key_click(VirtualKey.CapsLock);
                    capslock_busy_ = false;
                    capslock_pressed_ = false;
                    capslock_func_(e);
                }
                capslock_pressed_time_ = DateTime.MinValue;
            }
            e.Handled = true;
        }

        private void OnOtherKey(GlobalKeyboardHookEventArgs e)
        {
            var state = e.KeyboardState == KeyboardState.KeyUp || e.KeyboardState == KeyboardState.SysKeyUp ? "up" : "down";
            var keycode = (VirtualKey)e.KeyboardData.VirtualCode;
            if (keycode == VirtualKey.LeftShift && status_ == HookStatus.Hooking)
            {
                e.Handled = true;
                return;
            }

            var keydef = KeyDefs.FirstOrDefault(x => x.AdditionKey == keycode);
            if (keydef.Equals(default(KeyDef)) || e.KeyboardState == KeyboardState.KeyUp || !capslock_pressed_)
            {
                Logger.Info("normal key " + keycode + " " + state);
                return;
            }
            Logger.Info("hook key " + keycode + " " + state);
            status_ = HookStatus.Hooking;
            //key_up(VirtualKey.CapsLock);
            key_up(keydef.AdditionKey);
            key_down(keydef.ReplacingKey);
            status_ = HookStatus.Hooked;
            e.Handled = true;
        }

        private void ResetModifies()
        {
            if (modified_pressed_[VirtualKey.LeftControl])
            {
                key_up(VirtualKey.LeftControl);
            }
            if (modified_pressed_[VirtualKey.LeftWindows])
            {
                key_up(VirtualKey.LeftWindows);
            }
            if (modified_pressed_[VirtualKey.LeftMenu])
            {
                key_up(VirtualKey.LeftMenu);
            }
            if (modified_pressed_[VirtualKey.LeftShift])
            {
                key_up(VirtualKey.LeftShift);
            }
        }
    }
}
