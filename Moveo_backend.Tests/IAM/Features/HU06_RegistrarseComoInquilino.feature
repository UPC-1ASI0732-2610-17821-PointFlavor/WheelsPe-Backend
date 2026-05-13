@ignore
Feature: HU06 - Registrarse como inquilino
  Como inquilino
  Quiero registrarme en la plataforma
  Para buscar y alquilar autos según mis necesidades

  Background:
    Given la plataforma MOVEO está disponible
    And el endpoint POST /api/v1/users está activo

  Scenario: Completar registro y verificar identidad exitosamente
    Given el usuario selecciona registrarse como inquilino
    And completa su perfil con nombre, email, teléfono y contraseña válidos
    And sube su DNI y selfie para verificación de identidad
    When envía el formulario de registro al endpoint POST /api/v1/users con rol "tenant"
    Then el sistema responde con status 201
    And el usuario puede buscar y reservar autos inmediatamente
    And el estado de la cuenta queda en "Active"

  Scenario: Recibir recomendaciones personalizadas al primer inicio de sesión
    Given el inquilino finalizó el registro correctamente
    When inicia sesión por primera vez mediante POST /api/v1/auth/login
    Then el sistema responde con status 200 y un token JWT válido
    And el inquilino recibe recomendaciones de autos basadas en su ubicación y preferencias
