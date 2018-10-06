﻿using System;
using System.IO;
using System.Text;

namespace Bion
{
    public unsafe class BionReader : IDisposable
    {
        private Stream _stream;
        private Memory<byte> _buffer;

        private BionLookup _lookupDictionary;

        private BionMarker _currentMarker;
        private int _currentLength;
        private int _currentDepth;

        private short _lastPropertyLookupIndex;
        private string _currentDecodedString;

        // String and Property Name tokens, in order, are Len5b, Len2b, Len1b, Look1b, Look2b.
        private static sbyte[] LengthLookup = new sbyte[] { 5, 2, 1, -1, -2 };

        public BionReader(Stream stream) : this(stream, null)
        { }

        public BionReader(Stream stream, BionLookup lookupDictionary)
        {
            CloseStream = true;
            _stream = stream;
            _lookupDictionary = lookupDictionary;
        }

        public long BytesRead { get; private set; }
        public bool CloseStream { get; set; }
        public BionToken TokenType { get; private set; }

        public bool Read()
        {
            // Check for end (after reading one thing at the root depth)
            if (_currentDepth == 0 && BytesRead > 0)
            {
                TokenType = BionToken.None;
                return false;
            }

            // Read the current token marker
            _currentMarker = (BionMarker)ReadByte();
            _currentLength = 0;
            _currentDecodedString = null;
            BytesRead++;

            if(_currentMarker >= BionMarker.EndArray)
            {
                // Container. Token is all marker bits. LoL and Length zero.
                TokenType = (BionToken)_currentMarker;

                // Increment or Decrement depth
                _currentDepth += (_currentMarker >= BionMarker.StartArray ? 1 : -1);
            }
            else if(_currentMarker >= BionMarker.String)
            {
                // String
                TokenType = BionToken.String;
                _currentLength = ReadStringLength();
            }
            else if(_currentMarker >= BionMarker.PropertyName)
            {
                // Property Name
                TokenType = BionToken.PropertyName;
                _currentLength = ReadStringLength();
            }
            else if(_currentMarker >= BionMarker.False)
            {
                // Literal
                TokenType = (BionToken)_currentMarker;
            }
            else if(_currentMarker >= BionMarker.InlineInteger)
            {
                // Inline Int
                TokenType = BionToken.Integer;
            }
            else if(_currentMarker >= BionMarker.Float)
            {
                // Integer | NegativeInteger | Float
                TokenType = (_currentMarker >= BionMarker.NegativeInteger ? BionToken.Integer : BionToken.Float);

                // Length is last four bits
                _currentLength = (int)_currentMarker & 0x0F;
            }
            else
            {
                throw new BionSyntaxException($"@{BytesRead:n0}: Byte 0x{_currentMarker:X} is not a valid BION marker.");
            }

            // Read value
            if (_currentLength > 0)
            {
                _buffer = Read(_currentLength);
                BytesRead += _currentLength;
            }
            else
            {
                _buffer = Memory<byte>.Empty;
            }

            return true;
        }

        public bool Read(BionToken expected)
        {
            bool result = Read();
            Expect(expected);
            return result;
        }

        public void Expect(BionToken expected)
        {
            if (this.TokenType != expected) throw new BionSyntaxException(this, expected);
        }

        public bool CurrentBool()
        {
            if (TokenType == BionToken.True) return true;
            if (TokenType == BionToken.False) return false;
            throw new InvalidCastException($"@{BytesRead}: TokenType {TokenType} isn't a boolean type.");
        }

        public long CurrentInteger()
        {
            if (TokenType != BionToken.Integer) throw new BionSyntaxException($"@{BytesRead}: TokenType {TokenType} isn't an integer type.");

            // Inline Integer
            if (_currentLength == 0) return ((int)_currentMarker & 0x0F);

            // Decode 7-bit value
            ulong value = DecodeUnsignedInteger(_currentLength);

            // Negate if type was NegativeInteger
            if (_currentMarker < BionMarker.Integer)
            {
                return SafeNegate(value);
            }

            return (long)value;
        }

