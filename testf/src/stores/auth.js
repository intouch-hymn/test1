import { defineStore } from 'pinia'
import { ref, computed } from 'vue'
import { authAPI } from '@/services/api'

export const useAuthStore = defineStore('auth', () => {
  // State
  const user = ref(null)
  const token = ref(null)
  const loading = ref(false)
  const error = ref(null)

  // Getters
  const isAuthenticated = computed(() => !!token.value && !!user.value)
  const isAdmin = computed(() => user.value?.role === 'Admin' || user.value?.role === 'SuperAdmin')
  const isUser = computed(() => user.value?.role === 'User')

  // Actions
  const initializeAuth = () => {
    // ตรวจสอบ token ใน localStorage เมื่อ app เริ่มต้น
    const savedToken = localStorage.getItem('authToken')
    const savedUser = localStorage.getItem('user')

    if (savedToken && savedUser) {
      token.value = savedToken
      user.value = JSON.parse(savedUser)
    }
  }

  const login = async (email, password) => {
    loading.value = true
    error.value = null

    try {
      const response = await authAPI.login({ email, password })

      if (response.success || response.token) {
        // รองรับทั้ง response แบบใหม่และเก่า
        const tokenData = response.data || response
        const userData = {
          userId: tokenData.userId,
          name: tokenData.name,
          email: tokenData.email,
          role: tokenData.role,
        }

        // เก็บใน localStorage
        localStorage.setItem('authToken', tokenData.token)
        localStorage.setItem('user', JSON.stringify(userData))

        // อัปเดต state
        token.value = tokenData.token
        user.value = userData

        return { success: true }
      } else {
        error.value = response.message || 'Login failed'
        return { success: false, message: error.value }
      }
    } catch (err) {
      error.value = err.response?.data?.message || 'Network error'
      return { success: false, message: error.value }
    } finally {
      loading.value = false
    }
  }

  const register = async (userData) => {
    loading.value = true
    error.value = null

    try {
      const response = await authAPI.register(userData)

      if (response.success || response.token) {
        const tokenData = response.data || response
        const userInfo = {
          userId: tokenData.userId,
          name: tokenData.name,
          email: tokenData.email,
          role: tokenData.role,
        }

        localStorage.setItem('authToken', tokenData.token)
        localStorage.setItem('user', JSON.stringify(userInfo))

        token.value = tokenData.token
        user.value = userInfo

        return { success: true }
      } else {
        error.value = response.message || 'Registration failed'
        return { success: false, message: error.value }
      }
    } catch (err) {
      error.value = err.response?.data?.message || 'Network error'
      return { success: false, message: error.value }
    } finally {
      loading.value = false
    }
  }

  const logout = () => {
    // ลบข้อมูลจาก localStorage
    localStorage.removeItem('authToken')
    localStorage.removeItem('user')

    // ลบข้อมูลจาก state
    token.value = null
    user.value = null
    error.value = null
  }

  const clearError = () => {
    error.value = null
  }

  const hasRole = (role) => {
    return user.value?.role === role
  }

  return {
    // State
    user,
    token,
    loading,
    error,

    // Getters
    isAuthenticated,
    isAdmin,
    isUser,

    // Actions
    initializeAuth,
    login,
    register,
    logout,
    clearError,
    hasRole,
  }
})
