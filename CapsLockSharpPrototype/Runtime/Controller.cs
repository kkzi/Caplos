using System;
using System.Linq;
using System.Runtime.InteropServices;
using CapsLockSharpPrototype.Helper;
using static CapsLockSharpPrototype.Helper.GlobalKeyboardHook;
using static CapsLockSharpPrototype.Helper.KeyDefRuntime;
using trit = System.Int16;
namespace CapsLockSharpPrototype.Runtime
{
    public class Controller
    {
        [DllImport("user32.dll", EntryPoint = "keybd_event", SetLastError = true)]
        public static extern void SendKeyEvent(int bVk, byte bScan, uint dwFlags, UIntPtr dwExtraInfo);

        private static void key_up(VirtualKey key)
        {
            Logger.Info("-- key up " + key);
            SendKeyEvent((int)key, 0, 2, UIntPtr.Zero);
        }

        private static void key_down(VirtualKey key)
        {
            Logger.Info("-- key down " + key);
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
                if ((VirtualKey)e.KeyboardData.VirtualCode == VirtualKey.CapsLock)
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
            }
            else if (status_ == HookStatus.Hooked)
            {
                status_ = 0;
            }
            else
            {
                capslock_busy_ = true;
                key_click(VirtualKey.CapsLock);
                capslock_busy_ = false;
                capslock_pressed_ = false;
                capslock_func_(e);
            }
            e.Handled = true;
        }

        private void OnOtherKey(GlobalKeyboardHookEventArgs e)
        {
            var state = e.KeyboardState == KeyboardState.KeyUp || e.KeyboardState == KeyboardState.SysKeyUp ? "up" : "down";
            var keycode = (VirtualKey)e.KeyboardData.VirtualCode;

            if (e.KeyboardData.VirtualCode == (int)VirtualKey.LeftShift && status_ == HookStatus.Hooking)
            {
                Logger.Info("ignore key " + keycode + " " + state);
                e.Handled = true;
                return;
            }

            #region Capslock+Space 实现 左Ctrl+Space
            if (keycode == VirtualKey.Space)
            {
                if (e.KeyboardState == KeyboardState.KeyUp || !capslock_pressed_)
                {
                    Logger.Info("normal space " + state);
                    return;
                }

                Logger.Info("trigger ctrl+ space " + state);
                status_ = HookStatus.Hooking;
                //key_up(VirtualKey.CapsLock);
                key_down(VirtualKey.LeftControl);
                key_click(VirtualKey.Space);
                key_up(VirtualKey.LeftControl);
                status_ = HookStatus.Hooked;
                e.Handled = true;
                return;
            }
            #endregion
            else
            {
                var keyDef = KeyDefs.FirstOrDefault(x => x.AdditionKey == keycode);
                if (keyDef.Equals(default(KeyDef)) || e.KeyboardState == KeyboardState.KeyUp || !capslock_pressed_)
                {
                    Logger.Info("normal key " + keycode + " " + state);
                    return;
                }
                Logger.Info("hook key " + keycode + " " + state);
                status_ = HookStatus.Hooking;
                //key_up(VirtualKey.CapsLock);
                key_up(keyDef.AdditionKey);
                key_down(keyDef.ReplacingKey);
                status_ = HookStatus.Hooked;
                e.Handled = true;
            }
        }
    }
}
