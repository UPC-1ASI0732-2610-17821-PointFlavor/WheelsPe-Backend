using FluentAssertions;
using Moq;
using Moveo_backend.Shared.Domain.Repositories;
using Moveo_backend.UserReview.Application.Internal.CommandServices;
using Moveo_backend.UserReview.Domain.Model.Commands;
using Moveo_backend.UserReview.Domain.Repositories;
using UserReviewEntity = Moveo_backend.UserReview.Domain.Model.Aggregate.UserReview;
using Xunit;

namespace Moveo_backend.Tests.UserReview.Application.Internal.CommandServices;

public class UserReviewCommandServiceTests
{
    private readonly Mock<IUserReviewRepository> _repositoryMock;
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly UserReviewCommandService _service;

    public UserReviewCommandServiceTests()
    {
        _repositoryMock = new Mock<IUserReviewRepository>();
        _unitOfWorkMock = new Mock<IUnitOfWork>();
        _unitOfWorkMock.Setup(u => u.CompleteAsync()).Returns(Task.CompletedTask);
        _service = new UserReviewCommandService(_repositoryMock.Object, _unitOfWorkMock.Object);
    }

    private static CreateUserReviewCommand MakeCreateCommand() =>
        new(ReviewerId: 1, ReviewedUserId: 2, RentalId: 10,
            Rating: 4, Comment: "Excelente propietario", Type: "renter_to_owner");

    // HU12 - Calificar al propietario
    [Fact]
    public async Task Handle_CreateUserReviewCommand_ReturnsReviewWithCorrectRating()
    {
        _repositoryMock.Setup(r => r.AddAsync(It.IsAny<UserReviewEntity>())).Returns(Task.CompletedTask);

        var result = await _service.Handle(MakeCreateCommand());

        result.Should().NotBeNull();
        result!.Rating.Should().Be(4);
        result.Type.Should().Be("renter_to_owner");
        _unitOfWorkMock.Verify(u => u.CompleteAsync(), Times.Once);
    }

    // Actualizar reseña
    [Fact]
    public async Task Handle_UpdateUserReviewCommand_WhenNotFound_ReturnsNull()
    {
        _repositoryMock.Setup(r => r.FindByIdAsync(999)).ReturnsAsync((UserReviewEntity?)null);

        var result = await _service.Handle(new UpdateUserReviewCommand(Id: 999, Rating: 5, Comment: "Actualizado"));

        result.Should().BeNull();
        _unitOfWorkMock.Verify(u => u.CompleteAsync(), Times.Never);
    }

    [Fact]
    public async Task Handle_UpdateUserReviewCommand_WhenExists_UpdatesRating()
    {
        var review = new UserReviewEntity(MakeCreateCommand());
        _repositoryMock.Setup(r => r.FindByIdAsync(review.Id)).ReturnsAsync(review);

        var result = await _service.Handle(new UpdateUserReviewCommand(Id: review.Id, Rating: 5, Comment: "Perfecto"));

        result.Should().NotBeNull();
        result!.Rating.Should().Be(5);
        _unitOfWorkMock.Verify(u => u.CompleteAsync(), Times.Once);
    }

    // Eliminar reseña
    [Fact]
    public async Task Handle_DeleteUserReviewCommand_WhenExists_ReturnsTrue()
    {
        var review = new UserReviewEntity(MakeCreateCommand());
        _repositoryMock.Setup(r => r.FindByIdAsync(review.Id)).ReturnsAsync(review);

        var result = await _service.Handle(new DeleteUserReviewCommand(review.Id));

        result.Should().BeTrue();
        _repositoryMock.Verify(r => r.Remove(review), Times.Once);
        _unitOfWorkMock.Verify(u => u.CompleteAsync(), Times.Once);
    }

    [Fact]
    public async Task Handle_DeleteUserReviewCommand_WhenNotFound_ReturnsFalse()
    {
        _repositoryMock.Setup(r => r.FindByIdAsync(999)).ReturnsAsync((UserReviewEntity?)null);

        var result = await _service.Handle(new DeleteUserReviewCommand(999));

        result.Should().BeFalse();
    }
}
