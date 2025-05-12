<template>
  <div class="min-h-screen flex flex-col bg-gray-900 text-[#C8B280]">
    <!-- Fixed Top Navbar -->
    <header class="fixed top-0 left-0 w-full bg-dark-blue text-white z-50">
      <ArtisanNavBar />
    </header>

    <!-- Spacer for fixed navbar height -->
    <div class="h-16"></div>

    <!-- Admin Order History Section -->
    <main class="flex-grow container mx-auto px-6 py-8">
      <h2 class="text-3xl font-semibold mb-6 f text-center">All Orders</h2>
      <AdminOrderHistory />
    </main>
  </div>
</template>

<script setup>
import ArtisanNavBar from '@/components/ArtisansDashboard/ArtisanNavBar.vue'
import AdminOrderHistory from '@/components/AdminDashboard/AdminOrderHistory.vue'
</script>

<style scoped>
.bg-dark-blue {
  background-color: #2c3e50; /* Dark blue for the navbar */
}
</style>
