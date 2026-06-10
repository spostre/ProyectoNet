// ============================================================
// PROGRAM.CS - Punto de entrada de la aplicación ASP.NET Core
// Aquí se configura toda la inyección de dependencias (IoC container)
// y el pipeline de middlewares HTTP.
// El orden importa: los servicios se registran primero (builder.*),
// luego se configura el pipeline (app.*).
// ============================================================

var builder = WebApplication.CreateBuilder(args);

// ── 1. BASE DE DATOS ────────────────────────────────────────
// Lee la cadena de conexión de appsettings.json → ConnectionStrings → DefaultConnection
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
builder.Services.AddDbContext<AutoTallerDbContext>(options =>
{
    // MySqlServerVersion indica qué versión de MySQL Server se usa (8.0.30)
    // Esto permite a EF Core usar funciones específicas de esa versión
    var serverVersion = new MySqlServerVersion(new Version(8, 0, 30));
    options.UseMySql(connectionString, serverVersion);
});

// ── 2. UNIT OF WORK Y SERVICIOS DE NEGOCIO ──────────────────
// AddScoped: una instancia por request HTTP (ideal para repositorios y servicios con DbContext)
builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();           // Patrón Unit of Work (acceso a todos los repos)
builder.Services.AddScoped<IOrdenServicioService, OrdenServicioService>(); // Lógica de negocio de órdenes
builder.Services.AddScoped<IClienteService, ClienteService>();   // Lógica de negocio de clientes

// ── 3. AUTOMAPPER ────────────────────────────────────────────
// Registra el perfil de mapeo Entidad ↔ DTO definido en Api/Mappings/AutoMapperProfile.cs
// Inyectable como IMapper en controladores y servicios
builder.Services.AddAutoMapper(cfg => {
    cfg.AddProfile<AutoMapperProfile>();
});

// ── 4. RATE LIMITING (Limitación de peticiones por IP) ──────
// Previene abuso de la API. La configuración de límites está en appsettings.json → IpRateLimiting
builder.Services.AddMemoryCache(); // Necesario para almacenar los contadores de rate limiting en memoria
builder.Services.Configure<IpRateLimitOptions>(builder.Configuration.GetSection("IpRateLimiting"));
builder.Services.AddInMemoryRateLimiting();
builder.Services.AddSingleton<IRateLimitConfiguration, RateLimitConfiguration>();

// ── 5. AUTENTICACIÓN JWT ─────────────────────────────────────
// Lee los parámetros de JWT desde appsettings.json → JwtSettings
var jwtSettings = builder.Configuration.GetSection("JwtSettings");
var key = Encoding.ASCII.GetBytes(jwtSettings["Key"]!); // Clave secreta convertida a bytes para firmar/verificar tokens

// Configura el esquema de autenticación predeterminado como JWT Bearer
builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme; // Esquema para autenticar
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;    // Esquema para el challenge (401)
})
.AddJwtBearer(options =>
{
    options.RequireHttpsMetadata = false; // En desarrollo se permite HTTP (sin TLS)
    options.SaveToken = true;             // Guarda el token en el contexto HTTP para uso posterior
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuerSigningKey = true,                         // Verificar que el token fue firmado con nuestra clave
        IssuerSigningKey = new SymmetricSecurityKey(key),        // La clave usada para verificar la firma
        ValidateIssuer = true,                                   // Verificar que el campo "iss" del JWT coincide
        ValidateAudience = true,                                 // Verificar que el campo "aud" del JWT coincide
        ValidIssuer = jwtSettings["Issuer"],                     // Valor esperado de "iss" (quién emitió el token)
        ValidAudience = jwtSettings["Audience"],                 // Valor esperado de "aud" (quién consume el token)
        ClockSkew = TimeSpan.Zero                                // Sin margen de tiempo: el token expira exactamente cuando dice
    };
});

// IHttpContextAccessor permite acceder al HttpContext desde servicios no-controller
// (lo usa el interceptor de Auditoría para leer el JWT del request actual)
builder.Services.AddHttpContextAccessor();
builder.Services.AddControllers(); // Registra los controladores de la API

// ── 6. SWAGGER (Documentación interactiva de la API) ────────
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(); // Genera el JSON de OpenAPI en /swagger

var app = builder.Build(); // Construye la aplicación con todos los servicios registrados

// ── PIPELINE DE MIDDLEWARES HTTP ─────────────────────────────
// El orden es crítico: cada middleware procesa el request en el orden en que se agrega

if (app.Environment.IsDevelopment())
{
    // Swagger solo disponible en desarrollo (no exponer en producción sin protección)
    app.UseSwagger();
    app.UseSwaggerUI(); // UI interactiva en /swagger/index.html
}

app.UseIpRateLimiting(); // Aplicar límites de peticiones por IP antes de cualquier lógica

app.UseHttpsRedirection(); // Redirige HTTP → HTTPS (recomendado en producción)

app.UseAuthentication(); // Lee y valida el JWT del header Authorization (debe ir ANTES de Authorization)
app.UseAuthorization();  // Verifica los roles/permisos del usuario autenticado (aplica [Authorize])

app.MapControllers(); // Mapea las rutas de los controladores (ej. GET /api/clientes)

// ── 7. MIGRACIÓN AUTOMÁTICA DE BASE DE DATOS AL INICIAR ─────
// Al arrancar la app, aplica automáticamente las migraciones pendientes.
// Si la BD no existe, la crea. Esto garantiza que en cualquier PC nueva funcione sin pasos manuales.
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    try
    {
        var context = services.GetRequiredService<AutoTallerDbContext>();
        context.Database.Migrate(); // Aplica todas las migraciones pendientes (o crea la BD si no existe)
    }
    catch (Exception ex)
    {
        // Si falla la migración, registra el error pero no interrumpe el arranque completamente
        var logger = services.GetRequiredService<ILogger<Program>>();
        logger.LogError(ex, "Ocurrió un error al aplicar las migraciones de la base de datos.");
    }
}

app.Run(); // Inicia el servidor y empieza a escuchar peticiones HTTP
