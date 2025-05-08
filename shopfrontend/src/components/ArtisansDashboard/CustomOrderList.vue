<template>
  <div class="container mx-auto p-4 relative">
    <h1 class="text-2xl font-bold mb-4 text-center">Custom Orders</h1>

    <!-- Global Loader -->
    <div v-if="loading" class="flex justify-center items-center py-8">
      <div class="loader"></div>
      <span class="ml-2 text-gray-700">Loading orders...</span>
    </div>

    <!-- Error -->
    <div v-else-if="error" class="text-red-500 text-center">
      {{ error }}
    </div>

    <!-- Empty -->
    <div v-else-if="orders.length === 0" class="text-center text-gray-700">
      No custom orders found.
    </div>

    <!-- Grid of Cards -->
    <div v-else class="grid grid-cols-1 sm:grid-cols-2 lg:grid-cols-3 gap-6">
      <CustomOrderCard
          v-for="order in orders"
          :key="order.id"
          :order="order"
          :loading="loadingOrderIds.includes(order.id)"
          @accept="handleAccept"
          @decline="handleDecline"
          @chat="openChat"
          @finalize="openFinalizeModal"
          @view-detail="openDetailModal"
      />
    </div>

    <!-- Chat Modal -->
    <div
        v-if="activeChatOrderId"
        class="fixed inset-0 bg-black bg-opacity-60 z-50 flex justify-center items-center p-4 overflow-auto"
    >
      <div class="bg-white w-full max-w-2xl h-[90vh] rounded-lg shadow-lg overflow-hidden relative">
        <button
            @click="closeChat"
            class="absolute top-2 right-2 text-gray-600 hover:text-red-500 text-2xl font-bold z-10"
        >
          &times;
        </button>
        <ChatComponent :order-id="activeChatOrderId" />
      </div>
    </div>

    <!-- Finalize Modal -->
    <div
        v-if="activeFinalizeOrderId"
        class="fixed inset-0 bg-black bg-opacity-60 z-50 flex justify-center items-center p-4 overflow-auto"
    >
      <div class="bg-white w-full max-w-2xl rounded-lg shadow-lg overflow-hidden relative">
        <button
            @click="closeFinalizeModal"
            class="absolute top-2 right-2 text-gray-600 hover:text-red-500 text-2xl font-bold z-10"
        >
          &times;
        </button>
        <FinalizeForm
            :order-id="activeFinalizeOrderId"
            @close="closeFinalizeModal"
            @finalized="onFinalized"
        />
      </div>
    </div>

    <!-- Detail Modal -->
    <div
        v-if="activeDetailOrderId"
        class="fixed inset-0 bg-black bg-opacity-60 z-50 flex justify-center items-center p-4 overflow-auto"
    >
      <div class="bg-white w-full max-w-3xl rounded-lg shadow-lg overflow-hidden relative">
        <button
            @click="closeDetailModal"
            class="absolute top-2 right-2 text-gray-600 hover:text-red-500 text-2xl font-bold z-10"
        >
          &times;
        </button>
        <!-- Pass the full order object to your FinalizedOrder component -->
        <FinalizedOrder
            :order="detailOrder"
            @close="closeDetailModal"
        />
      </div>
    </div>
  </div>
</template>

<script setup>
import { ref, computed, onMounted } from 'vue'
import axios from 'axios'
import CustomOrderCard from '@/components/ArtisansDashboard/CustomOrderCard.vue'
import ChatComponent from '@/components/ChatComponent.vue'
import FinalizeForm from '@/components/ArtisansDashboard/FinalizeForm.vue'
import FinalizedOrder from '@/components/FinalizedOrder.vue'    // updated import

const apiBaseUrl = process.env.VUE_APP_API_URL
const ordersEndpoint = `${apiBaseUrl}/api/v1/artisan/custom-orders`

// state
const orders = ref([])
const loading = ref(true)
const error = ref('')
const loadingOrderIds = ref([])

// modal controls
const activeChatOrderId     = ref(null)
const activeFinalizeOrderId = ref(null)
const activeDetailOrderId   = ref(null)

// fetch list
const fetchOrders = async () => {
  loading.value = true
  error.value   = ''
  try {
    const token = localStorage.getItem('authToken')
    const resp  = await axios.get(ordersEndpoint, {
      headers: { Authorization: `Bearer ${token}` }
    })
    orders.value = resp.data.data
  } catch {
    error.value = 'Failed to fetch custom orders. Please try again later.'
  } finally {
    loading.value = false
  }
}

onMounted(fetchOrders)

// accept / decline with polling
const updateOrderStatus = async (orderId, targetStatus) => {
  const token = localStorage.getItem('authToken')
  try {
    loadingOrderIds.value.push(orderId)
    await axios.put(
        `${apiBaseUrl}/api/v1/artisan/custom-orders/${orderId}/status`,
        { status: targetStatus },
        { headers: { Authorization: `Bearer ${token}` } }
    )
    let attempts = 0
    const interval = setInterval(async () => {
      attempts++
      await fetchOrders()
      const ord = orders.value.find(o => o.id === orderId)
      if (ord?.status === targetStatus || attempts >= 10) {
        clearInterval(interval)
        loadingOrderIds.value = loadingOrderIds.value.filter(id => id !== orderId)
      }
    }, 2000)
  } catch {
    loadingOrderIds.value = loadingOrderIds.value.filter(id => id !== orderId)
  }
}

const handleAccept  = id => updateOrderStatus(id, 'accepted')
const handleDecline = id => updateOrderStatus(id, 'rejected')

// Chat
const openChat  = id => (activeChatOrderId.value = id)
const closeChat = () => (activeChatOrderId.value = null)

// Finalize
const openFinalizeModal  = id => (activeFinalizeOrderId.value = id)
const closeFinalizeModal = () => (activeFinalizeOrderId.value = null)
const onFinalized        = () => { closeFinalizeModal(); fetchOrders() }

// Detail
const openDetailModal  = id => (activeDetailOrderId.value = id)
const closeDetailModal = () => (activeDetailOrderId.value = null)

// detailOrder computed
const detailOrder = computed(() => {
  return orders.value.find(o => o.id === activeDetailOrderId.value) || null
})
</script>

<style scoped>
.loader {
  border: 4px solid #f3f3f3;
  border-top: 4px solid #3498db;
  border-radius: 50%;
  width: 24px;
  height: 24px;
  animation: spin 1s linear infinite;
}
@keyframes spin { to { transform: rotate(360deg); } }
</style>
