# Task 07 — Run AddRLS.sql automatically on boot 🟡 (review after)

Two small steps in two files. Do Step 1, then Step 2.

---

## Step 1 — make the .sql copy to the build output

**File:** `src/SalonOS.Infrastructure/SalonOS.Infrastructure.csproj`

**Find (exact):**
```xml
  <ItemGroup>
```
(the FIRST `<ItemGroup>` in the file)

**Replace with:**
```xml
  <ItemGroup>
    <None Update="Migrations\AddRLS.sql" CopyToOutputDirectory="PreserveNewest" />
  </ItemGroup>

  <ItemGroup>
```

**Done when:** the csproj has a `<None Update="Migrations\AddRLS.sql" ...>` line.

---

## Step 2 — execute it after migrations

**File:** `src/SalonOS.Api/Program.cs`

**Find (exact):**
```csharp
        try { bookingDb.Database.Migrate(); } catch { }
```

**Replace with:**
```csharp
        try { bookingDb.Database.Migrate(); } catch { }

        // Apply Row-Level Security (idempotent). The .sql uses GO batch separators,
        // which ExecuteSqlRaw can't run, so split on them first.
        try
        {
            var rlsPath = Path.Combine(AppContext.BaseDirectory, "Migrations", "AddRLS.sql");
            if (File.Exists(rlsPath))
            {
                var sql = File.ReadAllText(rlsPath);
                foreach (var batch in System.Text.RegularExpressions.Regex.Split(
                             sql, @"^\s*GO\s*$",
                             System.Text.RegularExpressions.RegexOptions.Multiline))
                {
                    if (!string.IsNullOrWhiteSpace(batch))
                        appDb.Database.ExecuteSqlRaw(batch);
                }
            }
        }
        catch { /* RLS is a backstop; don't block boot */ }
```

**Done when:** `Program.cs` reads `Migrations\AddRLS.sql` and runs it batch-by-batch.

**Verify (PowerShell):**
```powershell
Select-String -Path src\SalonOS.Api\Program.cs -Pattern "AddRLS.sql"
```
Expect 1 hit.

**⚠️ Human review / one-time check:** after `docker compose up`, run in the DB:
```sql
SELECT name FROM sys.security_policies WHERE name = 'TenantFilter';
```
It should return one row. If the path is wrong (no row), tell Claude — the `AppContext.BaseDirectory`
copy location may differ in this build.
