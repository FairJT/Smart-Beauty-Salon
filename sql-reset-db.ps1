$ErrorActionPreference = 'Stop'
# Drop SalonOSDB so EF migrations apply cleanly from scratch on next API start.
# (The previous run left a half-seeded DB: ArtistSchedules existed but Bookings
#  did not, because InitialBooking failed on "object already exists".)
$query = @"
IF DB_ID('SalonOSDB') IS NOT NULL
BEGIN
    ALTER DATABASE [SalonOSDB] SET SINGLE_USER WITH ROLLBACK IMMEDIATE;
    DROP DATABASE [SalonOSDB];
END
"@
sqlcmd -S "localhost\SQLEXPRESS" -E -Q $query -b
if ($LASTEXITCODE -eq 0) { Write-Host "Dropped SalonOSDB OK." }
else { Write-Host "sqlcmd exit code $LASTEXITCODE." }
