<template>
  <div class="container mx-auto p-6 bg-white text-gray-800 h-screen flex flex-col">
    <h1 class="text-3xl font-bold mb-6 text-center">Orders Received</h1>

    <!-- Toasts Stack -->
    <Toast :toasts="toasts" @close="removeToast" />

    <!-- Sorting Controls -->
    <div class="flex flex-wrap justify-end items-center mb-4 space-x-2">
      <label for="sortBy" class="text-sm">Sort by:</label>
      <select
          id="sortBy"
          v-model="sortBy"
          class="p-1 border border-gray-300 rounded focus:outline-none focus:ring-2 focus:ring-blue-400"
      >
        <option value="created_at">Created At</option>
        <option value="total_price">Total Price</option>
        <option value="status">Status</option>
      </select>

      <label for="sortOrder" class="text-sm">Order:</label>
      <select
          id="sortOrder"
          v-model="sortOrder"
          class="p-1 border border-gray-300 rounded focus:outline-none focus:ring-2 focus:ring-blue-400"
      >
        <option value="desc">Descending</option>
        <option value="asc">Ascending</option>
      </select>

      <button
          @click="fetchFilteredOrders(1)"
          class="bg-blue-500 hover:bg-blue-700 text-white py-1 px-3 rounded transition-colors focus:outline-none focus:ring-2 focus:ring-blue-400"
      >
        Sort
      </button>
    </div>

    <!-- Orders Container -->
    <section class="flex-1 overflow-y-auto space-y-6 pb-4">
      <!-- Loading State -->
      <div v-if="isLoading" class="flex justify-center items-center h-64">
        <div class="loader"></div>
        <span class="ml-2 text-gray-700">Loading orders...</span>
      </div>

      <!-- No Orders State -->
      <div v-else-if="orders.length === 0" class="text-center py-8">
        <p class="text-xl">No orders to display.</p>
      </div>

      <!-- Orders Card List -->
      <div v-else>
        <div
            v-for="order in orders"
            :key="order.id"
            class="bg-gray-50 shadow rounded-lg p-4 hover:shadow-lg transition"
        >
          <!-- Order Header -->
          <div class="flex flex-col sm:flex-row justify-between items-start sm:items-center mb-4">
            <h2 class="text-xl font-bold">Order #{{ order.id }}</h2>
            <div class="mt-2 sm:mt-0 text-sm text-gray-600">
              Ordered by: <span class="font-semibold">{{ order.user.name }}</span>
            </div>
          </div>

          <!-- Order Meta -->
          <div class="text-gray-700 mb-4">
            <span class="font-semibold">Created At:</span>
            {{ formatDate(order.created_at) }}
          </div>

          <!-- Order Products Table -->
          <div class="overflow-x-auto">
            <table class="min-w-full bg-white">
              <thead>
              <tr class="bg-gray-100">
                <th class="px-3 py-2 border-b text-left">Product</th>
                <th class="px-3 py-2 border-b text-center">Qty</th>
                <th class="px-3 py-2 border-b text-right">Price</th>
                <th class="px-3 py-2 border-b text-right">Total</th>
                <th class="px-3 py-2 border-b text-center">Status</th>
                <th class="px-3 py-2 border-b text-center">Action</th>
              </tr>
              </thead>
              <tbody>
              <tr
                  v-for="prod in order.products"
                  :key="prod.id"
                  class="hover:bg-gray-50"
              >
                <td class="px-3 py-2 border-b">{{ prod.name }}</td>
                <td class="px-3 py-2 border-b text-center">{{ prod.quantity }}</td>
                <td class="px-3 py-2 border-b text-right">${{ formatPrice(prod.price) }}</td>
                <td class="px-3 py-2 border-b text-right">${{ formatPrice(prod.total) }}</td>
                <td class="px-3 py-2 border-b text-center">
                  <select
                      v-model="prod.localStatus"
                      class="p-1 border border-gray-300 rounded focus:outline-none focus:ring-2 focus:ring-blue-400"
                  >
                    <option value="pending">Pending</option>
                    <option value="in production">In Production</option>
                    <option value="shipped">Shipped</option>
                    <option value="delivered">Delivered</option>
                  </select>
                </td>
                <td class="px-3 py-2 border-b text-center">
                  <button
                      @click="updateProductStatus(order.id, prod)"
                      class="bg-green-500 hover:bg-green-600 text-white py-1 px-3 rounded transition-colors focus:outline-none focus:ring-2 focus:ring-green-400"
                  >
                    Update
                  </button>
                </td>
              </tr>
              </tbody>
            </table>
          </div>
        </div>

        <!-- Pagination Controls -->
        <div class="flex justify-between items-center mt-4">
          <button
              @click="prevPage"
              :disabled="pagination.current_page === 1"
              class="bg-gray-300 hover:bg-gray-400 text-gray-700 py-1 px-3 rounded disabled:opacity-50"
          >
            Previous
          </button>
          <span class="text-sm text-gray-600">
            Page {{ pagination.current_page }} of {{ pagination.last_page }}
          </span>
          <button
              @click="nextPage"
              :disabled="pagination.current_page === pagination.last_page"
              class="bg-gray-300 hover:bg-gray-400 text-gray-700 py-1 px-3 rounded disabled:opacity-50"
          >
            Next
          </button>
        </div>
      </div>
    </section>
  </div>
