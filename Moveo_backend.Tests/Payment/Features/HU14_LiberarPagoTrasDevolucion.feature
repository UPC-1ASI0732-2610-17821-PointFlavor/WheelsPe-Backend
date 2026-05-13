@ignore
Feature: HU14 - Liberar pago al propietario tras devolución
  Como sistema
  Debo liberar el pago al propietario solo tras confirmar la devolución del auto en buen estado
  Para garantizar la equidad en la transacción y proteger los intereses de ambas partes

  Background:
    Given el alquiler tiene estado "Completed"
    And el pago está en estado "Held" retenido hasta confirmación
    And el endpoint de pagos /api/v1/payments está activo

  Scenario: Transferir pago al propietario tras devolución sin daños
    Given el inquilino confirma la devolución del auto en la app
    And no hay reporte de daños asociado al alquiler
    When el sistema procesa la confirmación de devolución
    Then se transfiere el 90% del monto total al propietario
    And el 10% se retiene como comisión de la plataforma
    And el estado del pago cambia a "Released"
    And el propietario recibe una notificación de que el pago fue liberado

  Scenario: Retener pago por daños reportados
    Given el alquiler tiene un reporte de daños pendiente de revisión
    When el equipo de soporte investiga y valida el reclamo
    Then el pago permanece en estado "Held" hasta resolver el incidente
    And se notifica a propietario e inquilino sobre la retención y sus motivos
    And el caso queda asociado a un ticket de soporte activo
