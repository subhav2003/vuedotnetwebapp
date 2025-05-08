<template>
  <div class="bg-white shadow-lg rounded-lg p-6 w-full">
    <!-- Toasts -->
    <Toast :toasts="toasts" @close="removeToast" />

    <!-- Header & Reload -->
    <div class="flex items-center justify-between mb-6">
      <h2 class="text-3xl font-semibold text-gray-800">Flagged Products</h2>
      <button
          @click="refreshProducts"
          class="flex items-center text-gray-600 hover:text-gray-800 transition"
      >
        <svg xmlns="http://www.w3.org/2000/svg" class="h-6 w-6 mr-1" fill="none" viewBox="0 0 24 24" stroke="currentColor">
          <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M4 4v5h.582m15.253 6A9 9 0 115.334 5.334M20 20v-5h-.581" />
        </svg>
        Reload
      </button>
    </div>

    <!-- Loading Indicator -->
    <div v-if="loading" class="flex items-center justify-center py-12">
      <svg class="animate-spin h-8 w-8 text-gray-700" xmlns="http://www.w3.org/2000/svg" fill="none" viewBox="0 0 24 24">
        <circle class="opacity-25" cx="12" cy="12" r="10" stroke="currentColor" stroke-width="4" />
        <path class="opacity-75" fill="currentColor" d="M4 12a8 8 0 018-8v8H4z" />
      </svg>
      <span class="ml-3 text-gray-700 font-medium">Loading flagged products...</span>
    </div>

    <!-- Error Message -->
    <p v-if="!loading && error" class="text-red-600 text-center py-4">{{ error }}</p>

    <!-- Products Table -->
    <div v-if="!loading" class="overflow-x-auto">
      <table v-if="products.length" class="min-w-full table-auto border-collapse bg-white rounded-md shadow-md">
        <thead class="bg-gray-100 text-gray-700 font-semibold">
        <tr>
          <th class="px-6 py-3 text-left">ID</th>
          <th class="px-6 py-3 text-left">Name</th>
          <th class="px-6 py-3 text-left">Artisan</th>
          <th class="px-6 py-3 text-left">Flags</th>
          <th class="px-6 py-3 text-left">Description</th>
          <th class="px-6 py-3"></th>
        </tr>
        </thead>
        <transition-group tag="tbody" name="fade" class="divide-y divide-gray-200">
          <tr v-for="prod in products" :key="prod.id" class="hover:bg-gray-50 transition-colors">
            <td class="px-6 py-4 font-semibold">{{ prod.id }}</td>
            <td class="px-6 py-4 font-medium">{{ prod.name }}</td>
            <td class="px-6 py-4">{{ prod.artisan?.user?.name || 'N/A' }}</td>
            <td class="px-6 py-4">
              <span class="inline-block bg-yellow-200 text-yellow-800 text-xs font-semibold px-2 py-1 rounded-full">
                {{ prod.flag_count }}
              </span>
            </td>
            <td class="px-6 py-4">{{ truncateDescription(prod.description, 60) }}</td>
            <td class="px-6 py-4 text-right">
              <button
                  @click="openDetailModal(prod)"
                  class="text-blue-600 hover:text-blue-800 font-medium transition"
              >
                View Details
              </button>
            </td>
          </tr>
        </transition-group>
      </table>
      <p v-else class="text-center text-gray-500 py-8">No flagged products available.</p>
    </div>

    <!-- Detail Modal -->
    <div v-if="selectedProduct" class="fixed inset-0 flex items-center justify-center bg-black bg-opacity-50 z-50">
      <div class="bg-white rounded-lg shadow-lg w-full max-w-2xl mx-4 overflow-auto max-h-[90vh] relative">
        <button
            @click="closeDetailModal"
            class="absolute top-2 right-2 text-gray-500 hover:text-red-600 text-2xl"
        >
          ✖
        </button>
        <div class="p-6">
          <h3 class="text-2xl font-semibold mb-4 text-gray-800">Product Details</h3>

          <!-- Two-column grid -->
          <div class="grid grid-cols-1 md:grid-cols-2 gap-4 text-gray-700">
            <div><strong>ID:</strong> {{ selectedProduct.id }}</div>
            <div><strong>Name:</strong> {{ selectedProduct.name }}</div>
            <div><strong>Artisan:</strong> {{ selectedProduct.artisan?.user?.name || 'N/A' }}</div>
            <div><strong>Flags:</strong> {{ selectedProduct.flag_count }}</div>
            <div class="md:col-span-2"><strong>Description:</strong> {{ selectedProduct.description }}</div>
            <div><strong>Category:</strong> {{ selectedProduct.category }}</div>
            <div><strong>Materials:</strong> {{ selectedProduct.materials }}</div>
            <div><strong>Price:</strong> ${{ selectedProduct.price }}</div>
            <div><strong>Quantity:</strong> {{ selectedProduct.quantity }}</div>
            <div><strong>Lead Time:</strong> {{ selectedProduct.lead_time }} days</div>
            <div class="md:col-span-2"><strong>Tags:</strong> {{ selectedProduct.tags?.join(', ') }}</div>
            <div><strong>Created:</strong> {{ formatDate(selectedProduct.created_at) }}</div>
            <div><strong>Updated:</strong> {{ formatDate(selectedProduct.updated_at) }}</div>
          </div>

          <!-- Image List -->
          <div v-if="selectedProduct.images?.length" class="mt-6 flex gap-2 overflow-x-auto">
            <img
                v-for="(img, idx) in selectedProduct.images" :key="idx"
                :src="img" class="h-32 rounded-md shadow" :alt="selectedProduct.name"
            />
          </div>

          <!-- Actions -->
          <div class="mt-6 flex justify-end gap-4">
            <button
                @click="resetFlag(selectedProduct)"
                class="px-4 py-2 bg-green-600 text-white rounded-md hover:bg-green-700 transition font-semibold"
            >
              Reset Flag
            </button>
            <button
                @click="confirmDelete(selectedProduct)"
                class="px-4 py-2 bg-red-600 text-white rounded-md hover:bg-red-700 transition font-semibold"
            >
              Delete
            </button>
          </div>
        </div>
      </div>
    </div>

    <!-- Confirm Delete Modal -->
    <div v-if="confirm.visible" class="fixed inset-0 flex items-center justify-center bg-black bg-opacity-50 z-50">
      <div class="bg-white rounded-lg p-6 shadow-lg w-full max-w-sm">
        <h3 class="text-xl font-semibold mb-4">Confirm Deletion</h3>
        <p class="mb-6">Are you sure you want to delete product #{{ confirm.product.id }}?</p>
        <div class="flex justify-end gap-4">
          <button
              @click="cancelDelete"
              class="px-4 py-2 bg-gray-200 rounded-md hover:bg-gray-300"
          >
            Cancel
          </button>
          <button
              @click="deleteProduct(confirm.product)"
              class="px-4 py-2 bg-red-600 text-white rounded-md hover:bg-red-700"
          >
            Delete
          </button>
        </div>
      </div>
    </div>
  </div>
