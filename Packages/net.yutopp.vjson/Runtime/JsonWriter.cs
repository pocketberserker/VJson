//
// Copyright (c) 2019- yutopp (yutopp@gmail.com)
//
// Distributed under the Boost Software License, Version 1.0. (See accompanying
// file LICENSE_1_0.txt or copy at  https://www.boost.org/LICENSE_1_0.txt)
//

using System;
using System.Collections.Generic;
using System.Buffers;
using System.Globalization;
using System.Text;
using System.Runtime.CompilerServices;

namespace VJson
{
    /// <summary>
    /// Write JSON data to buffer as UTF-8.
    /// </summary>
    // TODO: Add [Preserve] in Unity
    public struct JsonWriter
    {
        struct State
        {
            public StateKind Kind;
            public int Depth;
        }

        enum StateKind
        {
            ObjectKeyHead,
            ObjectKeyOther,
            ObjectValue,
            ArrayHead,
            ArrayOther,
            None,
        }

        private IBufferWriter<byte> _writer;
        private int _indent;

        private Stack<State> _states;

        public JsonWriter(IBufferWriter<byte> writer, int indent = 0)
        {
            _writer = writer;
            _indent = indent;
            _states = new Stack<State>();

            _states.Push(new State
            {
                Kind = StateKind.None,
                Depth = 0,
            });
        }

        public void WriteObjectStart()
        {
            var state = _states.Peek();
            if (state.Kind == StateKind.ObjectKeyHead || state.Kind == StateKind.ObjectKeyOther)
            {
                throw new Exception("");
            }

            WriteDelimiter();
            WriteRaw((byte)'{');

            _states.Push(new State
            {
                Kind = StateKind.ObjectKeyHead,
                Depth = state.Depth + 1,
            });
        }

        public void WriteObjectKey(string key)
        {
            var state = _states.Peek();
            if (state.Kind != StateKind.ObjectKeyHead && state.Kind != StateKind.ObjectKeyOther)
            {
                throw new Exception("");
            }

            WriteValue(key);
            WriteRaw((byte)':');

            _states.Pop();
            _states.Push(new State
            {
                Kind = StateKind.ObjectValue,
                Depth = state.Depth,
            });
        }

        public void WriteObjectEnd()
        {
            var state = _states.Peek();
            if (state.Kind != StateKind.ObjectKeyHead && state.Kind != StateKind.ObjectKeyOther)
            {
                throw new Exception("");
            }

            _states.Pop();

            if (state.Kind == StateKind.ObjectKeyOther)
            {
                WriteIndentBreakForHuman(_states.Peek().Depth);
            }
            WriteRaw((byte)'}');
        }

        public void WriteArrayStart()
        {
            var state = _states.Peek();
            if (state.Kind == StateKind.ObjectKeyHead || state.Kind == StateKind.ObjectKeyOther)
            {
                throw new Exception("");
            }

            WriteDelimiter();
            WriteRaw((byte)'[');

            _states.Push(new State
            {
                Kind = StateKind.ArrayHead,
                Depth = state.Depth + 1,
            });
        }

        public void WriteArrayEnd()
        {
            var state = _states.Peek();
            if (state.Kind != StateKind.ArrayHead && state.Kind != StateKind.ArrayOther)
            {
                throw new Exception("");
            }

            _states.Pop();

            if (state.Kind == StateKind.ArrayOther)
            {
                WriteIndentBreakForHuman(_states.Peek().Depth);
            }
            WriteRaw((byte)']');
        }

        public void WriteValue(bool v)
        {
            WriteDelimiter();

            if (v)
            {
                Span<byte> span = _writer.GetSpan(4);
                span[0] = (byte)'t';
                span[1] = (byte)'r';
                span[2] = (byte)'u';
                span[3] = (byte)'e';
                _writer.Advance(4);
            }
            else
            {
                Span<byte> span = _writer.GetSpan(5);
                span[0] = (byte)'f';
                span[1] = (byte)'a';
                span[2] = (byte)'l';
                span[3] = (byte)'s';
                span[4] = (byte)'e';
                _writer.Advance(5);
            }
        }

