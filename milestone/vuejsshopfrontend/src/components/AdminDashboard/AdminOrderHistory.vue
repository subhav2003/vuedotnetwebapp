<template>
  <div class="bg-white shadow-lg rounded-lg p-6 w-full">
    <!-- Toast stack -->
    <Toast :toasts="toasts" @close="removeToast" />

    <!-- Header & Reload -->
    <div class="flex items-center justify-between mb-6">
      <h2 class="text-3xl font-semibold text-gray-800">All Orders</h2>
      <button
          @click="fetchOrders"
          class="flex items-center text-gray-600 hover:text-gray-800 transition"
      >
        <svg xmlns="http://www.w3.org/2000/svg" class="h-6 w-6 mr-1" fill="none" viewBox="0 0 24 24" stroke="currentColor">
          <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M4 4v5h.582m15.253 6A9 9 0 115.334 5.334M20 20v-5h-.581" />
        </svg>
        Reload
      </button>
    </div>

    <!-- Sort & Date Filters -->
    <div class="flex flex-col md:flex-row md:items-center gap-4 mb-6">
      <div class="flex items-center gap-2 w-full md:w-auto">
        <label class="font-semibold text-gray-700 whitespace-nowrap">Sort by:</label>
        <select v-model="sortBy" class="border border-gray-300 rounded-md px-3 py-2 focus:ring-2 focus:ring-gray-400 transition w-full md:w-auto">
          <option value="id">Order No.</option>
          <option value="total_price">Total</option>
          <option value="status">Status</option>
        </select>
        <button @click="toggleSortDir" class="px-2 py-1 border border-gray-300 rounded-md hover:bg-gray-100 transition w-full md:w-auto">
          {{ sortDir === 'asc' ? '↑' : '↓' }}
        </button>
      </div>
      <input v-model="filters.start_date" type="date" class="border border-gray-300 rounded-md px-4 py-2 focus:ring-2 focus:ring-gray-400 transition w-full md:w-auto" />
      <input v-model="filters.end_date" type="date" class="border border-gray-300 rounded-md px-4 py-2 focus:ring-2 focus:ring-gray-400 transition w-full md:w-auto" />
      <button @click="applyFilters" class="bg-gray-800 text-white px-6 py-2 rounded-md hover:bg-gray-900 transition w-full md:w-auto">
        Search
      </button>
    </div>

    <!-- Loading Indicator -->
    <div v-if="loading" class="flex items-center justify-center py-12">
      <svg class="animate-spin h-8 w-8 text-gray-700" fill="none" viewBox="0 0 24 24">
        <circle class="opacity-25" cx="12" cy="12" r="10" stroke="currentColor" stroke-width="4"></circle>
        <path class="opacity-75" fill="currentColor" d="M4 12a8 8 0 018-8v8H4z"></path>
      </svg>
      <span class="ml-3 text-gray-700 font-medium">Loading orders...</span>
    </div>

    <!-- Orders Table -->
    <div v-else class="overflow-x-auto">
      <table v-if="orders.length" class="min-w-full table-auto border-collapse bg-white rounded-md shadow-md">
        <thead class="bg-gray-100 text-gray-700 font-semibold">
        <tr>
          <th class="px-6 py-3 text-left">Order No.</th>
          <th class="px-6 py-3 text-left">Customer</th>
          <th class="px-6 py-3 text-left">Date</th>
          <th class="px-6 py-3 text-left">Total</th>
          <th class="px-6 py-3 text-left">Status</th>
          <th class="px-6 py-3 text-right"></th>
        </tr>
        </thead>
        <tbody class="divide-y divide-gray-200">
        <tr v-for="order in orders" :key="order.id" class="hover:bg-gray-50">
          <td class="px-6 py-4 font-semibold">{{ order.id }}</td>
          <td class="px-6 py-4 font-medium">{{ order.user?.name }}</td>
          <td class="px-6 py-4">{{ formatDate(order.created_at) }}</td>
          <td class="px-6 py-4 text-green-600 font-semibold">${{ formatPrice(order.total_price) }}</td>
          <td class="px-6 py-4 capitalize font-semibold" :class="statusClass(order.status)">
            {{ order.status }}
          </td>
          <td class="px-6 py-4 text-right">
            <button @click="viewOrder(order)" class="text-gray-600 hover:text-gray-800 font-medium">
              View
            </button>
          </td>
        </tr>
        </tbody>
      </table>
      <p v-else class="text-center text-gray-500 py-8">No orders found.</p>
    </div>

    <!-- Pagination -->
    <div v-if="totalPages > 1" class="mt-6 flex flex-col sm:flex-row justify-between items-center gap-2">
      <button @click="changePage(currentPage - 1)" :disabled="currentPage === 1" class="px-4 py-2 rounded-md bg-gray-800 text-white hover:bg-gray-900 disabled:opacity-50">
        Previous
      </button>
      <div class="flex gap-1">
        <button v-for="p in pageNumbers" :key="p" @click="changePage(p)" :class="pageClass(p)">
          {{ p }}
        </button>
      </div>
      <button @click="changePage(currentPage + 1)" :disabled="currentPage === totalPages" class="px-4 py-2 rounded-md bg-gray-800 text-white hover:bg-gray-900 disabled:opacity-50">
        Next
      </button>
    </div>

    <!-- Detail & Status Modal -->
    <div v-if="showModal" class="fixed inset-0 flex items-center justify-center bg-black bg-opacity-50 z-50">
      <div class="bg-white w-full max-w-2xl p-6 rounded-lg shadow-lg overflow-y-auto max-h-[90vh] relative">
        <button @click="closeModal" class="absolute top-2 right-2 text-gray-500 hover:text-gray-700">✖</button>
        <h3 class="text-2xl font-semibold mb-4">Order #{{ selectedOrder.id }}</h3>
        <div class="space-y-2">
          <p><strong>Customer:</strong> {{ selectedOrder.user.name }} ({{ selectedOrder.user.email }})</p>
          <p><strong>Date:</strong> {{ formatDate(selectedOrder.created_at) }}</p>
          <p><strong>Status:</strong> {{ selectedOrder.status }}</p>
          <p><strong>Payment Method:</strong> {{ selectedOrder.payment.payment_method }}</p>
          <p><strong>Payment Status:</strong> {{ selectedOrder.payment.status }}</p>
          <p><strong>Transaction ID:</strong> {{ selectedOrder.payment.transaction_id }}</p>
          <p><strong>Total Price:</strong> ${{ formatPrice(selectedOrder.total_price) }}</p>
          <p><strong>Shipping Address:</strong> {{ formatAddress(selectedOrder.address) }}</p>
        </div>
        <!-- Status Update -->
        <div class="mt-4 flex items-center gap-2">
          <label class="font-semibold">Change Status:</label>
          <select v-model="statusUpdate" class="border border-gray-300 rounded-md px-3 py-2 focus:ring-2 focus:ring-gray-400 transition">
            <option v-for="s in statusOptions" :key="s" :value="s">{{ s }}</option>
          </select>
          <button @click="updateStatus" class="bg-blue-600 text-white px-4 py-2 rounded-md hover:bg-blue-700 transition font-semibold">
            Update Status
          </button>
        </div>
        <h4 class="mt-6 font-semibold">Products:</h4>
        <ul class="list-disc list-inside space-y-1">
          <li v-for="prod in selectedOrder.products" :key="prod.id">
            {{ prod.name }} × {{ prod.pivot.quantity }} @ ${{ prod.pivot.price }} each
          </li>
        </ul>
      </div>
    </div>
  </div>
