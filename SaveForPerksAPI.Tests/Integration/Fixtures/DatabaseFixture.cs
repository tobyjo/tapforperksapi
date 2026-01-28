using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using SaveForPerksAPI.DbContexts;
using SaveForPerksAPI.Entities;
using SaveForPerksAPI.Models;
using SaveForPerksAPI.Repositories;
using SaveForPerksAPI.Services;

namespace SaveForPerksAPI.Tests.Integration.Fixtures;

/// <summary>
/// Provides an in-memory database and services for integration tests.
/// Each test gets a fresh database to ensure complete isolation from real database.
/// NO connection to real SQL Server - everything is in-memory.
/// </summary>
public class DatabaseFixture : IDisposable
{
    private string _currentDatabaseName;
    
    public TapForPerksContext Context { get; private set; }
    public ISaveForPerksRepository Repository { get; private set; }
    public IMapper Mapper { get; private set; }
    public ILogger<RewardTransactionService> Logger { get; private set; }
    public RewardTransactionService Service { get; private set; }

    public DatabaseFixture()
    {
        ResetDatabase();
    }

    /// <summary>
    /// Creates a fresh in-memory database.
    /// Call this to get a completely clean database for each test.
    /// This ensures NO data leaks between tests and NO connection to real database.
    /// </summary>
    public void ResetDatabase()
    {
        // Dispose old context if exists
        Context?.Dispose();
        
        // Create NEW in-memory database with unique name
        _currentDatabaseName = Guid.NewGuid().ToString();
        
        var options = new DbContextOptionsBuilder<TapForPerksContext>()
            .UseInMemoryDatabase(databaseName: _currentDatabaseName) // Unique DB per reset
            .EnableSensitiveDataLogging()
            .Options;

        Context = new TapForPerksContext(options);
        
        // Ensure database is created (in-memory)
        Context.Database.EnsureCreated();

        // Create real repository (uses in-memory context)
        Repository = new SaveForPerksRepository(Context);

        // Create basic mapper for testing (maps ScanEvent to ScanEventDto)
        var mockMapper = new Mock<IMapper>();
        mockMapper.Setup(m => m.Map<ScanEventDto>(It.IsAny<ScanEvent>()))
            .Returns((ScanEvent se) => new ScanEventDto
            {
                Id = se.Id,
                UserId = se.UserId,
                RewardId = se.RewardId,
                QrCodeValue = se.QrCodeValue,
                PointsChange = se.PointsChange,
                ScannedAt = se.ScannedAt,
                RewardOwnerUserId = se.RewardOwnerUserId
            });
        Mapper = mockMapper.Object;

        // Create logger (minimal output for tests)
        var loggerFactory = LoggerFactory.Create(builder => 
            builder.AddConsole().SetMinimumLevel(LogLevel.Warning));
        Logger = loggerFactory.CreateLogger<RewardTransactionService>();

        // Create real service with in-memory dependencies
        Service = new RewardTransactionService(Repository, Mapper, Logger);
    }

    public void Dispose()
    {
        Context?.Database.EnsureDeleted(); // Clean up in-memory database
        Context?.Dispose();
    }
}