</template>

<script setup>
import { ref, reactive, onMounted } from 'vue'
import axios from 'axios'
import Toast from '@/components/Toast.vue'

const apiBaseUrl = process.env.VUE_APP_API_URL
const products = ref([])
const loading = ref(false)
const error = ref('')
const selectedProduct = ref(null)
const toasts = reactive([])
const confirm = reactive({ visible: false, product: null })

function removeToast(id) {
  const i = toasts.findIndex(t => t.id === id)
  if (i > -1) toasts.splice(i, 1)
}
function showToast(msg, type = 'success') {
  const id = Date.now() + Math.random()
  toasts.push({ id, message: msg, type })
  setTimeout(() => removeToast(id), 5000)
}

async function fetchFlaggedProducts() {
  loading.value = true; error.value = ''
  try {
    const { data } = await axios.get(`${apiBaseUrl}/api/v1/flagged-products`)
    products.value = data.data
  } catch (e) {
    error.value = e.response?.data?.message || 'Error fetching flagged products.'
    showToast(error.value, 'error')
  } finally { loading.value = false }
}

const refreshProducts = () => fetchFlaggedProducts()
const openDetailModal = p => selectedProduct.value = p
const closeDetailModal = () => selectedProduct.value = null

function confirmDelete(p) { confirm.product = p; confirm.visible = true }
function cancelDelete() { confirm.visible = false; confirm.product = null }

async function resetFlag(p) {
  try {
    await axios.post(`${apiBaseUrl}/api/v1/reset-flag`, { type: 'product', id: p.id })
    products.value = products.value.filter(x => x.id !== p.id)
    showToast('Flag reset successfully', 'success')
    closeDetailModal()
  } catch {
    showToast('Failed to reset flag', 'error')
  }
}

async function deleteProduct(p) {
  try {
    await axios.delete(`${apiBaseUrl}/api/v1/products/${p.id}`)
    products.value = products.value.filter(x => x.id !== p.id)
    showToast('Product deleted successfully', 'success')
    closeDetailModal(); cancelDelete()
  } catch {
    showToast('Failed to delete product', 'error')
  }
}

const truncateDescription = (t, m) => t?.length > m ? t.slice(0, m) + '...' : t
const formatDate = dt => new Date(dt).toLocaleDateString()

onMounted(fetchFlaggedProducts)
</script>

<style scoped>
.fade-enter-from { opacity: 0; transform: translateY(-5px); }
.fade-enter-active { transition: all 0.3s ease; }
.fade-enter-to { opacity: 1; transform: translateY(0); }
</style>