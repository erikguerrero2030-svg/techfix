# Publicar TechFix en Render

El código de esta carpeta está preparado para publicarse como servicio Docker con PostgreSQL mediante `render.yaml`. La base local de SQL Server no viaja a Render: allí se crea una base nueva.

1. Sube el contenido de esta carpeta como raíz de un repositorio GitLab o GitHub privado.
2. En Render, crea un **Blueprint** desde ese repositorio. Render leerá `render.yaml` y ofrecerá crear el servicio web y la base PostgreSQL.
3. Ingresa `TECHFIX_ADMIN_EMAIL` y una contraseña fuerte en `TECHFIX_ADMIN_PASSWORD` cuando Render pida esas variables. No guardes la contraseña en Git.
4. Espera el despliegue y abre la dirección `https://...onrender.com` que Render asigne. La aplicación crea el esquema, catálogo, horarios y primer administrador al arrancar.
5. Para probar: crea una cuenta de cliente en la página publicada, reserva una cita y confirma que aparezca en el panel del administrador.

La base de datos gratuita de Render **expira después de 30 días**. Para conservar citas a largo plazo, elige un plan de base de datos persistente. El servicio gratuito puede tardar en despertar tras un periodo sin visitas. La base de datos local y los 20 registros cargados en ella siguen separados de la instalación pública.

Documentación: [Render Blueprints](https://render.com/docs/blueprint-spec), [límites del plan gratis](https://render.com/docs/free), [TLS de Render](https://render.com/docs/tls).
