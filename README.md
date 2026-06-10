## 📌 Overview

Smart City Parking System is a web application that allows drivers to find and book parking slots in real time. It includes an admin dashboard with live heatmaps, user management, broadcast messaging, and analytical reports. The interface features a 3D animated background with glassmorphism and dark/light theme.

---

## ✨ Features

### 👤 User Side
- Register / Login (session-based)
- Find available parking slots by zone and vehicle type
- Book a slot with start/end time
- View booking history and wallet balance
- Receive real-time broadcast and private messages (polling every 30 seconds)

### 👑 Admin Dashboard
- **Dashboard**: Stats (total slots, available, today’s bookings, revenue), 7-day charts, recent bookings
- **Live Heatmap**: Interactive grid of slots (green=available, red=occupied); click to toggle status
- **Parking Slots**: Add, edit, delete slots (separate pages)
- **Bookings**: Filter by status, search by vehicle, cancel active bookings
- **User Management**: List users, block/unblock, delete
- **Reports**: Monthly revenue chart, slot utilization chart, export CSV
- **Broadcast System**: Send public announcements to all users OR private messages to specific users
- **Quick Actions**: Mark all slots available, cancel all active bookings, reset revenue counter
- **Settings**: Site name, default hourly rate, booking timeout
- **Notifications Page**: Users can view all past messages (public & private)

### 🎨 UI / UX
- 3D rotating torus knot + particles background (Three.js)
- Glassmorphism cards with neon blue borders
- Dark/Light theme toggle (persists)
- Responsive design (mobile, tablet, desktop)
- Bell icon with unread count for new notifications

### 🐳 Docker Support
- Multi-container: web (ASP.NET Core) + sqlserver (SQL Server Linux)
- One-command deployment: `docker-compose up --build`

---

## 🛠️ Tech Stack

| Layer | Technology |
|-------|------------|
| Backend | ASP.NET Core MVC (.NET 8) |
| Database | SQL Server (LocalDB / container) |
| ORM | Entity Framework Core |
| Frontend | HTML5, CSS3, Bootstrap 5, JavaScript, Three.js, Chart.js |
| Authentication | Session-based |
| Reporting | CSV export, Chart.js |
| Containerization | Docker, Docker Compose |

---

## 🚀 Local Setup

### Prerequisites
- [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)
- [Visual Studio 2022](https://visualstudio.microsoft.com/) (with ASP.NET workload)
- SQL Server LocalDB (comes with VS)

