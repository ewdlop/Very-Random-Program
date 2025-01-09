namespace 亂七八糟.Shared;

public interface IFASTERService
{
    void LogWriterThread();
    Task AsyncLogWriterAsync(int id);
    void ScanThread();
    Task AsyncScanAsync();
    void ReportThread();
    void CommitThread();
}