using MediatR;

namespace Epam.ItMarathon.ApiService.Application.UseCases.User.Commands
{
    /// <summary>
    /// Command for deleting a user by id with admin userCode.
    /// </summary>
    public record DeleteUserCommand(ulong UserId, string AdminUserCode) : IRequest<DeleteUserResult>;

    public class DeleteUserResult
    {
        public bool Success { get; set; }
        public string? ErrorCode { get; set; }
        public string? ErrorMessage { get; set; }
    }
}
