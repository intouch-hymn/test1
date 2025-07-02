<template>
  <div class="login-container">
    <form @submit.prevent="handleLogin" class="login-form">
      <h2>เข้าสู่ระบบ</h2>

      <!-- Error Message -->
      <div v-if="error" class="error-message">
        {{ error }}
      </div>

      <!-- Email Input -->
      <div class="form-group">
        <label for="email">อีเมล:</label>
        <input
          id="email"
          v-model="email"
          type="email"
          required
          placeholder="example@email.com"
          :disabled="loading"
        />
      </div>

      <!-- Password Input -->
      <div class="form-group">
        <label for="password">รหัสผ่าน:</label>
        <input
          id="password"
          v-model="password"
          type="password"
          required
          placeholder="รหัสผ่าน"
          :disabled="loading"
        />
      </div>

      <!-- Login Button -->
      <button type="submit" :disabled="loading" class="login-button">
        <span v-if="loading">กำลังเข้าสู่ระบบ...</span>
        <span v-else>เข้าสู่ระบบ</span>
      </button>

      <!-- Register Link -->
      <p class="register-link">
        ยังไม่มีบัญชี?
        <router-link to="/register">สมัครสมาชิก</router-link>
      </p>
    </form>
  </div>
</template>

<script>
import { authAPI } from '@/services/api'

export default {
  name: 'LoginForm',

  // Data - reactive properties
  data() {
    return {
      email: '',
      password: '',
      loading: false,
      error: null,
    }
  },

  methods: {
    async handleLogin() {
      this.loading = true
      this.error = null

      try {
        const response = await authAPI.login({
          email: this.email,
          password: this.password,
        })

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

          // Emit event ไปยัง parent component หรือ redirect
          this.$emit('login-success', { user: userData, token: tokenData.token })

          // Redirect based on user role
          if (userData.role === 'Admin' || userData.role === 'SuperAdmin') {
            this.$router.push('/admin')
          } else {
            this.$router.push('/dashboard')
          }
        } else {
          this.error = response.message || 'Login failed'
        }
      } catch (err) {
        this.error = err.response?.data?.message || 'Network error'
        console.error('Login error:', err)
      } finally {
        this.loading = false
      }
    },

    clearError() {
      this.error = null
    },
  },

  // Lifecycle hooks
  mounted() {
    // ตรวจสอบว่าล็อกอินอยู่หรือไม่
    const token = localStorage.getItem('authToken')
    if (token) {
      this.$router.push('/dashboard')
    }
  },
}
</script>

<style scoped>
.login-container {
  display: flex;
  justify-content: center;
  align-items: center;
  min-height: 100vh;
  padding: 20px;
  background: linear-gradient(135deg, #667eea 0%, #764ba2 100%);
}

.login-form {
  background: white;
  padding: 2rem;
  border-radius: 12px;
  box-shadow: 0 8px 32px rgba(0, 0, 0, 0.1);
  width: 100%;
  max-width: 400px;
}

.login-form h2 {
  text-align: center;
  margin-bottom: 1.5rem;
  color: #333;
  font-weight: 600;
}

.form-group {
  margin-bottom: 1rem;
}

.form-group label {
  display: block;
  margin-bottom: 0.5rem;
  font-weight: 500;
  color: #555;
}

.form-group input {
  width: 100%;
  padding: 0.75rem;
  border: 2px solid #ddd;
  border-radius: 8px;
  font-size: 1rem;
  transition: border-color 0.3s ease;
  box-sizing: border-box;
}

.form-group input:focus {
  outline: none;
  border-color: #667eea;
  box-shadow: 0 0 0 3px rgba(102, 126, 234, 0.1);
}

.login-button {
  width: 100%;
  padding: 0.75rem;
  background: linear-gradient(135deg, #667eea 0%, #764ba2 100%);
  color: white;
  border: none;
  border-radius: 8px;
  font-size: 1rem;
  font-weight: 600;
  cursor: pointer;
  transition:
    transform 0.2s,
    box-shadow 0.2s;
}

.login-button:hover:not(:disabled) {
  transform: translateY(-2px);
  box-shadow: 0 4px 12px rgba(102, 126, 234, 0.4);
}

.login-button:disabled {
  background: #ccc;
  cursor: not-allowed;
  transform: none;
  box-shadow: none;
}

.error-message {
  background-color: #fee;
  color: #c53030;
  padding: 0.75rem;
  border-radius: 8px;
  margin-bottom: 1rem;
  border: 1px solid #feb2b2;
  text-align: center;
}

.register-link {
  text-align: center;
  margin-top: 1.5rem;
  color: #666;
}

.register-link a {
  color: #667eea;
  text-decoration: none;
  font-weight: 500;
}

.register-link a:hover {
  text-decoration: underline;
}

/* Mobile Responsive */
@media (max-width: 768px) {
  .login-container {
    padding: 10px;
  }

  .login-form {
    padding: 1.5rem;
  }
}
</style>
