<template>
  <div class="p-6 bg-white rounded-lg shadow-md">
    <!-- Header -->
    <div class="flex items-center justify-between mb-4">
      <h2 class="text-2xl font-semibold text-gray-800">Product List</h2>
      <button
          @click="refreshProducts"
          class="text-gray-600 hover:text-gray-800 font-semibold"
      >
        ⟳ Reload
      </button>
    </div>

    <!-- Loading State -->
    <div v-if="loading" class="flex items-center justify-center py-6">
      <svg class="animate-spin h-6 w-6 text-gray-500" xmlns="http://www.w3.org/2000/svg" fill="none" viewBox="0 0 24 24">
        <circle class="opacity-25" cx="12" cy="12" r="10" stroke="currentColor" stroke-width="4" />
        <path class="opacity-75" fill="currentColor" d="M4 12a8 8 0 018-8v8H4z" />
      </svg>
    </div>

    <!-- Table -->
    <div v-else>
      <table class="w-full table-auto">
        <thead>
        <tr class="bg-gray-100">
          <th class="py-3 px-4 text-left text-sm font-medium text-gray-700">ID</th>
          <th class="py-3 px-4 text-left text-sm font-medium text-gray-700">Name</th>
          <th class="py-3 px-4 text-left text-sm font-medium text-gray-700">Category</th>
          <th class="py-3 px-4 text-right text-sm font-medium text-gray-700">Price</th>
          <th class="py-3 px-4 text-center text-sm font-medium text-gray-700">Qty</th>
          <th class="py-3 px-4 text-left text-sm font-medium text-gray-700">Artisan</th>
          <th class="py-3 px-4 text-right text-sm font-medium text-gray-700">Actions</th>
        </tr>
        </thead>
        <tbody>
        <tr
            v-for="product in products"
            :key="product.id"
            class="border-t hover:bg-gray-50 transition"
        >
          <td class="py-3 px-4 text-sm text-gray-600">{{ product.id }}</td>
          <td class="py-3 px-4 text-sm text-gray-800">{{ product.name }}</td>
          <td class="py-3 px-4 text-sm text-gray-600">
            {{ product.category?.name || '—' }}
          </td>
          <td class="py-3 px-4 text-sm text-green-600 text-right">
            ${{ product.price }}
          </td>
          <td class="py-3 px-4 text-sm text-gray-600 text-center">
            {{ product.available_quantity }}
          </td>
          <td class="py-3 px-4 text-sm text-gray-600">
            {{ product.artisan?.name || '—' }}
          </td>
          <td class="py-3 px-4 text-sm text-right space-x-2">
            <button
                @click="openDetail(product)"
                class="text-blue-600 hover:underline"
            >
              View
            </button>
            <button
                @click="openEdit(product)"
                class="text-red-600 hover:underline"
            >
              Edit
            </button>
          </td>
        </tr>
        </tbody>
      </table>

      <p v-if="products.length === 0" class="text-center py-6 text-gray-500">
        No products found.
      </p>
    </div>

    <!-- Detail Overlay -->
    <ProductDetail
        v-if="selectedProduct"
        :product="selectedProduct"
        @close="selectedProduct = null"
    />

    <!-- Edit Overlay -->
    <div
        v-if="editingProduct"
        class="fixed inset-0 bg-black bg-opacity-50 flex items-center justify-center z-50"
    >
      <div class="bg-white rounded-lg shadow-lg max-w-md w-full p-6 overflow-y-auto relative">
        <button
            @click="closeEdit"
            class="absolute top-2 right-2 text-gray-500 hover:text-red-600 text-xl"
        >&times;</button>
        <ProductEditForm
            :productId="editingProduct.id"
            @close="closeEdit"
            @updated="refreshProducts"
        />
      </div>
    </div>
  </div>
</template>

<script setup>
import { ref, onMounted } from 'vue';
import axios from 'axios';
import ProductDetail from '@/components/ProductDetail.vue';
import ProductEditForm from '@/components/ArtisansDashboard/ProductEditForm.vue';

const apiBaseUrl = process.env.VUE_APP_API_URL;
const products = ref([]);
const loading = ref(true);
const selectedProduct = ref(null);
const editingProduct = ref(null);

async function fetchProducts() {
  loading.value = true;
  try {
    const res = await axios.get(`${apiBaseUrl}/api/v1/products`);
    products.value = res.data.data;
  } catch (e) {
    console.error('Failed to load products', e);
  } finally {
    loading.value = false;
  }
}

function refreshProducts() {
  fetchProducts();
  editingProduct.value = null;
}

function openDetail(prod) {
  selectedProduct.value = prod;
  document.body.style.overflow = 'hidden';
}

function openEdit(prod) {
  editingProduct.value = prod;
  document.body.style.overflow = 'hidden';
}

function closeEdit() {
  editingProduct.value = null;
  document.body.style.overflow = '';
}

onMounted(fetchProducts);
</script>

<style scoped>
/* Scroll within table container if needed */
.p-6.overflow-auto { overflow-x: auto; }
</style>
