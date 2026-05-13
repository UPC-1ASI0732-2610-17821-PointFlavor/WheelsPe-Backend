Feature: Rental Request
  As a renter
  I want to create and query rental requests
  So that I can book vehicles on the MOVEO platform

  Scenario: Creating a rental initializes it as pending
    Given the rental service accepts new rental requests
    When I create a rental for vehicle 1 by renter 2 from owner 3 with total price 450.00
    Then the created rental should have status "pending"
    And the created rental should reference vehicle 1

  Scenario: Querying a non-existent rental returns null
    Given the rental service has no rental with id 999
    When I query the rental with id 999
    Then the rental query result should be null

  Scenario: Deleting an existing rental returns true
    Given a rental with id 1 exists in the service
    When I delete the rental with id 1
    Then the delete result should be true
