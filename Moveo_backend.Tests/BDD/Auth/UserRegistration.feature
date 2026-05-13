Feature: User Registration
  As a new user
  I want to register an account on the MOVEO platform
  So that I can access vehicle rental services

  Scenario: Successful registration with valid data
    Given no user exists with email "ana@moveo.com"
    When I register with name "Ana Torres", email "ana@moveo.com" and password "Secure123!"
    Then the registration should succeed
    And the returned user email should be "ana@moveo.com"

  Scenario: Registration fails when email is already taken
    Given a user already exists with email "carlos@moveo.com"
    When I register again with email "carlos@moveo.com" and password "Pass456!"
    Then the registration should fail with null result

  Scenario: Login succeeds with correct credentials
    Given a registered user with email "driver@moveo.com" and password "Road2025!"
    When I login with email "driver@moveo.com" and password "Road2025!"
    Then the login should succeed
    And the authenticated user email should be "driver@moveo.com"