</template>

<script setup>
import { ref, reactive, computed, onMounted } from 'vue';
import axios from 'axios';
import Toast from '@/components/Toast.vue';

const ordersApi = `${process.env.VUE_APP_API_URL}/api/v1/orders`;
const adminStatusApi = `${process.env.VUE_APP_API_URL}/api/v1/admin/orders`;

const orders = ref([]);
const loading = ref(false);
const currentPage = ref(1);
const totalPages = ref(1);
const filters = ref({ start_date: '', end_date: '' });
const sortBy = ref('id');
const sortDir = ref('desc');

// Toast stack
const toasts = reactive([]);
function removeToast(id) {
  const idx = toasts.findIndex(t => t.id === id);
  if (idx !== -1) toasts.splice(idx, 1);
}
function showToast(message, type = 'success') {
  const id = Date.now() + Math.random();
  toasts.push({ id, message, type });
  setTimeout(() => removeToast(id), 5000);
}

const showModal = ref(false);
const selectedOrder = ref({});
const statusUpdate = ref('');
const statusOptions = [
  'pending',
  'in production',
  'shipped',
  'out for delivery',
  'delivered',
  'canceled',
  'returned'
];

const statusClass = s => {
  if (s === 'pending') return 'text-yellow-600';
  if (['in production', 'shipped'].includes(s)) return 'text-blue-600';
  if (s === 'out for delivery') return 'text-orange-500';
  if (s === 'delivered') return 'text-green-600';
  return 'text-red-600';
};

