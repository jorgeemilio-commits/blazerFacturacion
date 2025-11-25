using blazerFacturacion.Components;
using blazerFacturacion.Components.Servicios;
using blazerFacturacion.Components.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Data.Sqlite; 
var builder = WebApplication.CreateBuilder(args);

var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");

builder.Services.AddDbContext<FacturaDbContexto>(options => options.UseSqlite(connectionString));

builder.Services.AddRazorComponents().AddInteractiveServerComponents();
builder.Services.AddScoped<ServicioFactura>();
builder.Services.AddScoped<ControladorFactura>();
builder.Services.AddScoped<ServicioArticulo>();
builder.Services.AddScoped<ServicioReporte>();

var app = builder.Build();


//  INICIALIZACIÓN DE BASE DE DATOS

using (var alcance = app.Services.CreateScope())
{
    var configuracion = alcance.ServiceProvider.GetRequiredService<IConfiguration>();
    var rutaBaseDatos = configuracion.GetConnectionString("DefaultConnection");

    try
    {
        using (var conexion = new SqliteConnection(rutaBaseDatos))
        {
            conexion.Open();

            // Crear las tablas si no existen
            string sentenciaSql = @"
                -- Tabla Facturas
                CREATE TABLE IF NOT EXISTS Facturas (
                    Id INTEGER PRIMARY KEY AUTOINCREMENT,
                    NombreCliente TEXT,
                    Fecha TEXT NOT NULL,
                    Total REAL DEFAULT 0
                );

                -- Tabla ArticulosFactura (Detalles de la factura)
                CREATE TABLE IF NOT EXISTS ArticulosFactura (
                    ArticuloId INTEGER PRIMARY KEY AUTOINCREMENT,
                    Nombre TEXT,
                    Cantidad INTEGER NOT NULL,
                    PrecioUnitario REAL NOT NULL,
                    FacturaId INTEGER NOT NULL,
                    FOREIGN KEY (FacturaId) REFERENCES Facturas(Id)
                );

                -- NUEVA: Tabla Articulos (Catálogo General)
                CREATE TABLE IF NOT EXISTS Articulos (
                    Id INTEGER PRIMARY KEY AUTOINCREMENT,
                    Nombre TEXT NOT NULL,
                    Precio REAL NOT NULL
                );
            ";

            //Ejecutar la sentencia SQL
            using (var comando = new SqliteCommand(sentenciaSql, conexion))
            {
                comando.ExecuteNonQuery();
            }
        }
    }
    catch (Exception ex)
    {
        Console.WriteLine("Error creando la Base de Datos: " + ex.Message);
    }
}

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

//app.UseHttpsRedirection();

app.UseStaticFiles();

app.UseAntiforgery();
app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.Run();
