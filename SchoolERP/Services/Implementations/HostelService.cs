using SchoolERP.Data;
using SchoolERP.Models.Entities;
using SchoolERP.Models.ViewModels;
using SchoolERP.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace SchoolERP.Services.Implementations
{
    public class HostelService : IHostelService
    {
        private readonly AppDbContext _db;

        public HostelService(AppDbContext db)
        {
            _db = db;
        }

        public async Task<IEnumerable<HostelWithRoomsVM>> GetAllHostelsAsync()
        {
            // Fetch all hostels with their rooms
            var hostels = await _db.Hostels
                .Include(h => h.Rooms) // Include related rooms
                .ToListAsync();

            // Map to ViewModel
            var result = hostels.Select(h => new HostelWithRoomsVM
            {
                Hostel = new Hostel
                {
                    HostelId = h.HostelId,
                    HostelName = h.HostelName,
                    Location = h.Location
                },
                Rooms = h.Rooms.Select(r => new RoomVM
                {
                    RoomId = r.RoomId,
                    RoomNumber = r.RoomNumber,
                    Capacity = r.Capacity
                    , RoomType = r.RoomType
                    , Fee = r.Fee
                }).ToList()
            });

            return result;
        }

        // Get hostel with rooms including occupancy
        public async Task<HostelWithRoomsVM> GetHostelWithRoomsAsync(int hostelId)
        {
            var hostel = await _db.Hostels.FindAsync(hostelId);

            var rooms = await _db.Rooms
                .Where(r => r.HostelId == hostelId)
                .Select(r => new RoomVM
                {
                    RoomId = r.RoomId,
                    RoomNumber = r.RoomNumber,
                    Capacity = r.Capacity,
                    RoomType = r.RoomType,
                    Fee = r.Fee,
                    OccupiedBeds = _db.StudentHostelAllocations.Count(a => a.RoomId == r.RoomId && a.IsActive)
                })
                .ToListAsync();

            return new HostelWithRoomsVM
            {
                Hostel = hostel,
                Rooms = rooms
            };
        }

        // Add new hostel with rooms
        public async Task AddHostelAsync(Hostel hostel, List<RoomVM> rooms)
        {
            await using var transaction = await _db.Database.BeginTransactionAsync();
            try
            {
                _db.Hostels.Add(hostel);
                await _db.SaveChangesAsync();

                if (rooms != null && rooms.Any())
                {
                    foreach (var room in rooms)
                    {
                        if (string.IsNullOrWhiteSpace(room.RoomNumber)) continue;

                        _db.Rooms.Add(new Room
                        {
                            HostelId = hostel.HostelId,
                            RoomNumber = room.RoomNumber,
                            Capacity = room.Capacity,
                            RoomType = room.RoomType,
                            Fee = room.Fee
                        });
                    }
                    await _db.SaveChangesAsync();
                }

                await transaction.CommitAsync();
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }
        }

        public async Task CreateHostelAsync(HostelWithRoomsVM model)
        {
            if (model == null)
                throw new ArgumentNullException(nameof(model));

            await using var transaction = await _db.Database.BeginTransactionAsync();
            try
            {
                // Add Hostel
                await _db.Hostels.AddAsync(model.Hostel);
                await _db.SaveChangesAsync();

                // Add Rooms
                if (model.Rooms != null && model.Rooms.Any())
                {
                    foreach (var room in model.Rooms)
                    {
                        if (string.IsNullOrWhiteSpace(room.RoomNumber))
                            continue;

                        await _db.Rooms.AddAsync(new Room
                        {
                            HostelId = model.Hostel.HostelId,
                            RoomNumber = room.RoomNumber,
                            Capacity = room.Capacity,
                            RoomType = room.RoomType,
                            Fee = room.Fee
                        });
                    }

                    await _db.SaveChangesAsync();
                }

                await transaction.CommitAsync();
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }
        }


        // Update hostel and rooms
        public async Task UpdateHostelAsync(HostelWithRoomsVM model)
        {
            await using var transaction = await _db.Database.BeginTransactionAsync();
            try
            {
                _db.Entry(model.Hostel).State = EntityState.Modified;

                foreach (var room in model.Rooms)
                {
                    // Delete room
                    if (room.IsDeleted && room.RoomId.HasValue)
                    {
                        var isOccupied = await _db.StudentHostelAllocations
                            .AnyAsync(a => a.RoomId == room.RoomId && a.IsActive);
                        if (isOccupied)
                            throw new Exception($"Cannot delete occupied room: {room.RoomNumber}");

                        var existing = await _db.Rooms.FindAsync(room.RoomId);
                        if (existing != null)
                            _db.Rooms.Remove(existing);

                        continue;
                    }

                    // New room
                    if (!room.RoomId.HasValue)
                    {
                        _db.Rooms.Add(new Room
                        {
                            HostelId = model.Hostel.HostelId,
                            RoomNumber = room.RoomNumber,
                            Capacity = room.Capacity,
                            RoomType = room.RoomType,
                            Fee = room.Fee
                        });
                    }
                    else
                    {
                        // Update existing
                        var existing = await _db.Rooms.FindAsync(room.RoomId);
                        if (existing != null)
                        {
                            int occupied = await _db.StudentHostelAllocations
                                .CountAsync(a => a.RoomId == room.RoomId && a.IsActive);

                            if (room.Capacity < occupied)
                                throw new Exception($"Room {room.RoomNumber} capacity cannot be less than occupied ({occupied})");

                            existing.RoomNumber = room.RoomNumber;
                            existing.Capacity = room.Capacity;
                            existing.RoomType = room.RoomType;
                            existing.Fee = room.Fee;
                        }
                    }
                }

                await _db.SaveChangesAsync();
                await transaction.CommitAsync();
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }
        }

        // Delete single room (if not occupied)
        public async Task DeleteRoomAsync(int roomId)
        {
            var isOccupied = await _db.StudentHostelAllocations
                .AnyAsync(a => a.RoomId == roomId && a.IsActive);

            if (isOccupied)
                throw new Exception("Cannot delete room, it has allocated students.");

            var room = await _db.Rooms.FindAsync(roomId);
            if (room != null)
            {
                _db.Rooms.Remove(room);
                await _db.SaveChangesAsync();
            }
        }

        public void AllocateRoom(int studentId, int roomId)
        {
            //var activeAllocation = db.StudentHostelAllocations
            //    .FirstOrDefault(x => x.StudentId == studentId && x.Status);

            //if (activeAllocation != null)
            //    throw new Exception("Student already assigned!");

            //var roomCount = db.StudentHostelAllocations
            //    .Count(x => x.RoomId == roomId && x.Status);

            //var room = db.Rooms.Find(roomId);

            //if (roomCount >= room.Capacity)
            //    throw new Exception("Room is full!");

            //var allocation = new StudentHostelAllocation
            //{
            //    StudentId = studentId,
            //    RoomId = roomId,
            //    JoinDate = DateTime.Now,
            //    Status = true
            //};

            //db.StudentHostelAllocations.Add(allocation);
            //db.SaveChanges();
        }

        public void GenerateMonthlyFees(DateTime month)
        {
            //var students = db.StudentHostelAllocations
            //    .Where(x => x.Status)
            //    .ToList();

            //foreach (var s in students)
            //{
            //    var room = db.Rooms.Find(s.RoomId);

            //    var fee = new HostelFee
            //    {
            //        StudentId = s.StudentId,
            //        AllocationId = s.AllocationId,
            //        FeeMonth = month,
            //        Amount = room.Fee,
            //        DueDate = month.AddDays(10),
            //        Status = "Unpaid"
            //    };

            //    db.HostelFees.Add(fee);
            //}

            //db.SaveChanges();
        }

        public void MakePayment(int feeId, decimal amount)
        {
            //var fee = db.HostelFees.Find(feeId);

            //if (fee == null)
            //    throw new Exception("Fee not found");

            //var totalPaid = db.Payments
            //    .Where(p => p.FeeId == feeId)
            //    .Sum(p => (decimal?)p.PaidAmount) ?? 0;

            //var remaining = fee.Amount - totalPaid;

            //if (amount > remaining)
            //    throw new Exception("Overpayment not allowed");

            //var payment = new Payment
            //{
            //    StudentId = fee.StudentId,
            //    FeeId = feeId,
            //    PaidAmount = amount,
            //    PaymentDate = DateTime.Now,
            //    PaymentMode = "Cash"
            //};

            //db.Payments.Add(payment);

            //if (amount == remaining)
            //    fee.Status = "Paid";
            //else
            //    fee.Status = "Partial";

            //db.SaveChanges();
        }
    }
}
