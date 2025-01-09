//using FASTER.core;
//using System.Diagnostics;
//using 亂七八糟.Shared;

//namespace 亂七八糟.Logging;


//using Microsoft.Extensions.Logging;


///// <summary>
///// <see href="https://github.com/microsoft/FASTER/blob/main/cs/samples/FasterLogSample/Program.cs""/>sample</see>
///// </summary>
///// <param name="logger"></param>
//public class FASTERService1(
//    string name,
//    ILogger<FASTERService1>? logger,
//    FasterLogSettings? config,
//    Func<FasterServiceConfiguration> getCurrentConfig) : IFASTERService, ILogger<FASTERService1>
//{
//    // Entry length can be between 1 and ((1 << FasterLogSettings.PageSizeBits) - 4)
//    protected const int entryLength = 1 << 10;
//    protected const string DEFAULT = "default";
//    protected static readonly byte[] staticEntry = new byte[entryLength];

//    private readonly FasterLog? _fasterLog = _lazyDefault.Value.FasterLog;
//    protected virtual FasterLog FasterLog
//    {
//        get => _fasterLog ?? _lazyDefault.Value.FasterLog;
//        init
//        {
//            _fasterLog = new FasterLog(config);
//        }
//    }

//    private readonly FasterLogScanIterator? _fasterLogScanIterator = _lazyDefault.Value.FasterLogScanIterator;
//    protected virtual FasterLogScanIterator FasterLogScanIterator
//    {
//        get => _fasterLogScanIterator ?? _lazyDefault.Value.FasterLogScanIterator;
//        init
//        {
//            _fasterLogScanIterator = FasterLog.Scan(FasterLog.BeginAddress, long.MaxValue);
//        }
//    }

//    public static readonly FasterLogSettings FasterLogSettings = new FasterLogSettings("./FasterLogSample", deleteDirOnDispose: true);

//    protected virtual ILogger<FASTERService1>? Logger => logger;

//    public FASTERService1(
//        string name,
//        ILoggerFactory? loggerFactory,
//        FasterLogSettings? config,
//        Func<FasterServiceConfiguration> getCurrentConfig) : this(
//        name, loggerFactory?.CreateLogger<FASTERService1>() ?? null, config, getCurrentConfig)
//    {
//        bool sync = true;

//        // Populate entry being inserted
//        for (int i = 0; i < entryLength; i++)
//        {
//            staticEntry[i] = (byte)i;
//        }

//        // Create settings to write logs and commits at specified local path
//        using FasterLogSettings? settings = config ?? new FasterLogSettings("./FasterLogSample", deleteDirOnDispose: true);

//        // FasterLog will recover and resume if there is a previous commit found
//        _fasterLog = new FasterLog(settings);

//        using (FasterLogScanIterator = _fasterLog.Scan(_fasterLog.BeginAddress, long.MaxValue))
//        {
//            if (sync)
//            {
//                // Log writer thread: create as many as needed
//                new Thread(new ThreadStart(LogWriterThread)).Start();

//                // Threads for iterator scan: create as many as needed
//                new Thread(() => ScanThread()).Start();

//                // Threads for reporting, commit
//                new Thread(new ThreadStart(ReportThread)).Start();
//                var t = new Thread(new ThreadStart(CommitThread));
//                t.Start();
//                t.Join();
//            }
//            else
//            {
//                // Async version of demo: expect lower performance
//                // particularly for small payload sizes

//                const int NumParallelTasks = 10_000;
//                ThreadPool.SetMinThreads(2 * Environment.ProcessorCount, 2 * Environment.ProcessorCount);
//                TaskScheduler.UnobservedTaskException += (object sender, UnobservedTaskExceptionEventArgs e) =>
//                {
//                    Console.WriteLine($"Unobserved task exception: {e.Exception}");
//                    e.SetObserved();
//                };

//                Task[] tasks = new Task[NumParallelTasks];
//                for (int i = 0; i < NumParallelTasks; i++)
//                {
//                    int local = i;
//                    tasks[i] = Task.Run(() => AsyncLogWriter(local));
//                }

