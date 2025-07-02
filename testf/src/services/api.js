import axios from 'axios'

// สำหรับ Vite ต้องใช้ import.meta.env
const API_BASE_URL = import.meta.env.VITE_API_BASE_URL || 'http://localhost:8080/api'

const apiClient = axios.create({
  baseURL: API_BASE_URL,
  headers: {
    'Content-Type': 'application/json',
  },
  timeout: 10000,
})

// Request interceptor - เพิ่ม JWT token ในทุก request
apiClient.interceptors.request.use(
  (config) => {
    const token = localStorage.getItem('authToken') // ดึง token จาก localStorage
    if (token) {
      config.headers.Authorization = `Bearer ${token}` // เพิ่ม token ใน header
    }
    return config // ส่ง config กลับไป
  },
  (error) => {
    return Promise.reject(error) // ถ้ามี error ใน request ให้ reject
  },
)

// Response interceptor - จัดการ errors
apiClient.interceptors.response.use(
  (response) => {
    return response
  },
  (error) => {
    if (error.response?.status === 401) {
      // Token หมดอายุ - ลบ token และ redirect
      localStorage.removeItem('authToken')
      localStorage.removeItem('user')
      window.location.href = '/login'
    }
    return Promise.reject(error)
  },
)

// Auth API functions
export const authAPI = {
  // ทดสอบการเชื่อมต่อ API
  test: async () => {
    const response = await apiClient.get('/auth/test')
    return response.data
  },

  // สมัครสมาชิก
  register: async (userData) => {
    const response = await apiClient.post('/auth/register', userData)
    return response.data
  },

  // เข้าสู่ระบบ
  login: async (credentials) => {
    const response = await apiClient.post('/auth/login', credentials)
    return response.data
  },

  // ดูโปรไฟล์
  getProfile: async () => {
    const response = await apiClient.get('/auth/profile')
    return response.data
  },

  // ดูข้อมูลจาก JWT token
  getUserInfo: async () => {
    const response = await apiClient.get('/auth/user-info')
    return response.data
  },
}

// Admin API functions
export const adminAPI = {
  // ดูข้อมูลสาธารณะ
  getPublicData: async () => {
    const response = await apiClient.get('/admin/public')
    return response.data
  },

  // ดูข้อมูลป้องกัน (ต้องล็อกอิน)
  getProtectedData: async () => {
    const response = await apiClient.get('/admin/protected')
    return response.data
  },

  // ดูข้อมูล Admin (เฉพาะ Admin)
  getAdminData: async () => {
    const response = await apiClient.get('/admin/admin-only')
    return response.data
  },

  // ดูข้อมูล User หรือ Admin
  getUserOrAdminData: async () => {
    const response = await apiClient.get('/admin/user-or-admin')
    return response.data
  },

  // ดูข้อมูลใน Token
  getTokenInfo: async () => {
    const response = await apiClient.get('/admin/token-info')
    return response.data
  },
}

// Users API functions
export const usersAPI = {
  // ดูรายชื่อผู้ใช้
  getUsers: async () => {
    const response = await apiClient.get('/users')
    return response.data
  },

  // ดูผู้ใช้คนเดียว
  getUser: async (id) => {
    const response = await apiClient.get(`/users/${id}`)
    return response.data
  },

  // สร้างผู้ใช้ใหม่
  createUser: async (userData) => {
    const response = await apiClient.post('/users', userData)
    return response.data
  },

  // แก้ไขผู้ใช้
  updateUser: async (id, userData) => {
    const response = await apiClient.put(`/users/${id}`, userData)
    return response.data
  },

  // ลบผู้ใช้
  deleteUser: async (id) => {
    const response = await apiClient.delete(`/users/${id}`)
    return response.data
  },
}

export default apiClient
