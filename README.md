# Doctor Appointment Management System

Doctor Appointment Management System, also called DAMS, is an ASP.NET Core MVC based hospital appointment management project. It manages patient registration, doctor profiles, doctor availability, appointment booking, OPD queue handling, admin monitoring, reporting, notifications, and a custom TCP client-server chat module.

The system is designed around three main users: Admin, Doctor, and Patient. Patients can book appointments with available doctors, doctors can manage their slots and daily appointments, and admins can supervise doctors, patients, requests, reports, and OPD activity.

## Participants

- Talal Tariq


## Project Summary

DAMS solves the common hospital problem of manual appointment handling. Instead of managing patients, doctor schedules, and OPD queues manually, the system provides a web-based application where patients can book appointments and track their status, doctors can manage their schedule and communicate with patients, and admins can monitor the whole system.

The project also includes a custom TCP based messaging server. This makes the project stronger from a distributed systems point of view because the web application and the messaging server are separate processes that communicate over a network socket.

## Main Features

- User registration and login
- Role based dashboards for Admin, Doctor, and Patient
- Patient profile completion
- Doctor profile and department management
- Doctor slot creation and management
- Appointment booking
- Appointment rescheduling
- Emergency and priority based queue support
- OPD queue monitoring
- Doctor request handling
- Notifications
- Reports for doctors, patients, and appointments
- Private chat between doctor and patient
- Broadcast messages from doctor to patients of the day
- Broadcast messages from admin to doctors, patients, or both
- Real-time style chat refresh using polling
- Session based authentication
- Browser cache prevention after login/logout

## Technology Stack

| Layer | Technology |
| --- | --- |
| Web Application | ASP.NET Core MVC |
| Language | C# |
| Framework Target | .NET 10.0 |
| Database | SQL Server |
| Data Access | ADO.NET with `System.Data.SqlClient` |
| Frontend | Razor Views, HTML, CSS, Bootstrap, JavaScript |
| Messaging | Custom TCP Client-Server |
| JSON Handling | Newtonsoft.Json |
| Architecture Style | MVC plus Service and Repository layers |

## Repository Structure

```text
DAMS/
├── DAMS.slnx
├── README.md
├── DAMS/
│   ├── Controllers/
│   ├── DBAccess/
│   ├── Models/
│   ├── Services/
│   ├── ViewModels/
│   ├── Views/
│   ├── wwwroot/
│   ├── Program.cs
│   ├── appsettings.json
│   └── DAMS.csproj
├── Server/
│   ├── Controllers/
│   ├── CoreFunc/
│   ├── Models/
│   ├── Netw/
│   ├── Services/
│   ├── Program.cs
│   └── Server.csproj
└── ClientD/
    └── ClientD.csproj
```

## Architecture

The project follows an MVC based architecture with additional service and repository layers.

```text
User Browser
    |
    v
ASP.NET Core MVC Controllers
    |
    v
Service Layer
    |
    v
Repository / DBAccess Layer
    |
    v
SQL Server Database

For chat:

DAMS Web Application
    |
    v
TcpClientService
    |
    v
Custom TCP Server
    |
    v
Connected Clients / Message Routing
```

## Main Modules

### Authentication Module

The authentication module handles login, registration, session creation, role checking, and logout. After login, the user is redirected according to their role.

Role based redirects:

| Role | Redirected To |
| --- | --- |
| Patient | Patient dashboard |
| Doctor | Doctor dashboard |
| Admin | Admin dashboard |

The project also prevents an already logged-in user from returning to the login page using the browser back button. Cache-control headers are added globally, and the login page checks whether a valid session already exists.

### Patient Module

The patient module allows a patient to:

- Register and login
- Complete their patient profile
- View dashboard
- Search doctors
- Book appointment
- View their own appointments
- Reschedule appointment
- View live queue information
- Send message to the doctor with whom they have a booked appointment
- View broadcast messages sent by doctor or admin

### Doctor Module

The doctor module allows a doctor to:

- Login and view dashboard
- Create appointment slots
- Manage slots
- View today's slots
- View appointments
- Manage OPD queue
- Message an individual booked patient
- Broadcast messages to all patients booked for that day
- View admin broadcast messages

### Admin Module

The admin module allows an admin to:

- View admin dashboard
- Handle doctor requests
- View doctors and patients
- Monitor OPD activity
- View reports
- Broadcast messages to all doctors
- Broadcast messages to all patients
- Broadcast messages to both doctors and patients

### Appointment Module

The appointment module is the core business module of the system. Patients book appointments against available doctor slots. Each appointment stores patient, doctor, slot, token number, date, status, and priority information.

Typical appointment statuses:

- Booked
- Completed
- Cancelled

Appointments are also used by the chat module. A patient and doctor can only message each other if a booked appointment exists between them.

### Queue Module

The queue module manages OPD order and token handling. It supports normal and emergency appointments and helps doctors/admins monitor patient flow.

