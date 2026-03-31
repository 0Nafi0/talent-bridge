# 🏝️ TalentBridge

**TalentBridge** is a modern, high-performance job portal designed to bridge the gap between world-class talent and top-tier recruiters. Built with a stunning "Hawaii" aesthetic featuring glassmorphism and fluid animations, it offers a premium user experience for the modern workforce.

---

## 🚀 Live Demo
- **Backend API**: [https://talent-bridge-qmxz.onrender.com/swagger](https://talent-bridge-qmxz.onrender.com/swagger)
- **Frontend UI**: [https://talent-bridge-psi.vercel.app]

---

## ✨ Features

### 👤 For Candidates
- **Dynamic Profile Management**: Custom dashboards with real-time profile editing.
- **Professional Skill Matrix**: Add and manage skills with experience tracking.
- **Smart Job Board**: Search, filter, and view detailed job listings with a premium UI.
- **Automated Applications**: One-click application process with cover letter support.
- **Application Tracking**: Monitor application statuses and match scores in real-time.

### 🏢 For Recruiters
- **Recruiter Command Center**: High-level overview of active jobs and applicants.
- **Job Creation Wizard**: Post new opportunities with specific skill requirements.
- **Hiring Pipeline Analytics**: Visual conversion rates (Shortlisted/Offered) using Ant Design Progress charts.
- **Applicant Management**: View candidates sorted by AI-calculated Match Scores.
- **Status Control**: Move candidates through hiring stages (Shortlisted, Interview, Offer, Reject).

### 🎨 Design & UX
- **Hawaii Theme**: Custom palette (#9B8EC7, #BDA6CE, #B4D3D9, #F2EAE0) with "Pacifico" cursive accents.
- **Glassmorphism**: Circular, blurred navbars and cards for a sleek, modern look.
- **Dark/Light/System Mode**: Seamless theme switching via a custom Theme Engine.
- **Responsive Design**: Fully optimized for desktop and mobile browsers.

---

## 🛠️ Tech Stack

### Backend
- **Framework**: .NET 10 Web API
- **Database**: PostgreSQL (Hosted on **Supabase**)
- **ORM**: Entity Framework Core
- **Auth**: ASP.NET Core Identity with JWT (JSON Web Tokens)
- **Architecture**: Controller-Repository pattern with DTOs for secure data transfer.

### Frontend
- **Framework**: React 19 (via **Vite**)
- **UI Library**: **Ant Design (v5+)**
- **Icons**: Lucide React & Ant Design Icons
- **HTTP Client**: Axios with interceptors for JWT handling.
- **State Management**: Context API (Auth, Theme).

---

## 🔧 Installation & Setup

### 1. Prerequisites
- [.NET 10 SDK](https://dotnet.microsoft.com/download)
- [Node.js (v18+)](https://nodejs.org/)
- [PostgreSQL Instance](https://www.postgresql.org/) (or Supabase account)

### 2. Backend Setup
```bash
cd TalentBridge.Api
dotnet restore
# Update ConnectionString in appsettings.json
dotnet run
```

### 3. Frontend Setup
```bash
cd frontend
npm install
# Create a .env file with VITE_API_URL=http://localhost:5257/api
npm run dev
```

---

## 🚢 Deployment & CI/CD
- **Version Control**: Git (GitHub)
- **Backend Deployment**: Hosted on **Render.com** (Auto-deploys from `master`).
- **Frontend Deployment**: Hosted on **Vercel** (Edge-optimized).
- **Database**: Production PostgreSQL on **Supabase**.

---

## 📄 License
This project is licensed under the MIT License.
