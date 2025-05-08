<template>
  <div class="fixed inset-0 z-50 flex items-center justify-center">
    <div class="absolute inset-0 bg-black bg-opacity-60"></div>
    <div class="relative bg-white w-11/12 max-w-3xl mx-auto p-6 rounded-2xl shadow-xl overflow-y-auto max-h-[90vh]">
      <!-- Close Button -->
      <button @click="$emit('close')" class="absolute top-4 right-4 text-gray-500 hover:text-gray-700 text-2xl">&times;</button>

      <!-- Header with plain reload -->
      <div class="flex justify-between items-center mb-6 py-2">
        <h2 class="text-3xl font-extrabold text-gray-800">✨ Admin Statistics ✨</h2>
        <button
            @click="reloadStats"
            :disabled="loading"
            class="flex items-center text-gray-600 hover:text-gray-800 transition"
        >
          <!-- spinner when loading -->
          <svg
              v-if="loading"
              class="animate-spin h-5 w-5 mr-2 text-gray-600"
              xmlns="http://www.w3.org/2000/svg"
              fill="none"
              viewBox="0 0 24 24"
          >
            <circle class="opacity-25" cx="12" cy="12" r="10" stroke="currentColor" stroke-width="4"/>
            <path class="opacity-75" fill="currentColor" d="M4 12a8 8 0 018-8v4l3-3-3-3v4a8 8 0 00-8 8z"/>
          </svg>

          <!-- curved reload icon when idle -->
          <svg
              v-else
              xmlns="http://www.w3.org/2000/svg"
              class="h-6 w-6 mr-2 text-gray-600"
              fill="none"
              viewBox="0 0 24 24"
              stroke="currentColor"
          >
            <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2"
                  d="M4 4v5h.582m15.253 6A9 9 0 115.334 5.334M20 20v-5h-.581" />
          </svg>

          Reload
        </button>
      </div>

      <!-- Loading Spinner -->
      <div v-if="loading" class="flex justify-center my-8">
        <div class="animate-spin rounded-full h-12 w-12 border-t-4 border-blue-500"></div>
      </div>

      <!-- Stats Grid -->
      <div v-else class="grid grid-cols-1 sm:grid-cols-2 lg:grid-cols-4 gap-6 mb-8">
        <!-- Users -->
        <div class="card bg-gradient-to-r from-indigo-200 to-indigo-400 text-indigo-900">
          <p class="text-sm uppercase tracking-wide">Total Users</p>
          <p class="text-3xl font-bold mt-2">{{ stats.users.total }}</p>
          <p class="mt-1 text-sm">Admins: {{ stats.users.admins }} | Artisans: {{ stats.users.artisans }} | Customers: {{ stats.users.customers }}</p>
        </div>
        <!-- Products -->
        <div class="card bg-gradient-to-r from-green-200 to-green-400 text-green-900">
          <p class="text-sm uppercase tracking-wide">Products</p>
          <p class="text-3xl font-bold mt-2">{{ stats.products.total }}</p>
          <p class="mt-1 text-sm">Flagged: {{ stats.products.flagged }}</p>
        </div>
        <!-- Orders -->
        <div class="card bg-gradient-to-r from-blue-200 to-blue-400 text-blue-900">
          <p class="text-sm uppercase tracking-wide">Orders</p>
          <p class="text-3xl font-bold mt-2">{{ stats.orders.total }}</p>
          <p class="mt-1 text-sm">Delivered: {{ stats.orders.delivered }} | Pending: {{ stats.orders.pending }}</p>
        </div>
        <!-- Revenue -->
        <div class="card bg-gradient-to-r from-yellow-200 to-yellow-400 text-yellow-900">
          <p class="text-sm uppercase tracking-wide">Total Revenue</p>
          <p class="text-3xl font-bold mt-2">${{ stats.orders.total_revenue.toFixed(2) }}</p>
          <p class="mt-1 text-sm">Avg Order: ${{ stats.orders.avg_order_value.toFixed(2) }}</p>
        </div>
        <!-- Custom Orders -->
        <div class="card bg-gradient-to-r from-purple-200 to-purple-400 text-purple-900">
          <p class="text-sm uppercase tracking-wide">Custom Orders</p>
          <p class="text-3xl font-bold mt-2">{{ stats.custom_orders.total }}</p>
          <p class="mt-1 text-sm">Revenue: ${{ stats.custom_orders.revenue.toFixed(2) }}</p>
        </div>
        <!-- Reviews -->
        <div class="card bg-gradient-to-r from-pink-200 to-pink-400 text-pink-900">
          <p class="text-sm uppercase tracking-wide">Reviews</p>
          <p class="text-3xl font-bold mt-2">{{ stats.reviews.total }}</p>
          <p class="mt-1 text-sm">Flagged: {{ stats.reviews.flagged }}</p>
        </div>
        <!-- Notifications -->
        <div class="card bg-gradient-to-r from-red-200 to-red-400 text-red-900">
          <p class="text-sm uppercase tracking-wide">Notifications</p>
          <p class="text-3xl font-bold mt-2">{{ stats.notifications.total }}</p>
          <p class="mt-1 text-sm">Unread: {{ stats.notifications.unread }}</p>
        </div>
        <!-- Cart Items -->
        <div class="card bg-gradient-to-r from-teal-200 to-teal-400 text-teal-900">
          <p class="text-sm uppercase tracking-wide">Cart Items</p>
          <p class="text-3xl font-bold mt-2">{{ stats.cart.total_items }}</p>
        </div>
      </div>

      <!-- Charts Section -->
      <div class="grid grid-cols-1 lg:grid-cols-2 gap-6">
        <!-- Roles Pie Chart -->
        <div class="card bg-white p-6 rounded-2xl shadow-md">
          <h3 class="text-lg font-semibold mb-4">User Roles Distribution</h3>
          <canvas v-if="!loading" ref="rolesPieCanvas"></canvas>
        </div>
        <!-- Category Bar Chart -->
        <div class="card bg-white p-6 rounded-2xl shadow-md">
          <h3 class="text-lg font-semibold mb-4">Products by Category</h3>
          <canvas v-if="!loading" ref="categoryBarCanvas"></canvas>
        </div>
      </div>
    </div>
  </div>
