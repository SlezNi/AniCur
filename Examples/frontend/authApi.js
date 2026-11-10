// Готовий клієнт API: npm-пакети не потрібні.
// У React / Next.js / Vite: import { createAuthApi } from "./authApi";
export function createAuthApi(baseUrl = "http://localhost:5080") {
  const api = `${baseUrl.replace(/\/$/, "")}/api/auth`;

  async function request(path, options) {
    const response = await fetch(`${api}${path}`, {
      ...options,
      headers: { "Content-Type": "application/json", ...options?.headers }
    });
    const text = await response.text();
    const body = text ? JSON.parse(text) : {};
    if (!response.ok) {
      const validation = body.errors
        ? Object.values(body.errors).flat().join(" ")
        : null;
      throw new Error(validation || body.message || body.title || `Помилка API (${response.status})`);
    }
    return body;
  }

  return {
    register: (username, email, password) => request("/register", {
      method: "POST", body: JSON.stringify({ username, email, password })
    }),
    login: (emailOrUsername, password) => request("/login", {
      method: "POST", body: JSON.stringify({ emailOrUsername, password })
    }),
    me: (token) => request("/me", {
      method: "GET", headers: { Authorization: `Bearer ${token}` }
    })
  };
}
