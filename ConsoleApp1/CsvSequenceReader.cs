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
    const Byte _separator = (Byte)',';
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

    public static CsvSequenceReader Create(ReadOnlySequence<Byte> sequence)
    {
        var reader = new SequenceReader<Byte>(sequence);
        var result = new CsvSequenceReader(reader);

        return result;
    }

    public Boolean TryReadEndOfLine()
    {
        var result = _reader.TryReadTo(out ReadOnlySpan<Byte> _, _newline.AsSpan(), advancePastDelimiter: true);

        return result;
        /*
        var length = (Int32)Math.Min(_reader.Remaining, _headerScanLimit);
        if(!_reader.TryReadExact(length, out var scanSequence))
            return false;

        var potentialNewlineIndex = -1;
        var previousSpanNewlineTailMatchCount = 0;

        //reminder to look into the api you're using before doing complex stuff like this
        foreach(var memory in scanSequence)
        {
            //did the previous memory detect a partial newline match?
            if(previousSpanNewlineTailMatchCount > 0)
            {
                var remainingCount = _newline.Length - previousSpanNewlineTailMatchCount;
                var remainder = _newline.AsSpan()[..remainingCount];
                //does current memory begin with remaining newline chars?
                if(memory.Span.StartsWith(remainder))
                {
                    //rewind using the previously stored potential newline index
                    _reader.Rewind(scanSequence.Length - potentialNewlineIndex - _newline.Length);
                    return true;
                }
            }

            var newlineIndex = memory.Span.IndexOf(_newline);
            if(newlineIndex != -1)
            {
                _reader.Rewind(scanSequence.Length - newlineIndex - _newline.Length);
                return true;
            }

            if(_newline.Length > 1)
            {
                //reset partial/split newline detection
                potentialNewlineIndex = -1;
                previousSpanNewlineTailMatchCount = 0;
                //rewind to end of examined memory
                _reader.Rewind(length - memory.Length);
                for(var i = _newline.Length - 1; i > 0; i++)
                {
                    //rewind to longest possible partial newline match
                    _reader.Rewind(i);
                    //extract tail
                    if(!_reader.TryReadExact(i, out var memoryTail))
                        throw new InvalidOperationException("impossible state");
                    //and check if newline begins with tail
                    if(new ReadOnlySpan<Byte>(_newline).IndexOf(memoryTail.FirstSpan) == 0)
                    {
                        potentialNewlineIndex = _reader.Position.GetInteger() - i;
                        previousSpanNewlineTailMatchCount = i;
                        break;
                    }
                }
                //advance to the end of scanSequence
                _reader.Advance(length - memory.Length);
                //reader is at same position as after extracting scanSequence
            }
        }

        return false;
        */
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
            if(!TryReadEndOfLine())
                continue;
            {
                record = (id, timestamp, measurement);
                return true;
            }
        } while(TryReadEndOfLine());

        record = default;
        return false;
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

        var commaIndex = doubleBytes.IndexOf(_separator);
        if(commaIndex == -1)
            return false; //unterminated field (missing unit field after)

        var slice = doubleBytes.Slice(0, commaIndex);
        if(!slice.TryParseAsDouble(out measurement))
            return false;

        _reader.Rewind(doubleBytes.Length - commaIndex);

        return true;
    }
    public Boolean TryReadTimestamp(out DateTimeOffset timestamp)
    {
        timestamp = default;
        var timestampBytes = (Span<Byte>)stackalloc Byte[_timestampFieldLength];
        if(!_reader.TryCopyTo(timestampBytes))
            return false;
        _reader.Advance(timestampBytes.Length);
        if(!Utf8Parser.TryParse(timestampBytes, out timestamp, out _, standardFormat: 'O'))
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
        var idBytes = (Span<Byte>)stackalloc Byte[_idFieldLength];
        if(!_reader.TryCopyTo(idBytes))
            return false;
        _reader.Advance(idBytes.Length);
        if(!Int64.TryParse(idBytes, out id))
            return false;

        return true;
    }
}

static class MemoryExtensions
{
    //public static Boolean TryCopyToAdvance(this ref SequenceReader<Byte> reader, Span<Byte> destination)
    //{
    //    if(reader.TryCopyTo(destination))
    //    {
    //        reader.Advance(destination.Length);
    //        return true;
    //    }

    //    return false;
    //}
    public static Int32 IndexOf(this ReadOnlySequence<Byte> sequence, Byte item)
    {
        if(sequence.IsSingleSegment)
            return sequence.FirstSpan.IndexOf(item);

        var predecessorsSize = 0;
        foreach(var memory in sequence)
        {
            var localIndex = memory.Span.IndexOf(item);
            if(localIndex != -1)
                return localIndex + predecessorsSize;

            predecessorsSize += memory.Length;
        }

        return -1;
    }
    public static Boolean TryParseAsDouble(this ReadOnlySequence<Byte> sequence, out Double value)
    {
        if(sequence.IsSingleSegment)
            return Double.TryParse(sequence.FirstSpan, CultureInfo.InvariantCulture, out value);

        var doubleBytes = (Span<Byte>)stackalloc Byte[checked((Int32)sequence.Length)];
        if(!new SequenceReader<Byte>(sequence).TryCopyTo(doubleBytes))
            throw new InvalidOperationException("impossible state");

        var result = Double.TryParse(doubleBytes, CultureInfo.InvariantCulture, out value);

        return result;
    }
}