using FluentAssertions;
using Moq;
using Moveo_backend.Notification.Application.Internal.CommandServices;
using Moveo_backend.Notification.Domain.Model.Commands;
using Moveo_backend.Notification.Domain.Repositories;
using Moveo_backend.Shared.Domain.Repositories;
using NotificationEntity = Moveo_backend.Notification.Domain.Model.Aggregate.Notification;
using Xunit;

namespace Moveo_backend.Tests.Notification.Application.Internal.CommandServices;

public class NotificationCommandServiceTests
{
    private readonly Mock<INotificationRepository> _repositoryMock;
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly NotificationCommandService _service;

    public NotificationCommandServiceTests()
    {
        _repositoryMock = new Mock<INotificationRepository>();
        _unitOfWorkMock = new Mock<IUnitOfWork>();
        _unitOfWorkMock.Setup(u => u.CompleteAsync()).Returns(Task.CompletedTask);
        _service = new NotificationCommandService(_repositoryMock.Object, _unitOfWorkMock.Object);
    }

    private static CreateNotificationCommand MakeCreateCommand() =>
        new(UserId: 1, Title: "Nueva reserva", Body: "Tu auto fue reservado",
            Type: "rental_request", RelatedEntityId: 10,
            RelatedEntityType: "rental", ActionUrl: null, ActionLabel: null, MetadataJson: null);

    // Crear notificación
    [Fact]
    public async Task Handle_CreateNotificationCommand_ReturnsNotificationWithCorrectTitle()
    {
        _repositoryMock.Setup(r => r.AddAsync(It.IsAny<NotificationEntity>())).Returns(Task.CompletedTask);

        var result = await _service.Handle(MakeCreateCommand());

        result.Should().NotBeNull();
        result!.Title.Should().Be("Nueva reserva");
        result.IsRead.Should().BeFalse();
        _unitOfWorkMock.Verify(u => u.CompleteAsync(), Times.Once);
    }

    // Marcar notificación como leída
    [Fact]
    public async Task Handle_MarkNotificationAsReadCommand_WhenExists_MarksAsRead()
    {
        var notification = new NotificationEntity(MakeCreateCommand());
        _repositoryMock.Setup(r => r.FindByIdAsync(notification.Id)).ReturnsAsync(notification);

        var result = await _service.Handle(new MarkNotificationAsReadCommand(notification.Id));

        result.Should().NotBeNull();
        result!.IsRead.Should().BeTrue();
        _unitOfWorkMock.Verify(u => u.CompleteAsync(), Times.Once);
    }

    [Fact]
    public async Task Handle_MarkNotificationAsReadCommand_WhenNotFound_ReturnsNull()
    {
        _repositoryMock.Setup(r => r.FindByIdAsync(999)).ReturnsAsync((NotificationEntity?)null);

        var result = await _service.Handle(new MarkNotificationAsReadCommand(999));

        result.Should().BeNull();
        _unitOfWorkMock.Verify(u => u.CompleteAsync(), Times.Never);
    }

    // Eliminar notificación
    [Fact]
    public async Task Handle_DeleteNotificationCommand_WhenExists_ReturnsTrue()
    {
        var notification = new NotificationEntity(MakeCreateCommand());
        _repositoryMock.Setup(r => r.FindByIdAsync(notification.Id)).ReturnsAsync(notification);

        var result = await _service.Handle(new DeleteNotificationCommand(notification.Id));

        result.Should().BeTrue();
        _repositoryMock.Verify(r => r.Remove(notification), Times.Once);
        _unitOfWorkMock.Verify(u => u.CompleteAsync(), Times.Once);
    }

    [Fact]
    public async Task Handle_DeleteNotificationCommand_WhenNotFound_ReturnsFalse()
    {
        _repositoryMock.Setup(r => r.FindByIdAsync(999)).ReturnsAsync((NotificationEntity?)null);

        var result = await _service.Handle(new DeleteNotificationCommand(999));

        result.Should().BeFalse();
    }
}
