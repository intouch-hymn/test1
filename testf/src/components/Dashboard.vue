<template>
  <div class="dashboard">
    <!-- Header -->
    <header class="dashboard-header">
      <h1>Dashboard</h1>
      <div class="user-info">
        <span class="welcome-text">สวัสดี, {{ currentUser ? currentUser.name : 'Guest' }}</span>
        <span v-if="currentUser" class="user-role" :class="roleClass">
          {{ currentUser.role }}
        </span>
        <button @click="handleLogout" class="logout-button">ออกจากระบบ</button>
      </div>
    </header>

    <main class="dashboard-content">
      <!-- API Test Section -->
      <section class="api-section">
        <h3>🧪 ทดสอบ API Endpoints</h3>

        <div class="api-buttons">
          <button @click="testPublicAPI" class="api-button public">🌐 Public API</button>

          <button @click="testProtectedAPI" class="api-button protected">🔒 Protected API</button>

          <button @click="testAdminAPI" class="api-button admin" :disabled="!isAdmin">
            🛡️ Admin API
          </button>

          <button @click="testUserOrAdminAPI" class="api-button user-admin">
            👥 User/Admin API
          </button>
        </div>

        <!-- API Results -->
        <div v-if="apiResult" class="api-result">
          <h4>✅ ผลลัพธ์ API:</h4>
          <pre>{{ formattedApiResult }}</pre>
        </div>

        <div v-if="apiError" class="api-error">
          <h4>❌ ข้อผิดพลาด:</h4>
          <p>{{ apiError }}</p>
        </div>
      </section>

      <!-- User Profile Section -->
      <section class="profile-section">
        <h3>👤 ข้อมูลผู้ใช้</h3>
        <div v-if="currentUser" class="profile-card">
          <div class="profile-item"><strong>ชื่อ:</strong> {{ currentUser.name }}</div>
          <div class="profile-item"><strong>อีเมล:</strong> {{ currentUser.email }}</div>
          <div class="profile-item">
            <strong>Role:</strong>
            <span class="role-badge" :class="roleClass">{{ currentUser.role }}</span>
          </div>
          <div class="profile-item"><strong>User ID:</strong> {{ currentUser.userId }}</div>
        </div>

        <button @click="getDetailedProfile" class="profile-button">🔄 อัปเดตข้อมูลโปรไฟล์</button>
      </section>

      <!-- Admin Section -->
      <section v-if="isAdmin" class="admin-section">
        <h3>🛡️ Admin Panel</h3>
        <div class="admin-actions">
          <button @click="getUsers" class="admin-button">👥 ดูรายชื่อผู้ใช้ทั้งหมด</button>
          <button @click="getTokenInfo" class="admin-button">🔍 ดูข้อมูลใน JWT Token</button>
          <button @click="clearAdminData" class="admin-button secondary">🗑️ ล้างข้อมูล</button>
        </div>

        <div v-if="adminData" class="admin-data">
          <h4>📊 ข้อมูล Admin:</h4>
          <pre>{{ formattedAdminData }}</pre>
        </div>
      </section>

      <!-- Statistics Section -->
      <section class="stats-section">
        <h3>📈 สถิติการใช้งาน</h3>
        <div class="stats-grid">
          <div class="stat-card">
            <div class="stat-number">{{ apiCallCount }}</div>
            <div class="stat-label">API Calls</div>
          </div>
          <div class="stat-card">
            <div class="stat-number">{{ loginTime }}</div>
            <div class="stat-label">Login Time</div>
          </div>
          <div class="stat-card" :class="{ 'admin-stat': isAdmin }">
            <div class="stat-number">{{ isAdmin ? 'Admin' : 'User' }}</div>
            <div class="stat-label">Access Level</div>
          </div>
        </div>
      </section>
    </main>
  </div>
</template>

<script>
import { adminAPI, usersAPI, authAPI } from '@/services/api'