        public void WriteValue(byte v)
        {
            WritePrimitive(v);
        }

        public void WriteValue(sbyte v)
        {
            WritePrimitive(v);
        }

        public void WriteValue(char v)
        {
            WritePrimitive(v);
        }

        public void WriteValue(decimal v)
        {
            WritePrimitive(v);
        }

        public void WriteValue(double v)
        {
            WritePrimitive(v);
        }

        public void WriteValue(float v)
        {
            WritePrimitive(v);
        }

        public void WriteValue(int v)
        {
            WritePrimitive(v);
        }

        public void WriteValue(uint v)
        {
            WritePrimitive(v);
        }

        public void WriteValue(long v)
        {
            WritePrimitive(v);
        }

        public void WriteValue(ulong v)
        {
            WritePrimitive(v);
        }

        public void WriteValue(short v)
        {
            WritePrimitive(v);
        }

        public void WriteValue(ushort v)
        {
            WritePrimitive(v);
        }

        public void WriteValue(string v)
        {
            WriteDelimiter();

            WriteRaw((byte)'\"');
            WriteString(v);
            WriteRaw((byte)'\"');
        }

        public void WriteValueNull()
        {
            WriteDelimiter();

            Span<byte> span = _writer.GetSpan(4);
            span[0] = (byte)'n';
            span[1] = (byte)'u';
            span[2] = (byte)'l';
            span[3] = (byte)'l';
            _writer.Advance(4);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        void WritePrimitive(byte v)
        {
            WritePrimitive((ulong)v);
        }

        void WriteRaw(byte v)
        {
            Span<byte> span = _writer.GetSpan(1);
            span[0] = v;
            _writer.Advance(1);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        void WritePrimitive(sbyte v)
        {
            WritePrimitive((long)v);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        void WritePrimitive(short v)
        {
            WritePrimitive((long)v);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        void WritePrimitive(ushort v)
        {
            WritePrimitive((ulong)v);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        void WritePrimitive(int v)
        {
            WritePrimitive((long)v);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        void WritePrimitive(uint v)
        {
            WritePrimitive((ulong)v);
        }

        void WritePrimitive(long v)
        {
            WriteDelimiter();
            
            long value = v;

            if (v < 0)
            {
                if (v == long.MinValue)
                {
                    var offset = 0;
                    Span<byte> span = _writer.GetSpan(20);
                    span[offset++] = (byte)'-';
                    span[offset++] = (byte)'9';
                    span[offset++] = (byte)'2';
                    span[offset++] = (byte)'2';
                    span[offset++] = (byte)'3';
                    span[offset++] = (byte)'3';
                    span[offset++] = (byte)'7';
                    span[offset++] = (byte)'2';
                    span[offset++] = (byte)'0';
                    span[offset++] = (byte)'3';
                    span[offset++] = (byte)'6';
                    span[offset++] = (byte)'8';
                    span[offset++] = (byte)'5';
                    span[offset++] = (byte)'4';
                    span[offset++] = (byte)'7';
                    span[offset++] = (byte)'7';
                    span[offset++] = (byte)'5';
                    span[offset++] = (byte)'8';
                    span[offset++] = (byte)'0';
                    span[offset++] = (byte)'8';
                    _writer.Advance(20);
                    return;
                }

                WriteRaw((byte)'-');
                value = unchecked(-value);
            }

            WriteUlong((ulong)value);
        }

        void WritePrimitive(ulong v)
        {
            WriteDelimiter();

            WriteUlong(v);
        }

        void WriteUlong(ulong v)
        {
            if (v < 10)
            {
                WriteRaw((byte)('0' + v));
                return;
            }

            ulong value = v;
            var digits = (int)Math.Floor(Math.Log10(v) + 1);

            Span<byte> span = _writer.GetSpan(digits);
            for (var i = digits; i > 0; i--)
            {
                var temp = '0' + value;
                value /= 10;
                span[i - 1] = (byte)(temp - value * 10);
            }

            _writer.Advance(digits);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        void WritePrimitive(char v)
        {
            WritePrimitive((ulong)v);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        void WritePrimitive(float v)
        {
            WritePrimitive(string.Format("{0:G9}", v));
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        void WritePrimitive(double v)
        {
            WritePrimitive(string.Format("{0:G17}", v));
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        void WritePrimitive(decimal v)
        {
            WritePrimitive(v.ToString(CultureInfo.InvariantCulture));
        }

        void WritePrimitive(string v)
        {
            WriteDelimiter();
            WriteString(v);
        }

        void WriteIndentBreakForHuman(int depth)
        {
            if (_indent > 0)
            {
                var size = depth * _indent + 1;
                Span<byte> span = _writer.GetSpan(size);

                span[0] = (byte)'\n';

                for (int i = 1; i < size; i++)
                {
                    span[i] = (byte)' ';
                }

                _writer.Advance(size);
            }
        }

        void WriteSpaceForHuman()
        {
            if (_indent > 0)
            {
                WriteRaw((byte)' ');
            }
        }

        void WriteDelimiter()
        {
            var state = _states.Peek();
            if (state.Kind == StateKind.ArrayHead)
            {
                WriteIndentBreakForHuman(state.Depth);

                _states.Pop();
                _states.Push(new State
                {
                    Kind = StateKind.ArrayOther,
                    Depth = state.Depth
                });
                return;
            }

            if (state.Kind == StateKind.ObjectKeyHead)
            {
                WriteIndentBreakForHuman(state.Depth);
            }

            if (state.Kind == StateKind.ArrayOther || state.Kind == StateKind.ObjectKeyOther)
            {
                WriteRaw((byte)',');

                WriteIndentBreakForHuman(state.Depth);
            }

            if (state.Kind == StateKind.ObjectValue)
            {
                WriteSpaceForHuman();

                _states.Pop();
                _states.Push(new State
                {
                    Kind = StateKind.ObjectKeyOther,
                    Depth = state.Depth
                });
            }
        }

        void WriteString(string s)
        {
            var from = 0;
            var max = Encoding.UTF8.GetByteCount(s);
            Span<byte> span = _writer.GetSpan(max);

            for (var i = 0; i < s.Length; i += char.IsSurrogatePair(s, i) ? 2 : 1)
            {
                if (char.IsSurrogatePair(s, i))
                {
                    from += Encoding.UTF8.GetBytes(s.AsSpan(i, 2), span.Slice(from, 4));
                    continue;
                }

                if (s[i] > 0x7f)
                {
                    var cs = s.AsSpan(i, 1);
                    from += Encoding.UTF8.GetBytes(cs, span.Slice(from, Encoding.UTF8.GetByteCount(cs)));
                    continue;
                }

                var c = s[i];
                byte modified = default(byte);
                if (c <= 0x20 || c == '\"' || c == '\\')
                {
                    switch(c)
                    {
                        case '\"':
                            modified = (byte)'\"';
                            break;
    
                        case '\\':
                            modified = (byte)'\\';
                            break;
    
                        case '\b':
                            modified = (byte)'b';
                            break;
    
                        case '\n':
                            modified = (byte)'n';
                            break;
    
                        case '\r':
                            modified = (byte)'r';
                            break;
    
                        case '\t':
                            modified = (byte)'t';
                            break;
                    }
                }
    
                if (modified != default(char))
                {
                    span[from] = (byte)'\\';
                    span[from + 1] = modified;

                    from += 2;
                    max++;
                    span = _writer.GetSpan(max);
                    
                    continue;
                }
                
                span[from] = (byte)c;
                from++;
            }

            _writer.Advance(max);
        }
    }
}
