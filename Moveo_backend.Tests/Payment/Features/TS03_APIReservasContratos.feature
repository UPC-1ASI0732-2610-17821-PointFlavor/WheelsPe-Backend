@ignore
Feature: TS03 - API de Reservas y Contratos
  Como Developer
  Necesito gestionar reservas y contratos vía API
  Para automatizar el flujo de alquiler entre usuarios

  Background:
    Given el servidor backend está corriendo
    And la base de datos está disponible
    And existe un inquilino autenticado con token JWT válido
    And existe un vehículo disponible con id vehicle-abc

  Scenario: Crear reserva con éxito
    Given el frontend envía una solicitud de reserva válida con vehicleId vehicle-abc del 2025-06-10 al 2025-06-15
    When llama al endpoint POST /api/v1/rentals con esos datos
    Then recibe status 201
    And la respuesta contiene el ID de reserva generado
    And la respuesta contiene la URL del contrato digital

  Scenario: Rechazar reserva por conflicto de disponibilidad
    Given el vehículo vehicle-abc ya tiene una reserva activa del 2025-06-10 al 2025-06-15
    When el inquilino intenta crear otra reserva para esas mismas fechas en POST /api/v1/rentals
    Then recibe status 400
    And el cuerpo de respuesta indica conflicto de disponibilidad para esas fechas

  Scenario: Obtener detalle de una reserva existente
    Given la reserva con id rental-xyz existe y pertenece al inquilino autenticado
    When llama a GET /api/v1/rentals/rental-xyz
    Then recibe status 200
    And la respuesta contiene los datos completos de la reserva incluyendo estado y contrato

  Scenario: Cancelar una reserva pendiente
    Given la reserva con id rental-xyz tiene estado "Pending"
    When el inquilino llama a DELETE /api/v1/rentals/rental-xyz
    Then recibe status 200
    And el estado de la reserva cambia a "Canceled"
    And el vehículo queda disponible nuevamente para esas fechas
