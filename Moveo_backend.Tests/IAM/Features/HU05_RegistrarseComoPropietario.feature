# language: es
Feature: HU05 - Registrarse como propietario
  Como propietario
  Quiero registrarme en la plataforma
  Para publicar mi auto y generar ingresos pasivos

  Background:
    Given la plataforma MOVEO está disponible
    And el endpoint POST /api/v1/users está activo

  Scenario: Completar perfil y subir documentos del vehículo exitosamente
    Given el usuario selecciona registrarse como propietario
    And completa su perfil con nombre, email, teléfono y contraseña válidos
    And sube los documentos del vehículo (tarjeta de propiedad, SOAT)
    When envía el formulario de registro al endpoint POST /api/v1/users con rol "owner"
    Then el sistema responde con status 201
    And la cuenta queda en estado "PendingVerification"
    And se genera un ID de usuario único

  Scenario: Activar cuenta tras verificación exitosa
    Given el usuario completó su registro con estado "PendingVerification"
    When el sistema de verificación aprueba los documentos del propietario
    Then el sistema notifica al usuario que su cuenta fue aprobada
    And el estado de la cuenta cambia a "Active"
    And el propietario puede publicar su auto y comenzar a recibir solicitudes de alquiler
