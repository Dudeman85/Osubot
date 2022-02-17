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

    private Dictionary<string, ushort> keyCodes = new Dictionary<string, ushort>() {
        { "a", 0x41 }, { "b", 0x42 }, { "c", 0x43 }, { "d", 0x44 }, { "e", 0x45 }, { "f", 0x46 }, { "g", 0x47 }, { "h", 0x48 }, { "i", 0x49 },
        { "j", 0x4a }, { "k", 0x4b }, { "l", 0x4c }, { "m", 0x4d }, { "n", 0x4e }, { "o", 0x4f }, { "p", 0x50 }, { "q", 0x51 }, { "r", 0x52 },
        { "s", 0x53 }, { "t", 0x54 }, { "u", 0x55 }, { "v", 0x56 }, { "w", 0x57 }, { "x", 0x58 }, { "y", 0x59 }, { "z", 0x5a }, { "0", 0x30 },
        { "1", 0x31}, { "2", 0x32}, { "3", 0x33}, { "4", 0x34}, { "5", 0x35}, { "6", 0x36}, { "7", 0x37}, { "8", 0x38}, { "9", 0x39}, { "np0", 0x60},
        { "np1", 0x61}, { "np2", 0x62}, { "np3", 0x63}, { "np4", 0x64}, { "np5", 0x65}, { "np6", 0x66}, { "np7", 0x67}, { "np8", 0x68}, { "np9", 0x69},
        { "multiply", 0x6a}, { "add", 0x6b}, { "seperator", 0x6c}, { "subtract", 0x6d}, { "decimal", 0x6e}, { "divide", 0x6f}, { "f1", 0x70},
        { "f2", 0x71}, { "f3", 0x72}, { "f4", 0x73}, { "f5", 0x74}, { "f6", 0x75}, { "f7", 0x76}, { "f8", 0x77}, { "f9", 0x78}, { "f10", 0x79},
        { "f11", 0x7a}, { "f12", 0x7b}, { "numlock", 0x90}, { "srolllock", 0x91}, { "lshift", 0xa0}, { "rshift", 0xa1}, { "lcontrol", 0xa2},
        { "rcontrol", 0xa3}, { "lalt", 0xa4}, { "ralt", 0xa5}, { "backspace", 0x08}, { "tab", 0x09}, { "clear", 0x0c}, { "enter", 0x0d},
        { "space", 0x20}, { "end", 0x23}, { "home", 0x24}, { "left", 0x25}, { "up", 0x26}, { "right", 0x27}, { "down", 0x28}, { "insert", 0x2d},
        { "delete", 0x2e} }; //I want die

    //Function inserts a keydown event for every key in keys
    public bool KeyDown(string[] keys)
    {
        INPUT[] input = new INPUT[keys.Length];
        //For each key to be pressed 
        for (int i = 0; i < input.Length; i++)
        {
            //If a string in the input does not match a known key, return failure
            if (!keyCodes.ContainsKey(keys[i]))
                return false;

            //Add a new INPUT type into the list of inputs
            input[i] = new INPUT
            {
                type = (int)InputType.Keyboard,
                u = new InputUnion
                {
                    ki = new KeyboardInput
                    {
                        wVk = keyCodes[keys[i].ToLower()], //Virtual Key Code of the key to be sent
                        wScan = 0,
                        dwFlags = (uint)(KeyEventF.KeyDown), //Press Key
                        dwExtraInfo = GetMessageExtraInfo()
                    }
                }
            };
        }
        //Send the created input class and return success
        SendInput((uint)input.Length, input, Marshal.SizeOf(typeof(INPUT)));
        return true;
    }

    //Function inserts a keyup event for every key in keys
    public bool KeyUp(string[] keys)
    {
        INPUT[] input = new INPUT[keys.Length];
        //For each key to be pressed 
        for (int i = 0; i < input.Length; i++)
        {
            //If a string in the input does not match a known key, return failure
            if (!keyCodes.ContainsKey(keys[i]))
                return false;

            //Add a new INPUT type into the list of inputs
            input[i] = new INPUT
            {
                type = (int)InputType.Keyboard,
                u = new InputUnion
                {
                    ki = new KeyboardInput
                    {
                        wVk = keyCodes[keys[i].ToString()], //Virtual Key Code of the key to be sent
                        wScan = 0,
                        dwFlags = (uint)(KeyEventF.KeyUp), //Release Key
                        dwExtraInfo = GetMessageExtraInfo()
                    }
                }
            };
        }
        //Send the created input class and return success
        SendInput((uint)input.Length, input, Marshal.SizeOf(typeof(INPUT)));
        return true;
    }

    //Function to press and release each key in keys for time ms
    public bool KeyPress(string[] keys, int time = 0)
    {
        if (!KeyDown(keys))
            return false;

        Thread.Sleep(time);

        if (!KeyUp(keys))
            return false;

        return true;
    }
}