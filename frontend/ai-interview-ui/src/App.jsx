import { useState } from 'react';
import Home from './pages/Home';
import Interview from './pages/Interview';
import Report from './pages/Report';
import Dashboard from './pages/Dashboard';
import History from './pages/History';
import Login from './pages/Login';
import Register from './pages/Register';
import { getCurrentUser, logout } from './services/authApi';

export default function App() {
  const [session, setSession] = useState(null);
  const [reportSessionId, setReportSessionId] = useState(null);
  const [page, setPage] = useState('dashboard');
  const [user, setUser] = useState(getCurrentUser());
  const start = () => { setSession(null); setReportSessionId(null); setPage('home'); };
  if (!user) return page === 'register' ? <Register onAuth={setUser} onLogin={() => setPage('login')} /> : <Login onAuth={setUser} onRegister={() => setPage('register')} />;
  if (reportSessionId) return <Report sessionId={reportSessionId} onRestart={start} onDashboard={() => { setReportSessionId(null); setPage('dashboard'); }} />;
  if (session) return <Interview session={session} onComplete={() => setReportSessionId(session.sessionId)} />;
  if (page === 'history') return <History onOpen={setReportSessionId} onBack={() => setPage('dashboard')} />;
  if (page === 'dashboard') return <><div className="user-nav"><span>{user.fullName}</span><button onClick={() => { logout(); setUser(null); setPage('login'); }}>Çıkış yap</button></div><Dashboard onStart={start} onHistory={() => setPage('history')} /></>;
  return <Home onStarted={setSession} />;
}
