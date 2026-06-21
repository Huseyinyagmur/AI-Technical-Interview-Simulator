import { useState } from 'react';
import Home from './pages/Home';
import Interview from './pages/Interview';
import Report from './pages/Report';

export default function App() {
  const [session, setSession] = useState(null);
  const [reportSessionId, setReportSessionId] = useState(null);
  if (reportSessionId) return <Report sessionId={reportSessionId} onRestart={() => { setSession(null); setReportSessionId(null); }} />;
  return session ? <Interview session={session} onComplete={() => setReportSessionId(session.sessionId)} /> : <Home onStarted={setSession} />;
}
