$ErrorActionPreference = 'SilentlyContinue'
$q = @"
USE [SalonOSDB];
SELECT 'Memberships' AS T, COUNT(*) AS cnt FROM [Memberships];
SELECT Id, UserId, TenantId, Role, IsActive FROM [Memberships];
SELECT 'AspNetUsers' AS T, COUNT(*) AS cnt FROM [AspNetUsers];
SELECT Id, UserName, UserType FROM [AspNetUsers];
SELECT 'Bookings' AS T, COUNT(*) AS cnt FROM [Bookings];
SELECT 'Tenants' AS T, COUNT(*) AS cnt FROM [Tenants];
SELECT Id, Name, Slug FROM [Tenants];
"@
sqlcmd -S "localhost\SQLEXPRESS" -E -s "|" -W -Q $q
