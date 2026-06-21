const baseUrl = import.meta.env.VITE_API_BASE_URL || 'http://localhost:5268/api';
const save = data => { localStorage.setItem('auth', JSON.stringify(data)); return data; };
async function auth(path, body) { const r = await fetch(`${baseUrl}${path}`, { method: 'POST', headers: { 'Content-Type': 'application/json' }, body: JSON.stringify(body) }); const data = await r.json().catch(() => ({})); if (!r.ok) throw new Error(data.message || data.detail || data.title || 'İşlem tamamlanamadı.'); return save(data); }
export const login = (email, password) => auth('/auth/login', { email, password });
export const register = (fullName, email, password) => auth('/auth/register', { fullName, email, password });
export const logout = () => localStorage.removeItem('auth');
export const getCurrentUser = () => JSON.parse(localStorage.getItem('auth') || 'null');
export const getToken = () => getCurrentUser()?.token;