### Notification Module

The notification module stores and displays system notifications for users. It is used to improve communication between the system and users.

### Report Module

The report module provides useful data for admin analysis, such as appointment reports, doctor reports, and patient reports.

## Chat Module

The chat module is one of the most important parts of the project. It uses both database storage and a custom TCP server.

### Chat Use Cases

The implemented chat use cases are:

| Sender | Receiver | Allowed Condition |
| --- | --- | --- |
| Doctor | Individual patient | Patient must have a booked appointment with that doctor |
| Doctor | All patients of the day | Patients must have booked appointments with that doctor for the selected day |
| Patient | Doctor | Patient must have a booked appointment with that doctor |
| Admin | All doctors | Admin can broadcast to doctors |
| Admin | All patients | Admin can broadcast to patients |
| Admin | Doctors and patients | Admin can broadcast to both groups |

### Private Chat

Private chat works between one doctor and one patient. The system checks the database before allowing the message. If there is no booked appointment between the doctor and patient, the message is rejected.

This prevents random patients from messaging any doctor and prevents unrelated doctors from messaging patients who are not assigned to them.

### Broadcast Chat

Broadcast messages are used when one sender sends the same message to many users.

Doctor broadcast:

- Doctor writes one broadcast message.
- System finds patients who have booked appointments with that doctor for that day.
- Message is saved for each valid patient.
- Patients can view the broadcast message in their broadcast messages page.

Admin broadcast:

- Admin selects the broadcast target.
- Target can be doctors, patients, or both.
- System saves the message for the selected users.
- Each receiver can see the broadcast message from their own account.

### Real-Time Style Updates

The chat pages use JavaScript polling. This means the browser repeatedly asks the server for new messages after a small interval.

In this project, the chat page refreshes messages automatically, so when one side sends a message, the other side can see it without manually reloading the page.

This is not SignalR or WebSocket based real-time communication. It is a simpler real-time style implementation using repeated AJAX requests.

## Custom TCP Client-Server Messaging

The project contains a separate `Server` project that starts a TCP server on port `9010`.

Server startup:

```csharp
var server = new TcpServer();
server.Start(9010);
```

The web application uses `TcpClientService` to connect to this server:

```csharp
private readonly string _serverIp = "127.0.0.1";
private readonly int _serverPort = 9010;
```

Messages are serialized into JSON and sent through the TCP connection. The server accepts clients and handles them using separate tasks.

This makes the system a distributed application because the web app and the TCP server are separate running programs that communicate through a network protocol.

## Is This Project Distributed?

Yes, the project has distributed system characteristics because it contains:

- A web application process
- A separate TCP server process
- Network communication over TCP
- JSON message transfer between processes
- Multiple users interacting through the system

The database is also a separate data storage component, which further supports the distributed nature of the project.

## Is This Project Parallel?

Yes, the project uses parallel/concurrent behavior in the TCP server. When a client connects, the server starts a new task to handle that client.

Example behavior:

```csharp
Task.Run(() => connection.HandleClient());
```

This allows multiple clients to be handled at the same time instead of making one client wait for another.

## Database

The project uses SQL Server. The connection string is stored in:

```text
DAMS/appsettings.json
```

Current connection string format:

```json
"ConnectionStrings": {
  "DefaultConnection": "Server=TALALTARIQ\\SQLEXPRESS;Database=DAMS;Trusted_Connection=True;"
}
```

Before running the project on another machine, update this connection string according to your SQL Server instance.

Important database entities include:

- Users
- Patients
- Doctors
- Departments
- DoctorSlots
- Appointments
- Notifications
- Messages
- DoctorRequests
- OPD status / queue related tables

## Prerequisites

Install the following before running the project:

- Visual Studio 2022 or later
- .NET SDK compatible with `net10.0`
- SQL Server or SQL Server Express
- SQL Server Management Studio, recommended

## How to Run the Project

### 1. Clone the Repository

```bash
git clone <repository-url>
cd DAMS
```

### 2. Configure Database Connection

Open:

```text
DAMS/appsettings.json
```

Update the SQL Server connection string:

```json
"ConnectionStrings": {
  "DefaultConnection": "Server=YOUR_SERVER_NAME;Database=DAMS;Trusted_Connection=True;"
}
```

### 3. Restore Packages

```bash
dotnet restore
```

### 4. Build the Solution

```bash
dotnet build
```

### 5. Run the TCP Server

Open one terminal:

```bash
dotnet run --project Server/Server.csproj
```

The server starts on:

```text
Port 9010
```

### 6. Run the Web Application

Open another terminal:

```bash
dotnet run --project DAMS/DAMS.csproj
```

Default web URLs:

```text
http://localhost:5140
https://localhost:7274
```

## Recommended Run Order

Run the TCP server first, then run the MVC web application.

```text
1. Start Server project
2. Start DAMS web project
3. Open browser
4. Login as Admin, Doctor, or Patient
5. Use appointment and chat features
```

