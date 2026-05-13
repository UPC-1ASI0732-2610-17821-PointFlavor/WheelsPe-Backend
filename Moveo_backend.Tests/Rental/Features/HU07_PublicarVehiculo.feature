# language: es
Feature: HU07 - Publicar un vehículo
  Como propietario
  Quiero publicar mi auto
  Para que los inquilinos puedan alquilarlo

  Background:
    Given el propietario está autenticado con un token JWT válido
    And su cuenta tiene estado "Active"
    And el endpoint POST /api/v1/vehicles está activo

  Scenario: Registrar vehículo con éxito
    Given el propietario accede a la sección de gestión de autos
    And tiene los datos del vehículo: brand Toyota, model Corolla, year 2022, dailyPrice 150.00, location Miraflores
    When ingresa los detalles del vehículo fotos y precio en POST /api/v1/vehicles
    Then el sistema responde con status 201
    And el auto se publica con estado "Active"
    And el vehículo aparece en los resultados de búsqueda de inquilinos

  Scenario: Recibir notificación cuando un inquilino selecciona el auto
    Given el auto fue publicado correctamente con un id de vehículo válido
    When un inquilino selecciona el auto para reservarlo
    Then el propietario recibe una notificación con los detalles de la solicitud
    And la notificación es accesible en GET /api/v1/notifications del propietario
