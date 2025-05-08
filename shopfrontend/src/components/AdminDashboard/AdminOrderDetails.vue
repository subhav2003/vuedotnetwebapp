<template>
  <div class="bg-white p-6 rounded-lg shadow-lg w-[90%] sm:w-[500px] relative z-50">
<!--    &lt;!&ndash; Close Button &ndash;&gt;-->
<!--    <div class="absolute top-0 right-0 p-2">-->
<!--      <button  class="text-2xl text-gray-500 hover:text-gray-700">&times;</button>-->
<!--    </div>-->
<!--    &lt;!&ndash; Close Button &ndash;&gt;-->
    <button
        @click="closeOrderDetails"
        class="absolute top-4 right-4 text-gray-500 hover:text-red-600 transition-colors"
    >
      <svg xmlns="http://www.w3.org/2000/svg" class="h-6 w-6" fill="none"
           viewBox="0 0 24 24" stroke="currentColor">
        <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2"
              d="M6 18L18 6M6 6l12 12" />
      </svg>
    </button>

    <!-- Order Info -->
    <h2 class="text-xl font-semibold text-brown-700 mb-4">Order #{{ order?.id }}</h2>
    <p><strong>Date:</strong> {{ formatDate(order?.created_at) }}</p>

    <!-- Status Dropdown -->
    <div class="flex items-center mt-2">
      <label class="font-semibold mr-2">Status:</label>
      <select
          v-model="selectedStatus"
          class="border border-gray-300 rounded-md px-3 py-1 text-sm focus:outline-none focus:ring focus:ring-brown-400"
      >
        <option v-for="status in statusOptions" :key="status" :value="status">{{ status }}</option>
      </select>

      <button
          @click="updateOrderStatus"
          :disabled="loading"
          class="ml-4 bg-brown-600 hover:bg-brown-700 text-white font-semibold text-sm px-4 py-1.5 rounded transition"
      >
        <span v-if="loading">Updating...</span>
        <span v-else>Update</span>
      </button>
    </div>

    <!-- Feedback Message -->
    <p v-if="message" class="mt-2 text-sm font-semibold text-green-600">
      {{ message }}
    </p>

    <p class="mt-2"><strong>Payment Method:</strong> {{ order?.payment_method }}</p>

    <!-- Order Progress Bar -->
    <div class="mt-6">
      <div class="flex justify-between items-center text-gray-600 text-sm font-semibold">
        <p>Order Placed</p>
        <p>In Production</p>
        <p>Shipped</p>
        <p>Out for Delivery</p>
        <p>Delivered</p>
      </div>
      <div class="flex items-center mt-2">
        <div v-for="(step, index) in progressSteps" :key="index" class="flex items-center">
          <div class="w-8 h-8 flex items-center justify-center rounded-full"
               :class="step.completed ? 'bg-green-600 text-white' : 'bg-gray-300'">
            {{ step.icon }}
          </div>
          <div v-if="index < progressSteps.length - 1" class="w-12 h-1"
               :class="step.completed ? 'bg-green-600' : 'bg-gray-300'"></div>
        </div>
      </div>
    </div>

    <!-- Product List -->
    <h3 class="mt-6 text-lg font-semibold">Products:</h3>
    <ul class="list-none">
      <li v-for="product in order?.products" :key="product.id" class="flex items-center space-x-3 py-2">
        <img :src="product.images?.[0] || '/placeholder.jpg'" alt="Product Image" class="w-12 h-12 rounded shadow object-cover" />
        <div>
          <p class="font-medium">{{ product.name }}</p>
          <p class="text-gray-600">${{ product.price }} (Qty: {{ product.quantity }})</p>
        </div>
      </li>
    </ul>
  </div>
</template>

<script setup>
import { ref, computed } from 'vue'
import axios from 'axios'

const props = defineProps({
  order: Object,
})

const emit = defineEmits(['close', 'status-updated'])

const apiBaseUrl = process.env.VUE_APP_API_URL
const selectedStatus = ref(props.order.status)
const message = ref('')
const loading = ref(false)

const statusOptions = [
  'pending',
  'in production',
  'shipped',
  'out for delivery',
  'delivered',
  'canceled'
]

const formatDate = (dateString) => {
  return dateString ? new Date(dateString).toLocaleDateString() : ''
}

const progressSteps = computed(() => {
  const statusMap = ['pending', 'in production', 'shipped', 'out for delivery', 'delivered']
  return statusMap.map((status, index) => ({
    label: status,
    icon: index + 1,
    completed: statusMap.indexOf(selectedStatus.value) >= index,
  }))
})

const updateOrderStatus = async () => {
  loading.value = true
  message.value = ''
  try {
    const token = localStorage.getItem('authToken')
    const response = await axios.post(`${apiBaseUrl}/api/v1/admin/orders/${props.order.id}/status`, {
      status: selectedStatus.value,
    }, {
      headers: { Authorization: `Bearer ${token}` }
    })

    message.value = response.data.message || 'Order status updated!'
    emit('status-updated', props.order.id, selectedStatus.value)

  } catch (err) {
    message.value = err?.response?.data?.message || 'Something went wrong.'
  } finally {
    loading.value = false
  }
}

const closeOrderDetails = () => {
  emit('close')
  document.body.classList.remove('no-scroll')
}
</script>

<style scoped>
.no-scroll {
  overflow: hidden;
}
</style>
