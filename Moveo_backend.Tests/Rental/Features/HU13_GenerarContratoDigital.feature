@ignore
Feature: HU13 - Generar contrato digital automático
  Como sistema
  Debo generar un contrato digital al confirmar la reserva
  Para formalizar el acuerdo entre las partes

  Background:
    Given la reserva fue aceptada por el propietario
    And el pago fue confirmado exitosamente

  Scenario: Crear contrato digital tras confirmación de pago
    Given la reserva tiene estado "Accepted" y el pago asociado tiene estado "Confirmed"
    When el sistema procesa la confirmación del pago en el flujo de reservas
    Then se genera automáticamente un documento de contrato
    And el contrato incluye condiciones fechas y datos de propietario e inquilino
    And la respuesta de la reserva contiene la URL del contrato generado

  Scenario: Enviar contrato por correo a ambas partes en menos de 5 minutos
    Given el contrato digital fue generado exitosamente
    When el sistema envía el contrato por correo electrónico
    Then tanto el propietario como el inquilino reciben el PDF adjunto con firma digital válida
    And el envío ocurre en menos de 5 minutos desde la generación
