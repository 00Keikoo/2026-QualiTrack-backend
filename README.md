# QualiTrack Backend

Backend API untuk sistem **QualiTrack** — aplikasi manajemen audit internal, temuan (finding), dan tindakan perbaikan (CAPA) berbasis standar ISO 9001 & ISO 14001.

---

## Tech Stack

- **Framework**: ASP.NET Core 10
- **ORM**: Entity Framework Core 10
- **Database**: PostgreSQL 16
- **Auth**: JWT Bearer Token
- **Email**: MailKit (Gmail SMTP)
- **PDF**: QuestPDF
- **Storage**: Local filesystem (Docker volume)
- **Deployment**: Docker + Docker Compose

---

## Cara Menjalankan

### Lokal (tanpa Docker)

```bash
# Clone repo
git clone https://github.com/pens-pbl/QualiTrack-backend.git
cd QualiTrack-backend

# Sesuaikan connection string di appsettings.json
# Jalankan migration
dotnet ef database update

# Jalankan server
dotnet run
```

### Lokal (dengan Docker)

```bash
# Buat file .env
echo "JWT_SECRET=your-secret-key-minimal-32-chars" > .env

# Build dan jalankan
docker compose up -d --build

# Cek status
docker compose ps
```

### Server Production

```bash
ssh d4@10.154.0.116
cd ~/apps/QualiTrack-backend
git pull origin main
docker compose down
docker compose up -d --build
```

## Environment Variables

| Variable | Keterangan |
|---|---|
| `ConnectionStrings__Supabase` | Connection string PostgreSQL |
| `Jwt__Key` | Secret key JWT (min 32 karakter) |
| `Jwt__Issuer` | Issuer JWT |
| `Jwt__Audience` | Audience JWT |
| `Storage__UseS3` | `true` untuk S3, `false` untuk local storage |

---

## API Endpoints

Base URL: `https://be.qualitrack.labs.it.pens.ac.id`

Auth header: `Authorization: Bearer {token}`

### 🔐 Auth — `/api/Auth`

| Method | Endpoint | Deskripsi | Auth |
|---|---|---|---|
| POST | `/register` | Register user baru + kirim OTP | ❌ |
| POST | `/verify-email` | Verifikasi OTP email | ❌ |
| POST | `/resend-otp` | Kirim ulang OTP | ❌ |
| POST | `/login` | Login → dapat JWT token | ❌ |
| POST | `/logout` | Logout | ✅ |
| GET | `/whoami` | Info user yang sedang login | ✅ |
| GET | `/profile` | Detail profil user | ✅ |
| GET | `/auditors` | List user dengan role Auditor | ✅ |
| GET | `/users` | List semua user | ✅ |
| GET | `/pic-candidates` | List kandidat PIC untuk CAPA | ✅ |
| PUT | `/update-profile` | Update nama lengkap | ✅ |
| POST | `/change-password` | Ganti password | ✅ |
| POST | `/upload-profile-photo` | Upload foto profil | ✅ |
| POST | `/request-email-change-otp` | Request OTP untuk ganti email | ✅ |
| POST | `/verify-email-change` | Verifikasi OTP → update email | ✅ |
| POST | `/forgot-password/request-otp` | Request OTP lupa password | ❌ |
| POST | `/forgot-password/verify-otp` | Verifikasi OTP → dapat reset token | ❌ |
| POST | `/forgot-password/reset` | Reset password | ❌ |

### 📋 Audit Plan — `/api/AuditPlan`

| Method | Endpoint | Deskripsi | Auth |
|---|---|---|---|
| GET | `/` | List semua audit plan | ✅ |
| GET | `/{id}` | Detail audit plan | ✅ |
| POST | `/` | Buat audit plan baru | ✅ Admin/QM |
| PUT | `/{id}` | Update audit plan | ✅ Admin/QM |
| DELETE | `/{id}` | Hapus audit plan | ✅ Admin/QM |

### 🗓️ Audit Session — `/api/AuditSession`

| Method | Endpoint | Deskripsi | Auth |
|---|---|---|---|
| POST | `/` | Mulai sesi audit | ✅ |
| GET | `/{id}` | Detail sesi audit | ✅ |
| GET | `/by-schedule/{scheduleId}` | Sesi berdasarkan schedule | ✅ |
| POST | `/{sessionId}/summary` | Tambah/update summary sesi | ✅ |
| GET | `/{sessionId}/summary` | Ambil summary sesi | ✅ |

### ✅ Audit Response — `/api/AuditResponse`

