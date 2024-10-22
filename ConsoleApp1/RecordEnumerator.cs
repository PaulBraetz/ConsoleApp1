namespace ConsoleApp1;

using System;

ref struct RecordEnumerator(CsvSequenceReader reader)
{
    private CsvSequenceReader _reader = reader;
    private (Int64 id, DateTimeOffset timestamp, Double measurement) _current;
    public (Int64 id, DateTimeOffset timestamp, Double measurement) Current
    {
        get => _current;
        private set => _current = value;
    }
    public Boolean MoveNext() => _reader.TryReadRecord(out _current);
    public RecordEnumerator GetEnumerator() => this;
}
