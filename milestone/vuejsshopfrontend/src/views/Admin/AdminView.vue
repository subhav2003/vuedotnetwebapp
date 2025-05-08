<template>
  <div class="min-h-screen bg-[#fdfaf6] font-sans text-[#472c20] px-6 py-8">
    <!-- Branding Header -->
    <div class="text-center mb-10">
      <h1 class="text-4xl md:text-5xl font-bold font-[Papyrus] tracking-wide">
        CraftConnect Admin
      </h1>
      <p class="text-sm text-gray-500 mt-2">Monitor and manage the marketplace</p>
    </div>

    <!-- Dashboard Grid -->
    <div class="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-4 gap-6">
      <DashboardCard title="Orders" icon="📦" @click="activePanel = 'orders'" />
      <DashboardCard title="Products" icon="🛍️" @click="activePanel = 'products'" />
      <DashboardCard title="Flagged Reviews" icon="🚩" @click="activePanel = 'flaggedReviews'" />
      <DashboardCard title="Flagged Products" icon="⚠️" @click="activePanel = 'flaggedProducts'" />
      <DashboardCard title="Flagged Artisans" icon="🔍" @click="activePanel = 'flaggedArtisans'" />
      <DashboardCard title="System Stats" icon="📊" @click="activePanel = 'systemStats'" />
      <!-- Logout Card -->
      <DashboardCard title="Logout" icon="🔒" @click="logout" />
    </div>

    <!-- SystemStats standalone overlay -->
    <SystemStats
        v-if="activePanel === 'systemStats'"
        @close="activePanel = null"
    />

    <!-- Other panels overlay -->
    <div
        v-else-if="activePanel"
        class="fixed inset-0 z-50 flex items-center justify-center bg-black bg-opacity-40 backdrop-blur-sm"
    >
      <div
          class="bg-white rounded-lg shadow-lg w-full max-w-6xl max-h-[90vh] overflow-y-auto relative flex flex-col animate-fade-in"
      >
        <button
            @click="activePanel = null"
            class="absolute top-4 right-4 text-gray-600 hover:text-red-600 text-2xl"
        >
          &times;
        </button>
        <h2 class="text-2xl font-semibold mb-4 capitalize px-6 pt-6">
          {{ panelTitle }}
        </h2>

        <div class="px-6 pb-6 space-y-6">
          <AdminOrderHistory
              v-if="activePanel === 'orders'"
              @view-order="openOrderDetail"
          />

          <div v-else-if="activePanel === 'products'">
            <AdminProductList
                @view-product="viewProduct"
                @edit-product="editProduct"
            />
            <div class="mt-6">
              <Category />
            </div>
          </div>

          <AdminFlaggedReviews v-else-if="activePanel === 'flaggedReviews'" />
          <AdminFlaggedProducts v-else-if="activePanel === 'flaggedProducts'" />
          <AdminFlaggedArtisans v-else-if="activePanel === 'flaggedArtisans'" />
        </div>
      </div>
    </div>

    <!-- Order Detail Overlay -->
    <AdminOrderDetails
        v-if="selectedOrder"
        :order="selectedOrder"
        @close="selectedOrder = null"
    />
  </div>
</template>

<script setup>
import { ref, computed } from 'vue'
import { useRouter } from 'vue-router'
import axios from 'axios'

import DashboardCard from '@/components/AdminDashboard/DashboardCard.vue'
import AdminOrderHistory from '@/components/AdminDashboard/AdminOrderHistory.vue'
import AdminOrderDetails from '@/components/AdminDashboard/AdminOrderDetails.vue'
import AdminProductList from '@/components/AdminDashboard/AdminProductList.vue'
import AdminFlaggedReviews from '@/components/AdminDashboard/AdminFlaggedReviews.vue'
import AdminFlaggedProducts from '@/components/AdminDashboard/AdminFlaggedProducts.vue'
import AdminFlaggedArtisans from '@/components/AdminDashboard/AdminFlaggedArtisans.vue'
import SystemStats from '@/components/AdminDashboard/SystemStats.vue'
import Category from '@/components/AdminDashboard/Category.vue'

const router = useRouter()
const activePanel = ref(null)
const selectedOrder = ref(null)

function openOrderDetail(order) {
  selectedOrder.value = order
}
function viewProduct(product) {
  // handle view
}
function editProduct(product) {
  // handle edit
}

const panelTitle = computed(() => {
  const map = {
    orders: 'Orders',
    products: 'Products',
    flaggedReviews: 'Flagged Reviews',
    flaggedProducts: 'Flagged Products',
    flaggedArtisans: 'Flagged Artisans',
    systemStats: 'System Stats',
  }
  return map[activePanel.value] || ''
})

async function logout() {
  try {
    const token = localStorage.getItem('authToken')
    await axios.post(
        `${process.env.VUE_APP_API_URL}/api/v1/logout`,
        {},
        { headers: { Authorization: `Bearer ${token}` } }
    )
  } catch {
    // ignore errors
  }
  localStorage.clear()
  router.push('/auth')
  window.location.reload()
}
</script>

<style scoped>
@keyframes fade-in {
  from { opacity: 0; transform: translateY(-10px); }
  to   { opacity: 1; transform: translateY(0); }
}
.animate-fade-in { animation: fade-in 0.4s ease-out; }
</style>
