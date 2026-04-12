using HSEM.Models;
using SQLite;

namespace HSEM.Services
{
    public class OfflineAttendanceService
    {
        private static readonly SemaphoreSlim _lock = new(1, 1);
        private readonly SQLiteAsyncConnection _db;

        public OfflineAttendanceService()
        {
            var dbPath = Path.Combine(
                FileSystem.AppDataDirectory,
                "attendance_offline.db3");

            _db = new SQLiteAsyncConnection(dbPath);
        }

        // =============================================
        //  Initialize — لازم تتستدعى قبل أي استخدام
        // =============================================
        public async Task InitializeAsync()
        {
            await _db.CreateTableAsync<LocalAttendanceRecord>();
            await _db.CreateTableAsync<LocalLocationEvent>();
            await MigrateAsync();
        }

        private async Task MigrateAsync()
        {
            try
            {
                var tableInfo = await _db.QueryAsync<ColumnNameInfo>(
                    "PRAGMA table_info(LocalAttendanceRecord)");

                var existingColumns = tableInfo
                    .Select(c => c.Name.ToLower())
                    .ToHashSet();

                if (!existingColumns.Contains("isofflinerecord"))
                    await _db.ExecuteAsync(
                        "ALTER TABLE LocalAttendanceRecord ADD COLUMN IsOfflineRecord INTEGER NOT NULL DEFAULT 0");

                if (!existingColumns.Contains("companylat"))
                    await _db.ExecuteAsync(
                        "ALTER TABLE LocalAttendanceRecord ADD COLUMN CompanyLat REAL NOT NULL DEFAULT 0");

                if (!existingColumns.Contains("companylng"))
                    await _db.ExecuteAsync(
                        "ALTER TABLE LocalAttendanceRecord ADD COLUMN CompanyLng REAL NOT NULL DEFAULT 0");

                if (!existingColumns.Contains("allowedradius"))
                    await _db.ExecuteAsync(
                        "ALTER TABLE LocalAttendanceRecord ADD COLUMN AllowedRadius REAL NOT NULL DEFAULT 0");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Migration error: {ex.Message}");
            }
        }

        // =============================================
        //  Attendance CRUD
        // =============================================

        // ✅ اسم موحد: SaveAsync
        public Task SaveAsync(LocalAttendanceRecord record)
            => _db.InsertAsync(record);

        // ✅ Alias للتوافق
        public Task SaveLocalAsync(LocalAttendanceRecord record)
            => SaveAsync(record);

        public Task<List<LocalAttendanceRecord>> GetPendingAsync()
            => _db.Table<LocalAttendanceRecord>()
                  .Where(r => !r.IsSynced)
                  .ToListAsync();

        public async Task DeleteAfterSyncAsync(int id)
        {
            await _lock.WaitAsync();
            try
            {
                var record = await _db.FindAsync<LocalAttendanceRecord>(id);
                if (record != null)
                    await _db.DeleteAsync(record);
            }
            finally { _lock.Release(); }
        }

        public Task<List<LocalAttendanceRecord>> GetAllAsync()
            => _db.Table<LocalAttendanceRecord>()
                  .OrderByDescending(r => r.DeviceTime)
                  .ToListAsync();

        // =============================================
        //  Location Events CRUD
        // =============================================
        public Task SaveLocationEventAsync(LocalLocationEvent evt)
            => _db.InsertAsync(evt);

        public Task<List<LocalLocationEvent>> GetPendingLocationEventsAsync()
            => _db.Table<LocalLocationEvent>()
                  .Where(e => !e.IsSynced)
                  .ToListAsync();

        public async Task DeleteLocationEventAsync(int id)
        {
            await _lock.WaitAsync();
            try
            {
                var evt = await _db.FindAsync<LocalLocationEvent>(id);
                if (evt != null)
                    await _db.DeleteAsync(evt);
            }
            finally { _lock.Release(); }
        }

        public async Task CleanOldSyncedAsync(int keepDays = 30)
        {
            var cutoff = DateTime.Now.AddDays(-keepDays);
            var old = await _db.Table<LocalAttendanceRecord>()
                               .Where(r => r.IsSynced && r.DeviceTime < cutoff)
                               .ToListAsync();
            foreach (var r in old)
                await _db.DeleteAsync(r);

            var oldEvents = await _db.Table<LocalLocationEvent>()
                               .Where(e => e.IsSynced && e.Time < cutoff)
                               .ToListAsync();
            foreach (var e in oldEvents)
                await _db.DeleteAsync(e);
        }
    }

    public class ColumnNameInfo
    {
        [SQLite.Column("name")]
        public string Name { get; set; }
    }
}