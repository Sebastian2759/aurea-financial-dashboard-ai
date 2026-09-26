# Enunciado traducido

Fuente: PDF facilitado por el usuario. Este documento describe la prueba; no sustituye las decisiones aprobadas.

Prueba para candidatos

Panel financiero en tiempo real con RBAC dinámico

Esta prueba está diseñada para evaluar la ingeniería de prompts, la integración de API, la implementación de seguridad (RBAC) y la verificación de código con plazos realistas.

Objetivo

Desarrollar un panel full-stack de mercados financieros en tiempo real que consuma API públicas de datos financieros, implemente control de acceso basado en roles (RBAC) y restrinja dinámicamente los elementos de la interfaz y los endpoints de la API según los privilegios del usuario.

Tiempo previsto para completar la prueba: 1-2 horas

Uso de IA: Se recomienda encarecidamente. Se espera que los candidatos utilicen IA para generar la estructura inicial, programar, refactorizar y crear pruebas con rapidez.

Requisitos técnicos

1. Ingesta e integración de datos (API pública)

Integrar la API gratuita de CoinGecko para obtener datos de criptomonedas.

Mostrar métricas en tiempo real (p. ej., precios de activos, volatilidad de 24 h, tipos de cambio y capitalización de mercado), con controles, menús desplegables o selectores que permitan elegir qué métricas se muestran.

Gestionar adecuadamente los límites de solicitudes de la API mediante una capa de caché, datos simulados de respaldo o reintentos con espera exponencial.

2. Arquitectura de control de acceso basado en roles (RBAC)

Implementar un motor de autenticación (JWT o token de sesión simulado) con tres roles distintos:

Rol

Nivel de acceso y privilegios

Lector (Viewer)

Acceso de solo lectura a métricas estándar y resúmenes del mercado.

Operador (Trader)

Acceso a métricas estándar y posibilidad de crear, editar o eliminar activos en una lista de seguimiento personalizada.

Administrador (Admin)

Acceso completo: ver datos, editar listas de seguimiento, ver registros del sistema y ajustar los umbrales globales del panel (como los que activan alertas de volatilidad del mercado).

Requisitos de aplicación de permisos

Control en el backend: El middleware debe aplicar estos permisos en todas las rutas del servidor y endpoints de la API, devolviendo los códigos HTTP 401 Unauthorized o 403 Forbidden ante solicitudes no autorizadas.

Control en el frontend: La interfaz debe deshabilitar visualmente u ocultar componentes, botones y vistas según el rol autenticado.

3. Funcionalidades principales e interfaz

Selector de roles: Un control o selector en la interfaz para cambiar rápidamente de usuario autenticado (p. ej., "Iniciar sesión como lector", "Iniciar sesión como operador" o "Iniciar sesión como administrador") para realizar pruebas.

Gráficos financieros interactivos: Al menos un gráfico dinámico (Chart.js, Recharts o D3) que muestre tendencias históricas o en tiempo real.

Registro de auditoría y actividad (solo Admin): Un registro consultable de las acciones de los usuarios (p. ej., el operador X añadió BTC a su lista de seguimiento; el administrador Y actualizó un umbral).

Entregables e instrucciones de entrega

1.

Repositorio de código fuente (enlace de GitHub) que contenga:

Implementación del frontend y del backend.

Un archivo docker-compose.yml O un script de inicio con un solo comando (npm run start:all, make dev, etc.).

2.

Registro AI_PROMPTS.md: Un documento breve que detalle:

Los prompts clave utilizados para generar la estructura inicial o resolver partes complejas del proyecto.

Al menos un ejemplo en el que la IA haya generado código incorrecto o inseguro (un error, una alucinación o una falla de RBAC), y cómo lo detectaste y corregiste.

3.

Pruebas automatizadas:

Un conjunto automatizado de pruebas unitarias o de integración que verifique la implementación.
