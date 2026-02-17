import React, { useState, useEffect } from 'react';
import Login from './components/Login';
import Signup from './components/Signup';
import Onboarding from './components/Onboarding';
import Dashboard from './components/Dashboard';
import { authService } from './services/authService';
import './App.css';

function App() {
  const [currentView, setCurrentView] = useState('login');
  const [user, setUser] = useState(null);
  const [hasPreferences, setHasPreferences] = useState(false);
  const [showOnboarding, setShowOnboarding] = useState(false);
  const [loading, setLoading] = useState(true);

  const checkPreferences = async () => {
    try {
      await authService.getPreferences();
      setHasPreferences(true);
      setShowOnboarding(false);
    } catch (err) {
      setHasPreferences(false);
      setShowOnboarding(true);
    } finally {
      setLoading(false);
    }
  };

  useEffect(() => {
    const savedUser = localStorage.getItem('user');
    if (savedUser) {
      const parsedUser = JSON.parse(savedUser);
      setUser(parsedUser);
      checkPreferences();
    } else {
      setLoading(false);
    }
  }, []);

  const handleLogin = async (response) => {
    setUser({
      id: response.userId,
      email: response.email,
      name: response.name
    });
    await checkPreferences();
  };

  const handleSignup = async (response) => {
    setUser({
      id: response.userId,
      email: response.email,
      name: response.name
    });
    setShowOnboarding(true);
    setHasPreferences(false);
  };

  const handleOnboardingComplete = () => {
    setShowOnboarding(false);
    setHasPreferences(true);
  };

  const handleChangePreferences = () => {
    setShowOnboarding(true);
    setHasPreferences(false);
  };

  const handleLogout = () => {
    localStorage.removeItem('token');
    localStorage.removeItem('user');
    setUser(null);
  };

  if (loading) {
    return (
      <div className="App">
        <div className="welcome-container">
          <p>Loading...</p>
        </div>
      </div>
    );
  }

  if (user) {
    if (showOnboarding) {
      return (
        <div className="App">
          <Onboarding onComplete={handleOnboardingComplete} />
        </div>
      );
    }

    return (
      <div className="App">
        <Dashboard user={user} onLogout={handleLogout} onChangePreferences={handleChangePreferences} />
      </div>
    );
  }

  return (
    <div className="App">
      <div className="auth-container">
        <div className="auth-tabs">
          <button
            className={currentView === 'login' ? 'active' : ''}
            onClick={() => setCurrentView('login')}
          >
            Login
          </button>
          <button
            className={currentView === 'signup' ? 'active' : ''}
            onClick={() => setCurrentView('signup')}
          >
            Sign Up
          </button>
        </div>
        {currentView === 'login' ? (
          <Login onLogin={handleLogin} />
        ) : (
          <Signup onSignup={handleSignup} />
        )}
      </div>
    </div>
  );
}

export default App;