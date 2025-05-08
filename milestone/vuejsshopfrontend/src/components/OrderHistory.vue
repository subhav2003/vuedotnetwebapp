<template>
  <div class="bg-white shadow-lg rounded-lg p-6 w-full">
    <!-- Header & Reload -->
    <div class="flex items-center justify-between mb-6">
      <h2 class="text-3xl font-semibold text-gray-800">Order History</h2>
      <button
          @click="fetchOrders"
          class="flex items-center text-gray-600 hover:text-gray-800 transition"
      >
        <svg
            xmlns="http://www.w3.org/2000/svg"
            class="h-6 w-6 mr-1"
            fill="none"
            viewBox="0 0 24 24"
            stroke="currentColor"
        >
          <path
              stroke-linecap="round"
              stroke-linejoin="round"
              stroke-width="2"
              d="M4 4v5h.582m15.253 6A9 9 0 115.334 5.334M20 20v-5h-.581"
          />
        </svg>
        Reload
      </button>
    </div>

    <!-- Sort & Date Filters -->
    <div class="flex flex-col md:flex-row md:items-center gap-4 mb-6">
      <!-- Sort by -->
      <div class="flex items-center gap-2 w-full md:w-auto">
        <label class="font-semibold text-gray-700 whitespace-nowrap">Sort by:</label>
        <select
            v-model="sortBy"
            class="border border-gray-300 rounded-md px-3 py-2 focus:ring-2 focus:ring-gray-400 transition w-full md:w-auto"
        >
          <option value="id">Order No.</option>
          <option value="total_price">Total</option>
          <option value="status">Status</option>
        </select>
        <button
            @click="toggleSortDir"
            class="px-2 py-1 border border-gray-300 rounded-md focus:outline-none hover:bg-gray-100 transition w-full md:w-auto"
        >
          {{ sortDir === 'asc' ? '↑' : '↓' }}
        </button>
      </div>

      <!-- Date range -->
      <input
          v-model="filters.start_date"
          type="date"
          class="border border-gray-300 rounded-md px-4 py-2 focus:ring-2 focus:ring-gray-400 transition font-semibold w-full md:w-auto"
      />
      <input
          v-model="filters.end_date"
          type="date"
          class="border border-gray-300 rounded-md px-4 py-2 focus:ring-2 focus:ring-gray-400 transition font-semibold w-full md:w-auto"
      />

      <button
          @click="applyFilters"
          class="bg-gray-800 text-white px-6 py-2 rounded-md hover:bg-gray-900 transition font-semibold w-full md:w-auto"
      >
        Search
      </button>
    </div>

    <!-- Loading Indicator -->
    <div v-if="loading" class="flex items-center justify-center py-12">
      <svg
          class="animate-spin h-8 w-8 text-gray-700"
          xmlns="http://www.w3.org/2000/svg"
          fill="none"
          viewBox="0 0 24 24"
      >
        <circle
            class="opacity-25"
            cx="12"
            cy="12"
            r="10"
            stroke="currentColor"
            stroke-width="4"
        ></circle>
        <path
            class="opacity-75"
            fill="currentColor"
            d="M4 12a8 8 0 018-8v8H4z"
        ></path>
      </svg>
      <span class="ml-3 text-gray-700 font-medium">Loading orders...</span>
    </div>

    <!-- Orders Table -->
    <div v-else class="overflow-x-auto">
      <table
          v-if="orders.length"
          class="min-w-full table-auto border-collapse bg-white rounded-md overflow-hidden shadow-md"
      >
        <thead class="bg-gray-100 text-gray-700 font-semibold">
        <tr>
          <th class="px-6 py-3 text-left">Order No.</th>
          <th class="px-6 py-3 text-left">Order Date</th>
          <th class="px-6 py-3 text-left">Total</th>
          <th class="px-6 py-3 text-left">Status</th>
          <th class="px-6 py-3"></th>
        </tr>
        </thead>
        <transition-group tag="tbody" name="fade" class="divide-y divide-gray-200">
          <tr
              v-for="order in orders"
              :key="order.id"
              class="hover:bg-gray-50 transition-colors duration-200"
          >
            <td class="px-6 py-4 font-semibold">{{ order.id }}</td>
            <td class="px-6 py-4">{{ formatDate(order.created_at) }}</td>
            <td class="px-6 py-4 text-green-600 font-semibold">
              ${{ formatPrice(order.total_price) }}
            </td>
            <td
                class="px-6 py-4 capitalize font-semibold"
                :class="{
                'text-yellow-600': order.status === 'pending',
                'text-green-600': ['completed','delivered'].includes(order.status),
                'text-red-600': ['canceled','returned'].includes(order.status)
              }"
            >
              {{ order.status }}
            </td>
            <td class="px-6 py-4 text-right">
              <button
                  @click="viewOrder(order)"
                  class="text-gray-600 hover:text-gray-800 font-medium transition"
              >
                View
              </button>
            </td>
          </tr>
        </transition-group>
      </table>

      <p v-else class="text-center text-gray-500 py-8">No orders found.</p>
    </div>

    <!-- Pagination -->
    <div
        v-if="totalPages > 1"
        class="mt-6 flex flex-col sm:flex-row justify-between items-center gap-2"
    >
      <button
          @click="changePage(currentPage - 1)"
          :disabled="currentPage === 1"
          class="px-4 py-2 rounded-md font-semibold transition disabled:opacity-50 bg-gray-800 text-white hover:bg-gray-900 w-full sm:w-auto"
      >
        Previous
      </button>

      <div class="flex items-center gap-1">
        <button
            v-for="p in pageNumbers"
            :key="p"
            @click="changePage(p)"
            :class="[
            'px-3 py-1 rounded transition',
            currentPage === p
              ? 'bg-gray-800 text-white'
              : 'bg-gray-200 text-gray-700 hover:bg-gray-300'
          ]"
        >
          {{ p }}
        </button>
      </div>

      <button
          @click="changePage(currentPage + 1)"
          :disabled="currentPage === totalPages"
          class="px-4 py-2 rounded-md font-semibold transition disabled:opacity-50 bg-gray-800 text-white hover:bg-gray-900 w-full sm:w-auto"
      >
        Next
      </button>
    </div>
  </div>
