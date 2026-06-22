# A1 — Artist leave request + own contracts (+ all new Artist permissions) 🟡

## Step 1 — add the new permission constants
**File:** `src/SalonOS.Shared/Authorization/Permissions.cs`
**Find (exact):**
```csharp
    // ─── Platform / Tenant ──────────────────────────────────
```
**Replace with:**
```csharp
    // ─── Artist self-service ─────────────────────────────────
    public const string LeaveRequestOwn        = "leave.request.own";
    public const string AppointmentCheckIn     = "appointment.checkin";
    public const string RescheduleRequestCreate = "reschedule.request.create";
    public const string ClientNoteManageOwn    = "clientnote.manage.own";
    public const string ProductUsageRecord     = "productusage.record";
    public const string StaffRequestCreate     = "staffrequest.create";

    // ─── Platform / Tenant ──────────────────────────────────
```

## Step 2 — give them to the Artist role
**File:** `src/SalonOS.Shared/Authorization/RolePermissions.cs`
**Find (exact):**
```csharp
                Permissions.FinancePayoutViewOwn,
                Permissions.NotificationViewOwn,
            },
```
**Replace with:**
```csharp
                Permissions.FinancePayoutViewOwn,
                Permissions.NotificationViewOwn,
                Permissions.LeaveRequestOwn,
                Permissions.AppointmentCheckIn,
                Permissions.RescheduleRequestCreate,
                Permissions.ClientNoteManageOwn,
                Permissions.ProductUsageRecord,
                Permissions.StaffRequestCreate,
            },
```
(This is the block that ends with `FinancePayoutViewOwn` — that line is unique to the Artist role.)

## Step 3 — let the artist declare their own leave
**File:** `src/Modules/Booking/API/Controllers/LeaveController.cs`
**Find (exact):**
```csharp
    [HttpPost]
    [HasPermission(Permissions.SalonEdit)]
    public async Task<IActionResult> Create([FromBody] CreateLeaveDto dto)
```
**Replace with:**
```csharp
    // Artist declares their OWN leave — goes to the manager as Pending.
    [HttpPost("my")]
    [HasPermission(Permissions.LeaveRequestOwn)]
    public async Task<IActionResult> RequestMine([FromBody] CreateLeaveDto dto)
    {
        var artistId = User.FindFirst("artist_id")?.Value;
        if (string.IsNullOrEmpty(artistId) || !Guid.TryParse(artistId, out var parsedArtistId))
            return Forbid();

        var leave = new Leave
        {
            ArtistId = parsedArtistId,
            StartDateTime = dto.StartDateTime,
            EndDateTime = dto.EndDateTime,
            Reason = dto.Reason,
            Status = LeaveStatus.Pending,
            TenantId = _tenant.TenantId
        };
        _db.Leaves.Add(leave);
        await _db.SaveChangesAsync();
        return CreatedAtAction(nameof(GetByArtist), new { artistId = leave.ArtistId }, leave);
    }

    [HttpPost]
    [HasPermission(Permissions.SalonEdit)]
    public async Task<IActionResult> Create([FromBody] CreateLeaveDto dto)
```

## Step 4 — let the artist view their own per-service contracts
**File:** `src/SalonOS.Api/Controllers/StaffServiceContractController.cs`
**Find (exact):**
```csharp
    [HttpPost]
    [HasPermission(Permissions.StaffContractManage)]
    public async Task<IActionResult> Create([FromBody] ContractRequest r)
```
**Replace with:**
```csharp
    [HttpGet("my")]
    [HasPermission(Permissions.StaffPerformanceView)]
    public async Task<IActionResult> MyContracts()
    {
        var artistId = User.FindFirst("artist_id")?.Value;
        if (string.IsNullOrEmpty(artistId) || !Guid.TryParse(artistId, out var parsedArtistId))
            return Forbid();
        return Ok(await _db.StaffServiceContracts.Where(c => c.ArtistId == parsedArtistId && c.IsActive).ToListAsync());
    }

    [HttpPost]
    [HasPermission(Permissions.StaffContractManage)]
    public async Task<IActionResult> Create([FromBody] ContractRequest r)
```

**Done when:** build succeeds; `POST /api/leaves/my` works for an artist (creates a Pending leave),
and `GET /api/staff-contracts/my` returns the artist's own contracts.