async function fetchOrders() {
  loading.value = true;
  try {
    const { data } = await axios.get(ordersApi, {
      params: {
        page: currentPage.value,
        per_page: 10,
        start_date: filters.value.start_date,
        end_date: filters.value.end_date,
        sort_by: sortBy.value,
        sort_dir: sortDir.value,
      },
    });
    orders.value = data.data.map(o => ({ ...o, total_price: parseFloat(o.total_price) }));
    totalPages.value = data.last_page;
  } catch (err) {
    console.error(err);
    showToast('Failed to fetch orders', 'error');
  } finally {
    loading.value = false;
  }
}

const applyFilters = () => { currentPage.value = 1; fetchOrders(); };
const toggleSortDir = () => { sortDir.value = sortDir.value === 'asc' ? 'desc' : 'asc'; fetchOrders(); };
const changePage = p => { if (p < 1 || p > totalPages.value) return; currentPage.value = p; fetchOrders(); };
const pageNumbers = computed(() => Array.from({ length: totalPages.value }, (_, i) => i + 1));
const pageClass = p =>
    p === currentPage.value
        ? 'bg-gray-800 text-white px-3 py-1 rounded'
        : 'bg-gray-200 text-gray-700 hover:bg-gray-300 px-3 py-1 rounded';
const formatDate = iso => new Date(iso).toLocaleDateString();
const formatPrice = a => (isNaN(a) ? '0.00' : a.toFixed(2));
const formatAddress = adr => `${adr.street}, ${adr.city}, ${adr.state} ${adr.postal_code}, ${adr.country}`;

function viewOrder(order) {
  selectedOrder.value = order;
  statusUpdate.value = order.status;
  showModal.value = true;
}

const closeModal = () => { showModal.value = false; };

async function updateStatus() {
  try {
    await axios.post(`${adminStatusApi}/${selectedOrder.value.id}/status`, { status: statusUpdate.value });
    selectedOrder.value.status = statusUpdate.value;
    const idx = orders.value.findIndex(o => o.id === selectedOrder.value.id);
    if (idx !== -1) orders.value[idx].status = statusUpdate.value;
    showToast('Order status updated', 'success');
  } catch (err) {
    console.error(err);
    showToast('Update failed', 'error');
  }
}

onMounted(fetchOrders);
</script>

<style scoped>
.fade-enter-from { opacity: 0; transform: translateY(-5px); }
.fade-enter-active { transition: all 0.3s ease; }
.fade-enter-to { opacity: 1; transform: translateY(0); }
</style>
