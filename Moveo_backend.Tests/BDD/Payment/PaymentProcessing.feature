Feature: Payment Processing
  As a platform user
  I want to create and manage payments for my rentals
  So that rental transactions can be completed securely

  Scenario: Creating a new payment initializes it as pending
    Given the payment command service is available
    When I create a payment for rental 5 with amount 300.00 in "PEN" via "credit_card" of type "rental"
    Then the payment should be created successfully
    And the payment status should be "pending"

  Scenario: Deleting a non-existent payment returns false
    Given the payment command service is available
    When I attempt to delete payment with id 999
    Then the payment delete result should be false

  Scenario: Payment currency defaults to PEN when not specified
    Given a payment is created with no explicit currency
    When the payment is initialized
    Then the payment currency should be "PEN"
