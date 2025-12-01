using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using UrDoggy.Data;
using UrDoggy.Website;
using UrDoggy.Core.Models;
using UrDoggy.Data.Repositories;
using UrDoggy.Services.Interfaces;
using UrDoggy.Services.Service;
using UrDoggy.Website.Hubs;
using Microsoft.AspNetCore.Authentication.Cookies;
using UrDoggy.Data.Repositories.Group_Repository; // GroupRepository, GroupPostRepository, UserGroupRepositrory
using UrDoggy.Services.Interfaces.GroupServices;  // IGroupUserService, IAdminGroupService, IModeratorService
using UrDoggy.Services.Service.GroupServices; // GroupUserService, AdminGroupService, ModeratorService


var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();
builder.Services.AddRazorPages();
builder.Services.AddSession();

//Database
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddIdentity<User, IdentityRole<int>>(options =>
{
    options.Password.RequireDigit = true;
    options.Password.RequireUppercase = false;
    options.Password.RequiredLength = 6;
    options.Password.RequireNonAlphanumeric = false;
    options.Password.RequireLowercase = false;
    options.SignIn.RequireConfirmedAccount = false;
})
.AddEntityFrameworkStores<ApplicationDbContext>()
.AddDefaultTokenProviders()
.AddUserManager<UserManager<User>>()
.AddRoleManager<RoleManager<IdentityRole<int>>>();


builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});


//Authentication
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath = "/Auth/Login";
        options.AccessDeniedPath = "/Auth/AccessDenied";
        options.ExpireTimeSpan = TimeSpan.FromDays(7);
    });

builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("AdminOnly", policy => policy.RequireRole("Admin"));
});

builder.Services.AddSignalR();
//Repository
builder.Services.AddScoped<UserRepository>();
builder.Services.AddScoped<PostRepository>();
builder.Services.AddScoped<CommentRepository>();
builder.Services.AddScoped<FriendRepository>();
builder.Services.AddScoped<MessageRepositpry>();
builder.Services.AddScoped<MediaRepository>();
builder.Services.AddScoped<NotificationRepository>();
builder.Services.AddScoped<ReportRepository>();

builder.Services.AddScoped<GroupRepository>();
builder.Services.AddScoped<GroupPostRepository>();
builder.Services.AddScoped<UserGroupRepositrory>();

//Interface
builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<IPostService, PostService>();
builder.Services.AddScoped<IMessageService, MessageService>();
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IReportService, ReportService>();
builder.Services.AddScoped<INotificationService, NotificationService>();
builder.Services.AddScoped<IFriendService, FriendService>();
builder.Services.AddScoped<ICommentService, CommentService>();
builder.Services.AddScoped<IMediaService, MediaService>();

builder.Services.AddScoped<IGroupUserService, GroupUserService>();
builder.Services.AddScoped<IAdminGroupService, AdminGroupService>();
builder.Services.AddScoped<IModeratorService, ModeratorService>();


var app = builder.Build();

//Admin Seeder
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    try
    {
        await AdminSeeder.SeederAdmin(services);
    }
    catch (Exception ex)
    {
        var logger = services.GetRequiredService<ILogger<Program>>();
        logger.LogError(ex, "An error occurred while seeding admin user. Error: {ErrorMessage}", ex.Message);

        if (ex.InnerException != null)
        {
            logger.LogError("Inner exception: {InnerException}", ex.InnerException.Message);
        }
    }
}

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();
app.UseSession();
app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Auth}/{action=Login}/{id?}");

app.MapHub<ChatHub>("/chathub");
app.MapHub<NotificationHub>("/notificationhub");
app.MapControllers();
app.UseCors("AllowAll");

app.Run();