export default {
  name: 'DashboardView',

  data() {
    return {
      currentUser: null,
      apiResult: null,
      apiError: null,
      adminData: null,
      apiCallCount: 0,
      loginTime: '',
      detailedProfile: null,
    }
  },

  computed: {
    isAdmin() {
      if (!this.currentUser) return false
      return this.currentUser.role === 'Admin' || this.currentUser.role === 'SuperAdmin'
    },

    isUser() {
      if (!this.currentUser) return false
      return this.currentUser.role === 'User'
    },

    roleClass() {
      if (!this.currentUser) return ''
      return this.currentUser.role.toLowerCase()
    },

    formattedApiResult() {
      return JSON.stringify(this.apiResult, null, 2)
    },

    formattedAdminData() {
      return JSON.stringify(this.adminData, null, 2)
    },
  },

  methods: {
    initializeUser() {
      try {
        const savedUser = localStorage.getItem('user')
        if (savedUser) {
          this.currentUser = JSON.parse(savedUser)
          this.loginTime = new Date().toLocaleTimeString('th-TH')
        } else {
          this.$router.push('/login')
        }
      } catch (error) {
        console.error('Error parsing user data:', error)
        this.$router.push('/login')
      }
    },

    clearResults() {
      this.apiResult = null
      this.apiError = null
      this.apiCallCount++
    },

    async testPublicAPI() {
      this.clearResults()
      try {
        const result = await adminAPI.getPublicData()
        this.apiResult = result
      } catch (error) {
        this.apiError = error.response?.data?.message || error.message
      }
    },

    async testProtectedAPI() {
      this.clearResults()
      try {
        const result = await adminAPI.getProtectedData()
        this.apiResult = result
      } catch (error) {
        this.apiError = error.response?.data?.message || error.message
      }
    },

    async testAdminAPI() {
      this.clearResults()
      try {
        const result = await adminAPI.getAdminData()
        this.apiResult = result
      } catch (error) {
        this.apiError = error.response?.data?.message || error.message
      }
    },

    async testUserOrAdminAPI() {
      this.clearResults()
      try {
        const result = await adminAPI.getUserOrAdminData()
        this.apiResult = result
      } catch (error) {
        this.apiError = error.response?.data?.message || error.message
      }
    },

    async getDetailedProfile() {
      try {
        const result = await authAPI.getProfile()
        this.detailedProfile = result
        this.apiResult = result
        this.apiError = null
      } catch (error) {
        this.apiError = error.response?.data?.message || error.message
      }
    },

    async getUsers() {
      try {
        const result = await usersAPI.getUsers()
        this.adminData = result
        this.apiError = null
      } catch (error) {
        this.apiError = error.response?.data?.message || error.message
      }
    },

    async getTokenInfo() {
      try {
        const result = await adminAPI.getTokenInfo()
        this.adminData = result
        this.apiError = null
      } catch (error) {
        this.apiError = error.response?.data?.message || error.message
      }
    },

    clearAdminData() {
      this.adminData = null
      this.apiError = null
    },

    handleLogout() {
      localStorage.removeItem('authToken')
      localStorage.removeItem('user')
      this.$router.push('/login')
    },
  },

  mounted() {
    this.initializeUser()
  },

  watch: {
    $route() {
      this.initializeUser()
    },
  },
}
</script>

