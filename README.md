# Busticket

Plataforma web para la gestión de rutas de transporte intermunicipal y la compra de boletos de bus en línea. Permite a los usuarios buscar rutas, elegir asientos de forma visual e interactiva y pagar de forma segura, todo desde el navegador.

---

## Características principales

- **Búsqueda de rutas** con mapa interactivo (Leaflet + trazado real por carretera con OSRM)
- **Selección visual de asientos** con estado en tiempo real (libre, reservado, ocupado)
- **Tipos de bus configurables**: Normal (un piso), Dos pisos y VIP, con cantidad de asientos personalizada
- **Carrito de compra** y flujo de pago con preview de tarjeta en tiempo real
- **Generación de boletos en PDF** enviados al correo del comprador
- **Panel de administrador** para gestionar rutas, empresas y ventas
- **Panel de empresa** para que cada transportadora gestione sus propias rutas
- **Perfil de usuario** con historial de viajes
- **Centro de ayuda** con preguntas frecuentes

---

## Tecnologías

| Capa | Tecnología |
|---|---|
| Backend | ASP.NET Core MVC (.NET 10) · C# |
| ORM | Entity Framework Core |
| Base de datos | SQL Server 2022 |
| Frontend | Tailwind CSS CDN · JavaScript vanilla |
| Mapas | Leaflet.js · OSRM (enrutamiento por carretera) |
| Autenticación | ASP.NET Core Identity |
| PDF | (generación server-side) |
| Correo | SMTP / servicio de email |

---

## Roles de usuario

| Rol | Acceso |
|---|---|
| **Cliente** | Buscar rutas, comprar boletos, ver historial |
| **Empresa** | Gestionar sus propias rutas y ver ventas |
| **Admin** | Acceso completo: rutas, empresas, reportes |

---

## Requisitos previos

- [.NET 10 SDK](https://dotnet.microsoft.com/download)
- SQL Server 2022 (o Docker: `docker run -e "ACCEPT_EULA=Y" -e "SA_PASSWORD=Bus123Tick!" -p 1433:1433 -d mcr.microsoft.com/mssql/server:2022-latest`)
- dotnet-ef tools: `dotnet tool install --global dotnet-ef`

---

## Instalación y ejecución

```bash
# 1. Clonar el repositorio
git clone https://github.com/carlos2barragan/BusTickets.git
cd BusTickets

# 2. Configurar la cadena de conexión en Busticket/appsettings.json
#    (ajustar Server, User Id y Password según tu entorno)

# 3. Aplicar migraciones y crear la base de datos
cd Busticket
dotnet ef database update

# 4. Ejecutar
dotnet run
```

La aplicación queda disponible en `http://localhost:5180`.

---

## Estructura del proyecto

```
Busticket/
├── Controllers/          # Lógica de cada módulo (Admin, PanelEmpresa, Rutas, Pago…)
├── Models/               # Entidades y ViewModels
├── Views/                # Vistas Razor por módulo
│   ├── Shared/           # Header compartido (_SharedHeader.cshtml)
│   ├── Admin/
│   ├── PanelEmpresa/
│   ├── Rutas/
│   ├── Pago/
│   ├── Perfil/
│   └── Ayuda/
├── Data/                 # ApplicationDbContext
├── Migrations/           # Historial de migraciones EF Core
└── wwwroot/              # Archivos estáticos (CSS, JS, imágenes)
```
