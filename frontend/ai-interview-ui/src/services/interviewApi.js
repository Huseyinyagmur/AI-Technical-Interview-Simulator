const baseUrl = import.meta.env.VITE_API_URL || 'http://localhost:5181/api';
async function request(path, options = {}) {
  const response = await fetch(`${baseUrl}${path}`, { headers: { 'Content-Type': 'application/json' }, ...options });
  if (!response.ok) { const body = await response.json().catch(() => ({})); throw new Error(body.message || 'Request failed.'); }
  return response.json();
}
export const startInterview = (topic, difficulty) => request('/interviews/start', { method: 'POST', body: JSON.stringify({ topic, difficulty }) });
export const submitAnswer = (sessionId, questionId, answer) => request(`/interviews/${sessionId}/answer`, { method: 'POST', body: JSON.stringify({ questionId, answer }) });
export const getReport = (sessionId) => request(`/interviews/${sessionId}/report`);
