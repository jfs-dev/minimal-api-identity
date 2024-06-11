using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using minimal_api_identity.Data;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseInMemoryDatabase("minimal-api-identity"));

builder.Services.AddIdentityApiEndpoints<IdentityUser>()
    .AddRoles<IdentityRole>()
    .AddEntityFrameworkStores<AppDbContext>();

builder.Services.AddAuthentication();
builder.Services.AddAuthorization();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
    app.MapSwagger().RequireAuthorization();
}

app.UseAuthentication();
app.UseAuthorization();

app.MapIdentityApi<IdentityUser>();

app.MapGet("/", async (UserManager<IdentityUser> userManager, RoleManager<IdentityRole> roleManager) =>
{
    var userAdmin = await userManager.FindByNameAsync("peter.parker@marvel.com");
    if (userAdmin is not null)
        return Results.Ok("User Admin: peter.parker@marvel.com - Password: Mary@Jane2024");

    var createUserAdmin = new IdentityUser("peter.parker@marvel.com") {  Email = "peter.parker@marvel.com" };
    var createRoleAdmin = new IdentityRole("Admin");

    await userManager.CreateAsync(createUserAdmin, "Mary@Jane2024");
    await roleManager.CreateAsync(createRoleAdmin);
    await userManager.AddToRoleAsync(createUserAdmin, "Admin");

    return Results.Ok("User Admin: peter.parker@marvel.com - Password: Mary@Jane2024");
});

app.MapGet("/v1/anonymous/", [AllowAnonymous] () => "Anônimo");

app.MapGet("/v1/user/", () => "Usuário").RequireAuthorization();

app.MapGet("/v1/admin/", [Authorize(Roles = "Admin")]() => "Administrador").RequireAuthorization();

app.MapPost("v1/logout/", (SignInManager<IdentityUser> signInManager) =>
{
    signInManager.SignOutAsync();
    
    return Results.Ok();
}).RequireAuthorization();

app.Run();
