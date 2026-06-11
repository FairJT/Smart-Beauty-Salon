using Microsoft.EntityFrameworkCore;
using SmartSalon.Data;
using SmartSalon.DTOs;
using SmartSalon.Models;

namespace SmartSalon.Services
{
    public class AppointmentService : IAppointmentService
    {
        private readonly ApplicationDbContext _db;
        private readonly INotificationService _notification;

        public AppointmentService(ApplicationDbContext db, INotificationService notification)
        {
            _db = db;
            _notification = notification;
        }

        public async Task<SlotsResponseDto?> GetSlotsAsync(int artistId, DateTime date, int duration)
        {
            var dayStart = date.Date.AddHours(9);
            var dayEnd = date.Date.AddHours(20);

            var booked = await _db.Appointments
                .Where(a => a.ArtistId == artistId
                         && a.StartTime.Date == date.Date
                         && a.Status != AppointmentStatus.Cancelled)
                .Select(a => new { a.StartTime, a.EndTime })
                .ToListAsync();

            var freeSlots = new List<SlotDto>();
            var current = dayStart;

            while (current.AddMinutes(duration) <= dayEnd)
            {
                var slotEnd = current.AddMinutes(duration);
                var isBusy = booked.Any(b => current < b.EndTime && slotEnd > b.StartTime);

                if (!isBusy)
                    freeSlots.Add(new SlotDto
                    {
                        Start = current.ToString("HH:mm"),
                        End = slotEnd.ToString("HH:mm"),
                        StartFull = current
                    });

                current = current.AddMinutes(30);
            }

            return new SlotsResponseDto
            {
                Date = date.ToString("yyyy-MM-dd"),
                ArtistId = artistId,
                Duration = duration,
                Slots = freeSlots
            };
        }

        public async Task<CreateAppointmentResponseDto?> CreateAsync(CreateAppointmentDto dto, string clientId)
        {
            var salonExists = await _db.Salons.AnyAsync(s => s.Id == dto.SalonId && s.IsActive);
            if (!salonExists) return null;

            var artistExists = await _db.Artists.AnyAsync(a =>
                a.Id == dto.ArtistId && a.SalonId == dto.SalonId && a.IsActive);
            if (!artistExists) return null;

            var serviceExists = await _db.SalonServices.AnyAsync(s =>
                s.Id == dto.ServiceId && s.SalonId == dto.SalonId && s.IsActive);
            if (!serviceExists) return null;

            var endTime = dto.StartTime.AddMinutes(dto.DurationMinutes);

            var hasConflict = await _db.Appointments.AnyAsync(a =>
                a.ArtistId == dto.ArtistId &&
                a.Status != AppointmentStatus.Cancelled &&
                dto.StartTime < a.EndTime &&
                endTime > a.StartTime);

            if (hasConflict) return null;

            var appointment = new Appointment
            {
                ClientId = clientId,
                ArtistId = dto.ArtistId,
                SalonId = dto.SalonId,
                ServiceId = dto.ServiceId,
                StartTime = dto.StartTime,
                EndTime = endTime,
                DurationMinutes = dto.DurationMinutes,
                EstimatedPrice = dto.EstimatedPrice,
                DepositAmount = dto.EstimatedPrice * 0.3m,
                Notes = dto.Notes,
                Status = AppointmentStatus.Pending
            };

            _db.Appointments.Add(appointment);
            await _db.SaveChangesAsync();

            // Notify salon manager
            var salon = await _db.Salons.FindAsync(dto.SalonId);
            if (salon != null)
            {
                var client = await _db.Users.FindAsync(clientId);
                var clientName = client != null ? $"{client.FirstName} {client.LastName}" : "Unknown";
                var dateTime = dto.StartTime.ToString("yyyy/MM/dd HH:mm");

                await _notification.SendNewAppointmentToManagerAsync(
                    salon.ManagerId, clientName, dateTime);
            }

            return new CreateAppointmentResponseDto
            {
                Message = "Appointment booked successfully",
                Id = appointment.Id,
                Deposit = appointment.DepositAmount
            };
        }

