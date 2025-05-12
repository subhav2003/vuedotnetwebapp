<template>
  <div class="w-full bg-[#0f0f0f] text-[#C8B280] min-h-screen p-6 rounded-xl shadow-md border border-[#333] font-sans">
    <Toast :toasts="toasts" @close="removeToast" />

    <!-- Header -->
    <div class="flex justify-between items-center mb-6">
      <h2 class="text-3xl font-semibold text-[#F5DEB3] ">Order History</h2>
      <button @click="fetchOrders" class="flex items-center text-sm text-[#C8B280] hover:text-white transition">
        <svg xmlns="http://www.w3.org/2000/svg" class="h-5 w-5 mr-1" fill="none" viewBox="0 0 24 24" stroke="currentColor">
          <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M4 4v5h.582m15.253 6A9 9 0 115.334 5.334M20 20v-5h-.581" />
        </svg>
        Reload
      </button>
    </div>

    <!-- Orders Table -->
    <div v-if="loading" class="text-center py-8 text-[#999]">Loading orders...</div>
    <div v-else class="overflow-x-auto">
      <table v-if="orders.length" class="min-w-full text-sm table-auto">
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
          <td class="px-6 py-4 text-green-400 font-semibold">${{ formatPrice(order.totalPrice) }}</td>
          <td class="px-6 py-4 capitalize" :class="statusClass(order.orderStatus)">
            {{ order.orderStatus }}
          </td>
          <td class="px-6 py-4">{{ order.isPaid ? 'Yes' : 'No' }}</td>
          <td class="px-6 py-4 text-right">
            <button @click="viewOrder(order)" class="text-[#C8B280] hover:text-white text-sm transition">View</button>
          </td>
        </tr>
        </tbody>
      </table>
      <p v-else class="text-center text-[#999] py-10">No orders found.</p>
    </div>

    <!-- Order Modal -->
    <div v-if="showModal" class="fixed inset-0 flex items-center justify-center bg-black bg-opacity-60 z-50">
      <div class="bg-[#1a1a1a] text-[#C8B280] w-full max-w-2xl p-6 rounded-lg shadow-lg max-h-[90vh] overflow-y-auto relative">
        <button @click="closeModal" class="absolute top-2 right-2 text-[#888] hover:text-red-500 text-xl">&times;</button>
        <h3 class="text-2xl font-semibold mb-4 text-[#F5DEB3]">Order #{{ selectedOrder.id }}</h3>

        <div class="space-y-2 text-sm">
          <p><strong>Status:</strong> {{ selectedOrder.orderStatus }}</p>
          <p><strong>Total Price:</strong> ${{ formatPrice(selectedOrder.totalPrice) }}</p>
          <p><strong>Discount:</strong> ${{ formatPrice(selectedOrder.discountAmount) }}
            <span v-if="selectedOrder.appliedDiscounts">({{ selectedOrder.appliedDiscounts }})</span></p>
          <p><strong>Paid:</strong> {{ selectedOrder.isPaid ? 'Yes' : 'No' }}</p>
          <p><strong>Order Date:</strong> {{ formatDate(selectedOrder.orderDate) }}</p>
          <p><strong>Pickup Deadline:</strong> {{ formatDate(selectedOrder.pickupDeadline) }}</p>
        </div>

        <!-- Book Items -->
        <div class="mt-4">
          <h4 class="font-semibold text-lg mb-2 text-[#F5DEB3]">Ordered Books:</h4>
          <ul class="space-y-2 text-sm">
            <li
                v-for="item in selectedOrder.items"
                :key="item.bookId"
                class="border border-[#333] rounded-md p-3 bg-[#2a2a2a] flex gap-4 items-center"
            >
              <img
                  :src="getImageUrl(item.image)"
                  alt="Book Cover"
                  class="w-16 h-20 object-cover rounded shadow"
              />
              <div>
                <p class="font-bold text-[#F5DEB3]">{{ item.title }}</p>
                <p><strong>ID:</strong> {{ item.bookId }}</p>
                <p><strong>Qty:</strong> {{ item.quantity }}</p>
                <p><strong>Unit Price:</strong> ${{ formatPrice(item.unitPrice) }}</p>
                <p v-if="item.discountApplied"><strong>Discount:</strong> ${{ formatPrice(item.discountApplied) }}</p>
              </div>
            </li>
          </ul>
        </div>

        <!-- Claim Order -->
        <div class="mt-6 border-t border-[#333] pt-4">
          <h4 class="font-semibold text-lg mb-2 text-[#F5DEB3]">Claim This Order</h4>
          <form @submit.prevent="submitClaimCode" class="flex gap-3">
            <input
                v-model="claimCodeInput"
                type="text"
                placeholder="Enter Claim Code"
                class="flex-1 p-2 rounded-lg bg-[#181818] text-[#C8B280] border border-[#3a3a3a] focus:outline-none focus:ring-2 focus:ring-[#C8B280]"
                required
            />
            <button
                type="submit"
                class="bg-[#A47148] hover:bg-[#FFD700] text-black px-4 py-2 rounded-lg transition"
            >
              Submit
            </button>
          </form>
        </div>
      </div>
    </div>
  </div>
