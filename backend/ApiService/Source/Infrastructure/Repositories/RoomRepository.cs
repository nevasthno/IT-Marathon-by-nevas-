using AutoMapper;
using CSharpFunctionalExtensions;
using Epam.ItMarathon.ApiService.Domain.Abstract;
using Epam.ItMarathon.ApiService.Domain.Aggregate.Room;
using Epam.ItMarathon.ApiService.Domain.Shared.ValidationErrors;
using Epam.ItMarathon.ApiService.Infrastructure.Database;
using Epam.ItMarathon.ApiService.Infrastructure.Database.Models.Room;
using Epam.ItMarathon.ApiService.Infrastructure.Database.Models.Room.Extensions;
using FluentValidation.Results;
using FluentValidation;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;

namespace Epam.ItMarathon.ApiService.Infrastructure.Repositories
{
    internal class RoomRepository(AppDbContext context, IMapper mapper, ILogger<RoomRepository> logger)
        : IRoomRepository
    {
        /// <inheritdoc/>
        public async Task<Result<Room, ValidationResult>> AddAsync(Room item, CancellationToken cancellationToken)
        {
            await using var transaction = await context.Database.BeginTransactionAsync(cancellationToken);
            try
            {
                var adminAuthCode = item.Users.First(user => user.IsAdmin).AuthCode;
                var roomEf = mapper.Map<RoomEf>(item);
                var adminEf = roomEf.Users.FirstOrDefault(user => user.AuthCode == adminAuthCode);
                roomEf.Admin = null;
                roomEf.AdminId = null;
                await context.Rooms.AddAsync(roomEf, cancellationToken);
                await context.SaveChangesAsync(cancellationToken);

                roomEf.Admin = adminEf!;
                roomEf.AdminId = adminEf!.Id;
                await context.SaveChangesAsync(cancellationToken);
                await transaction.CommitAsync(cancellationToken);

                var domainRoom = mapper.Map<Room>(roomEf);
                return Result.Success<Room, ValidationResult>(domainRoom);
            }
            catch (DbUpdateException exception)
            {
                await transaction.RollbackAsync(cancellationToken);
                logger.LogError(exception.ToString());
                throw;
            }
        }

        /// <inheritdoc/>
        public async Task<Result> UpdateAsync(Room roomToUpdate, CancellationToken cancellationToken)
        {
            var existingRoom = await context.Rooms
                .Include(room => room.Users)
                .ThenInclude(user => user.Wishes)
                .FirstOrDefaultAsync(room => room.Id == roomToUpdate.Id, cancellationToken);

            if (existingRoom is null)
            {
                return Result.Failure($"Room with Id={roomToUpdate.Id} not found.");
            }

            var updatedRoomEf = mapper.Map<RoomEf>(roomToUpdate);

            try
            {
                context.Rooms.Update(existingRoom.SyncRoom(updatedRoomEf));
                await context.SaveChangesAsync(cancellationToken);
            }
            catch (DbUpdateException exception)
            {
                logger.LogError(exception.ToString());
                throw;
            }

            return Result.Success();
        }

        /// <inheritdoc/>
        public async Task<Result<Room, ValidationResult>> GetByUserCodeAsync(string userCode,
            CancellationToken cancellationToken)
        {
            var result = await GetByCodeAsync(room => room.Users.Any(user => user.AuthCode == userCode),
                cancellationToken, true);
            return result;
        }

        public async Task<Result<Room, ValidationResult>> GetByRoomCodeAsync(string roomCode,
            CancellationToken cancellationToken)
        {
            var result = await GetByCodeAsync(room => room.InvitationCode == roomCode, cancellationToken, true);
            return result;
        }

        /// <summary>
        /// Implementation expected by DeleteUserHandler — uses ulong id
        /// </summary>
        public async Task<Result<Room, ValidationResult>> GetByIdAsync(ulong id, CancellationToken cancellationToken)
        {
            var roomEf = await context.Rooms
                .Include(r => r.Users)
                .ThenInclude(u => u.Wishes)
                .FirstOrDefaultAsync(r => r.Id == id, cancellationToken);

            if (roomEf == null)
            {
                var validation = new ValidationResult(new[]
                {
            new ValidationFailure("id", "Room with such id not found")
        });

                return Result.Failure<Room, ValidationResult>(validation);
            }

            var domainRoom = mapper.Map<Room>(roomEf);
            return Result.Success<Room, ValidationResult>(domainRoom);
        }


        private async Task<Result<Room, ValidationResult>> GetByCodeAsync(Expression<Func<RoomEf, bool>> codeExpression,
            CancellationToken cancellationToken, bool includeUsers = false)
        {
            var roomQuery = context.Rooms.AsQueryable();
            if (includeUsers)
            {
                roomQuery = roomQuery.Include(room => room.Users).ThenInclude(user => user.Wishes);
            }

            var roomEf = await roomQuery.FirstOrDefaultAsync(codeExpression, cancellationToken);

            if (roomEf == null)
            {
                var validation = new ValidationResult(new[]
                {
        new ValidationFailure("id", "Room with such id not found")
    });

                return Result.Failure<Room, ValidationResult>(validation);
            }


            var domainRoom = mapper.Map<Room>(roomEf);
            return Result.Success<Room, ValidationResult>(domainRoom);
        }
    }
}
