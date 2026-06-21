const baseUrl = import.meta.env.VITE_API_BASE_URL || 'http://localhost:5268/api';
const token = () => JSON.parse(localStorage.getItem('auth') || 'null')?.token;
async function request(path, options = {}) {
  try {
    const response = await fetch(`${baseUrl}${path}`, { headers: { 'Content-Type': 'application/json', ...(token() ? { Authorization: `Bearer ${token()}` } : {}) }, ...options });
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
export async function downloadReportPdf(sessionId) { const response = await fetch(`${baseUrl}/interviews/${sessionId}/report/pdf`, { headers: { ...(token() ? { Authorization: `Bearer ${token()}` } : {}) } }); if (!response.ok) { const raw = await response.text(); let body = {}; try { body = JSON.parse(raw); } catch { body = { message: raw }; } const messages = { 400: 'PDF oluşturmak için mülakat tamamlanmış olmalı.', 401: 'Oturum süreniz dolmuş olabilir, tekrar giriş yapın.', 403: 'Bu raporu indirme yetkiniz yok.', 404: 'Rapor bulunamadı.' }; throw new Error(body.message || body.detail || messages[response.status] || 'PDF raporu indirilemedi.'); } const blob = await response.blob(); const url = URL.createObjectURL(blob); const link = document.createElement('a'); link.href = url; link.download = `interview-report-${sessionId}.pdf`; document.body.appendChild(link); link.click(); link.remove(); setTimeout(() => URL.revokeObjectURL(url), 1000); }