| Method | Endpoint | Deskripsi | Auth |
|---|---|---|---|
| POST | `/batch` | Submit jawaban checklist sekaligus | ✅ |
| POST | `/progress` | Simpan progress checklist | ✅ |
| GET | `/by-session/{sessionId}` | Jawaban berdasarkan sesi | ✅ |

### 📝 Finding — `/api/Finding`

| Method | Endpoint | Deskripsi | Auth |
|---|---|---|---|
| GET | `/` | List finding + filter | ✅ |
| GET | `/{id}` | Detail finding | ✅ |
| GET | `/without-capa` | Finding yang belum ada CAPA | ✅ |
| GET | `/by-session/{sessionId}` | Finding berdasarkan sesi | ✅ |
| POST | `/` | Catat finding baru | ✅ |
| PUT | `/{id}` | Update finding | ✅ |
| DELETE | `/{id}` | Hapus finding | ✅ |

### 🔧 CAPA — `/api/Capa`

| Method | Endpoint | Deskripsi | Auth |
|---|---|---|---|
| GET | `/` | List semua CAPA | ✅ |
| GET | `/{id}` | Detail CAPA | ✅ |
| GET | `/overdue` | CAPA yang melewati deadline | ✅ |
| POST | `/finding/{findingId}` | Buat CAPA dari finding | ✅ |
| PUT | `/{id}` | Update CAPA | ✅ |
| POST | `/{id}/actions` | Tambah action CAPA | ✅ |
| POST | `/{id}/closeout` | Close out CAPA | ✅ |
| DELETE | `/{id}` | Hapus CAPA | ✅ |

### 📁 Upload Evidence — `/api/Upload`

| Method | Endpoint | Deskripsi | Auth |
|---|---|---|---|
| POST | `/finding/{findingId}` | Upload evidence finding | ✅ |
| POST | `/capa-action/{actionId}` | Upload evidence CAPA action | ✅ |
| GET | `/finding/{findingId}` | List file evidence finding | ✅ |
| DELETE | `/{fileId}` | Hapus file evidence | ✅ |

### 📋 Checklist — `/api/Checklist`

| Method | Endpoint | Deskripsi | Auth |
|---|---|---|---|
| GET | `/` | List semua checklist template | ✅ |
| GET | `/{id}` | Detail checklist | ✅ |
| GET | `/{id}/items` | Item-item checklist | ✅ |
| POST | `/` | Buat checklist baru | ✅ |
| DELETE | `/{id}` | Hapus checklist | ✅ |

### 📊 Dashboard — `/api/Dashboard`

| Method | Endpoint | Deskripsi | Auth |
|---|---|---|---|
| GET | `/summary` | Summary cards dashboard | ✅ |
| GET | `/compliance-score` | Compliance score per department | ✅ |
| GET | `/audit-schedule` | Jadwal audit bulanan | ✅ |
| GET | `/monthly-report` | Laporan bulanan | ✅ |

### 📄 Audit Report — `/api/AuditReport`

| Method | Endpoint | Deskripsi | Auth |
|---|---|---|---|
| GET | `/` | List audit yang sudah completed | ✅ |
| GET | `/{sessionId}` | Detail laporan audit | ✅ |

### 📄 PDF — `/api/Pdf`

| Method | Endpoint | Deskripsi | Auth |
|---|---|---|---|
| GET | `/audit-report/{sessionId}` | Generate & download PDF laporan audit | ✅ |

### ❤️ Health Check — `/api/Health`

| Method | Endpoint | Deskripsi | Auth |
|---|---|---|---|
| GET | `/` | Cek status server | ❌ |

---

## Auth Flow

```
Register → OTP dikirim ke email → verify-email → bisa login

Lupa password:
forgot-password/request-otp → forgot-password/verify-otp → forgot-password/reset

Ganti email:
request-email-change-otp → verify-email-change
```

---

## Role

| Role | Keterangan |
|---|---|
| `Admin` | Akses penuh |
| `QualityManager` | Kelola audit plan, lihat semua data |
| `Auditor` | Jalankan sesi audit, catat finding |
| `AuditorInternal` | Auditor internal |
| `Auditee` | Pihak yang diaudit |

---

## Struktur Project

```
QualiTrack/
├── Controllers/       # API endpoints
├── Models/            # Entity models
├── DTOs/              # Data Transfer Objects
├── Data/              # DbContext + seeder
├── Services/          # Email, PDF, storage services
├── Migrations/        # EF Core migrations
├── Middlewares/       # Global exception handler
├── Filters/           # Validation filters
├── Repositories/      # Repository pattern
├── Dockerfile
├── docker-compose.yml
└── docker-compose.dev.yml
```

---

