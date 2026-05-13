using FluentAssertions;
using Moveo_backend.Payment.Domain.Model.Commands;
using Xunit;
using PaymentEntity = Moveo_backend.Payment.Domain.Model.Aggregate.Payment;

namespace Moveo_backend.Tests.Payment.Domain.Model.Aggregate;

public class PaymentTests
{
    private static CreatePaymentCommand DefaultCommand() =>
        new CreatePaymentCommand(
            PayerId: 1,
            RecipientId: 2,
            RentalId: 10,
            Amount: 540m,
            Currency: "PEN",
            Method: "card",
            Type: "rental",
            Description: "Pago de alquiler");

    // HU11 / TS03 - Crear pago
    [Fact]
    public void Constructor_FromCommand_SetsAllPropertiesCorrectly()
    {
        var payment = new PaymentEntity(DefaultCommand());

        payment.PayerId.Should().Be(1);
        payment.RecipientId.Should().Be(2);
        payment.RentalId.Should().Be(10);
        payment.Amount.Should().Be(540m);
        payment.Currency.Should().Be("PEN");
        payment.Method.Should().Be("card");
        payment.Type.Should().Be("rental");
        payment.Status.Should().Be("pending");
        payment.CompletedAt.Should().BeNull();
    }

    // HU14 - Liberar pago tras devolución
    [Fact]
    public void Complete_SetsStatusCompletedAndTransactionId()
    {
        var payment = new PaymentEntity(DefaultCommand());

        payment.Complete("TXN-ABC-123");

        payment.Status.Should().Be("completed");
        payment.TransactionId.Should().Be("TXN-ABC-123");
        payment.CompletedAt.Should().NotBeNull();
    }

    [Fact]
    public void Complete_WithNullTransactionId_SetsStatusCompleted()
    {
        var payment = new PaymentEntity(DefaultCommand());

        payment.Complete(null);

        payment.Status.Should().Be("completed");
        payment.TransactionId.Should().BeNull();
    }

    // HU21 - Reembolso por incidencia
    [Fact]
    public void Refund_SetsStatusRefundedAndReason()
    {
        var payment = new PaymentEntity(DefaultCommand());

        payment.Refund("Auto no cumplía condiciones");

        payment.Status.Should().Be("refunded");
        payment.Reason.Should().Be("Auto no cumplía condiciones");
    }

    [Fact]
    public void Fail_SetsStatusFailedAndReason()
    {
        var payment = new PaymentEntity(DefaultCommand());

        payment.Fail("Fondos insuficientes");

        payment.Status.Should().Be("failed");
        payment.Reason.Should().Be("Fondos insuficientes");
    }

    [Fact]
    public void DefaultConstructor_SetsCurrencyToPen()
    {
        var payment = new PaymentEntity();
        payment.Currency.Should().Be("PEN");
        payment.Status.Should().Be("pending");
    }

    // HU14 - Retener pago (PartialUpdate status)
    [Fact]
    public void PartialUpdate_WithStatus_ChangesOnlyStatus()
    {
        var payment = new PaymentEntity(DefaultCommand());
        var originalAmount = payment.Amount;

        var patch = new PatchPaymentCommand(
            Id: payment.Id,
            Status: "refunded",
            TransactionId: null,
            Reason: "Daño reportado",
            CompletedAt: null);

        payment.PartialUpdate(patch);

        payment.Status.Should().Be("refunded");
        payment.Reason.Should().Be("Daño reportado");
        payment.Amount.Should().Be(originalAmount);
    }
}
