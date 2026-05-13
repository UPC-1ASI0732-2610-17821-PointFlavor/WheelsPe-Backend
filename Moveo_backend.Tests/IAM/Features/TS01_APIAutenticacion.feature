# language: es
Feature: TS01 - API de Autenticación (Login/Register)
  Como Developer
  Necesito una API para login y registro de usuarios
  Que garantice la seguridad de las credenciales

  Background:
    Given el servidor backend está corriendo
    And la base de datos está disponible

  Scenario: Registrar nuevo usuario con éxito
    Given el frontend envía datos de registro válidos
      | campo     | valor            |
      | firstName | Juan             |
      | lastName  | García           |
      | email     | juan@example.com |
      | password  | Password123!     |
      | role      | tenant           |
    When llama al endpoint POST /api/v1/users
    Then recibe status 201
    And el cuerpo de respuesta contiene el objeto del usuario creado con rol asignado
    And la contraseña no está expuesta en la respuesta

  Scenario: Rechazar registro por correo duplicado
    Given el email "juan@example.com" ya existe en la base de datos
    When el frontend intenta registrar un nuevo usuario con ese mismo email en POST /api/v1/users
    Then recibe status 409
    And el cuerpo de respuesta contiene un mensaje de conflicto

  Scenario: Login exitoso con credenciales válidas
    Given el usuario "juan@example.com" está registrado con contraseña "Password123!"
    When llama al endpoint POST /api/v1/auth/login con esas credenciales
    Then recibe status 200
    And el cuerpo de respuesta contiene un token JWT válido

  Scenario: Rechazar login con credenciales inválidas
    Given el usuario "juan@example.com" está registrado
    When llama al endpoint POST /api/v1/auth/login con contraseña incorrecta "Wrong123!"
    Then recibe status 401
    And el cuerpo de respuesta contiene un mensaje de error de autenticación
