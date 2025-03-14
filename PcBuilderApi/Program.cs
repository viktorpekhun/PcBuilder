var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
builder.Services.AddOpenApi();

// Додаємо CORS перед викликом `Build()`
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontend",
        policy => policy.WithOrigins("http://localhost:5173") // URL Vite
                        .AllowAnyMethod()
                        .AllowAnyHeader());
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

// Використовуємо CORS перед авторизацією
app.UseHttpsRedirection();

app.UseCors("AllowFrontend"); // Додаємо тут

app.UseAuthorization();

app.MapControllers();

app.Run();
