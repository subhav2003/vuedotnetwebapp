<template>
  <div class="min-h-screen flex flex-col bg-gray-900 text-[#C8B280]">
    <!-- Fixed Top Navbar -->
    

    <!-- Spacer for fixed navbar height -->
    <div class="h-16"></div>
    

    <!-- Admin Order History Section -->
    <main class="flex-grow container mx-auto px-6 py-8">
      <div class="flex justify-between items-center mb-6">
        <h2 class="text-3xl font-semibold">Staff Order Dashboard</h2>
        <button
            @click="logout"
            class="px-4 py-2 rounded bg-white text-black hover:bg-gray-200 transition duration-300"
        >
          Logout
        </button>
      </div>

      <AdminOrderHistory />
    </main>

  </div>
</template>

<script setup>

import AdminOrderHistory from '@/components/AdminDashboard/AdminOrderHistory.vue'
import {useRouter} from "vue-router";
const router = useRouter()

function logout() {
  
  localStorage.clear()
  router.push('/auth')
  window.location.reload()
}
</script>

<style scoped>
.bg-dark-blue {
  background-color: #2c3e50; /* Dark blue for the navbar */
}
</style>
