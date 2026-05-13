@ignore
Feature: HU15 - Verificar identidad con DNI y selfie
  Como sistema
  Debo verificar la identidad del usuario con DNI y selfie
  Para prevenir fraudes y garantizar autenticidad en la plataforma

  Background:
    Given el usuario está registrado con estado "PendingVerification"
    And el sistema de verificación de identidad está activo
    And el endpoint de actualización de usuario está disponible en PUT /api/v1/users/{id}

  Scenario: Aprobar verificación de identidad automáticamente
    Given el usuario sube una foto legible de su DNI y una selfie clara
    When el sistema compara los rostros y valida los datos del documento
    Then el sistema aprueba la verificación automáticamente
    And el estado del usuario cambia a "Verified"
    And el perfil del usuario muestra el distintivo de "Verificado"

  Scenario: Rechazar verificación por documentos inválidos
    Given el usuario sube una foto de DNI ilegible o que no coincide con la selfie
    When el sistema procesa los documentos
    Then el sistema rechaza la verificación automáticamente
    And el estado del usuario permanece en "PendingVerification"
    And el usuario recibe una notificación indicando el motivo del rechazo

  Scenario: Reintentar verificación tras fallo
    Given la verificación del usuario falló anteriormente
    When el usuario reenvía sus documentos DNI y selfie actualizados
    Then el sistema vuelve a intentar la validación automática
    And si los documentos siguen siendo inválidos el caso se deriva a verificación manual
    And el usuario recibe notificación del nuevo estado del proceso
