namespace 亂七八糟.Shared;

public interface IFASTERService
{
    void LogWriterThread();
    void AsyncLogWriter(int id);
    Task AsyncLogWriterAsync(int id, CancellationToken cancellationToken);
    void ScanThread();
    Task ScanThreadAsync(CancellationToken cancellationToken);
    void AsyncScan(CancellationToken cancellationToken);
    Task AsyncScanAsync(CancellationToken cancellationToken);
    void ReportThread(TimeSpan? timeOut);
    Task ReportThreadAsync(TimeSpan? timeOut, CancellationToken cancellationToken);
    void CommitThread(TimeSpan? timeSpan);
    Task CommitThreadAsync(TimeSpan? timeOut, CancellationToken cancellationToken);
}