## Important Files

| File | Purpose |
| --- | --- |
| `DAMS/Program.cs` | Configures MVC, session, repositories, services, middleware, and routing |
| `DAMS/Controllers/AuthController.cs` | Handles login, registration, logout, and role redirect |
| `DAMS/Controllers/PatientController.cs` | Handles patient dashboard, profile, appointments, and patient actions |
| `DAMS/Controllers/DoctorController.cs` | Handles doctor dashboard, slots, appointments, and queue |
| `DAMS/Controllers/AdminController.cs` | Handles admin dashboard, requests, reports, and monitoring |
| `DAMS/Controllers/MessageController.cs` | Handles private chat and broadcast message pages/APIs |
| `DAMS/Services/MessageService.cs` | Contains chat business logic and validation |
| `DAMS/DBAccess/MessageRep.cs` | Handles message database operations |
| `DAMS/Services/TcpClientService.cs` | Sends messages from web app to custom TCP server |
| `DAMS/Services/ChatIdGenerator.cs` | Creates consistent chat IDs for doctor-patient conversations |
| `DAMS/Views/Message/Chat.cshtml` | Private chat UI |
| `DAMS/Views/Message/Broadcasts.cshtml` | Broadcast message UI |
| `Server/Program.cs` | Starts the TCP server |
| `Server/Netw/TCPServer.cs` | Listens for TCP clients |
| `Server/Netw/ClientConnection.cs` | Handles connected TCP clients |
| `Server/CoreFunc/MessageRoute.cs` | Routes messages on the server side |
| `Server/Models/MessageModel.cs` | Message model used by the TCP server |

## Security and Validation

The system includes several important validation rules:

- Session based user authentication
- Role based page access
- Patients can only message doctors with booked appointments
- Doctors can only message patients with booked appointments
- Doctor broadcasts are limited to patients booked for that doctor on that day
- Admin broadcasts are controlled by target selection
- Browser cache is disabled for authenticated pages to reduce back-button login issues

## Browser Back Button Handling

After login, if the user presses the browser back button, the system should not expose the login page as an active page. This is handled by:

- Adding no-cache headers globally
- Checking active session on the login page
- Redirecting logged-in users back to their dashboard

This improves session behavior and prevents confusing navigation after login.

## Message Flow

Private message flow:

```text
User writes message
    |
    v
MessageController receives request
    |
    v
MessageService validates sender, receiver, and appointment
    |
    v
MessageRep saves message in database
    |
    v
TcpClientService sends message to TCP server
    |
    v
Receiver chat page fetches updated messages through polling
```

Broadcast message flow:

```text
Doctor/Admin writes broadcast
    |
    v
System identifies target users
    |
    v
Message is saved for each receiver
    |
    v
TCP message is sent to recipients
    |
    v
Receivers view message in Broadcasts page
```

## Viva Explanation

### What is DAMS?

DAMS is a Doctor Appointment Management System that manages patients, doctors, appointments, OPD queues, reports, notifications, and communication between users.

### Why did you use MVC?

MVC separates the project into Model, View, and Controller layers. This makes the code easier to manage because UI, business flow, and data models are separated.

### Why did you use services and repositories?

Services contain business logic, while repositories handle database operations. This separation makes the code cleaner and easier to update.

### How does appointment booking work?

The doctor creates available slots. A patient selects a doctor and slot, then books an appointment. The system stores the appointment with patient ID, doctor ID, slot ID, token number, appointment date, and status.

### How does private chat work?

Private chat works only between a doctor and patient who have a booked appointment. Before saving or sending a message, the system validates that the appointment exists.

### How does doctor broadcast work?

A doctor can broadcast a message to all patients who have booked appointments with that doctor for the current day. The system finds those patients and saves the message for each one.

### How does admin broadcast work?

Admin can broadcast to all doctors, all patients, or both. The system checks the selected target and creates message records for the matching users.

### How do messages update automatically?

The chat UI uses JavaScript polling. It calls the server after a fixed interval and refreshes the message list when new messages are available.

### Is this project parallel?

Yes. The TCP server handles clients using separate tasks, allowing multiple clients to be served at the same time.

### Is this project distributed?

Yes. The web application, TCP server, and SQL Server database are separate components that communicate with each other.

## Limitations

- The project uses polling instead of SignalR/WebSockets for live UI updates.
- Database scripts/migrations should be added for easier setup on new machines.
- Password hashing should be added before production use if plain password storage is currently used.
- More automated tests can be added for appointment and chat validation.
- UI can be improved with more responsive design and dashboard charts.

## Future Enhancements

- Add SignalR for true real-time chat
- Add email/SMS reminders
- Add online payment support
- Add prescription upload
- Add doctor rating and feedback
- Add patient medical history
- Add appointment cancellation rules
- Add audit logs for admin actions
- Add database migration scripts
- Add unit and integration tests

## License

This project is created for academic learning and demonstration purposes.

