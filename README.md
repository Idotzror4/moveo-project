# Moveo Crypto Dashboard - Ido Tzror

A full-stack cryptocurrency dashboard application with personalized content, user authentication, and feedback system.

## Live Application

- **Frontend**: [https://moveo-project-xi.vercel.app](https://moveo-project-xi.vercel.app)
- **Backend API**: [https://moveo-project-mpi3.onrender.com](https://moveo-project-mpi3.onrender.com)
- **GitHub Repository**: [https://github.com/Idotzror4/moveo-project](https://github.com/Idotzror4/moveo-project)

## Features

- User Authentication: Secure registration and login with JWT tokens
- Onboarding Questionnaire: Personalized experience based on user preferences
- Market News: Latest cryptocurrency news from CryptoPanic API
- Coin Prices: Real-time prices from CoinGecko API (filtered by user preferences)
- AI Insights: Daily AI-generated market insights from OpenRouter (with fallback)
- Memes: Dynamic crypto memes from Reddit API (adapted to user preferences)
- Voting System: Users can vote on each content section to improve recommendations

## Technologies Used

### Frontend
- React 19.2.4
- Axios for API calls
- CSS3 for styling

### Backend
- ASP.NET Core 9.0
- Entity Framework Core with SQLite
- JWT Authentication
- BCrypt for password hashing

### External APIs
- CryptoPanic - Market news
- CoinGecko - Cryptocurrency prices
- OpenRouter - AI insights (optional, with fallback)
- Reddit API - Memes

## Prerequisites

- Node.js 16+ and npm
- .NET 9.0 SDK
- Git

## Installation & Setup

### Backend Setup

1. Navigate to the backend directory:
```bash
cd MoveoBackend
```

2. Restore dependencies:
```bash
dotnet restore
```

3. Run database migrations:
```bash
dotnet ef database update
```

4. Optional: Configure OpenRouter API key in `appsettings.json`:
```json
{
  "OpenRouter": {
    "ApiKey": "your-api-key-here"
  }
}
```

5. Run the backend:
```bash
dotnet run
```

The backend will run on `http://localhost:5000`

### Frontend Setup

1. Navigate to the frontend directory:
```bash
cd frontend
```

2. Install dependencies:
```bash
npm install
```

3. Create `.env` file (optional, defaults to `http://localhost:5000`):
```
REACT_APP_API_URL=http://localhost:5000
```

4. Run the frontend:
```bash
npm start
```

The frontend will run on `http://localhost:3000`

## Project Structure

```
moveoProject/
├── frontend/
│   ├── src/
│   │   ├── components/
│   │   │   ├── Dashboard.js
│   │   │   ├── Login.js
│   │   │   ├── Signup.js
│   │   │   └── Onboarding.js
│   │   ├── services/
│   │   │   └── authService.js
│   │   └── App.js
│   └── package.json
│
└── MoveoBackend/
    ├── Controllers/
    │   ├── AuthController.cs
    │   ├── DashboardController.cs
    │   ├── OnboardingController.cs
    │   └── VoteController.cs
    ├── Models/
    │   ├── User.cs
    │   ├── UserPreferences.cs
    │   ├── Vote.cs
    │   └── DailyContent.cs
    ├── DTOs/
    ├── Data/
    │   └── ApplicationDbContext.cs
    └── Program.cs
```

## Database Schema

- Users: User accounts (email, name, password hash)
- UserPreferences: Onboarding questionnaire responses
- Votes: User votes on content sections
- DailyContent: Cached daily content (news, prices, AI insights)

## API Endpoints

### Authentication
- POST /api/auth/register - Register new user
- POST /api/auth/login - Login user

### Onboarding
- POST /api/onboarding - Save user preferences
- GET /api/onboarding - Get user preferences

### Dashboard
- GET /api/dashboard/news - Get market news
- GET /api/dashboard/prices - Get coin prices
- GET /api/dashboard/ai-insight - Get AI insight
- GET /api/dashboard/meme - Get crypto meme

### Voting
- POST /api/vote - Submit vote
- GET /api/vote - Get user votes

All endpoints except `/api/auth/*` require JWT authentication.

## Voting Mechanism

The voting system allows users to provide feedback on each content section. Users can vote positive or negative on Market News, Coin Prices, AI Insight, and Memes.

How it works:
- Users can vote positive or negative on each section
- Votes are stored in the database and persist across sessions
- Users can change their vote at any time
- After voting, users see a "Thank you for your feedback!" message

How voting improves the model:
- Content Quality: Identifies which content users find valuable
- Personalization: Learns individual user preferences for better recommendations
- AI Training: Provides labeled data for improving AI insight generation
- Content Curation: Helps filter and rank content based on user feedback

Votes are stored in the database and can be analyzed to improve the AI recommendation algorithms and content curation.

## Deployment

### Step 1: Prepare GitHub Repository

1. Initialize git repository:
```bash
git init
git add .
git commit -m "Initial commit"
```

2. Create a new repository on GitHub and push:
```bash
git remote add origin https://github.com/yourusername/moveo-project.git
git branch -M main
git push -u origin main
```

### Step 2: Deploy Backend

#### Option A: Deploy to Render

1. Go to [Render Dashboard](https://dashboard.render.com)
2. Click "New +" → "Web Service"
3. Connect your GitHub repository: `Idotzror4/moveo-project`
4. Configure:
   - Name: `moveo-project`
   - Language: `Docker`
   - Root Directory: `MoveoBackend`
   - Dockerfile Path: `Dockerfile`
5. Set Environment Variables:
   ```
   Jwt__Key=YourSuperSecretKeyThatShouldBeAtLeast32CharactersLong!
   Jwt__Issuer=MoveoBackend
   Jwt__Audience=MoveoBackend
   OpenRouter__ApiKey=your-openrouter-api-key (optional)
   AllowedOrigins=https://moveo-project-xi.vercel.app,http://localhost:3000
   ```
6. The backend will automatically run database migrations on startup
7. Copy your backend URL (e.g., `https://moveo-project-mpi3.onrender.com`)

#### Option B: Deploy to Railway

1. Go to Railway Dashboard
2. Click "New Project" → "Deploy from GitHub repo"
3. Select your repository (Railway auto-detects .NET projects)
4. Set Environment Variables:
   ```
   Jwt__Key=YourSuperSecretKeyThatShouldBeAtLeast32CharactersLong!
   Jwt__Issuer=MoveoBackend
   Jwt__Audience=MoveoBackend
   OpenRouter__ApiKey=your-openrouter-api-key (optional)
   AllowedOrigins=https://your-frontend-url.vercel.app,https://your-frontend-url.netlify.app
   ```
5. Copy your backend URL

### Step 3: Deploy Frontend

#### Option A: Deploy to Vercel

1. Go to [Vercel Dashboard](https://vercel.com)
2. Click "Add New..." → "Project"
3. Import your GitHub repository: `Idotzror4/moveo-project`
4. Configure:
   - Framework Preset: Create React App
   - Root Directory: `frontend`
   - Build Command: `npm run build`
   - Output Directory: `build`
5. Set Environment Variable:
   ```
   REACT_APP_API_URL=https://moveo-project-mpi3.onrender.com
   ```
6. Deploy and copy your frontend URL (e.g., `https://moveo-project-xi.vercel.app`)

#### Option B: Deploy to Netlify

1. Go to Netlify Dashboard
2. Click "Add new site" → "Import an existing project"
3. Connect your GitHub repository
4. Configure:
   - Base directory: `frontend`
   - Build command: `npm install && npm run build`
   - Publish directory: `frontend/build`
5. Set Environment Variable:
   ```
   REACT_APP_API_URL=https://your-backend-url.onrender.com
   ```
6. Deploy and copy your frontend URL

### Step 4: Update Backend CORS

After deploying the frontend, update the backend's `AllowedOrigins` environment variable to include your frontend URL, then redeploy the backend.

### Step 5: Test Deployment

1. Visit your frontend URL
2. Register a new user
3. Complete the onboarding questionnaire
4. Verify all dashboard sections load correctly
5. Test voting functionality

## Environment Variables

### Backend
- Jwt__Key - Secret key for JWT tokens (required, min 32 characters)
- Jwt__Issuer - JWT issuer name (optional, default: MoveoBackend)
- Jwt__Audience - JWT audience name (optional, default: MoveoBackend)
- OpenRouter__ApiKey - OpenRouter API key for AI insights (optional)
- AllowedOrigins - Comma-separated list of allowed frontend URLs

### Frontend
- REACT_APP_API_URL - Backend API URL (default: `http://localhost:5000`)

## Troubleshooting

### Backend Issues
- Database not found: Ensure migrations run on first deployment
- CORS errors: Verify `AllowedOrigins` includes your frontend URL exactly (including https://)
- JWT errors: Ensure `Jwt__Key` is at least 32 characters long

### Frontend Issues
- API calls failing: Check that `REACT_APP_API_URL` is set correctly and backend is accessible
- Build errors: Ensure all dependencies are in `package.json` and Node version is compatible
