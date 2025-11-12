using System.Threading;
using System.Threading.Tasks;
using Xunit;
using Moq;
using Epam.ItMarathon.ApiService.Application.UseCases.User.Commands;
using Epam.ItMarathon.ApiService.Application.UseCases.User.Handlers;
using Epam.ItMarathon.ApiService.Domain.Abstract;
using Epam.ItMarathon.ApiService.Domain.Entities.User;
using Epam.ItMarathon.ApiService.Domain.Aggregate.Room;
using CSharpFunctionalExtensions;
using FluentValidation.Results;

public class DeleteUserTests
{
    private readonly Mock<IUserReadOnlyRepository> _userRepo = new();
    private readonly Mock<IRoomRepository> _roomRepo = new();
    private readonly DeleteUserHandler _handler;

    public DeleteUserTests()
    {
        _handler = new DeleteUserHandler(_userRepo.Object, _roomRepo.Object);
    }

    [Fact]
    public async Task DeleteUser_Success()
    {
        var admin = new User { Id = 1, RoomId = 10, IsAdmin = true };
        var user = new User { Id = 2, RoomId = 10, IsAdmin = false };
        var room = new Room { Id = 10, ClosedOn = null };
        _userRepo.Setup(r => r.GetByIdAsync(2, It.IsAny<CancellationToken>(), true, false)).ReturnsAsync(Result.Success<User, ValidationResult>(user));
        _userRepo.Setup(r => r.GetByCodeAsync("adminCode", It.IsAny<CancellationToken>(), true, false)).ReturnsAsync(Result.Success<User, ValidationResult>(admin));
        _roomRepo.Setup(r => r.GetByIdAsync(10, It.IsAny<CancellationToken>())).ReturnsAsync(Result.Success<Room, ValidationResult>(room));
        _userRepo.Setup(r => r.DeleteAsync(2, It.IsAny<CancellationToken>())).ReturnsAsync(Result.Success<bool, ValidationResult>(true));

        var result = await _handler.Handle(new DeleteUserCommand(2, "adminCode"), CancellationToken.None);
        Assert.True(result.Success);
    }

    [Fact]
    public async Task DeleteUser_UserNotFound()
    {
        _userRepo.Setup(r => r.GetByIdAsync(2, It.IsAny<CancellationToken>(), true, false)).ReturnsAsync(Result.Failure<User, ValidationResult>(new ValidationResult()));
        var result = await _handler.Handle(new DeleteUserCommand(2, "adminCode"), CancellationToken.None);
        Assert.False(result.Success);
        Assert.Equal("UserNotFound", result.ErrorCode);
    }

    [Fact]
    public async Task DeleteUser_AdminNotFound()
    {
        var user = new User { Id = 2, RoomId = 10, IsAdmin = false };
        _userRepo.Setup(r => r.GetByIdAsync(2, It.IsAny<CancellationToken>(), true, false)).ReturnsAsync(Result.Success<User, ValidationResult>(user));
        _userRepo.Setup(r => r.GetByCodeAsync("adminCode", It.IsAny<CancellationToken>(), true, false)).ReturnsAsync(Result.Failure<User, ValidationResult>(new ValidationResult()));
        var result = await _handler.Handle(new DeleteUserCommand(2, "adminCode"), CancellationToken.None);
        Assert.False(result.Success);
        Assert.Equal("AdminNotFound", result.ErrorCode);
    }

    [Fact]
    public async Task DeleteUser_NotAdmin()
    {
        var admin = new User { Id = 1, RoomId = 10, IsAdmin = false };
        var user = new User { Id = 2, RoomId = 10, IsAdmin = false };
        _userRepo.Setup(r => r.GetByIdAsync(2, It.IsAny<CancellationToken>(), true, false)).ReturnsAsync(Result.Success<User, ValidationResult>(user));
        _userRepo.Setup(r => r.GetByCodeAsync("adminCode", It.IsAny<CancellationToken>(), true, false)).ReturnsAsync(Result.Success<User, ValidationResult>(admin));
        var result = await _handler.Handle(new DeleteUserCommand(2, "adminCode"), CancellationToken.None);
        Assert.False(result.Success);
        Assert.Equal("NotAdmin", result.ErrorCode);
    }

    [Fact]
    public async Task DeleteUser_DifferentRooms()
    {
        var admin = new User { Id = 1, RoomId = 11, IsAdmin = true };
        var user = new User { Id = 2, RoomId = 10, IsAdmin = false };
        _userRepo.Setup(r => r.GetByIdAsync(2, It.IsAny<CancellationToken>(), true, false)).ReturnsAsync(Result.Success<User, ValidationResult>(user));
        _userRepo.Setup(r => r.GetByCodeAsync("adminCode", It.IsAny<CancellationToken>(), true, false)).ReturnsAsync(Result.Success<User, ValidationResult>(admin));
        var result = await _handler.Handle(new DeleteUserCommand(2, "adminCode"), CancellationToken.None);
        Assert.False(result.Success);
        Assert.Equal("DifferentRooms", result.ErrorCode);
    }

