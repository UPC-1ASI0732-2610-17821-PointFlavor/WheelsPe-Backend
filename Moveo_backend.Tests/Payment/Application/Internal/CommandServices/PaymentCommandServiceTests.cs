using FluentAssertions;
using Moq;
using Moveo_backend.Payment.Application.Internal.CommandServices;
using Moveo_backend.Payment.Domain.Model.Commands;
using Moveo_backend.Payment.Domain.Repositories;
using Moveo_backend.Shared.Domain.Repositories;
using PaymentEntity = Moveo_backend.Payment.Domain.Model.Aggregate.Payment;
using Xunit;

namespace Moveo_backend.Tests.Payment.Application.Internal.CommandServices;

public class PaymentCommandServiceTests
{
    private readonly Mock<IPaymentRepository> _repositoryMock;
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly PaymentCommandService _service;

    public PaymentCommandServiceTests()
    {
        _repositoryMock = new Mock<IPaymentRepository>();
        _unitOfWorkMock = new Mock<IUnitOfWork>();
        _unitOfWorkMock.Setup(u => u.CompleteAsync()).Returns(Task.CompletedTask);
        _service = new PaymentCommandService(_repositoryMock.Object, _unitOfWorkMock.Object);
    }

    // HU11 / TS03 - Crear pago al confirmar reserva
    [Fact]
    public async Task Handle_CreatePaymentCommand_CreatesAndReturnsPayment()
    {
        var command = new CreatePaymentCommand(
            PayerId: 1, RecipientId: 2, RentalId: 10,
            Amount: 540m, Currency: "PEN", Method: "card", Type: "rental");

        _repositoryMock.Setup(r => r.AddAsync(It.IsAny<PaymentEntity>()))
            .Returns(Task.CompletedTask);

        var result = await _service.Handle(command);

        result.Should().NotBeNull();
        result!.Amount.Should().Be(540m);
        result.Status.Should().Be("pending");
        _repositoryMock.Verify(r => r.AddAsync(It.IsAny<PaymentEntity>()), Times.Once);
        _unitOfWorkMock.Verify(u => u.CompleteAsync(), Times.Once);
    }

    // HU14 - Liberar pago
    [Fact]
    public async Task Handle_PatchPaymentCommand_WhenPaymentExists_UpdatesStatus()
    {
        var existingPayment = new PaymentEntity(new CreatePaymentCommand(
            1, 2, 10, 540m, "PEN", "card", "rental"));

        _repositoryMock.Setup(r => r.FindByIdAsync(existingPayment.Id))
            .ReturnsAsync(existingPayment);

        var patch = new PatchPaymentCommand(
            Id: existingPayment.Id,
            Status: "completed",
            TransactionId: "TXN-001");

        var result = await _service.Handle(patch);

        result.Should().NotBeNull();
        _unitOfWorkMock.Verify(u => u.CompleteAsync(), Times.Once);
    }

    [Fact]
    public async Task Handle_PatchPaymentCommand_WhenNotFound_ReturnsNull()
    {
        _repositoryMock.Setup(r => r.FindByIdAsync(999))
            .ReturnsAsync((PaymentEntity?)null);

        var patch = new PatchPaymentCommand(Id: 999, Status: "completed");

        var result = await _service.Handle(patch);

        result.Should().BeNull();
        _unitOfWorkMock.Verify(u => u.CompleteAsync(), Times.Never);
    }

    // HU14 - Eliminar pago
    [Fact]
    public async Task Handle_DeletePaymentCommand_WhenPaymentExists_ReturnsTrue()
    {
        var existingPayment = new PaymentEntity(new CreatePaymentCommand(
            1, 2, 10, 540m, "PEN", "card", "rental"));

        _repositoryMock.Setup(r => r.FindByIdAsync(existingPayment.Id))
            .ReturnsAsync(existingPayment);

        var result = await _service.Handle(new DeletePaymentCommand(existingPayment.Id));

        result.Should().BeTrue();
        _repositoryMock.Verify(r => r.Remove(existingPayment), Times.Once);
    }

    [Fact]
    public async Task Handle_DeletePaymentCommand_WhenNotFound_ReturnsFalse()
    {
        _repositoryMock.Setup(r => r.FindByIdAsync(999))
            .ReturnsAsync((PaymentEntity?)null);

        var result = await _service.Handle(new DeletePaymentCommand(999));

        result.Should().BeFalse();
    }
}
