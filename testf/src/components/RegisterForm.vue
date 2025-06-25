<template>
  <div class="register-container">
    <form @submit.prevent="handleRegister" class="register-form">
      <h2>สมัครสมาชิก</h2>

      <!-- Error Message -->
      <div v-if="error" class="error-message">
        {{ error }}
      </div>

      <!-- Success Message -->
      <div v-if="successMessage" class="success-message">
        {{ successMessage }}
      </div>

      <!-- Name Input -->
      <div class="form-group">
        <label for="name">ชื่อ:</label>
        <input
          id="name"
          v-model="formData.name"
          type="text"
          required
          placeholder="ชื่อของคุณ"
          :disabled="loading"
        />
      </div>

      <!-- Email Input -->
      <div class="form-group">
        <label for="email">อีเมล:</label>
        <input
          id="email"
          v-model="formData.email"
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
          v-model="formData.password"
          type="password"
          required
          placeholder="รหัสผ่าน (อย่างน้อย 6 ตัวอักษร)"
          :disabled="loading"
          minlength="6"
        />
      </div>

      <!-- Age Input -->
      <div class="form-group">
        <label for="age">อายุ:</label>
        <input
          id="age"
          v-model.number="formData.age"
          type="number"
          required
          placeholder="อายุของคุณ"
          :disabled="loading"
          min="1"
          max="150"
        />
      </div>

      <!-- Register Button -->
      <button type="submit" :disabled="loading || !isFormValid" class="register-button">
        <span v-if="loading">กำลังสมัครสมาชิก...</span>
        <span v-else>สมัครสมาชิก</span>
      </button>

      <!-- Login Link -->
      <p class="login-link">
        มีบัญชีอยู่แล้ว?
        <router-link to="/login">เข้าสู่ระบบ</router-link>
      </p>
    </form>
  </div>
</template>

<script>
import { authAPI } from '@/services/api'

export default {
  name: 'RegisterForm',

  data() {
    return {
      formData: {
        name: '',
        email: '',
        password: '',
        age: null,
      },
      loading: false,
      error: null,
      successMessage: null,
    }
  },

  computed: {
    isFormValid() {
      return (
        this.formData.name.trim() !== '' &&
        this.formData.email.trim() !== '' &&
        this.formData.password.length >= 6 &&
        this.formData.age >= 1 &&
        this.formData.age <= 150
      )
    },
  },

  methods: {
    async handleRegister() {
      this.loading = true
      this.error = null
      this.successMessage = null

      try {
        const response = await authAPI.register(this.formData)

        if (response.success || response.token) {
          this.successMessage = 'สมัครสมาชิกสำเร็จ! กำลังเข้าสู่ระบบ...'

          const tokenData = response.data || response
          const userData = {
            userId: tokenData.userId,
            name: tokenData.name,
            email: tokenData.email,
            role: tokenData.role,
          }

          localStorage.setItem('authToken', tokenData.token)
          localStorage.setItem('user', JSON.stringify(userData))

          setTimeout(() => {
            this.$router.push('/dashboard')
          }, 1000)
        } else {
          this.error = response.message || 'Registration failed'
        }
      } catch (err) {
        this.error = err.response?.data?.message || 'Network error'
        console.error('Registration error:', err)
      } finally {
        this.loading = false
      }
    },

    clearError() {
      this.error = null
    },
  },

  mounted() {
    const token = localStorage.getItem('authToken')
    if (token) {
      this.$router.push('/dashboard')
    }
  },
}
</script>

<style scoped>
.register-container {
  display: flex;
  justify-content: center;
  align-items: center;
  min-height: 100vh;
  padding: 20px;
  background: linear-gradient(135deg, #667eea 0%, #764ba2 100%);
}

.register-form {
  background: white;
  padding: 2rem;
  border-radius: 12px;
  box-shadow: 0 8px 32px rgba(0, 0, 0, 0.1);
  width: 100%;
  max-width: 450px;
}

.register-form h2 {
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

.register-button {
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

.register-button:hover:not(:disabled) {
  transform: translateY(-2px);
  box-shadow: 0 4px 12px rgba(102, 126, 234, 0.4);
}

.register-button:disabled {
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

.success-message {
  background-color: #f0fff4;
  color: #22543d;
  padding: 0.75rem;
  border-radius: 8px;
  margin-bottom: 1rem;
  border: 1px solid #9ae6b4;
  text-align: center;
}

.login-link {
  text-align: center;
  margin-top: 1.5rem;
  color: #666;
}

.login-link a {
  color: #667eea;
  text-decoration: none;
  font-weight: 500;
}

.login-link a:hover {
  text-decoration: underline;
}

@media (max-width: 768px) {
  .register-container {
    padding: 10px;
  }

  .register-form {
    padding: 1.5rem;
  }
}
</style>
