using CSharpFunctionalExtensions;
using Epam.ItMarathon.ApiService.Application.UseCases.User.Commands;
using Epam.ItMarathon.ApiService.Domain.Abstract;
using Epam.ItMarathon.ApiService.Domain.Shared.ValidationErrors;
using FluentValidation.Results;
using MediatR;
using System.Threading;
using System.Threading.Tasks;

namespace Epam.ItMarathon.ApiService.Application.UseCases.User.Handlers
{
    public class DeleteUserHandler : IRequestHandler<DeleteUserCommand, DeleteUserResult>
    {
        private readonly IUserReadOnlyRepository _userRepo;
        private readonly IRoomRepository _roomRepo;

        public DeleteUserHandler(IUserReadOnlyRepository userRepo, IRoomRepository roomRepo)
        {
            _userRepo = userRepo;
            _roomRepo = roomRepo;
        }

        public async Task<DeleteUserResult> Handle(DeleteUserCommand request, CancellationToken cancellationToken)
        {
            var userToDeleteResult = await _userRepo.GetByIdAsync(request.UserId, cancellationToken, true);
            if (userToDeleteResult.IsFailure)
                return new DeleteUserResult { Success = false, ErrorCode = "UserNotFound", ErrorMessage = "User with id not found." };

            var userToDelete = userToDeleteResult.Value;

            var adminResult = await _userRepo.GetByCodeAsync(request.AdminUserCode, cancellationToken, true);
            if (adminResult.IsFailure)
                return new DeleteUserResult { Success = false, ErrorCode = "AdminNotFound", ErrorMessage = "Admin user with code not found." };

            var adminUser = adminResult.Value;

            if (!adminUser.IsAdmin)
                return new DeleteUserResult { Success = false, ErrorCode = "NotAdmin", ErrorMessage = "User is not admin." };

            if (userToDelete.RoomId != adminUser.RoomId)
                return new DeleteUserResult { Success = false, ErrorCode = "DifferentRooms", ErrorMessage = "Users belong to different rooms." };

            if (userToDelete.Id == adminUser.Id)
                return new DeleteUserResult { Success = false, ErrorCode = "SameUser", ErrorMessage = "Cannot delete yourself as admin." };

            var roomResult = await _roomRepo.GetByIdAsync(adminUser.RoomId, cancellationToken);
            if (roomResult.IsFailure)
                return new DeleteUserResult { Success = false, ErrorCode = "RoomNotFound", ErrorMessage = "Room not found." };
            var room = roomResult.Value;
            if (room.ClosedOn != null)
                return new DeleteUserResult { Success = false, ErrorCode = "RoomClosed", ErrorMessage = "Room is already closed." };

            var deleteResult = await _userRepo.DeleteAsync(userToDelete.Id, cancellationToken);
            if (deleteResult.IsFailure)
                return new DeleteUserResult { Success = false, ErrorCode = "DeleteFailed", ErrorMessage = deleteResult.Error.Errors.FirstOrDefault()?.ErrorMessage ?? "Delete failed." };

            return new DeleteUserResult { Success = true };
        }
    }
}
