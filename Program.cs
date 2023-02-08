using Microsoft.AspNetCore.Mvc;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddSqlServer<ApplicationDbContext>(builder.Configuration["Database:SqlServer"]);

 
var app = builder.Build();
var configuration = app.Configuration;
ProductRepository.Init(configuration);

// ENDPOINT INSERIR
app.MapPost("/products", (ProductRequest productRequest, ApplicationDbContext context) =>
{   
    var category = context.Categories.Where(c => c.Id == productRequest.CategoryId).First();
    var product = new Product {
        Code = productRequest.Code,
        Name = productRequest.Name,
        Description = productRequest.Description,
        Category = category
    };
    context.Products.Add(product);
    context.SaveChanges();
    return Results.Created($"/products/{product.Id}", product.Id);
});

// ENDPOINT CONSULTAR POR ID
//api.app.com/user/{code}
app.MapGet("/products/{code}", ([FromRoute] string code) =>
{
    var product = ProductRepository.GetBy(code);
    if (product != null)
    {
        return Results.Ok(product);
    }
    else
    {
        return Results.NotFound();
    }

});

// ENDPOINT ALTERAR
app.MapPut("/products", (Product product) =>
{
    var productSaved = ProductRepository.GetBy(product.Code);
    productSaved.Name = product.Name;
    return Results.Ok();
});

// ENDPOINT DELETAR
app.MapDelete("/products/{code}", ([FromRoute] string code) =>
{
    var productSaved = ProductRepository.GetBy(code);
    ProductRepository.Remove(productSaved);
    return Results.Ok();
});

// ENDPOINT CONFIGURAÇÃO DE AMBIENTE
if (app.Environment.IsStaging())
    app.MapGet("/configuration/database", (IConfiguration configuration) =>
    {
        return Results.Ok($"{configuration["database:connection"]}/{configuration["database:port"]}");
    });

app.Run();