</template>

<script setup>
import { ref, reactive, onMounted } from 'vue';
import axios from 'axios';
import Toast from '@/components/Toast.vue';

const orders = ref([]);
const selectedOrder = ref({});
const showModal = ref(false);
const loading = ref(true);
const claimCodeInput = ref('');
const toasts = reactive([]);

const apiUrl = `${process.env.VUE_APP_API_URL}/api/order`;
const claimUrl = `${apiUrl}/claim`;

function formatDate(date) {
  return date ? new Date(date).toLocaleDateString() : '—';
}
function formatPrice(val) {
  return (parseFloat(val) || 0).toFixed(2);
}
function statusClass(status) {
  return {
    'text-yellow-400': status === 'pending',
    'text-green-500': status === 'delivered',
    'text-red-500': status === 'cancelled',
    'text-blue-400': status === 'claimed'
  };
}
function getImageUrl(path) {
  return `${process.env.VUE_APP_API_URL}${path}`;
}
function showToast(message, type = 'success') {
  const id = Date.now() + Math.random();
  toasts.push({ id, message, type });
  setTimeout(() => removeToast(id), 5000);
}
function removeToast(id) {
  const index = toasts.findIndex(t => t.id === id);
  if (index !== -1) toasts.splice(index, 1);
}

async function fetchOrders() {
  loading.value = true;
  try {
    const res = await axios.get(apiUrl, {
      headers: { Authorization: `Bearer ${localStorage.getItem('authToken')}` }
    });
    orders.value = res.data;
  } catch {
    showToast('Failed to load orders', 'error');
  } finally {
    loading.value = false;
  }
}

async function viewOrder(order) {
  try {
    const res = await axios.get(`${apiUrl}/${order.id}`, {
      headers: { Authorization: `Bearer ${localStorage.getItem('authToken')}` }
    });
    selectedOrder.value = res.data;
    claimCodeInput.value = '';
    showModal.value = true;
  } catch {
    showToast('Failed to load order detail', 'error');
  }
}

function closeModal() {
  showModal.value = false;
}

async function submitClaimCode() {
  const code = claimCodeInput.value.trim();
  if (!code) return showToast('Enter a valid claim code.', 'error');

  try {
    const res = await axios.put(`${claimUrl}/${code}`, null, {
      headers: { Authorization: `Bearer ${localStorage.getItem('authToken')}` }
    });
    showToast(`Order #${res.data.id} successfully claimed.`, 'success');
    showModal.value = false;
    await fetchOrders();
  } catch (err) {
    const msg = err.response?.data || 'Invalid claim code or already claimed.';
    showToast(typeof msg === 'string' ? msg : msg.error || 'Claim failed.', 'error');
  }
}

onMounted(fetchOrders);
</script>

<style scoped>
th, td {
  white-space: nowrap;
}
</style>