        public unsafe double CurrentFloat()
        {
            if (TokenType != BionToken.Float) throw new BionSyntaxException($"@{BytesRead}: TokenType {TokenType} isn't a float type.");

            // Decode as an integer and coerce .NET into reinterpreting the bytes
            ulong value = DecodeUnsignedInteger(_currentLength);

            if (_currentLength <= 5)
            {
                uint asInt = (uint)value;
                return (double)*(float*)&asInt;
            }
            else
            {
                
                return *(double*)&value;
            }
        }

        public string CurrentString()
        {
            if (TokenType == BionToken.Null) return null;
            if (TokenType != BionToken.PropertyName && TokenType != BionToken.String) throw new BionSyntaxException($"@{BytesRead}: TokenType {TokenType} isn't a string type.");

            if (_currentDecodedString == null)
            {
                _currentDecodedString = Encoding.UTF8.GetString(_buffer.Span);
            }

            return _currentDecodedString;
        }

        private int ReadStringLength()
        {
            sbyte lengthOfLength = LengthLookup[(byte)TokenType - (byte)_currentMarker];

            if (lengthOfLength < 0)
            {
                lengthOfLength = (sbyte)-lengthOfLength;
                _buffer = Read(lengthOfLength);
                BytesRead += lengthOfLength;

                short lookupIndex = (short)DecodeUnsignedInteger(lengthOfLength);
                if (_lookupDictionary == null) throw new BionSyntaxException($"@{BytesRead}: Found {TokenType} lookup for index {lookupIndex}, but no LookupDictionary was passed to the reader.");

                if (TokenType == BionToken.PropertyName)
                {
                    _currentDecodedString = _lookupDictionary.PropertyName(lookupIndex);
                    _lastPropertyLookupIndex = lookupIndex;
                }
                else
                {
                    // A string value lookup can only appear right after a property name which is also indexed; look up the index from last time.
                    _currentDecodedString = _lookupDictionary.Value(_lastPropertyLookupIndex, lookupIndex);
                }

                return 0;
            }
            else
            {
                _buffer = Read(lengthOfLength);
                BytesRead += lengthOfLength;
                return (int)DecodeUnsignedInteger(lengthOfLength);
            }
        }

        private ulong DecodeUnsignedInteger(int length)
        {
            ulong value = 0;

            for (int i = length - 1; i >= 0; --i)
            {
                value = value << 7;
                value += (ulong)(_buffer.Span[i] & 0x7F);
            }

            return value;
        }

        private long SafeNegate(ulong value)
        {
            // Decrement to ensure in range, then cast
            long inRange = (long)(value - 1);

            // Negate and undo the decrement
            return -inRange - 1;
        }

        public void Dispose()
        {
            if (_stream != null)
            {
                if (CloseStream) _stream.Dispose();
                _stream = null;
            }

            if(_lookupDictionary != null)
            {
                _lookupDictionary.Dispose();
                _lookupDictionary = null;
            }
        }

        #region Inlined Buffered Stream
        private byte ReadByte()
        {
            if (_innerIndex >= _innerLength) ReadNext(1);
            return _innerBuffer[_innerIndex++];
        }

        private Memory<byte> Read(int length)
        {
            if (_innerLength - _innerIndex < length) length = ReadNext(length);
            Memory<byte> result = new Memory<byte>(_innerBuffer, _innerIndex, length);
            _innerIndex += length;
            return result;
        }

        byte[] _innerBuffer = new byte[16384];
        int _innerIndex;
        int _innerLength;

        private int ReadNext(int size)
        {
            byte[] readInto = _innerBuffer;

            // Resize if needed
            if (size > _innerBuffer.Length)
            {
                readInto = new byte[Math.Max(_innerBuffer.Length * 5 / 4, size)];
            }

            // Copy unused bytes
            int lengthLeft = _innerLength - _innerIndex;
            if (lengthLeft > 0)
            {
                Buffer.BlockCopy(_innerBuffer, _innerIndex, readInto, 0, lengthLeft);
            }

            // Fill remaining buffer
            _innerLength = lengthLeft + _stream.Read(readInto, lengthLeft, readInto.Length - lengthLeft);

            // Reset variables
            _innerBuffer = readInto;
            _innerIndex = 0;

            // Return the safe size to read, if less than size
            return Math.Min(size, _innerLength);
        }
        #endregion
    }
}
