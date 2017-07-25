using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Flee.Parsing.grammatica_1._5.alpha2.PerCederberg.Grammatica.Runtime
{
    /**
     * A character buffer that automatically reads from an input source
     * stream when needed. This class keeps track of the current position
     * in the buffer and its line and column number in the original input
     * source. It allows unlimited look-ahead of characters in the input,
     * reading and buffering the required data internally. As the
     * position is advanced, the buffer content prior to the current
     * position is subject to removal to make space for reading new
     * content. A few characters before the current position are always
     * kept to enable boundary condition checks.
     */
    internal class ReaderBuffer
    {
        public const int BlockSize = 1024;
        private char[] _buffer = new char[BlockSize * 4];
        private int _pos = 0;
        private int _length = 0;
        private TextReader _input;
        private int _line = 1;
        private int _column = 1;

        public ReaderBuffer(TextReader input)
        {
            this._input = input;
        }
        public void Dispose()
        {
            _buffer = null;
            _pos = 0;
            _length = 0;
            if (_input != null)
            {
                try
                {
                    _input.Close();
                }
                catch (Exception)
                {
                    // Do nothing
                }
                _input = null;
            }
        }

        public int Position => _pos;
        public int LineNumber => _line;
        public int ColumnNumber => _column;
        public int Length => _length;

        public string Substring(int index, int length)
        {
            return new string(_buffer, index, length);
        }

        public override string ToString()
        {
            return new string(_buffer, 0, _length);
        }

        public int Peek(int offset)
        {
            int index = _pos + offset;

            // Avoid most calls to EnsureBuffered(), since we are in a
            // performance hotspot here. This check is not exhaustive,
            // but only present here to speed things up.
            if (index >= _length)
            {
                EnsureBuffered(offset + 1);
                index = _pos + offset;
            }
            return (index >= _length) ? -1 : _buffer[index];
        }

        public string Read(int offset)
        {
            EnsureBuffered(offset + 1);
            if (_pos >= _length)
            {
                return null;
            }
            else
            {
                var count = _length - _pos;
                if (count > offset)
                {
                    count = offset;
                }
                UpdateLineColumnNumbers(count);
                var result = new string(_buffer, _pos, count);
                _pos += count;
                if (_input == null && _pos >= _length)
                {
                    Dispose();
                }
                return result;
            }
        }

        private void UpdateLineColumnNumbers(int offset)
        {
            for (int i = 0; i < offset; i++)
            {
                if (_buffer[_pos + i] == '\n')
                {
                    _line++;
                    _column = 1;
                }
                else
                {
                    _column++;
                }
            }
        }

        private void EnsureBuffered(int offset)
        {
            // Check for end of stream or already read characters
            if (_input == null || _pos + offset < _length)
            {
                return;
            }

            // Remove (almost all) old characters from buffer
            if (_pos > BlockSize)
            {
                _length -= (_pos - 16);
                Array.Copy(_buffer, _pos - 16, _buffer, 0, _length);
                _pos = 16;
            }

            // Calculate number of characters to read
            var size = _pos + offset - _length + 1;
            if (size % BlockSize != 0)
            {
                size = (1 + size / BlockSize) * BlockSize;
            }
            EnsureCapacity(_length + size);

            // Read characters
            try
            {
                while (_input != null && size > 0)
                {
                    var readSize = _input.Read(_buffer, _length, size);
                    if (readSize > 0)
                    {
                        _length += readSize;
                        size -= readSize;
                    }
                    else
                    {
                        _input.Close();
                        _input = null;
                    }
                }
            }
            catch (IOException e)
            {
                _input = null;
                throw e;
            }
        }

        private void EnsureCapacity(int size)
        {
            if (_buffer.Length >= size)
            {
                return;
            }
            if (size % BlockSize != 0)
            {
                size = (1 + size / BlockSize) * BlockSize;
            }
            Array.Resize(ref _buffer, size);
        }
    }
}
