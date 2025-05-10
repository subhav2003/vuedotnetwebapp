<template>
  <div class="w-full bg-[#0f0f0f] text-[#C8B280] rounded-xl shadow-md border border-[#333]">
    <!-- Header -->
    <div class="flex items-center justify-between px-6 pt-6">
      <h2 class="text-xl font-semibold text-[#F5DEB3]">Order History</h2>
      <button
          @click="fetchOrders"
          class="flex items-center text-sm text-[#C8B280] hover:text-white transition"
      >
        <svg xmlns="http://www.w3.org/2000/svg" class="h-4 w-4 mr-1" viewBox="0 0 24 24" stroke="currentColor" fill="none">
          <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M4 4v5h.582m15.253 6A9 9 0 115.334 5.334M20 20v-5h-.581" />
        </svg>
        Reload
      </button>
    </div>

    <!-- Table -->
    <div class="overflow-x-auto mt-4">
      <table v-if="orders.length" class="min-w-full table-auto text-sm">
        <thead class="bg-[#1a1a1a] border-b border-[#333] text-[#F5DEB3]">
        <tr>
          <th class="text-left px-6 py-3 font-semibold">Order ID</th>
          <th class="text-left px-6 py-3 font-semibold">Date</th>
          <th class="text-left px-6 py-3 font-semibold">Total</th>
          <th class="text-left px-6 py-3 font-semibold">Status</th>
          <th class="text-left px-6 py-3 font-semibold">Paid</th>
          <th class="text-left px-6 py-3"></th>
        </tr>
        </thead>
        <tbody class="divide-y divide-[#333]">
        <tr v-for="order in orders" :key="order.id" class="hover:bg-[#1d1d1d]">
          <td class="px-6 py-4 font-semibold">#{{ order.id }}</td>
          <td class="px-6 py-4">{{ formatDate(order.orderDate) }}</td>
          <td class="px-6 py-4 text-green-400 font-semibold">${{ order.totalPrice.toFixed(2) }}</td>
          <td
              class="px-6 py-4 capitalize"
              :class="{
                'text-yellow-400': order.orderStatus === 'pending',
                'text-green-500': order.orderStatus === 'completed',
                'text-red-500': order.orderStatus === 'cancelled'
              }"
          >
            {{ order.orderStatus }}
          </td>
          <td class="px-6 py-4">{{ order.isPaid ? 'Yes' : 'No' }}</td>
          <td class="px-6 py-4 text-right">
            <button
                @click="viewOrder(order)"
                class="text-[#C8B280] hover:text-white text-sm transition"
            >
              View
            </button>
          </td>
        </tr>
        </tbody>
      </table>

      <p v-else class="text-center text-[#888] py-10">No orders found.</p>
    </div>
  </div>
</template>

<script setup>
import { ref, onMounted, defineEmits } from 'vue';
import axios from 'axios';

const emit = defineEmits(['view-order']);
const baseUrl = process.env.VUE_APP_API_URL || window.location.origin;

const orders = ref([]);
const loading = ref(false);

function formatDate(dateStr) {
  return new Date(dateStr).toLocaleDateString();
}

function viewOrder(order) {
  emit('view-order', order);
}

async function fetchOrders() {
  loading.value = true;
  try {
    const res = await axios.get(`${baseUrl}/api/order/my`, {
      headers: {
        Authorization: `Bearer ${localStorage.getItem('token')}`
      }
    });
    orders.value = res.data;
  } catch (err) {
    console.error('Failed to fetch orders:', err);
  } finally {
    loading.value = false;
  }
}

onMounted(fetchOrders);
</script>

<style scoped>
th,
td {
  white-space: nowrap;
}
</style>
