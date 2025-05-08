<template>
  <div class="w-72 bg-white rounded-lg shadow-lg overflow-hidden">
    <div class="bg-brown-100 text-brown-800 px-4 py-2 font-semibold text-sm border-b">Messages</div>

    <div v-if="loading" class="p-4 text-center text-sm text-gray-600">
      Loading chats...
    </div>

    <div v-else-if="error" class="p-4 text-center text-red-500 text-sm">
      {{ error }}
    </div>

    <div v-else-if="orders.length === 0" class="p-4 text-center text-sm text-gray-500">
      No active chats yet.
    </div>

    <ul v-else class="divide-y">
      <li
          v-for="order in orders"
          :key="order.id"
          class="flex items-start px-4 py-3 hover:bg-brown-50 transition cursor-pointer"
          @click="$emit('open-chat', order.id)"
      >
        <div class="flex-1">
          <div class="font-medium text-sm text-brown-800 truncate">Order #{{ order.id }}</div>
          <div class="text-xs text-gray-500 truncate">{{ order.description || 'No description' }}</div>
        </div>
        <div class="ml-2 text-xs text-gray-400">
          {{ formatDate(order.updated_at) }}
        </div>
      </li>
    </ul>
  </div>
</template>

<script setup>
import { ref, onMounted } from 'vue'
import axios from 'axios'

const apiUrl = process.env.VUE_APP_API_URL || 'http://127.0.0.1:8000'
const orders = ref([])
const loading = ref(true)
const error = ref('')

const fetchAcceptedOrders = async () => {
  const token = localStorage.getItem('authToken')
  try {
    const response = await axios.get(`${apiUrl}/api/v1/accepted-custom-orders`, {
      headers: { Authorization: `Bearer ${token}` }
    })
    orders.value = response.data.data
  } catch (err) {
    error.value = 'Failed to load chats.'
    console.error(err)
  } finally {
    loading.value = false
  }
}

const formatDate = (dateStr) => {
  const date = new Date(dateStr)
  return date.toLocaleDateString()
}

onMounted(fetchAcceptedOrders)
</script>