        public async Task<List<AppointmentListItemDto>> GetMineAsync(string clientId)
        {
            return await _db.Appointments
                .Where(a => a.ClientId == clientId)
                .Include(a => a.Salon)
                .Include(a => a.Artist).ThenInclude(ar => ar!.User)
                .Include(a => a.Service)
                .OrderByDescending(a => a.StartTime)
                .Select(a => new AppointmentListItemDto
                {
                    Id = a.Id,
                    StartTime = a.StartTime,
                    EndTime = a.EndTime,
                    Status = (int)a.Status,
                    EstimatedPrice = a.EstimatedPrice,
                    DepositAmount = a.DepositAmount,
                    IsRated = a.IsRated,
                    Rating = a.Rating,
                    Comment = a.Comment,
                    SalonName = a.Salon!.Name,
                    ArtistName = a.Artist!.User!.FirstName + " " + a.Artist.User.LastName,
                    ServiceName = a.Service!.Name
                })
                .ToListAsync();
        }

        public async Task<bool> ConfirmAsync(int id, string userId)
        {
            var appointment = await _db.Appointments
                .Include(a => a.Salon)
                .Include(a => a.Client)
                .FirstOrDefaultAsync(a => a.Id == id);

            if (appointment == null) return false;
            if (appointment.Salon?.ManagerId != userId) return false;

            appointment.Status = AppointmentStatus.Confirmed;
            await _db.SaveChangesAsync();

            // Notify client
            if (appointment.Client != null)
            {
                var salonName = appointment.Salon?.Name ?? "";
                var dateTime = appointment.StartTime.ToString("yyyy/MM/dd HH:mm");
                await _notification.SendAppointmentConfirmedAsync(
                    appointment.Client.Id, salonName, dateTime);
            }

            return true;
        }

        public async Task<bool> CompleteAsync(int id, string userId)
        {
            var appointment = await _db.Appointments
                .Include(a => a.Client)
                .Include(a => a.Salon)
                .FirstOrDefaultAsync(a => a.Id == id);

            if (appointment == null) return false;
            if (appointment.Salon?.ManagerId != userId) return false;

            appointment.Status = AppointmentStatus.Completed;

            if (appointment.Client != null)
            {
                var pointsEarned = (int)(appointment.EstimatedPrice / 10000);
                appointment.Client.LoyaltyPoints += pointsEarned;
                appointment.Client.TotalVisits += 1;
            }

            await _db.SaveChangesAsync();
            return true;
        }

        public async Task<bool> CancelAsync(int id, string userId)
        {
            var appointment = await _db.Appointments
                .Include(a => a.Salon)
                .Include(a => a.Client)
                .FirstOrDefaultAsync(a => a.Id == id && a.ClientId == userId);

            if (appointment == null) return false;

            if (appointment.Status == AppointmentStatus.Completed ||
                appointment.Status == AppointmentStatus.Cancelled)
                return false;

            if (appointment.StartTime <= DateTime.Now.AddHours(2))
                return false;

            appointment.Status = AppointmentStatus.Cancelled;
            await _db.SaveChangesAsync();

            // Notify salon manager
            if (appointment.Salon != null && appointment.Client != null)
            {
                var clientName = $"{appointment.Client.FirstName} {appointment.Client.LastName}";
                await _notification.SendAppointmentCancelledAsync(
                    appointment.Salon.ManagerId, clientName);
            }

            return true;
        }

        public async Task<(bool Success, string Message, decimal? RatingAvg)> RateAsync(
            int id, string userId, RateRequestDto request)
        {
            var appointment = await _db.Appointments
                .Include(a => a.Artist)
                .FirstOrDefaultAsync(a => a.Id == id && a.ClientId == userId);

            if (appointment == null)
                return (false, "Appointment not found", null);

            if (appointment.Status != AppointmentStatus.Completed)
                return (false, "Only completed appointments can be rated", null);

            if (appointment.IsRated)
                return (false, "Already rated", null);

            appointment.IsRated = true;
            appointment.Rating = request.Rating;
            appointment.Comment = request.Comment;

            var artist = appointment.Artist!;
            var oldTotal = artist.RatingCount;
            var oldAvg = (double)artist.RatingAvg;

            artist.RatingCount = oldTotal + 1;
            artist.RatingAvg = (decimal)((oldAvg * oldTotal + request.Rating) / artist.RatingCount);

            await _db.SaveChangesAsync();
            return (true, "Rating submitted successfully", artist.RatingAvg);
        }
    }
}