</template>

<script setup>
import { ref, reactive, onMounted } from "vue";
import axios from "axios";
import Toast from "@/components/Toast.vue";

const toasts = reactive([]);
const removeToast = (id) => {
  const idx = toasts.findIndex((t) => t.id === id);
  if (idx !== -1) toasts.splice(idx, 1);
};
const showToast = (message, type = "success") => {
  const id = Date.now() + Math.random();
  toasts.push({ id, message, type });
  setTimeout(() => removeToast(id), 3000);
};

const orders = ref([]);
const isLoading = ref(true);
const pagination = reactive({ current_page: 1, last_page: 1 });
const sortBy = ref("created_at");
const sortOrder = ref("desc");
const apiBaseUrl = process.env.VUE_APP_API_URL;

const fetchFilteredOrders = async (page = 1) => {
  isLoading.value = true;
  try {
    const token = localStorage.getItem("authToken");
    const resp = await axios.get(
        `${apiBaseUrl}/api/v1/orders-sorted?page=${page}&sort_by=${sortBy.value}&order=${sortOrder.value}`,
        { headers: { Authorization: `Bearer ${token}` } }
    );

    const { success, message, data: ordersData, pagination: pageMeta } = resp.data;

    if (!success) {
      showToast(message, "error");
      orders.value = [];
      return;
    }

    // normalize and attach localStatus
    orders.value = ordersData.map((o) => {
      const raw = o.products;
      const arr = Array.isArray(raw) ? raw : Object.values(raw || {});
      return {
        ...o,
        products: arr.map((p) => ({ ...p, localStatus: p.status })),
      };
    });

    pagination.current_page = pageMeta.current_page;
    pagination.last_page = pageMeta.total_pages;
  } catch (err) {
    showToast("Failed to fetch orders.", "error");
  } finally {
    isLoading.value = false;
  }
};

const updateProductStatus = async (orderId, prod) => {
  const prev = prod.status;
  try {
    const token = localStorage.getItem("authToken");
    await axios.put(
        `${apiBaseUrl}/api/v1/orders/${orderId}/products/${prod.id}/status`,
        { status: prod.localStatus },
        { headers: { Authorization: `Bearer ${token}` } }
    );
    prod.status = prod.localStatus;
    showToast("Status updated.", "success");
  } catch {
    prod.localStatus = prev;
    showToast("Update failed.", "error");
  }
};

const formatDate = (d) => new Date(d).toLocaleString();
const formatPrice = (p) => parseFloat(p).toFixed(2);
const nextPage = () =>
    pagination.current_page < pagination.last_page &&
    fetchFilteredOrders(pagination.current_page + 1);
const prevPage = () =>
    pagination.current_page > 1 && fetchFilteredOrders(pagination.current_page - 1);

onMounted(() => fetchFilteredOrders(1));
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
@keyframes spin {
  to {
    transform: rotate(360deg);
  }
}
</style>
