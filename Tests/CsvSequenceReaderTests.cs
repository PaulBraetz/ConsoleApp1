namespace Tests;

using System.Globalization;

using ConsoleApp1;

public class CsvSequenceReaderTests
{
    private static String PrepareCsvFile(String contents)
    {
        var path = Path.GetTempFileName();
        File.WriteAllText(path, contents);
        return path;
    }
    [Fact]
    public void TryReadRecord_yields_record_for_single_window()
    {
        var path = PrepareCsvFile("0000021001,2024-10-21T20:46:18.1937080+00:00,6.634817514544766,kWh");
        using var fileManager = MappedFileManager.Create(path);
        using var viewManager = fileManager.CreateView();
        var reader = CsvSequenceReader.Create(new(viewManager.Memory));

        var success = reader.TryReadRecord(out var record);
        Assert.True(success);
        Assert.Equal(21001, record.id);
        Assert.Equal(DateTimeOffset.Parse("2024-10-21T20:46:18.1937080+00:00", CultureInfo.InvariantCulture), record.timestamp);
        Assert.Equal(Double.Parse("6.634817514544766", CultureInfo.InvariantCulture), record.measurement, 1e-6);
    }
    [Fact]
    public void TryReadRecord_yields_record_for_split_window()
    {
        var path = PrepareCsvFile("0000021001,2024-10-21T20:46:18.1937080+00:00,6.634817514544766,kWh");
        using var fileManager = MappedFileManager.Create(path);
        using var viewManager = fileManager.CreateView(offset: 0, size: 16);
        var reader = CsvSequenceReader.Create(new(viewManager.Memory));

        var success = reader.TryReadRecord(out var record);
        Assert.True(success);
        Assert.Equal(21001, record.id);
        Assert.Equal(DateTimeOffset.Parse("2024-10-21T20:46:18.1937080+00:00", CultureInfo.InvariantCulture), record.timestamp);
        Assert.Equal(Double.Parse("6.634817514544766", CultureInfo.InvariantCulture), record.measurement, 1e-6);
    }
}