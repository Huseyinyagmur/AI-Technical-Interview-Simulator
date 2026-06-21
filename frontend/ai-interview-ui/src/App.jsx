import { useState } from 'react';
import Home from './pages/Home';
import Interview from './pages/Interview';
import Report from './pages/Report';
import Dashboard from './pages/Dashboard';
import History from './pages/History';

export default function App() {
  const [session, setSession] = useState(null);
  const [reportSessionId, setReportSessionId] = useState(null);
  const [page, setPage] = useState('dashboard');
  const start = () => { setSession(null); setReportSessionId(null); setPage('home'); };
  if (reportSessionId) return <Report sessionId={reportSessionId} onRestart={start} onDashboard={() => { setReportSessionId(null); setPage('dashboard'); }} />;
  if (session) return <Interview session={session} onComplete={() => setReportSessionId(session.sessionId)} />;
  if (page === 'history') return <History onOpen={setReportSessionId} onBack={() => setPage('dashboard')} />;
  if (page === 'dashboard') return <Dashboard onStart={start} onHistory={() => setPage('history')} />;
  return <Home onStarted={setSession} />;
}
