using FluentAssertions;
using Moq;
using Moveo_backend.Shared.Domain.Repositories;
using Moveo_backend.Support.Application.Internal.CommandServices;
using Moveo_backend.Support.Domain.Model.Aggregate;
using Moveo_backend.Support.Domain.Model.Commands;
using Moveo_backend.Support.Domain.Repositories;
using Xunit;

namespace Moveo_backend.Tests.Support.Application.Internal.CommandServices;

public class SupportTicketCommandServiceTests
{
    private readonly Mock<ISupportTicketRepository> _repositoryMock;
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly SupportTicketCommandService _service;

    public SupportTicketCommandServiceTests()
    {
        _repositoryMock = new Mock<ISupportTicketRepository>();
        _unitOfWorkMock = new Mock<IUnitOfWork>();
        _unitOfWorkMock.Setup(u => u.CompleteAsync()).Returns(Task.CompletedTask);
        _service = new SupportTicketCommandService(_repositoryMock.Object, _unitOfWorkMock.Object);
    }

    private static CreateSupportTicketCommand MakeCreateCommand() =>
        new(UserId: 1, Subject: "Vehículo dañado", Description: "El auto llegó con rayones",
            Category: "rental_issue", Priority: "high", Type: "damage",
            RelatedId: 10, RelatedType: "rental",
            EstimatedCost: 350m, VehicleId: 5, VehicleName: "Toyota Corolla",
            RentalId: 10, RenterId: 2, RenterName: "Carlos López",
            AttachmentsJson: null);

    // HU13 - Crear ticket de soporte
    [Fact]
    public async Task Handle_CreateSupportTicketCommand_ReturnsTicketWithStatusOpen()
    {
        _repositoryMock.Setup(r => r.AddAsync(It.IsAny<SupportTicket>())).Returns(Task.CompletedTask);

        var result = await _service.Handle(MakeCreateCommand());

        result.Should().NotBeNull();
        result!.Subject.Should().Be("Vehículo dañado");
        result.Status.Should().Be("open");
        _unitOfWorkMock.Verify(u => u.CompleteAsync(), Times.Once);
    }

    // Cerrar ticket
    [Fact]
    public async Task Handle_CloseSupportTicketCommand_WhenExists_SetsStatusClosed()
    {
        var ticket = new SupportTicket(MakeCreateCommand());
        _repositoryMock.Setup(r => r.FindByIdAsync(ticket.Id)).ReturnsAsync(ticket);

        var result = await _service.Handle(new CloseSupportTicketCommand(ticket.Id));

        result.Should().NotBeNull();
        result!.Status.Should().Be("closed");
        _unitOfWorkMock.Verify(u => u.CompleteAsync(), Times.Once);
    }

    [Fact]
    public async Task Handle_CloseSupportTicketCommand_WhenNotFound_ReturnsNull()
    {
        _repositoryMock.Setup(r => r.FindByIdAsync(999)).ReturnsAsync((SupportTicket?)null);

        var result = await _service.Handle(new CloseSupportTicketCommand(999));

        result.Should().BeNull();
        _unitOfWorkMock.Verify(u => u.CompleteAsync(), Times.Never);
    }

    // Eliminar ticket
    [Fact]
    public async Task Handle_DeleteSupportTicketCommand_WhenExists_ReturnsTrue()
    {
        var ticket = new SupportTicket(MakeCreateCommand());
        _repositoryMock.Setup(r => r.FindByIdAsync(ticket.Id)).ReturnsAsync(ticket);

        var result = await _service.Handle(new DeleteSupportTicketCommand(ticket.Id));

        result.Should().BeTrue();
        _repositoryMock.Verify(r => r.Remove(ticket), Times.Once);
        _unitOfWorkMock.Verify(u => u.CompleteAsync(), Times.Once);
    }

    [Fact]
    public async Task Handle_DeleteSupportTicketCommand_WhenNotFound_ReturnsFalse()
    {
        _repositoryMock.Setup(r => r.FindByIdAsync(999)).ReturnsAsync((SupportTicket?)null);

        var result = await _service.Handle(new DeleteSupportTicketCommand(999));

        result.Should().BeFalse();
    }
}
