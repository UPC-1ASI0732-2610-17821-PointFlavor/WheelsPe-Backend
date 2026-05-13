# language: es
Feature: HU10 - Filtrar autos por ubicación y fecha
  Como inquilino
  Quiero buscar autos disponibles en mi zona y en las fechas que necesito
  Para planificar mi viaje o actividad con anticipación

  Background:
    Given el inquilino está autenticado
    And existen vehículos publicados en la plataforma
    And el endpoint GET /api/v1/vehicles está activo

  Scenario: Aplicar filtros básicos de ubicación y fecha con éxito
    Given el inquilino ingresa su distrito "Miraflores" y rango de fechas del 2025-06-10 al 2025-06-15
    When llama a GET /api/v1/vehicles con parámetros location Miraflores y las fechas indicadas
    Then el sistema responde con status 200
    And la respuesta solo contiene autos disponibles en ese periodo y ubicación

  Scenario: Reordenar resultados por preferencia
    Given el inquilino ya aplicó filtros de ubicación Miraflores y fechas válidas
    When aplica filtros adicionales ordenando por precio ascendente
    Then el sistema responde con status 200
    And la lista de autos se reordena por precio de menor a mayor
    And no se pierden los filtros previos de ubicación y fecha
