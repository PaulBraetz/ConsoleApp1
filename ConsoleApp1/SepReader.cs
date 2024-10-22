namespace ConsoleApp1;

using System;
using System.Collections.Generic;
using System.Linq;

using nietras.SeparatedValues;

class SepReader(String path)
{
    public IEnumerable<(Int64 id, DateTimeOffset timestamp, Double measurement)> Read()
    {
        using var reader = Sep.Reader().FromFile(path);

        _ = reader.MoveNext();

        foreach(var row in reader)
        {
            var record = (id: row[0].Parse<Int64>(), timestamp: row[1].Parse<DateTimeOffset>(), measurement: row[2].Parse<Double>());
            yield return record;
        }

        reader.Dispose();
    }

    public IEnumerable<(Int64 id, DateTimeOffset timestamp, Double measurement)> ReadParallel()
    {
        using var reader = Sep.Reader().FromFile(path);
        var result = reader.ParallelEnumerate(row => (id: row[0].Parse<Int64>(), timestamp: row[1].Parse<DateTimeOffset>(), measurement: row[2].Parse<Double>()));

        return result;
    }
}
