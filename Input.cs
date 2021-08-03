using System;
using System.Runtime.InteropServices;
using System.Threading;
using System.Collections.Generic;

public class Input
{
    //User32 Imports
    [DllImport("user32.dll", SetLastError = true)]
    private static extern uint SendInput(uint nInputs, INPUT[] pInputs, int cbSize);
    [DllImport("user32.dll")]
    private static extern IntPtr GetMessageExtraInfo();

    //Input Struct
    [StructLayout(LayoutKind.Sequential)]
    private struct KeyboardInput
    {
        public ushort wVk;
        public ushort wScan;
        public uint dwFlags;
        public uint time;
        public IntPtr dwExtraInfo;
    }
    [StructLayout(LayoutKind.Sequential)]
    private struct MouseInput
    {
        public int dx;
        public int dy;
        public uint mouseData;
        public uint dwFlags;
        public uint time;
        public IntPtr dwExtraInfo;
    }
    [StructLayout(LayoutKind.Sequential)]
    private struct HardwareInput
    {
        public uint uMsg;
        public ushort wParamL;
        public ushort wParamH;
    }
    [StructLayout(LayoutKind.Explicit)]
    private struct InputUnion
    {
        [FieldOffset(0)] public MouseInput mi;
        [FieldOffset(0)] public KeyboardInput ki;
        [FieldOffset(0)] public HardwareInput hi;
    }
    private struct INPUT
    {
        public int type;
        public InputUnion u;
    }

    //Flags
    [Flags]
    private enum InputType
    {
        Mouse = 0,
        Keyboard = 1,
        Hardware = 2
    }
    [Flags]
    private enum KeyEventF
    {
        KeyDown = 0x0000,
        ExtendedKey = 0x0001,
        KeyUp = 0x0002,
        Unicode = 0x0004,
        Scancode = 0x0008
    }
    [Flags]
    private enum MouseEventF
    {
        Absolute = 0x8000,
        HWheel = 0x01000,
        Move = 0x0001,
        MoveNoCoalesce = 0x2000,
        LeftDown = 0x0002,
        LeftUp = 0x0004,
        RightDown = 0x0008,
        RightUp = 0x0010,
        MiddleDown = 0x0020,
        MiddleUp = 0x0040,
        VirtualDesk = 0x4000,
        Wheel = 0x0800,
        XDown = 0x0080,
        XUp = 0x0100
    }

    private Dictionary<char, ushort> keyCodes = new Dictionary<char, ushort>() { { 'a', 0x1E }, { 'b', 0x30 }, { 'c', 0x2E }, { 'd', 0x20 }, { 'e', 0x12 }, { 'f', 0x21 }, { 'g', 0x22 }, { 'h', 0x23 }, { 'i', 0x17 }, { 'j', 0x24 }, { 'k', 0x25 }, { 'l', 0x26 }, { 'm', 0x32 }, { 'n', 0x31 }, { 'o', 0x18 }, { 'p', 0x19 }, { 'q', 0x10 }, { 'r', 0x13 }, { 's', 0x1F }, { 't', 0x14 }, { 'u', 0x16 }, { 'v', 0x2F }, { 'w', 0x11 }, { 'x', 0x2D }, { 'y', 0x15 }, { 'z', 0x2C } };

    public bool KeyDown(string keys)
    {
        INPUT[] input = new INPUT[keys.Length];
        for (int i = 0; i < input.Length; i++)
        {
            if (!Char.IsLetter(keys[i]))
                return false;
            input[i] = new INPUT
            {
                type = (int)InputType.Keyboard,
                u = new InputUnion
                {
                    ki = new KeyboardInput
                    {
                        wVk = 0,
                        wScan = keyCodes[Char.ToLower(keys[i])],
                        dwFlags = (uint)(KeyEventF.KeyDown | KeyEventF.Scancode),
                        dwExtraInfo = GetMessageExtraInfo()
                    }
                }
            };
        }
        SendInput((uint)input.Length, input, Marshal.SizeOf(typeof(INPUT)));
        return true;
    }

    public bool KeyUp(string keys)
    {
        INPUT[] input = new INPUT[keys.Length];
        for (int i = 0; i < input.Length; i++)
        {
            if (!Char.IsLetter(keys[i]))
                return false;
            input[i] = new INPUT
            {
                type = (int)InputType.Keyboard,
                u = new InputUnion
                {
                    ki = new KeyboardInput
                    {
                        wVk = 0,
                        wScan = keyCodes[Char.ToLower(keys[i])],
                        dwFlags = (uint)(KeyEventF.KeyUp | KeyEventF.Scancode),
                        dwExtraInfo = GetMessageExtraInfo()
                    }
                }
            };
        }
        SendInput((uint)input.Length, input, Marshal.SizeOf(typeof(INPUT)));
        return true;
    }

    public bool KeyPress(string keys, int time = 0)
    {
        if (!KeyDown(keys))
            return false;
        Thread.Sleep(time);
        if (!KeyUp(keys))
            return false;
        return true;
    }
}