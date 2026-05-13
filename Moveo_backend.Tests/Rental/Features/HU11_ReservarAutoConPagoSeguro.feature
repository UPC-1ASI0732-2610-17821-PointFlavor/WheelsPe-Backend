# language: es
Feature: HU11 - Reservar un auto con pago seguro
  Como inquilino
  Quiero reservar un auto y realizar el pago de forma segura
  Para garantizar la transacción

  Background:
    Given el inquilino está autenticado con token JWT válido
    And el auto seleccionado está disponible en las fechas requeridas
    And el endpoint POST /api/v1/rentals está activo

  Scenario: Bloquear auto y generar contrato tras confirmar reserva
    Given el inquilino selecciona un auto disponible con id de vehículo válido
    And elige el método de pago tarjeta de crédito
    When confirma la reserva llamando a POST /api/v1/rentals con los datos de la reserva
    Then el sistema responde con status 201
    And el sistema bloquea el vehículo cambiando su disponibilidad
    And se genera un contrato digital asociado a la reserva
    And la respuesta contiene el id de reserva y la URL del contrato

  Scenario: Rechazar reserva si el auto no está disponible
    Given el auto seleccionado ya tiene una reserva activa para las mismas fechas
    When el inquilino intenta crear una nueva reserva para esas fechas en POST /api/v1/rentals
    Then el sistema responde con status 400
    And el cuerpo de respuesta indica conflicto de disponibilidad para esas fechas

  Scenario: Recibir confirmación final tras aceptación del propietario
    Given la reserva fue creada y el pago fue procesado exitosamente
    When el propietario acepta la reserva
    Then el inquilino recibe una notificación de confirmación final
    And la notificación incluye los detalles de entrega del vehículo
