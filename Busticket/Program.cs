using Busticket.Data;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.StaticFiles;
using Microsoft.EntityFrameworkCore;
using QuestPDF.Infrastructure;
using Busticket.Services;

var builder = WebApplication.CreateBuilder(args);

// ============================================================
// 1. CONFIGURACIÓN DE SERVICIOS (Dependency Injection)
// ============================================================

// Base de Datos
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(
        builder.Configuration.GetConnectionString("DefaultConnection")
    )
);

// Identity (Autenticación y Roles)
builder.Services.AddIdentity<IdentityUser, IdentityRole>(options =>
{
    options.SignIn.RequireConfirmedAccount = false;
    options.Password.RequireDigit = false;
    options.Password.RequiredLength = 6;
    options.Password.RequireNonAlphanumeric = false;
    options.Password.RequireUppercase = false;
})
.AddEntityFrameworkStores<ApplicationDbContext>()
.AddDefaultTokenProviders();

// Configuración de Cookies de Identity
builder.Services.ConfigureApplicationCookie(options =>
{
    options.LoginPath = "/Auth/Login";
    options.AccessDeniedPath = "/Auth/Login";
    options.ExpireTimeSpan = TimeSpan.FromMinutes(30);
    options.SlidingExpiration = true;
    options.Cookie.HttpOnly = true;
});

// Sesión (Esencial para el Carrito de Compras)
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true; // Forzar cookie incluso sin consentimiento previo
});

builder.Services.AddAuthorization();
builder.Services.AddControllersWithViews();

// Servicios de Terceros y Personalizados
QuestPDF.Settings.License = LicenseType.Community;
builder.Services.AddTransient<EmailService>();

var app = builder.Build();

// ============================================================
// 2. CONFIGURACIÓN DEL PIPELINE (Middleware - EL ORDEN IMPORTA)
// ============================================================

if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
}
else
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

// app.UseHttpsRedirection();

// Configuración de Archivos Estáticos y tipos MIME (GLB para modelos 3D)
var provider = new FileExtensionContentTypeProvider();
provider.Mappings[".glb"] = "model/gltf-binary";
provider.Mappings[".json"] = "application/json";

app.UseStaticFiles(new StaticFileOptions
{
    ContentTypeProvider = provider,
    ServeUnknownFileTypes = true
});

app.UseRouting();

// --- ORDEN CORRECTO DE AUTH Y SESIÓN ---
app.UseSession();        // 1. Sesión disponible primero
app.UseAuthentication(); // 2. Quién es el usuario
app.UseAuthorization();  // 3. Qué puede hacer el usuario

// ============================================================
// 3. RUTAS Y SEMILLADO (Seeding)
// ============================================================

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}"
);

// Inicializar Roles y Administrador automáticamente
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    try
    {
        await IdentitySeeder.SeedRolesAndAdmin(services);
    }
    catch (Exception ex)
    {
        var logger = services.GetRequiredService<ILogger<Program>>();
        logger.LogError(ex, "Error al ejecutar el Seeder de Identity.");
    }
}

app.Run();