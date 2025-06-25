import { createRouter, createWebHistory } from 'vue-router'
import LoginForm from '@/components/LoginForm.vue'
import Dashboard from '@/components/Dashboard.vue'
import RegisterForm from '@/components/RegisterForm.vue'

const router = createRouter({
  history: createWebHistory(import.meta.env.BASE_URL),
  routes: [
    {
      path: '/',
      redirect: '/login',
    },
    {
      path: '/login',
      name: 'Login',
      component: LoginForm,
    },
    {
      path: '/register',
      name: 'Register',
      component: RegisterForm,
    },
    {
      path: '/dashboard',
      name: 'Dashboard',
      component: Dashboard,
      meta: { requiresAuth: true },
    },
    {
      path: '/admin',
      name: 'Admin',
      component: Dashboard,
      meta: { requiresAuth: true, requiresAdmin: true },
    },
  ],
})

// Navigation Guards
router.beforeEach((to, from, next) => {
  const token = localStorage.getItem('authToken')
  const user = localStorage.getItem('user')
  const isAuthenticated = !!(token && user)

  if (to.meta.requiresAuth && !isAuthenticated) {
    next('/login')
    return
  }

  if (to.meta.requiresAdmin && isAuthenticated) {
    const userData = JSON.parse(user)
    if (userData.role !== 'Admin' && userData.role !== 'SuperAdmin') {
      next('/dashboard')
      return
    }
  }

  if ((to.path === '/login' || to.path === '/register') && isAuthenticated) {
    next('/dashboard')
    return
  }

  next()
})

export default router
