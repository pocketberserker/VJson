//
// Copyright (c) 2019- yutopp (yutopp@gmail.com)
//
// Distributed under the Boost Software License, Version 1.0. (See accompanying
// file LICENSE_1_0.txt or copy at  https://www.boost.org/LICENSE_1_0.txt)
//

using System;
using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;
using System.Collections.Generic;

namespace VJson
{
    public ref struct JsonReader
    {
        private readonly ReadOnlySpan<byte> bytes;
        private int offset;

        private List<byte> strCache;

        public JsonReader(in ReadOnlySpan<byte> bytes)
        {
            this.bytes = bytes;
            offset = 0;

            strCache = new List<byte>();
        }

        public INode Read()
        {
            var node = ReadElement();

            if (offset < bytes.Length)
            {
                throw NodeExpectedError("EOS");
            }

            return node;
        }

        INode ReadElement()
        {
            SkipWS();
            var node = ReadValue();
            SkipWS();

            return node;
        }

        INode ReadValue()
        {
            INode node = null;

            if ((node = ReadObject()) != null)
            {
                return node;
            }

            if ((node = ReadArray()) != null)
            {
                return node;
            }

            if ((node = ReadString()) != null)
            {
                return node;
            }

            if ((node = ReadNumber()) != null)
            {
                return node;
            }

            if ((node = ReadLiteral()) != null)
            {
                return node;
            }

            throw NodeExpectedError("value");
        }

        INode ReadObject()
        {
            if (offset >= bytes.Length)
            {
                return null;
            }

            ref readonly var next = ref bytes[offset];
            if (next != (byte)'{')
            {
                return null;
            }
            offset++; // Discard

            var node = new ObjectNode();

            for (int i = 0; ; ++i)
            {
                SkipWS();

                if (offset >= bytes.Length)
                {
                    throw NodeExpectedError("object");
                }

                next = ref bytes[offset];
                if (next == (byte)'}')
                {
                    offset++; // Discard
                    break;
                }

                if (i > 0)
                {
                    if (next != (byte)',')
                    {
                        throw TokenExpectedError(',');
                    }
                    offset++; // Discard
                }

                SkipWS();
                INode keyNode = ReadString();
                if (keyNode == null)
                {
                    throw NodeExpectedError("string");
                }
                SkipWS();

                next = ref bytes[offset];
                if (next != (byte)':')
                {
                    throw TokenExpectedError(':');
                }
                offset++; // Discard

                INode elemNode = ReadElement();
                if (elemNode == null)
                {
                    throw NodeExpectedError("element");
                }

                node.AddElement(((StringNode)keyNode).Value, elemNode);
            }

            return node;
        }

        INode ReadArray()
        {
            if (offset >= bytes.Length)
            {
                return null;
            }

            ref readonly var next = ref bytes[offset];
            if (next != (byte)'[')
            {
                return null;
            }
            offset++; // Discard

            var node = new ArrayNode();

            for (int i = 0; ; ++i)
            {
                SkipWS();

                if (offset >= bytes.Length)
                {
                    throw NodeExpectedError("array");
                }

                next = ref bytes[offset];
                if (next == (byte)']')
                {
                    offset++; // Discard
                    break;
                }

                if (i > 0)
                {
                    if (next != (byte)',')
                    {
                        throw TokenExpectedError(',');
                    }
                    offset++; // Discard
                }

                INode elemNode = ReadElement();
                if (elemNode == null)
                {
                    throw NodeExpectedError("element");
                }

                node.AddElement(elemNode);
            }

            return node;
        }

        INode ReadString()
        {
            if (offset >= bytes.Length)
            {
                return null;
            }

            ref readonly var next = ref bytes[offset];
            if (next != (byte)'"')
            {
                return null;
            }
            offset++; // Discard

            for (; ; )
            {
                if (offset >= bytes.Length)
                {
                    throw TokenExpectedError('\"');
                }

                next = ref bytes[offset];
                switch (next)
                {
                    case (byte)'"':
                        offset++; // Discard

                        var span = CommitBuffer();
                        var str = Regex.Unescape(span);
                        return new StringNode(str);

                    case (byte)'\\':
                        offset++; // Discard

                        if (!ReadEscape())
                        {
                            throw NodeExpectedError("escape");
                        };
                        break;

                    default:
                        ref readonly var c = ref bytes[offset++]; // Consume
                        var codePoint = (int)c;
                        var isPair = char.IsHighSurrogate((char)c);
                        if (isPair)
                        {
                            if (offset >= bytes.Length)
                            {
                                throw NodeExpectedError("low-surrogate");
                            }

                            next = ref bytes[offset++];  // Consume
                            if (!char.IsLowSurrogate((char)next))
                            {
                                throw NodeExpectedError("low-surrogate");
                            }
                            codePoint = char.ConvertToUtf32((char)c, (char)next);
                        }

                        if (codePoint < 0x20 || codePoint > 0x10ffff)
                        {
                            throw NodeExpectedError("unicode char (0x20 <= char <= 0x10ffff");
                        }

                        SaveToBuffer(c);
                        if (isPair)
                        {
                            SaveToBuffer(next);
                        }

                        break;
                }
            }
        }

        bool ReadEscape()
        {
            if (offset >= bytes.Length)
            {
                return false;
            }

            ref readonly var next = ref bytes[offset];
            switch (next)
            {
                case (byte)'"':
                    SaveToBuffer((byte)'\\');
                    SaveToBuffer(bytes[offset++]);
                    return true;

                case (byte)'\\':
                    SaveToBuffer((byte)'\\');
                    SaveToBuffer(bytes[offset++]);
                    return true;

                case (byte)'/':
                    // Escape is not required in C#
                    SaveToBuffer(bytes[offset++]);
                    return true;

                case (byte)'b':
                    SaveToBuffer((byte)'\\');
                    SaveToBuffer(bytes[offset++]);
                    return true;

                case (byte)'n':
                    SaveToBuffer((byte)'\\');
                    SaveToBuffer(bytes[offset++]);
                    return true;

                case (byte)'r':
                    SaveToBuffer((byte)'\\');
                    SaveToBuffer(bytes[offset++]);
                    return true;

                case (byte)'t':
                    SaveToBuffer((byte)'\\');
                    SaveToBuffer(bytes[offset++]);
                    return true;

                case (byte)'u':
                    SaveToBuffer((byte)'\\');
                    SaveToBuffer(bytes[offset++]);
                    for (int i = 0; i < 4; ++i)
                    {
                        if (!ReadHex())
                        {
                            throw NodeExpectedError("hex");
                        }
                    }
                    return true;

                default:
                    return false;
            }
        }

        bool ReadHex()
        {
            if (ReadDigit())
            {
                return true;
            }

            if (offset >= bytes.Length)
            {
                return false;
            }

            ref readonly var next = ref bytes[offset];
            if (next >= (byte)'A' && next <= (byte)'F')
            {
                SaveToBuffer(bytes[offset++]);
                return true;
            }

            if (next >= (byte)'a' && next <= (byte)'f')
            {
                SaveToBuffer(bytes[offset++]);
                return true;
            }

            return false;
        }

        INode ReadNumber()
        {
            if (!ReadInt())
            {
                return null;
            }

            var isFloat = false;
            isFloat |= ReadFrac();
            isFloat |= ReadExp();

            var span = CommitBuffer();
            if (isFloat)
            {
                var v = double.Parse(span, CultureInfo.InvariantCulture); // TODO: Fix for large numbers
                return new FloatNode(v);
            } else {
                var v = long.Parse(span, CultureInfo.InvariantCulture);   // TODO: Fix for large numbers
                return new IntegerNode(v);
            }
        }

        bool ReadInt()
        {
            if (ReadOneNine())
            {
                ReadDigits();
                return true;
            }

            if (ReadDigit())
            {
                return true;
            }

            if (offset >= bytes.Length)
            {
                return false;
            }

            ref readonly var next = ref bytes[offset];
            if (next != (byte)'-')
            {
                return false;
            }

            SaveToBuffer(bytes[offset++]);

            if (ReadOneNine())
            {
                ReadDigits();
                return true;
            }

            if (ReadDigit())
            {
                return true;
            }

            throw NodeExpectedError("number");
        }

        bool ReadDigits()
        {
            if (!ReadDigit())
            {
                return false;
            }

            while (ReadDigit()) { }
            return true;
        }

        bool ReadDigit()
        {
            if (offset >= bytes.Length)
            {
                return false;
            }

            ref readonly var next = ref bytes[offset];
            if (next != (byte)'0')
            {
                return ReadOneNine();
            }

            SaveToBuffer(bytes[offset++]);

            return true;
        }

        bool ReadOneNine()
        {
            if (offset >= bytes.Length)
            {
                return false;
            }

            ref readonly var next = ref bytes[offset];
            if (next < (byte)'1' || next > (byte)'9')
            {
                return false;
            }

            SaveToBuffer(bytes[offset++]);

            return true;
        }

        bool ReadFrac()
        {
            if (offset >= bytes.Length)
            {
                return false;
            }

            ref readonly var next = ref bytes[offset];
            if (next != (byte)'.')
            {
                return false;
            }

            SaveToBuffer(bytes[offset++]);

            if (!ReadDigits())
            {
                throw NodeExpectedError("digits");
            }

            return true;
        }

        bool ReadExp()
        {
            if (offset >= bytes.Length)
            {
                return false;
            }

            ref readonly var next = ref bytes[offset];
            if (next != (byte)'E' && next != (byte)'e')
            {
                return false;
            }

            SaveToBuffer(bytes[offset++]);

            ReadSign();

            if (!ReadDigits())
            {
                throw NodeExpectedError("digits");
            }

            return true;
        }

        bool ReadSign()
        {
            if (offset >= bytes.Length)
            {
                return false;
            }

            ref readonly var next = ref bytes[offset];
            if (next != (byte)'+' && next != (byte)'-')
            {
                return false;
            }

            SaveToBuffer(bytes[offset++]);

            return true;
        }

        INode ReadLiteral()
        {
            if (offset >= bytes.Length)
            {
                return null;
            }

            var s = String.Empty;

            ref readonly var next = ref bytes[offset];
            switch (next)
            {
                case (byte)'t':
                    // Maybe true
                    s = ConsumeChars(4);
                    if (s.ToLower() != "true")
                    {
                        throw NodeExpectedError("true");
                    }
                    return new BooleanNode(true);

                case (byte)'f':
                    // Maybe false
                    s = ConsumeChars(5);
                    if (s.ToLower() != "false")
                    {
                        throw NodeExpectedError("false");
                    }
                    return new BooleanNode(false);

                case (byte)'n':
                    // Maybe null
                    s = ConsumeChars(4);
                    if (s.ToLower() != "null")
                    {
                        throw NodeExpectedError("null");
                    }
                    return new NullNode();

                default:
                    return null;
            }
        }

        void SkipWS()
        {
            for (; ; )
            {
                if (offset >= bytes.Length)
                {
                    return;
                }

                ref readonly var next = ref bytes[offset];
                switch (next)
                {
                    case 0x0009:
                    case 0x000a:
                    case 0x000d:
                    case 0x0020:
                        offset++; // Discard
                        break;

                    default:
                        return;
                }
            }
        }

        void SaveToBuffer(byte c)
        {
            strCache.Add(c);
        }

        string CommitBuffer()
        {
            var span = Encoding.UTF8.GetString(strCache.ToArray());
            strCache.Clear();

            return span;
        }

        string ConsumeChars(int length)
        {
            var o = offset;
            offset += length;
            return Encoding.UTF8.GetString(bytes.Slice(o, length));
        }

        ParseFailedException NodeExpectedError(string expected)
        {
            var msg = String.Format(
                "A node \"{0}\" is expected but '{1}' is provided",
                expected,
                offset < bytes.Length ? ((char)bytes[offset]).ToString() : "<EOS>"
            );
            return new ParseFailedException(msg, (ulong)offset);
        }

        ParseFailedException TokenExpectedError(char expected)
        {
            var msg = String.Format(
                "A charactor '{0}' is expected but '{1}' is provided",
                expected,
                offset < bytes.Length ? ((char)bytes[offset]).ToString() : "<EOS>"
            );
            return new ParseFailedException(msg, (ulong)offset);
        }
    }

    public class ParseFailedException : Exception
    {
        public ParseFailedException(string message, ulong pos)
        : base(String.Format("{0} (at position {1})", message, pos))
        {
        }
    }
}
