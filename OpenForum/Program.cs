using Application.Interfaces.Contexts;
using Application.Interfaces.Mappers;
using Application.Interfaces.Services;
using Application.Mappers;
using Application.Services;
using EntityContext;
using Identity.Context;
using Identity.Models;
using Identity.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.EntityFrameworkCore;
using OpenForum.Services;
using Repository;
using Repository.interfaces;

var builder = WebApplication.CreateBuilder(args);

// Register MVC services
builder.Services.AddControllersWithViews();
builder.Services.AddRazorPages();


// Register DbContexts
builder.Services.AddDbContext<ForumContext>(options =>
    options.UseSqlServer(
        builder.Configuration.GetConnectionString("DefaultConnection")
    ));

builder.Services.AddDbContext<IdentityContext>(options =>
    options.UseSqlServer(
        builder.Configuration.GetConnectionString("IdentityConnection")));

// Register identity
builder.Services
    .AddIdentity<ApplicationUser, IdentityRole>(options =>
    {
        options.SignIn.RequireConfirmedAccount = false;

        options.Password.RequiredLength = 8;
        options.Password.RequireDigit = true;
        options.Password.RequireUppercase = true;
        options.Password.RequireLowercase = true;
        options.Password.RequireNonAlphanumeric = false;
    })
    .AddEntityFrameworkStores<IdentityContext>()
    .AddDefaultTokenProviders();

builder.Services.AddHttpContextAccessor();

builder.Services.AddScoped<IUserContext, UserContext>();

builder.Services.ConfigureApplicationCookie(options =>
{
    options.LoginPath = "/Identity/Account/Login";
    options.AccessDeniedPath = "/Identity/Account/AccessDenied";
});

builder.Services.AddTransient<IEmailSender, EmailSender>();
builder.Services.AddScoped<IdentitySeeder>();


// Register repositories
builder.Services.AddScoped<ITopicRepository, TopicRepository>();
builder.Services.AddScoped<IPostRepository, PostRepository>();
builder.Services.AddScoped<ICommentRepository, CommentRepository>();


// Register mappers
builder.Services.AddScoped<ITopicMapper, TopicMapper>();
builder.Services.AddScoped<IPostMapper, PostMapper>();
builder.Services.AddScoped<ICommentMapper, CommentMapper>();


// Register services
builder.Services.AddScoped<ITopicService, TopicService>();
builder.Services.AddScoped<IPostService, PostService>();
builder.Services.AddScoped<ICommentService, CommentService>();
builder.Services.AddScoped<ICurrentUserService, CurrentUserService>();


var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
}

//Only HTTPS allowed
app.UseHttpsRedirection();

// Static files (wwwroot)
app.UseStaticFiles();

// Routing
app.UseRouting();

// Auth
app.UseAuthentication();

app.UseAuthorization();

// Conventional MVC routing
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Topic}/{action=Index}/{id?}");

app.MapRazorPages();

using (var scope = app.Services.CreateScope())
{
    var seeder = scope.ServiceProvider.GetRequiredService<IdentitySeeder>();

    await seeder.SeedAsync();
}

app.Run();