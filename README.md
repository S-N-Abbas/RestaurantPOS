## 🍽️ Restaurant POS System (WPF | .NET 8 | MVVM)
A modern, scalable **Point of Sale (POS)** system designed for restaurant environments, built using **.NET 8 WPF** with a clean **MVVM architecture**.

> ⚠️ This project is currently under active development.

---

## 🚀 Overview

This project aims to replicate real-world POS systems used in restaurants, focusing on:

* High-performance UI for touch-based interaction
* Clean and scalable architecture
* Real-time order management
* Table-based workflow (dine-in focused)

The goal is not just to build a working app, but to design a **production-grade system architecture**.

---

## 🧠 Key Features (Implemented)

### ✅ Table Management

* Visual table layout
* Active table highlighting
* Occupied / free state detection
* Smooth table switching (no navigation reload)

### ✅ Order Management

* Add/remove items dynamically
* Real-time quantity updates (+ / − controls)
* Automatic total calculation
* Persistent order per table (in-memory)

### ✅ Navigation System

* Fully decoupled navigation using `NavigationService`
* ViewModel-first navigation
* No code-behind logic

### ✅ MVVM Architecture

* Clean separation of concerns
* No UI logic in business layer
* Dependency Injection throughout

### ✅ Logging

* Integrated with Serilog
* File-based logging for diagnostics

---

## 🏗️ Architecture

The project follows a layered architecture inspired by real-world enterprise systems:

```
UI (WPF Views)
│
├── ViewModels (MVVM)
│
├── Services (Navigation, Session Management)
│
├── Stores (State Management)
│
├── Domain (Core Business Models)
│
└── Infrastructure (EF Core, SQLite)
```

### Key Design Principles:

* **Single Responsibility**
* **Separation of Concerns**
* **Testability**
* **Scalability**

---

## 🧩 Technologies Used

* **.NET 8**
* **WPF (Windows Presentation Foundation)**
* **MVVM Toolkit (CommunityToolkit.Mvvm)**
* **Entity Framework Core (SQLite)**
* **Microsoft Dependency Injection**
* **Serilog**

---

## 💾 Data Layer (In Progress)

* SQLite database integration
* Entity Framework Core for ORM
* Repository pattern for data access
* Domain ↔ UI state mapping (in progress)

---

## 🔄 Current Development Focus

* Database persistence for orders and tables
* Mapping between domain entities and UI state
* Table data loading from database
* Order lifecycle management (open → paid → closed)

---

## 📌 Planned Features

* 💳 Payment processing (cash / card / split)
* 🧾 Receipt generation
* 🖨️ Thermal printer integration
* 📊 Sales reporting dashboard
* 👥 User authentication & roles
* 🍔 Dynamic menu management from database
* 🎨 Advanced UI/UX (animations, touch optimization)

---

## 🎯 Why This Project Stands Out

Unlike basic CRUD apps, this project demonstrates:

* Real-world POS workflow design
* Advanced state management
* Clean MVVM implementation without shortcuts
* Scalable architecture ready for production features

---

## 📷 Screenshots

### Login
<img width="1919" height="1079" alt="Screenshot 2026-03-26 150425" src="https://github.com/user-attachments/assets/502c5803-16db-4139-9ca9-a906cc8d7b7c" />

### Tables View
<img width="1919" height="1079" alt="Screenshot 2026-03-26 150435" src="https://github.com/user-attachments/assets/3f56ef52-187f-453b-b5f4-d254e034af28" />

### Order View
<img width="1919" height="1079" alt="Screenshot 2026-03-26 150442" src="https://github.com/user-attachments/assets/e9358831-d3d6-45ec-bc0e-1ebadaf68f79" />

### Table Switch
<img width="1919" height="1079" alt="Screenshot 2026-03-26 150507" src="https://github.com/user-attachments/assets/b440ec44-9100-4537-8351-b7e5a053b6e1" />

### Cover Selection
<img width="1919" height="1079" alt="Screenshot 2026-03-26 150519" src="https://github.com/user-attachments/assets/9c32a532-c8ad-41c0-898d-959ed964a26a" />

### Payment Screen
<img width="1919" height="1079" alt="Screenshot 2026-03-26 150453" src="https://github.com/user-attachments/assets/4d07eda9-0b15-419c-a1a8-acfdb42f660a" />

### User and Role Management
<img width="1919" height="1079" alt="Screenshot 2026-03-26 150535" src="https://github.com/user-attachments/assets/efa58fe0-82b1-40d5-b2f0-4e823df32f24" />

### Settings
<img width="1919" height="1079" alt="Screenshot 2026-03-26 150544" src="https://github.com/user-attachments/assets/91dd3e4a-6ff5-4fa9-a6ac-eb7da29feff4" />

---

## ⚙️ Getting Started

### Prerequisites

* .NET 8 SDK
* Visual Studio 2022+

### Run the Project

```bash
git clone https://github.com/S-N-Abbas/RestaurantPOS
cd restaurant-pos
```

Open in Visual Studio and run the solution.

---

## 🤝 Contributing

This project is currently personal, but suggestions and feedback are welcome.

---

## 📄 License

This project is for educational and portfolio purposes.

---

## 👨‍💻 Author

**Syed Abbas**

---

## ⭐ Final Note

This project reflects my focus on building **real-world, production-ready systems**, not just demos.

If you're a recruiter or developer reviewing this:

* The architecture is intentional
* The patterns are scalable
* The system is designed for growth

---

⭐ If you find this project interesting, feel free to star the repository!
