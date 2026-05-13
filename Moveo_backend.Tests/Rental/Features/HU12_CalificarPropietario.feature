@ignore
Feature: HU12 - Calificar al propietario después del alquiler
  Como inquilino
  Quiero dejar una reseña sobre mi experiencia
  Para ayudar a otros usuarios y mejorar la calidad del servicio

  Background:
    Given el inquilino está autenticado
    And el alquiler ha finalizado con estado "Completed"
    And el endpoint POST /api/v1/reviews está activo

  Scenario: Publicar calificación y comentario exitosamente
    Given el inquilino accede a su historial de reservas y selecciona el alquiler finalizado
    When califica al propietario con 4 estrellas y deja el comentario Excelente servicio
    And envía la calificación a POST /api/v1/reviews
    Then el sistema responde con status 201
    And la calificación se publica en el perfil del propietario

  Scenario: Actualizar promedio de calificaciones del propietario
    Given la calificación fue enviada exitosamente
    When el sistema procesa la nueva calificación
    Then el promedio de calificaciones del propietario se actualiza correctamente
    And el propietario recibe una notificación informando sobre la nueva calificación