</template>

<script setup>
import { ref, onMounted, onBeforeUnmount, nextTick } from 'vue';
import axios from 'axios';
import { Chart, registerables } from 'chart.js';

Chart.register(...registerables);
const apiBaseUrl = process.env.VUE_APP_API_URL;

const loading = ref(true);
const stats = ref({
  users: { total: 0, admins: 0, artisans: 0, customers: 0 },
  products: { total: 0, flagged: 0 },
  orders: { total: 0, delivered: 0, pending: 0, total_revenue: 0, avg_order_value: 0 },
  custom_orders: { total: 0, revenue: 0 },
  reviews: { total: 0, flagged: 0 },
  notifications: { total: 0, unread: 0 },
  cart: { total_items: 0 },
  categories: { per_category: [] }
});

const rolesPieCanvas = ref(null);
const categoryBarCanvas = ref(null);
let rolesChart = null;
let categoryChart = null;

async function fetchStats() {
  loading.value = true;
  rolesChart?.destroy();
  categoryChart?.destroy();
  try {
    const res = await axios.get(`${apiBaseUrl}/api/v1/admin/stats`);
    stats.value = res.data.data;
    loading.value = false;
    await nextTick();
    initRolesPieChart();
    initCategoryBarChart();
  } catch (e) {
    console.error(e);
    loading.value = false;
  }
}

function reloadStats() {
  fetchStats();
}

onMounted(() => {
  fetchStats();
});

onBeforeUnmount(() => {
  rolesChart?.destroy();
  categoryChart?.destroy();
});

function initRolesPieChart() {
  const ctx = rolesPieCanvas.value.getContext('2d');
  const { admins, artisans, customers } = stats.value.users;
  rolesChart = new Chart(ctx, {
    type: 'pie',
    data: {
      labels: ['Admins', 'Artisans', 'Customers'],
      datasets: [{ data: [admins, artisans, customers], backgroundColor: ['#6366F1', '#10B981', '#3B82F6'], borderWidth: 2 }]
    },
    options: { responsive: true, plugins: { legend: { position: 'bottom' } } }
  });
}

function initCategoryBarChart() {
  const ctx = categoryBarCanvas.value.getContext('2d');
  const labels = stats.value.categories.per_category.map(c => c.category);
  const data = stats.value.categories.per_category.map(c => c.product_count);
  categoryChart = new Chart(ctx, {
    type: 'bar',
    data: {
      labels,
      datasets: [{ label: 'Products', data, borderWidth: 1 }]
    },
    options: { responsive: true, scales: { y: { beginAtZero: true } } }
  });
}
</script>

<style scoped>
.card { @apply p-6 rounded-2xl shadow-md transition hover:-translate-y-1 }
</style>