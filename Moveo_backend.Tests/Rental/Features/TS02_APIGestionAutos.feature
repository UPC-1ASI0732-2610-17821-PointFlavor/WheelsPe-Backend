@ignore
Feature: TS02 - API de Gestión de Autos
  Como Developer
  Quiero endpoints para crear, leer, actualizar y eliminar autos
  Para que los propietarios gestionen su inventario de forma fluida y en tiempo real

  Background:
    Given el servidor backend está corriendo
    And la base de datos está disponible
    And existe un propietario autenticado con token JWT válido

  Scenario: Crear nuevo auto con éxito
    Given el frontend envía datos válidos de un auto
    When llama al endpoint POST /api/v1/vehicles con token de propietario
    Then recibe status 201
    And la respuesta contiene el auto creado con su ID generado

  Scenario: Obtener datos de un auto existente
    Given el auto con id 123 existe en la base de datos
    When llama a GET /api/v1/vehicles/123
    Then recibe status 200
    And la respuesta incluye los datos completos del auto incluyendo disponibilidad y precio diario

  Scenario: Actualizar datos de un auto existente
    Given el auto con id 123 pertenece al propietario autenticado
    When llama a PUT /api/v1/vehicles/123 con nuevo precio 180.00
    Then recibe status 200
    And la respuesta contiene el auto actualizado con el nuevo precio

  Scenario: Eliminar un auto existente
    Given el auto con id 123 pertenece al propietario autenticado
    And el auto no tiene reservas activas
    When llama a DELETE /api/v1/vehicles/123
    Then recibe status 204
    And el auto ya no aparece en GET /api/v1/vehicles
