<template>
  <div class="bg-white shadow-lg rounded-lg p-6 w-full">
    <!-- Header & Reload -->
    <div class="flex items-center justify-between mb-6">
      <h2 class="text-3xl font-semibold text-gray-800">Custom Orders</h2>
      <button
          @click="fetchCustomOrders"
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
              d="M4 4v5h.581m15.362 2A9 9 0 115.637 5.637M20 20v-5h-.581"
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
          <option value="budget">Budget</option>
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
          class="border border-gray-300 rounded-md px-4 py-2 focus:ring-2 focus:ring-gray-400 transition w-full md:w-auto"
      />
      <input
          v-model="filters.end_date"
          type="date"
          class="border border-gray-300 rounded-md px-4 py-2 focus:ring-2 focus:ring-gray-400 transition w-full md:w-auto"
      />

      <!-- Apply -->
      <button
          @click="applyFilters"
          class="bg-gray-800 text-white px-6 py-2 rounded-md hover:bg-gray-900 transition font-semibold w-full md:w-auto"
      >
        Apply
      </button>
    </div>

    <!-- Loading -->
    <div v-if="loading" class="flex items-center justify-center py-8">
      <svg
          class="animate-spin h-8 w-8 text-gray-700"
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
        />
        <path
            class="opacity-75"
            fill="currentColor"
            d="M4 12a8 8 0 018-8v8H4z"
        />
      </svg>
      <span class="ml-3 text-gray-700 font-medium">
        Loading custom orders...
      </span>
    </div>

    <!-- Table -->
    <div v-else class="overflow-x-auto">
      <table
          v-if="customOrders.length"
          class="min-w-full divide-y divide-gray-200 table-auto"
      >
        <thead class="bg-gray-50">
        <tr>
          <th class="px-6 py-3 text-left text-sm font-semibold text-gray-700">
            Order No.
          </th>
          <th class="px-6 py-3 text-left text-sm font-semibold text-gray-700">
            Date
          </th>
          <th class="px-6 py-3 text-left text-sm font-semibold text-gray-700">
            Budget
          </th>
          <th class="px-6 py-3 text-left text-sm font-semibold text-gray-700">
            Status
          </th>
          <th class="px-6 py-3 text-center text-sm font-semibold text-gray-700">
            Action
          </th>
        </tr>
        </thead>
        <tbody class="bg-white divide-y divide-gray-200">
        <tr
            v-for="order in customOrders"
            :key="order.id"
            class="hover:bg-gray-50 transition-colors duration-200"
        >
          <td class="px-6 py-4 text-sm font-semibold text-gray-800">
            {{ order.id }}
          </td>
          <td class="px-6 py-4 text-sm text-gray-800">
            {{ formatDate(order.created_at) }}
          </td>
          <td class="px-6 py-4 text-sm font-semibold text-green-600">
            ${{ formatPrice(order.budget) }}
          </td>
          <td
              class="px-6 py-4 text-sm capitalize font-semibold"
              :class="statusClass(order.status)"
          >
            {{ order.status }}
          </td>
          <td class="px-6 py-4 text-sm text-center">
            <button
                v-if="order.status === 'finalized'"
                @click="viewFinalization(order)"
                class="bg-blue-600 text-white px-3 py-1 rounded hover:bg-blue-700 transition font-semibold w-full md:w-auto"
            >
              View Finalization
            </button>
            <button
                v-else
                @click="$emit('view-details', order)"
                class="text-gray-600 hover:text-gray-800 font-medium transition"
            >
              › View details
            </button>
          </td>
        </tr>
        </tbody>
      </table>

      <p v-else class="text-center text-gray-500 py-8">
        No custom orders found.
      </p>
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
              : 'bg-gray-100 text-gray-700 hover:bg-gray-200'
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
import { ref, computed, onMounted } from 'vue';
import axios from 'axios';

const emit = defineEmits(['view-details', 'view-finalization']);
const apiBase = `${process.env.VUE_APP_API_URL}/api/v1/custom-orders`;

const customOrders = ref([]);
const currentPage = ref(1);
const totalPages = ref(1);
const loading = ref(true);

const filters = ref({ start_date: '', end_date: '' });
const sortBy = ref('id');
const sortDir = ref('desc');
const perPage = 5;

async function fetchCustomOrders() {
  loading.value = true;
  try {
    const token = localStorage.getItem('authToken');
    const { data } = await axios.get(apiBase, {
      headers: { Authorization: `Bearer ${token}` },
      params: {
        page: currentPage.value,
        per_page: perPage,
        start_date: filters.value.start_date,
        end_date: filters.value.end_date,
        sort_by: sortBy.value,
        sort_dir: sortDir.value,
      },
    });
    customOrders.value = data.data.map(o => ({
      ...o,
      budget: parseFloat(o.budget) || 0,
    }));
    totalPages.value = data.last_page || 1;
  } finally {
    loading.value = false;
  }
}

const applyFilters = () => {
  currentPage.value = 1;
  fetchCustomOrders();
};
const toggleSortDir = () => {
  sortDir.value = sortDir.value === 'asc' ? 'desc' : 'asc';
  fetchCustomOrders();
};
const changePage = p => {
  if (p < 1 || p > totalPages.value) return;
  currentPage.value = p;
  fetchCustomOrders();
};

const pageNumbers = computed(() =>
    Array.from({ length: totalPages.value }, (_, i) => i + 1)
);
const formatDate = d => new Date(d).toLocaleDateString();
const formatPrice = n => isNaN(n) ? '0.00' : n.toFixed(2);
const statusClass = s =>
    s === 'pending'
        ? 'text-yellow-600'
        : ['accepted', 'completed'].includes(s)
            ? 'text-green-600'
            : 'text-red-600';

const viewFinalization = order => emit('view-finalization', order);

onMounted(fetchCustomOrders);
</script>

<style scoped>
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
