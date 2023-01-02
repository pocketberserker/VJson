//
// Copyright (c) 2019- yutopp (yutopp@gmail.com)
//
// Distributed under the Boost Software License, Version 1.0. (See accompanying
// file LICENSE_1_0.txt or copy at  https://www.boost.org/LICENSE_1_0.txt)
//

using System;
using System.IO;
using System.Text;
using System.Buffers;
using NUnit.Framework;

namespace VJson.UnitTests
{
    public class JsonWriterPrimitiveValueTests
    {
        [Test]
        public void ValueWriteTest()
        {
            var b = new ArrayBufferWriter<byte>();
            var f = new JsonWriter(b);
            {
                f.WriteValue(1);
            }

            var actual = Encoding.UTF8.GetString(b.WrittenMemory.ToArray());
            Assert.AreEqual("1", actual);
        }

        [Test]
        public void EmojiValueWriteTest()
        {
            var b = new ArrayBufferWriter<byte>();
            var f = new JsonWriter(b);
            {
                f.WriteValue("🍣");
            }

            // Check UTF-8 sequence
            var actualArr = b.WrittenMemory.ToArray();
            Assert.AreEqual(6, actualArr.Length);
            Assert.AreEqual(0x22, actualArr[0]);
            Assert.AreEqual(0xF0, actualArr[1]);
            Assert.AreEqual(0x9F, actualArr[2]);
            Assert.AreEqual(0x8D, actualArr[3]);
            Assert.AreEqual(0xA3, actualArr[4]);
            Assert.AreEqual(0x22, actualArr[5]);
            var actual = Encoding.UTF8.GetString(b.WrittenMemory.ToArray());
            Assert.AreEqual("\"🍣\"", actual);
        }

        [Test]
        public void EscapeSequenceValueWriteTest()
        {
            var b = new ArrayBufferWriter<byte>();
            var f = new JsonWriter(b);
            {
                f.WriteValue("\"\\/\b\n\r\t");
            }

            var actual = Encoding.UTF8.GetString(b.WrittenMemory.ToArray());
            Assert.AreEqual("\"\\\"\\\\/\\b\\n\\r\\t\"", actual);
        }
    }

    public class JsonWriterObjectTests
    {
        [Test]
        public void EmptyTest()
        {
            var b = new ArrayBufferWriter<byte>();
            var f = new JsonWriter(b);
            {
                f.WriteObjectStart();
                f.WriteObjectEnd();
            }

            var actual = Encoding.UTF8.GetString(b.WrittenMemory.ToArray());
            Assert.AreEqual(@"{}", actual);
        }

        [Test]
        public void SingleTest()
        {
            var b = new ArrayBufferWriter<byte>();
            var f = new JsonWriter(b);
            {
                f.WriteObjectStart();
                f.WriteObjectKey("foo");
                f.WriteValue(42);
                f.WriteObjectEnd();
            }

            var actual = Encoding.UTF8.GetString(b.WrittenMemory.ToArray());
            Assert.AreEqual(@"{""foo"":42}", actual);
        }

        [Test]
        public void MultiTest()
        {
            var b = new ArrayBufferWriter<byte>();
            var f = new JsonWriter(b);
            {
                f.WriteObjectStart();
                f.WriteObjectKey("foo");
                f.WriteValue(42);
    
                f.WriteObjectKey("bar");
                f.WriteValue(84);
                f.WriteObjectEnd();
            }

            var actual = Encoding.UTF8.GetString(b.WrittenMemory.ToArray());
            Assert.AreEqual(@"{""foo"":42,""bar"":84}", actual);
        }

        [Test]
        public void NestedTest()
        {
            var b = new ArrayBufferWriter<byte>();
            var f = new JsonWriter(b);
            {
                f.WriteObjectStart();
                f.WriteObjectKey("foo");

                f.WriteObjectStart();
                f.WriteObjectKey("bar");
                f.WriteValue(84);
                f.WriteObjectEnd();
                
                f.WriteObjectEnd();
            }

            var actual = Encoding.UTF8.GetString(b.WrittenMemory.ToArray());
            Assert.AreEqual(@"{""foo"":{""bar"":84}}", actual);
        }
    }

    public class JsonWriterArrayTests
    {
        [Test]
        public void EmptyTest()
        {
            var b = new ArrayBufferWriter<byte>();
            var f = new JsonWriter(b);
            {
                f.WriteArrayStart();
                f.WriteArrayEnd();
            }

            var actual = Encoding.UTF8.GetString(b.WrittenMemory.ToArray());
            Assert.AreEqual(@"[]", actual);
        }

        [Test]
        public void SingleTest()
        {
            var b = new ArrayBufferWriter<byte>();
            var f = new JsonWriter(b);
            {
                f.WriteArrayStart();
                f.WriteValue(42);
                f.WriteArrayEnd();
            }

            var actual = Encoding.UTF8.GetString(b.WrittenMemory.ToArray());
            Assert.AreEqual(@"[42]", actual);
        }

        [Test]
        public void MultiTest()
        {
            var b = new ArrayBufferWriter<byte>();
            var f = new JsonWriter(b);
            {
                f.WriteArrayStart();
                f.WriteValue(42);
                f.WriteValue("aaa");
                f.WriteArrayEnd();
            }

            var actual = Encoding.UTF8.GetString(b.WrittenMemory.ToArray());
            Assert.AreEqual(@"[42,""aaa""]", actual);
        }

        [Test]
        public void NestedTest()
        {
            var b = new ArrayBufferWriter<byte>();
            var f = new JsonWriter(b);
            {
                f.WriteArrayStart();
                f.WriteValue(42);
                f.WriteArrayStart();
                f.WriteValue("aaa");
                f.WriteArrayEnd();
                f.WriteArrayEnd();
            }

            var actual = Encoding.UTF8.GetString(b.WrittenMemory.ToArray());
            Assert.AreEqual(@"[42,[""aaa""]]", actual);
        }
    }

    public class JsonWriterCompoundTests
    {
        [Test]
        public void CompoundTest()
        {
            var b = new ArrayBufferWriter<byte>();
            var f = new JsonWriter(b);
            {
                f.WriteArrayStart();
    
                f.WriteObjectStart();
                f.WriteObjectKey("foo");
                f.WriteValue(42);
                f.WriteObjectEnd();
    
                f.WriteObjectStart();
                f.WriteObjectKey("foo");
                f.WriteArrayStart();
                f.WriteValue(84);
                f.WriteArrayEnd();
                f.WriteObjectEnd();
    
                f.WriteArrayEnd();
            }

            var actual = Encoding.UTF8.GetString(b.WrittenMemory.ToArray());
            Assert.AreEqual(@"[{""foo"":42},{""foo"":[84]}]", actual);
        }
    }
}
