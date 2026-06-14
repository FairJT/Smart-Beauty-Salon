# Endpoint ↔ Screen Matrix

| Screen | Role | HTTP | Endpoint | Provider | Dart Model |
|---|---|---|---|---|---|
| SalonManager Dashboard | SalonManager | `GET` | `/api/salons/{id}/dashboard` | `SalonsController.GetDashboard` | `SalonManagerDashboard` |
| Artist Dashboard | Artist | `GET` | `/api/artist-schedule/my/dashboard` | `ArtistScheduleController.GetMyDashboard` | `ArtistDashboard` |
| Client Home | Client | `GET` | `/api/me/home` | `MeController.GetHome` | `ClientDashboard` |
| SuperAdmin Dashboard | SuperAdmin | `GET` | `/api/admin/stats` | `AdminController.GetStats` | `SuperAdminDashboard` |

## Response schemas

### `GET /api/salons/{id}/dashboard`
```json
{
  "todayAppointments": 12,
  "upcomingAppointments": 8,
  "revenue": { "amount": 8500000, "currency": "IRR" },
  "artistUtilization": [
    { "artistId": 1, "artistName": "...", "todayAppointments": 4, "completedToday": 2, "utilizationPercent": 25.0 }
  ],
  "activeServiceCount": 15,
  "activeArtistCount": 5,
  "subscriptionStatus": "active"
}
```

### `GET /api/artist-schedule/my/dashboard`
```json
{
  "todayAppointments": 3,
  "upcomingAppointments": 5,
  "nextAppointment": { "id": 42, "startTime": "2026-06-13T14:00:00", "clientName": "...", "serviceName": "...", "status": 2 },
  "ratingAvg": 4.5,
  "ratingCount": 28,
  "monthAppointments": 42,
  "monthRevenue": { "amount": 3500000, "currency": "IRR" }
}
```

### `GET /api/me/home`
```json
{
  "upcomingBookings": 2,
  "nextBooking": { "id": 10, "startTime": "...", "salonName": "...", "serviceName": "...", "artistName": "...", "status": 2 },
  "loyaltyPoints": 120,
  "totalVisits": 15,
  "unreadNotifications": 3,
  "favoriteSalons": []
}
```