//                var scan = Task.Run(() => AsyncScan());

//                // Threads for reporting, commit
//                new Thread(new ThreadStart(ReportThread)).Start();
//                new Thread(new ThreadStart(CommitThread)).Start();

//                Task.WaitAll(tasks);
//                Task.WaitAll(scan);

//            }
//        }
//    }

//    protected static readonly Lazy<FASTERService1> _lazyDefault = new Lazy<FASTERService1>(() =>
//    {
//        ILoggerFactory loggerFactory = LoggerFactory.Create(builder => builder.AddConsole());
//        return new FASTERService1(DEFAULT, loggerFactory: loggerFactory, FasterLogSettings, () => FasterServiceConfiguration.Default);
//    });

//    public static void LogWriterThread()
//    {
//        FasterLog log = _lazyDefault.Value.FasterLog;

//        while (true)
//        {
//            // TryEnqueue - can be used with throttling/back-off
//            // Accepts byte[] and ReadOnlySpan<byte>
//            while (!log.TryEnqueue(staticEntry, out _)) ;

//            // Synchronous blocking enqueue
//            // Accepts byte[] and ReadOnlySpan<byte>
//            // log.Enqueue(entry);

//            // Batched enqueue - batch must fit on one page
//            // Add this to class:
//            //   static readonly ReadOnlySpanBatch spanBatch = new ReadOnlySpanBatch(10);
//            // while (!log.TryEnqueue(spanBatch, out _)) ;
//        }
//    }

//    /// <summary>
//    /// Async version of enqueue
//    /// </summary>
//    public static async Task AsyncLogWriter(int id)
//    {
//        bool batched = false;

//        await Task.Yield();

//        FasterLog log = _lazyDefault.Value.FasterLog;

//        if (!batched)
//        {
//            // Single commit version - append each item and wait for commit
//            // Needs high parallelism (NumParallelTasks) for perf
//            // Needs separate commit thread to perform regular commit
//            // Otherwise we commit only at page boundaries
//            while (true)
//            {
//                try
//                {
//                    await log.EnqueueAndWaitForCommitAsync(staticEntry);
//                }
//                catch (Exception ex)
//                {
//                    Console.WriteLine($"{nameof(AsyncLogWriter)}({id}): {ex}");
//                }
//            }
//        }
//        else
//        {
//            // Batched version - we enqueue many entries to memory,
//            // then wait for commit periodically
//            int count = 0;
//            while (true)
//            {
//                await log.EnqueueAsync(staticEntry);
//                if (count++ % 100 == 0)
//                {
//                    await log.WaitForCommitAsync();
//                }
//            }
//        }
//    }

//    public static void ScanThread()
//    {
//        byte[] result;

//        FasterLogScanIterator iter = _lazyDefault.Value.FasterLogScanIterator;
//        FasterLog log = _lazyDefault.Value.FasterLog;

//        while (true)
//        {
//            while (!iter.GetNext(out result, out _, out _))
//            {
//                if (iter.Ended) return;
//                iter.WaitAsync().AsTask().GetAwaiter().GetResult();
//            }

//            // Memory pool variant:
//            // iter.GetNext(pool, out IMemoryOwner<byte> resultMem, out int length, out long currentAddress)

//            if (Different(result, staticEntry))
//                throw new Exception("Invalid entry found");

//            // Example of random read from given address
//            // (result, _) = log.ReadAsync(iter.CurrentAddress).GetAwaiter().GetResult();

//            // Truncate until start of most recently read page
//            log.TruncateUntilPageStart(iter.NextAddress);

//            // Truncate log until after most recently read entry
//            // log.TruncateUntil(iter.NextAddress);
//        }

//        // Example of recoverable (named) iterator:
//        // using (iter = log.Scan(log.BeginAddress, long.MaxValue, "foo"))
//    }

//    public static async Task AsyncScan()
//    {
//        FasterLog log = _lazyDefault.Value.FasterLog;
//        FasterLogScanIterator iter = log.Scan(log.BeginAddress, long.MaxValue);

