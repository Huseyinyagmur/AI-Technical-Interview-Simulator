const baseUrl = import.meta.env.VITE_API_BASE_URL || 'http://localhost:5268/api';
async function request(path, options = {}) {
  try {
    const response = await fetch(`${baseUrl}${path}`, { headers: { 'Content-Type': 'application/json' }, ...options });
    if (!response.ok) {
      const body = await response.json().catch(() => ({}));
      const fallback = response.status >= 500 ? 'Backend hatası oluştu. API çalışıyor mu ve migration uygulandı mı?' : 'İstek tamamlanamadı.';
      throw new Error(body.message || fallback);
    }
    return response.json();
  } catch (error) {
    if (error instanceof TypeError) throw new Error('Backend bağlantısı başarısız. API çalışıyor mu ve migration uygulandı mı?');
    throw error;
  }
}
export const startInterview = (topic, difficulty, track) => request('/interviews/start', { method: 'POST', body: JSON.stringify({ topic, difficulty, track }) });
export const getTracks = () => request('/tracks');
export const submitAnswer = (sessionId, questionId, answer) => request(`/interviews/${sessionId}/answer`, { method: 'POST', body: JSON.stringify({ questionId, answer }) });
export const getReport = (sessionId) => request(`/interviews/${sessionId}/report`);
export const getHistory = () => request('/interviews/history');
export const getDashboard = () => request('/dashboard/summary');