<style scoped>
.dashboard {
  min-height: 100vh;
  background: linear-gradient(135deg, #f5f7fa 0%, #c3cfe2 100%);
}

.dashboard-header {
  background: white;
  padding: 1rem 2rem;
  box-shadow: 0 2px 10px rgba(0, 0, 0, 0.1);
  display: flex;
  justify-content: space-between;
  align-items: center;
  border-bottom: 3px solid #667eea;
}

.dashboard-header h1 {
  color: #333;
  font-weight: 700;
  margin: 0;
}

.user-info {
  display: flex;
  align-items: center;
  gap: 1rem;
}

.welcome-text {
  font-weight: 500;
  color: #555;
}

.user-role {
  padding: 0.4rem 1rem;
  border-radius: 20px;
  font-size: 0.875rem;
  font-weight: 600;
  color: white;
  text-transform: uppercase;
  letter-spacing: 0.5px;
}

.user-role.user {
  background: linear-gradient(135deg, #48bb78, #38a169);
}

.user-role.admin {
  background: linear-gradient(135deg, #e53e3e, #c53030);
}

.logout-button {
  padding: 0.5rem 1rem;
  background: linear-gradient(135deg, #718096, #4a5568);
  color: white;
  border: none;
  border-radius: 6px;
  cursor: pointer;
  font-weight: 500;
  transition: all 0.2s;
}

.logout-button:hover {
  transform: translateY(-1px);
  box-shadow: 0 4px 12px rgba(113, 128, 150, 0.4);
}

.dashboard-content {
  padding: 2rem;
  max-width: 1200px;
  margin: 0 auto;
}

.api-section,
.profile-section,
.admin-section,
.stats-section {
  background: white;
  padding: 1.5rem;
  border-radius: 12px;
  margin-bottom: 2rem;
  box-shadow: 0 4px 20px rgba(0, 0, 0, 0.08);
  border: 1px solid #e2e8f0;
}

.api-section h3,
.profile-section h3,
.admin-section h3,
.stats-section h3 {
  margin-top: 0;
  color: #2d3748;
  font-weight: 600;
}

.api-buttons {
  display: grid;
  grid-template-columns: repeat(auto-fit, minmax(200px, 1fr));
  gap: 1rem;
  margin-bottom: 1.5rem;
}

.api-button {
  padding: 0.75rem 1.5rem;
  border: none;
  border-radius: 8px;
  cursor: pointer;
  font-weight: 600;
  transition: all 0.3s ease;
  text-transform: uppercase;
  letter-spacing: 0.5px;
}

.api-button.public {
  background: linear-gradient(135deg, #3182ce, #2c5282);
  color: white;
}

.api-button.protected {
  background: linear-gradient(135deg, #ed8936, #dd6b20);
  color: white;
}

.api-button.admin {
  background: linear-gradient(135deg, #e53e3e, #c53030);
  color: white;
}

.api-button.user-admin {
  background: linear-gradient(135deg, #48bb78, #38a169);
  color: white;
}

.api-button:hover:not(:disabled) {
  transform: translateY(-2px);
  box-shadow: 0 6px 20px rgba(0, 0, 0, 0.15);
}

.api-button:disabled {
  background: #e2e8f0;
  color: #a0aec0;
  cursor: not-allowed;
  transform: none;
  box-shadow: none;
}

.api-result,
.admin-data {
  background: linear-gradient(135deg, #f7fafc, #edf2f7);
  padding: 1rem;
  border-radius: 8px;
  border: 1px solid #e2e8f0;
  margin-top: 1rem;
}

.api-error {
  background: linear-gradient(135deg, #fed7d7, #feb2b2);
  color: #742a2a;
  padding: 1rem;
  border-radius: 8px;
  border: 1px solid #fc8181;
  margin-top: 1rem;
}

.profile-card {
  background: linear-gradient(135deg, #f7fafc, #edf2f7);
  padding: 1.5rem;
  border-radius: 8px;
  border: 1px solid #e2e8f0;
  margin-bottom: 1rem;
}

.profile-item {
  margin-bottom: 0.75rem;
  padding: 0.5rem 0;
  border-bottom: 1px solid #e2e8f0;
}

.profile-item:last-child {
  border-bottom: none;
  margin-bottom: 0;
}

.role-badge {
  padding: 0.25rem 0.75rem;
  border-radius: 15px;
  font-size: 0.75rem;
  font-weight: 600;
  color: white;
  text-transform: uppercase;
}

.role-badge.user {
  background: #48bb78;
}

.role-badge.admin {
  background: #e53e3e;
}

.profile-button {
  padding: 0.75rem 1.5rem;
  background: linear-gradient(135deg, #667eea, #764ba2);
  color: white;
  border: none;
  border-radius: 8px;
  cursor: pointer;
  font-weight: 500;
  transition: all 0.2s;
}

.profile-button:hover {
  transform: translateY(-1px);
  box-shadow: 0 4px 12px rgba(102, 126, 234, 0.4);
}

.admin-section {
  border-left: 4px solid #e53e3e;
  background: linear-gradient(135deg, #fff5f5, #fed7d7);
}

.admin-actions {
  display: flex;
  gap: 1rem;
  margin-bottom: 1rem;
  flex-wrap: wrap;
}

.admin-button {
  padding: 0.75rem 1.5rem;
  background: linear-gradient(135deg, #e53e3e, #c53030);
  color: white;
  border: none;
  border-radius: 8px;
  cursor: pointer;
  font-weight: 500;
  transition: all 0.2s;
}

.admin-button.secondary {
  background: linear-gradient(135deg, #718096, #4a5568);
}

.admin-button:hover {
  transform: translateY(-1px);
  box-shadow: 0 4px 12px rgba(229, 62, 62, 0.4);
}

.stats-section {
  background: linear-gradient(135deg, #667eea, #764ba2);
  color: white;
}

.stats-section h3 {
  color: white;
}

.stats-grid {
  display: grid;
  grid-template-columns: repeat(auto-fit, minmax(150px, 1fr));
  gap: 1rem;
}

.stat-card {
  background: rgba(255, 255, 255, 0.1);
  padding: 1.5rem;
  border-radius: 8px;
  text-align: center;
  backdrop-filter: blur(10px);
  border: 1px solid rgba(255, 255, 255, 0.2);
}

.stat-card.admin-stat {
  background: rgba(229, 62, 62, 0.2);
}

.stat-number {
  font-size: 2rem;
  font-weight: 700;
  margin-bottom: 0.5rem;
}

.stat-label {
  font-size: 0.875rem;
  opacity: 0.8;
  text-transform: uppercase;
  letter-spacing: 0.5px;
}

pre {
  white-space: pre-wrap;
  word-wrap: break-word;
  font-size: 0.875rem;
  line-height: 1.5;
  max-height: 300px;
  overflow-y: auto;
}

/* Mobile Responsive */
@media (max-width: 768px) {
  .dashboard-header {
    flex-direction: column;
    gap: 1rem;
    text-align: center;
  }

  .user-info {
    flex-direction: column;
    gap: 0.5rem;
  }

  .dashboard-content {
    padding: 1rem;
  }

  .api-buttons {
    grid-template-columns: 1fr;
  }

  .admin-actions {
    flex-direction: column;
  }

  .stats-grid {
    grid-template-columns: 1fr;
  }
}
</style>
