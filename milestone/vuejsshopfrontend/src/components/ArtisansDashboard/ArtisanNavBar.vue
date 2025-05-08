<template>
  <header
      class="fixed top-0 left-0 w-full h-16 bg-navy-900 text-white shadow z-50 flex items-center justify-between px-4"
  >
    <!-- Logo + Title -->
    <div class="flex items-center space-x-2">
      <img
          src="@/assets/pustakalaya-logo.png"
          alt="Craft Connect Logo"
          class="w-10 h-10 object-contain"
      />
      <span class="hidden md:inline-block text-lg font-bold">Dashboard</span>
    </div>

    <!-- Nav Links (Desktop) -->
    <nav class="hidden lg:flex space-x-4">
      <router-link
          to="/artisan"
          class="py-2 px-3 rounded hover:bg-white hover:text-black transition-all duration-300"
          active-class="bg-white text-black font-bold"
      >Home</router-link>
      <router-link
          to="/artisan/productmanagement"
          class="py-2 px-3 rounded hover:bg-white hover:text-black transition-all duration-300"
          active-class="bg-white text-black font-bold"
      >Products</router-link>
      <router-link
          to="/artisan/ordermanagement"
          class="py-2 px-3 rounded hover:bg-white hover:text-black transition-all duration-300"
          active-class="bg-white text-black font-bold"
      >Orders</router-link>
      <router-link
          to="/artisan/profilemanagement"
          class="py-2 px-3 rounded hover:bg-white hover:text-black transition-all duration-300"
          active-class="bg-white text-black font-bold"
      >Settings</router-link>
      <!-- Logout Button -->
      <button
          @click="logout"
          class="py-2 px-3 rounded hover:bg-white hover:text-black transition-all duration-300"
      >
        Logout
      </button>
    </nav>

    <!-- Hamburger Menu Button (Mobile) -->
    <button
        class="block lg:hidden p-2 text-white rounded hover:bg-navy-700"
        @click="isMenuOpen = !isMenuOpen"
    >
      <svg
          v-if="!isMenuOpen"
          xmlns="http://www.w3.org/2000/svg"
          fill="none"
          viewBox="0 0 24 24"
          stroke="currentColor"
          class="h-6 w-6"
      >
        <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M4 6h16M4 12h16M4 18h16" />
      </svg>
      <svg
          v-else
          xmlns="http://www.w3.org/2000/svg"
          fill="none"
          viewBox="0 0 24 24"
          stroke="currentColor"
          class="h-6 w-6"
      >
        <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M6 18L18 6M6 6l12 12" />
      </svg>
    </button>

    <!-- Mobile Menu -->
    <transition name="fade">
      <div
          v-if="isMenuOpen"
          class="absolute top-full left-0 w-full bg-navy-900 text-white lg:hidden"
      >
        <nav class="flex flex-col space-y-2 p-2">
          <router-link
              to="/artisan"
              class="py-2 px-3 rounded hover:bg-white hover:text-black transition-all duration-300"
              active-class="bg-white text-black font-bold"
              @click="isMenuOpen = false"
          >Home</router-link>
          <router-link
              to="/artisan/productmanagement"
              class="py-2 px-3 rounded hover:bg-white hover:text-black transition-all duration-300"
              active-class="bg-white text-black font-bold"
              @click="isMenuOpen = false"
          >Products</router-link>
          <router-link
              to="/artisan/ordermanagement"
              class="py-2 px-3 rounded hover:bg-white hover:text-black transition-all duration-300"
              active-class="bg-white text-black font-bold"
              @click="isMenuOpen = false"
          >Orders</router-link>
          <router-link
              to="/artisan/profilemanagement"
              class="py-2 px-3 rounded hover:bg-white hover:text-black transition-all duration-300"
              active-class="bg-white text-black font-bold"
              @click="isMenuOpen = false"
          >Settings</router-link>
          <!-- Logout Button -->
          <button
              @click="logout"
              class="w-full text-left py-2 px-3 rounded hover:bg-white hover:text-black transition-all duration-300"
          >
            Logout
          </button>
        </nav>
      </div>
    </transition>
  </header>
</template>

<script setup>
import { ref } from 'vue'
import { useRouter } from 'vue-router'
import axios from 'axios'

const isMenuOpen = ref(false)
const router = useRouter()

async function logout() {
  try {
    const token = localStorage.getItem('authToken')
    await axios.post(
        `${import.meta.env.VUE_APP_API_URL}/api/v1/logout`,
        {},
        { headers: { Authorization: `Bearer ${token}` } }
    )
  } catch {
    // ignore
  }
  localStorage.clear()
  router.push('/auth')
  window.location.reload()
}
</script>

<style scoped>
.bg-navy-900 {
  background-color: #1a1f36;
}

.fade-enter-active,
.fade-leave-active {
  transition: opacity 0.2s;
}
.fade-enter,
.fade-leave-to {
  opacity: 0;
}
</style>