//        await foreach ((byte[] result, int length, long currentAddress, long nextAddress) in iter.GetAsyncEnumerable())
//        {
//            if (Different(result, staticEntry))
//                throw new Exception("Invalid entry found");
//            log.TruncateUntilPageStart(iter.NextAddress);
//        }
//    }

//    public static async void ReportThread()
//    {
//        FasterLog log = _lazyDefault.Value.FasterLog;
//        FasterLogScanIterator iter = _lazyDefault.Value.FasterLogScanIterator;

//        long lastTime = 0;
//        long lastValue = log.TailAddress;
//        long lastIterValue = log.BeginAddress;

//        Stopwatch sw = new();
//        sw.Start();

//        while (true)
//        {
//            await Task.Delay(5000);

//            var nowTime = sw.ElapsedMilliseconds;
//            var nowValue = log.TailAddress;

//            Console.WriteLine("Append Throughput: {0} MB/sec, Tail: {1}",
//                (nowValue - lastValue) / (1000 * (nowTime - lastTime)), nowValue);
//            lastValue = nowValue;

//            if (iter != null)
//            {
//                var nowIterValue = iter.NextAddress;
//                Console.WriteLine("Scan Throughput: {0} MB/sec, Iter pos: {1}",
//                    (nowIterValue - lastIterValue) / (1000 * (nowTime - lastTime)), nowIterValue);
//                lastIterValue = nowIterValue;
//            }

//            lastTime = nowTime;
//        }
//    }

//    public static void CommitThread()
//    {
//        FasterLog log = _lazyDefault.Value.FasterLog;
//        //Task<LinkedCommitInfo> prevCommitTask = null;
//        while (true)
//        {
//            Thread.Sleep(5);
//            log.Commit(true);

//            // Async version
//            // await log.CommitAsync();

//            // Async version that catches all commit failures in between
//            //try
//            //{
//            //    prevCommitTask = await log.CommitAsync(prevCommitTask);
//            //}
//            //catch (CommitFailureException e)
//            //{
//            //    Console.WriteLine(e);
//            //    prevCommitTask = e.LinkedCommitInfo.nextTcs.Task;
//            //}
//        }
//    }

//    private static bool Different(ReadOnlySpan<byte> b1, ReadOnlySpan<byte> b2)
//    {
//        return !b1.SequenceEqual(b2);
//    }

//    public bool IsEnabled(LogLevel logLevel) =>
//        getCurrentConfig().LogLevelToColorMap.ContainsKey(logLevel);

//    public void Log<TState>(
//        LogLevel logLevel,
//        EventId eventId,
//        TState state,
//        Exception? exception,
//        Func<TState, Exception?, string> formatter)
//    {
//        if (!IsEnabled(logLevel))
//        {
//            return;
//        }

//        FasterServiceConfiguration config = getCurrentConfig();
//        if (config.EventId == 0 || config.EventId == eventId.Id)
//        {
//            ConsoleColor originalColor = Console.ForegroundColor;

//            Console.ForegroundColor = config.LogLevelToColorMap[logLevel];
//            Console.WriteLine($"[{eventId.Id,2}: {logLevel,-12}]");

//            Console.ForegroundColor = originalColor;
//            Console.Write($"{name} - ");

//            Console.ForegroundColor = config.LogLevelToColorMap[logLevel];
//            Console.Write($"{formatter(state, exception)}");

//            Console.ForegroundColor = originalColor;
//            Console.WriteLine();
//        }
//    }

//    public IDisposable? BeginScope<TState>(TState state) where TState : notnull
//    {
//        return default;
//    }

//    protected struct ReadOnlySpanBatch : IReadOnlySpanBatch
//    {
//        private readonly int _batchSize;
//        public ReadOnlySpanBatch(int batchSize) => _batchSize = batchSize;
//        public ReadOnlySpan<byte> Get(int index) => staticEntry;
//        public int TotalEntries() => _batchSize;
//    }
//}