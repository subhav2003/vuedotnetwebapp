<template>
  <div class="min-h-screen pt-16 bg-gradient-to-b from-gray-50 to-gray-100">
    <ArtisanNavBar />
    <div class="container mx-auto px-4 py-8">
      <h1 class="text-4xl font-extrabold mb-8 text-gray-800 animate-fade-in">✨ Artisan Dashboard ✨</h1>

      <!-- Stats Cards -->
      <div v-if="loading" class="flex justify-center my-8">
        <div class="animate-spin rounded-full h-12 w-12 border-t-4 border-blue-500"></div>
      </div>
      <div v-else class="grid grid-cols-1 sm:grid-cols-2 lg:grid-cols-4 gap-6 mb-12">
        <div class="card bg-gradient-to-r from-green-200 to-green-400 text-green-900">
          <p class="text-sm uppercase tracking-wide">Products Sold</p>
          <p class="text-3xl font-bold mt-2">{{ stats.productsSold }}</p>
        </div>
        <div class="card bg-gradient-to-r from-blue-200 to-blue-400 text-blue-900">
          <p class="text-sm uppercase tracking-wide">Product Revenue</p>
          <p class="text-3xl font-bold mt-2">${{ stats.productRevenue.toFixed(2) }}</p>
        </div>
        <div class="card bg-gradient-to-r from-purple-200 to-purple-400 text-purple-900">
          <p class="text-sm uppercase tracking-wide">Custom Order Revenue</p>
          <p class="text-3xl font-bold mt-2">${{ stats.customRevenue.toFixed(2) }}</p>
        </div>
        <div class="card bg-gradient-to-r from-yellow-200 to-yellow-400 text-yellow-900">
          <p class="text-sm uppercase tracking-wide">Custom Orders</p>
          <ul class="mt-2 space-y-1">
            <li v-for="(count, status) in stats.customOrdersByStatus" :key="status">
              <span class="capitalize font-medium">{{ status }}:</span> {{ count }}
            </li>
          </ul>
        </div>
      </div>

      <!-- Charts -->
      <div class="grid grid-cols-1 lg:grid-cols-2 gap-8">
        <div class="card bg-white">
          <h2 class="text-xl font-semibold mb-4">Monthly Revenue Trend</h2>
          <div v-if="loading" class="flex justify-center items-center h-64">
            <div class="animate-spin rounded-full h-12 w-12 border-t-4 border-green-500"></div>
          </div>
          <canvas v-else ref="lineCanvas" class="rounded-lg shadow-md"></canvas>
        </div>
        <div class="card bg-white">
          <h2 class="text-xl font-semibold mb-4">Revenue by Category</h2>
          <div v-if="loading" class="flex justify-center items-center h-64">
            <div class="animate-spin rounded-full h-12 w-12 border-t-4 border-purple-500"></div>
          </div>
          <canvas v-else ref="pieCanvas" class="rounded-lg shadow-md"></canvas>
        </div>
      </div>
    </div>
  </div>
</template>

<script setup>
import { ref, onMounted, onBeforeUnmount, nextTick } from 'vue';
import axios from 'axios';
import ArtisanNavBar from '@/components/ArtisansDashboard/ArtisanNavBar.vue';
import { Chart, registerables } from 'chart.js';

Chart.register(...registerables);
const apiBaseUrl = process.env.VUE_APP_API_URL;
const loading = ref(true);
const stats = ref({ productsSold: 0, productRevenue: 0, customRevenue: 0, customOrdersByStatus: {} });
const lineCanvas = ref(null), pieCanvas = ref(null);
let lineChart = null, pieChart = null;

onMounted(async () => {
  try {
    const data = (await axios.get(`${apiBaseUrl}/api/v1/artisan/dashboard`)).data;
    stats.value.productsSold = data.total_products_sold;
    stats.value.productRevenue = data.product_revenue;
    stats.value.customRevenue = data.custom_order_revenue;
    stats.value.customOrdersByStatus = data.custom_orders_by_status;
    loading.value = false;
    await nextTick();
    initLineChart(data.monthly_trend);
    initPieChart(data.revenue_by_category);
  } catch (err) {
    console.error(err);
    loading.value = false;
  }
});
onBeforeUnmount(() => { lineChart?.destroy(); pieChart?.destroy(); });

function initLineChart(data) {
  const ctx = lineCanvas.value?.getContext('2d'); if (!ctx) return;
  lineChart = new Chart(ctx, { type: 'line', data: { labels: Object.keys(data), datasets: [{ label: 'Revenue', data: Object.values(data), fill: true, tension: 0.3, borderColor: '#10b981', backgroundColor: 'rgba(16,185,129,0.2)', pointRadius: 4 }] }, options: { responsive: true } });
}
function initPieChart(data) {
  const ctx = pieCanvas.value?.getContext('2d'); if (!ctx) return;
  const labels = data.map(d=>d.name), vals = data.map(d=>d.value);
  pieChart = new Chart(ctx, { type: 'pie', data: { labels, datasets: [{ data: vals, backgroundColor: ['#3b82f6','#8b5cf6','#f59e0b','#ef4444'], borderColor: '#fff', borderWidth: 2 }] }, options: { responsive: true, plugins: { legend: { position: 'bottom' } } } });
}
</script>

<style scoped>
.card { @apply p-6 rounded-2xl shadow-xl transition transform hover:-translate-y-1 }
h1 { @apply animate-fade-in }
@keyframes fade-in { from { opacity:0; transform:translateY(-10px) } to { opacity:1; transform:translateY(0) } }
.animate-fade-in { animation:fade-in 0.8s ease-out }
</style>
