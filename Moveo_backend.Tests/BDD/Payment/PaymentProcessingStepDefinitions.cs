using FluentAssertions;
using Moq;
using Moveo_backend.Payment.Domain.Model.Commands;
using Moveo_backend.Payment.Domain.Services;
using Reqnroll;
using PaymentEntity = Moveo_backend.Payment.Domain.Model.Aggregate.Payment;

namespace Moveo_backend.Tests.BDD.Payment;

[Binding]
public class PaymentProcessingStepDefinitions
{
    private readonly Mock<IPaymentCommandService> _paymentServiceMock = new();
    private PaymentEntity? _createdPayment;
    private bool _deleteResult;
    private PaymentEntity? _initializedPayment;

    [Given(@"the payment command service is available")]
    public void GivenThePaymentCommandServiceIsAvailable()
    {
        _paymentServiceMock
            .Setup(s => s.Handle(It.IsAny<CreatePaymentCommand>()))
            .ReturnsAsync((CreatePaymentCommand cmd) => new PaymentEntity(cmd));

        _paymentServiceMock
            .Setup(s => s.Handle(It.IsAny<DeletePaymentCommand>()))
            .ReturnsAsync(false);
    }

    [Given(@"a payment is created with no explicit currency")]
    public void GivenAPaymentIsCreatedWithNoExplicitCurrency()
    {
    }

    [When(@"I create a payment for rental (.*) with amount (.*) in ""(.*)"" via ""(.*)"" of type ""(.*)""")]
    public async Task WhenICreateAPayment(int rentalId, decimal amount, string currency, string method, string type)
    {
        var command = new CreatePaymentCommand(1, 2, rentalId, amount, currency, method, type);
        _createdPayment = await _paymentServiceMock.Object.Handle(command);
    }

    [When(@"I attempt to delete payment with id (.*)")]
    public async Task WhenIAttemptToDeletePaymentWithId(int id)
    {
        var command = new DeletePaymentCommand(id);
        _deleteResult = await _paymentServiceMock.Object.Handle(command);
    }

    [When(@"the payment is initialized")]
    public void WhenThePaymentIsInitialized()
    {
        var command = new CreatePaymentCommand(1, 2, 1, 100m, null, "transfer", "rental");
        _initializedPayment = new PaymentEntity(command);
    }

    [Then(@"the payment should be created successfully")]
    public void ThenThePaymentShouldBeCreatedSuccessfully()
    {
        _createdPayment.Should().NotBeNull();
    }

    [Then(@"the payment status should be ""(.*)""")]
    public void ThenThePaymentStatusShouldBe(string status)
    {
        _createdPayment!.Status.Should().Be(status);
    }

    [Then(@"the payment delete result should be false")]
    public void ThenThePaymentDeleteResultShouldBeFalse()
    {
        _deleteResult.Should().BeFalse();
    }

    [Then(@"the payment currency should be ""(.*)""")]
    public void ThenThePaymentCurrencyShouldBe(string currency)
    {
        _initializedPayment!.Currency.Should().Be(currency);
    }
}
