namespace ConsoleApp1;

using System;
using System.Linq;

using Microsoft.CodeAnalysis;
using Spectre.Console;
using System.Buffers;
using System.Buffers.Text;
using System.Globalization;
using System.IO;

public ref struct CsvSequenceReader
{
    public CsvSequenceReader(SequenceReader<Byte> reader) => _reader = reader;

    const Int32 _idFieldLength = 10;
    const Char _separator = ',';
    const Int32 _timestampFieldLength = 33;
    const Int32 _maxDoubleByteLength = 512;
    const Int32 _unitFieldLength = 3;
    const Int32 _headerScanLimit = 512;
    const Int32 _eolScanLimit = 512;
    static readonly Byte[] _newline = [(Byte)'\r', (Byte)'\n'];

    SequenceReader<Byte> _reader;

    //public static CsvSequenceReader Create(Stream source)
    //{
    //    //how to get sequence reader??
    //}

    public static CsvSequenceReader Create(MappedFileMemoryManager viewManager)
    {
        var reader = new SequenceReader<Byte>(new ReadOnlySequence<Byte>(viewManager.Memory));
        var result = new CsvSequenceReader(reader);

        return result;
    }

    public Boolean TryReadEndOfLine()
    {
        var length = (Int32)Math.Min(_reader.Remaining, _headerScanLimit);
        if(!_reader.TryReadExact(length, out var scanSequence))
            return false;
        var newlineIndex = scanSequence.FirstSpan.IndexOf(_newline);
        if(newlineIndex == -1)
            return false;

        _reader.Rewind(scanSequence.Length - newlineIndex - _newline.Length);

        return true;
    }
    public Boolean TryReadRecord(out (Int64 id, DateTimeOffset timestamp, Double measurement) record)
    {
        do
        {
            if(!TryReadId(out var id))
                continue;
            if(!TryReadSeparator())
                continue;
            if(!TryReadTimestamp(out var timestamp))
                continue;
            if(!TryReadSeparator())
                continue;
            if(!TryReadMeasurement(out var measurement))
                continue;
            if(!TryReadSeparator())
                continue;
            if(!TryReadUnit())
                continue;
            {
                record = (id, timestamp, measurement);
                return true;
            }
        } while(TryReadEndOfLine());

        record = default;
        return false;
    }
    public Boolean TryReadNewLine()
    {
        if(!_reader.TryReadExact(_newline.Length, out var newlineBytes))
            return false;
        if(!newlineBytes.FirstSpan.SequenceEqual(_newline))
            return false;

        return true;
    }
    public Boolean TryReadUnit()
    {
        if(!_reader.TryReadExact(_unitFieldLength, out _))
            return false;

        return true;
    }
    public Boolean TryReadMeasurement(out Double measurement)
    {
        measurement = default;
        var length = (Int32)Math.Min(_reader.Remaining, _maxDoubleByteLength);
        if(!_reader.TryReadExact(length, out var doubleBytes))
            return false;
        var commaIndex = doubleBytes.FirstSpan.IndexOf((Byte)',');
        if(commaIndex == -1)
            return false; //unterminated field (missing unit field after)
        if(!Double.TryParse(doubleBytes.FirstSpan[..commaIndex], CultureInfo.InvariantCulture, out measurement))
            return false;

        _reader.Rewind(doubleBytes.Length - commaIndex);

        return true;
    }
    public Boolean TryReadTimestamp(out DateTimeOffset timestamp)
    {
        timestamp = default;
        if(!_reader.TryReadExact(_timestampFieldLength, out var timestampBytes))
            return false;
        if(!Utf8Parser.TryParse(timestampBytes.FirstSpan, out timestamp, out _, standardFormat: 'O'))
            return false;

        return true;
    }
    public Boolean TryReadSeparator()
    {
        if(!_reader.TryRead(out var separatorByte))
            return false;
        if(separatorByte != _separator)
            return false;

        return true;
    }
    public Boolean TryReadId(out Int64 id)
    {
        id = default;
        if(!_reader.TryReadExact(_idFieldLength, out var idBytes))
            return false;
        if(!Int64.TryParse(idBytes.FirstSpan, out id))
            return false;

        return true;
    }
}