</template>

<script setup>
import { ref, computed, onMounted, defineEmits } from 'vue';
import axios from 'axios';

const emit = defineEmits(['view-order']);
const apiBase = `${process.env.VUE_APP_API_URL}/api/v1/user/orders`;

const orders = ref([]);
const loading = ref(false);
const currentPage = ref(1);
const totalPages = ref(1);

// filter & sort state
const filters = ref({
  start_date: '',
  end_date: '',
});
const sortBy = ref('id');
const sortDir = ref('desc');

const applyFilters = () => {
  currentPage.value = 1;
  fetchOrders();
};

const toggleSortDir = () => {
  sortDir.value = sortDir.value === 'asc' ? 'desc' : 'asc';
  fetchOrders();
};

async function fetchOrders() {
  loading.value = true;
  try {
    const { data } = await axios.get(apiBase, {
      params: {
        page: currentPage.value,
        per_page: 5,
        start_date: filters.value.start_date,
        end_date: filters.value.end_date,
        sort_by: sortBy.value,
        sort_dir: sortDir.value,
      },
    });

    orders.value = data.data.map(o => ({
      ...o,
      total_price: parseFloat(o.total_price) || 0,
    }));
    totalPages.value = data.last_page;
  } catch (err) {
    console.error('Fetch orders error:', err);
  } finally {
    loading.value = false;
  }
}

const formatDate = iso => new Date(iso).toLocaleDateString();
const formatPrice = amt => (isNaN(amt) ? '0.00' : amt.toFixed(2));

const changePage = p => {
  if (p < 1 || p > totalPages.value) return;
  currentPage.value = p;
  fetchOrders();
};

const pageNumbers = computed(() => Array.from({ length: totalPages.value }, (_, i) => i + 1));

const viewOrder = order => emit('view-order', order);

onMounted(fetchOrders);
</script>

<style scoped>
/* fade-in for new rows */
.fade-enter-from {
  opacity: 0;
  transform: translateY(-5px);
}
.fade-enter-active {
  transition: all 0.3s ease;
}
.fade-enter-to {
  opacity: 1;
  transform: translateY(0);
}
</style>
