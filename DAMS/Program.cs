using DAMS.DBAccess;
using DAMS.Repositories;
using DAMS.Services;

var builder = WebApplication.CreateBuilder(args);

// ================= MVC =================
builder.Services.AddControllersWithViews();

// ================= SESSION =================
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});

// ================= CONNECTION STRING =================
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");

// ================= REPOSITORIES =================
builder.Services.AddScoped(_ => new AppointmentRep(connectionString));
builder.Services.AddScoped(_ => new DoctorRep(connectionString));
builder.Services.AddScoped(_ => new UserRep(connectionString));
builder.Services.AddScoped(_ => new AuthRep(connectionString));
builder.Services.AddScoped(_ => new DepartmentRep(connectionString));
builder.Services.AddScoped(_ => new PatientRep(connectionString));
builder.Services.AddScoped(_ => new DoctorSlotRep(connectionString));
builder.Services.AddScoped(_=>new NotificationRep(connectionString));
builder.Services.AddScoped(_ => new DoctorRequestRep(connectionString));
builder.Services.AddScoped(_ => new OPDRepository(connectionString));
builder.Services.AddScoped(_ => new MessageRep(connectionString));
// ================= SERVICES =================
builder.Services.AddScoped<DoctorService>();
builder.Services.AddScoped<PatientService>();
builder.Services.AddScoped<UserService>();
builder.Services.AddScoped<DepServices>();
builder.Services.AddScoped<DoctorSlotServices>();
builder.Services.AddScoped<QueueInitializerService>(); 
builder.Services.AddScoped<TokenGenerationService>();
builder.Services.AddSingleton<QueueService>(); 
builder.Services.AddScoped<AppointmentService>();
builder.Services.AddScoped<AuthService>();
builder.Services.AddScoped<MiddleMan>();
builder.Services.AddScoped<NotificationService>();
builder.Services.AddScoped<DoctorRequestService>();
builder.Services.AddScoped<AdminControlService>();
builder.Services.AddScoped<AdminRequestService>();
builder.Services.AddScoped<ReportService>();
builder.Services.AddScoped<MessageService>();
builder.Services.AddScoped<TcpClientService>();

var app = builder.Build();

// ================= PIPELINE =================
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();

app.Use(async (context, next) =>
{
    context.Response.Headers["Cache-Control"] = "no-cache, no-store, must-revalidate";
    context.Response.Headers["Pragma"] = "no-cache";
    context.Response.Headers["Expires"] = "0";

    await next();
});

app.UseRouting();

app.UseSession();

app.MapStaticAssets();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}"
).WithStaticAssets();
//using (var scope = app.Services.CreateScope())
//{
//    var initializer = scope.ServiceProvider.GetRequiredService<QueueInitializerService>();
//    await initializer.InitializeAsync();
//}
app.Run();
