using UnityEngine;
using UnityEngine.InputSystem;

namespace AllIn1SpriteShader
{
    public static class InputKeyConverter
    {
#if ENABLE_INPUT_SYSTEM
        public static Key GetKeyFromKeycode(KeyCode keyCode)
        {
            Key res = Key.Space;
            switch (keyCode)
            {
                case KeyCode.A:
                    res = Key.A;
                    break;
                case KeyCode.B:
                    res = Key.B;
                    break;
                case KeyCode.C:
                    res = Key.C;
                    break;
                case KeyCode.D:
                    res = Key.D;
                    break;
                case KeyCode.E:
                    res = Key.E;
                    break;
                case KeyCode.F:
                    res = Key.F;
                    break;
                case KeyCode.G:
                    res = Key.G;
                    break;
                case KeyCode.H:
                    res = Key.H;
                    break;
                case KeyCode.I:
                    res = Key.I;
                    break;
                case KeyCode.J:
                    res = Key.J;
                    break;
                case KeyCode.K:
                    res = Key.K;
                    break;
                case KeyCode.L:
                    res = Key.L;
                    break;
                case KeyCode.M:
                    res = Key.M;
                    break;
                case KeyCode.N:
                    res = Key.N;
                    break;
                case KeyCode.O:
                    res = Key.O;
                    break;
                case KeyCode.P:
                    res = Key.P;
                    break;
                case KeyCode.Q:
                    res = Key.Q;
                    break;
                case KeyCode.R:
                    res = Key.R;
                    break;
                case KeyCode.S:
                    res = Key.S;
                    break;
                case KeyCode.T:
                    res = Key.T;
                    break;
                case KeyCode.U:
                    res = Key.U;
                    break;
                case KeyCode.V:
                    res = Key.V;
                    break;
                case KeyCode.W:
                    res = Key.W;
                    break;
                case KeyCode.X:
                    res = Key.X;
                    break;
                case KeyCode.Y:
                    res = Key.Y;
                    break;
                case KeyCode.Z:
                    res = Key.Z;
                    break;
                case KeyCode.Alpha0:
                    res = Key.Digit0;
                    break;
                case KeyCode.Alpha1:
                    res = Key.Digit1;
                    break;
                case KeyCode.Alpha2:
                    res = Key.Digit2;
                    break;
                case KeyCode.Alpha3:
                    res = Key.Digit3;
                    break;
                case KeyCode.Alpha4:
                    res = Key.Digit4;
                    break;
                case KeyCode.Alpha5:
                    res = Key.Digit5;
                    break;
                case KeyCode.Alpha6:
                    res = Key.Digit6;
                    break;
                case KeyCode.Alpha7:
                    res = Key.Digit7;
                    break;
                case KeyCode.Alpha8:
                    res = Key.Digit8;
                    break;
                case KeyCode.Alpha9:
                    res = Key.Digit9;
                    break;
                case KeyCode.Space:
                    res = Key.Space;
                    break;
                case KeyCode.Return:
                    res = Key.Enter;
                    break;
                case KeyCode.Escape:
                    res = Key.Escape;
                    break;
                case KeyCode.Backspace:
                    res = Key.Backspace;
                    break;
                case KeyCode.Tab:
                    res = Key.Tab;
                    break;
                case KeyCode.LeftShift:
                    res = Key.LeftShift;
                    break;
                case KeyCode.RightShift:
                    res = Key.RightShift;
                    break;
                case KeyCode.LeftControl:
                    res = Key.LeftCtrl;
                    break;
                case KeyCode.RightControl:
                    res = Key.RightCtrl;
                    break;
                case KeyCode.LeftAlt:
                    res = Key.LeftAlt;
                    break;
                case KeyCode.RightAlt:
                    res = Key.RightAlt;
                    break;
                case KeyCode.LeftCommand:
                    res = Key.LeftCommand;
                    break;
                case KeyCode.RightCommand:
                    res = Key.RightCommand;
                    break;
                case KeyCode.LeftWindows:
                    res = Key.LeftWindows;
                    break;
                case KeyCode.RightWindows:
                    res = Key.RightWindows;
                    break;
                case KeyCode.UpArrow:
                    res = Key.UpArrow;
                    break;
                case KeyCode.DownArrow:
                    res = Key.DownArrow;
                    break;
                case KeyCode.LeftArrow:
                    res = Key.LeftArrow;
                    break;
                case KeyCode.RightArrow:
                    res = Key.RightArrow;
                    break;
                case KeyCode.F1:
                    res = Key.F1;
                    break;
                case KeyCode.F2:
                    res = Key.F2;
                    break;
                case KeyCode.F3:
                    res = Key.F3;
                    break;
                case KeyCode.F4:
                    res = Key.F4;
                    break;
                case KeyCode.F5:
                    res = Key.F5;
                    break;
                case KeyCode.F6:
                    res = Key.F6;
                    break;
                case KeyCode.F7:
                    res = Key.F7;
                    break;
                case KeyCode.F8:
                    res = Key.F8;
                    break;
                case KeyCode.F9:
                    res = Key.F9;
                    break;
                case KeyCode.F10:
                    res = Key.F10;
                    break;
                case KeyCode.F11:
                    res = Key.F11;
                    break;
                case KeyCode.F12:
                    res = Key.F12;
                    break;
                case KeyCode.Insert:
                    res = Key.Insert;
                    break;
                case KeyCode.Delete:
                    res = Key.Delete;
                    break;
                case KeyCode.Home:
                    res = Key.Home;
                    break;
                case KeyCode.End:
                    res = Key.End;
                    break;
                case KeyCode.PageUp:
                    res = Key.PageUp;
                    break;
                case KeyCode.PageDown:
                    res = Key.PageDown;
                    break;
                case KeyCode.Keypad0:
                    res = Key.Numpad0;
                    break;
                case KeyCode.Keypad1:
                    res = Key.Numpad1;
                    break;
                case KeyCode.Keypad2:
                    res = Key.Numpad2;
                    break;
                case KeyCode.Keypad3:
                    res = Key.Numpad3;
                    break;
                case KeyCode.Keypad4:
                    res = Key.Numpad4;
                    break;
                case KeyCode.Keypad5:
                    res = Key.Numpad5;
                    break;
                case KeyCode.Keypad6:
                    res = Key.Numpad6;
                    break;
                case KeyCode.Keypad7:
                    res = Key.Numpad7;
                    break;
                case KeyCode.Keypad8:
                    res = Key.Numpad8;
                    break;
                case KeyCode.Keypad9:
                    res = Key.Numpad9;
                    break;
                case KeyCode.KeypadDivide:
                    res = Key.NumpadDivide;
                    break;
                case KeyCode.KeypadMultiply:
                    res = Key.NumpadMultiply;
                    break;
                case KeyCode.KeypadMinus:
                    res = Key.NumpadMinus;
                    break;
                case KeyCode.KeypadPlus:
                    res = Key.NumpadPlus;
                    break;
                case KeyCode.KeypadEnter:
                    res = Key.NumpadEnter;
                    break;
                case KeyCode.KeypadPeriod:
                    res = Key.NumpadPeriod;
                    break;
                case KeyCode.CapsLock:
                    res = Key.CapsLock;
                    break;
                case KeyCode.ScrollLock:
                    res = Key.ScrollLock;
                    break;
                case KeyCode.Pause:
                    res = Key.Pause;
                    break;
                case KeyCode.Quote:
                    res = Key.Quote;
                    break;
                case KeyCode.Comma:
                    res = Key.Comma;
                    break;
                case KeyCode.Minus:
                    res = Key.Minus;
                    break;
                case KeyCode.Period:
                    res = Key.Period;
                    break;
                case KeyCode.Slash:
                    res = Key.Slash;
                    break;
                case KeyCode.Semicolon:
                    res = Key.Semicolon;
                    break;
                case KeyCode.Equals:
                    res = Key.Equals;
                    break;
                case KeyCode.LeftBracket:
                    res = Key.LeftBracket;
                    break;
                case KeyCode.RightBracket:
                    res = Key.RightBracket;
                    break;
                case KeyCode.Backslash:
                    res = Key.Backslash;
                    break;
                case KeyCode.BackQuote:
                    res = Key.Backquote;
                    break;
                default:
                    res = Key.Space;
                    break;
            }

            return res;
        }
#endif
    }
}