    [Fact]
    public async Task DeleteUser_SameUser()
    {
        var admin = new User { Id = 2, RoomId = 10, IsAdmin = true };
        var user = new User { Id = 2, RoomId = 10, IsAdmin = false };
        _userRepo.Setup(r => r.GetByIdAsync(2, It.IsAny<CancellationToken>(), true, false)).ReturnsAsync(Result.Success<User, ValidationResult>(user));
        _userRepo.Setup(r => r.GetByCodeAsync("adminCode", It.IsAny<CancellationToken>(), true, false)).ReturnsAsync(Result.Success<User, ValidationResult>(admin));
        var result = await _handler.Handle(new DeleteUserCommand(2, "adminCode"), CancellationToken.None);
        Assert.False(result.Success);
        Assert.Equal("SameUser", result.ErrorCode);
    }

    [Fact]
    public async Task DeleteUser_RoomClosed()
    {
        var admin = new User { Id = 1, RoomId = 10, IsAdmin = true };
        var user = new User { Id = 2, RoomId = 10, IsAdmin = false };
        var room = new Room { Id = 10, ClosedOn = System.DateTime.UtcNow };
        _userRepo.Setup(r => r.GetByIdAsync(2, It.IsAny<CancellationToken>(), true, false)).ReturnsAsync(Result.Success<User, ValidationResult>(user));
        _userRepo.Setup(r => r.GetByCodeAsync("adminCode", It.IsAny<CancellationToken>(), true, false)).ReturnsAsync(Result.Success<User, ValidationResult>(admin));
        _roomRepo.Setup(r => r.GetByIdAsync(10, It.IsAny<CancellationToken>())).ReturnsAsync(Result.Success<Room, ValidationResult>(room));
        var result = await _handler.Handle(new DeleteUserCommand(2, "adminCode"), CancellationToken.None);
        Assert.False(result.Success);
        Assert.Equal("RoomClosed", result.ErrorCode);
    }

    [Fact]
    public async Task DeleteUser_RoomNotFound()
    {
        var admin = new User { Id = 1, RoomId = 10, IsAdmin = true };
        var user = new User { Id = 2, RoomId = 10, IsAdmin = false };
        _userRepo.Setup(r => r.GetByIdAsync(2, It.IsAny<CancellationToken>(), true, false)).ReturnsAsync(Result.Success<User, ValidationResult>(user));
        _userRepo.Setup(r => r.GetByCodeAsync("adminCode", It.IsAny<CancellationToken>(), true, false)).ReturnsAsync(Result.Success<User, ValidationResult>(admin));
        _roomRepo.Setup(r => r.GetByIdAsync(10, It.IsAny<CancellationToken>())).ReturnsAsync(Result.Failure<Room, ValidationResult>(new ValidationResult()));
        var result = await _handler.Handle(new DeleteUserCommand(2, "adminCode"), CancellationToken.None);
        Assert.False(result.Success);
        Assert.Equal("RoomNotFound", result.ErrorCode);
    }

    [Fact]
    public async Task DeleteUser_DeleteFailed()
    {
        var admin = new User { Id = 1, RoomId = 10, IsAdmin = true };
        var user = new User { Id = 2, RoomId = 10, IsAdmin = false };
        var room = new Room { Id = 10, ClosedOn = null };
        _userRepo.Setup(r => r.GetByIdAsync(2, It.IsAny<CancellationToken>(), true, false)).ReturnsAsync(Result.Success<User, ValidationResult>(user));
        _userRepo.Setup(r => r.GetByCodeAsync("adminCode", It.IsAny<CancellationToken>(), true, false)).ReturnsAsync(Result.Success<User, ValidationResult>(admin));
        _roomRepo.Setup(r => r.GetByIdAsync(10, It.IsAny<CancellationToken>())).ReturnsAsync(Result.Success<Room, ValidationResult>(room));
        _userRepo.Setup(r => r.DeleteAsync(2, It.IsAny<CancellationToken>())).ReturnsAsync(Result.Failure<bool, ValidationResult>(new ValidationResult(new[] { new FluentValidation.Results.ValidationFailure("Delete", "Delete failed.") } )));
        var result = await _handler.Handle(new DeleteUserCommand(2, "adminCode"), CancellationToken.None);
        Assert.False(result.Success);
        Assert.Equal("DeleteFailed", result.ErrorCode);
    }
}
