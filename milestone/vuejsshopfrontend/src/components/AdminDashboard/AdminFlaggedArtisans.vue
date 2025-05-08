<template>
  <div class="bg-white shadow-lg rounded-lg p-6 w-full">
    <!-- Toast stack -->
    <Toast :toasts="toasts" @close="removeToast" />

    <!-- Header & Reload -->
    <div class="flex items-center justify-between mb-6">
      <h2 class="text-3xl font-semibold text-gray-800">Flagged Artisans</h2>
      <button @click="refreshArtisans" class="flex items-center text-gray-600 hover:text-gray-800 transition">
        <svg xmlns="http://www.w3.org/2000/svg" class="h-6 w-6 mr-1" viewBox="0 0 24 24" fill="none" stroke="currentColor">
          <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M4 4v5h.582m15.253 6A9 9 0 115.334 5.334M20 20v-5h-.581" />
        </svg>
        Reload
      </button>
    </div>

    <!-- Loading Indicator -->
    <div v-if="loading" class="flex items-center justify-center py-12">
      <svg class="animate-spin h-8 w-8 text-gray-700" viewBox="0 0 24 24" fill="none">
        <circle class="opacity-25" cx="12" cy="12" r="10" stroke="currentColor" stroke-width="4" />
        <path class="opacity-75" fill="currentColor" d="M4 12a8 8 0 018-8v8H4z" />
      </svg>
      <span class="ml-3 text-gray-700 font-medium">Loading flagged artisans...</span>
    </div>

    <!-- Error Message -->
    <p v-if="!loading && error" class="text-red-600 text-center py-4">{{ error }}</p>

    <!-- Artisans Table -->
    <div v-if="!loading && artisans.length" class="overflow-x-auto">
      <table class="min-w-full table-auto border-collapse bg-white rounded-md shadow-md">
        <thead class="bg-gray-100 text-gray-700 font-semibold">
        <tr>
          <th class="px-6 py-3 text-left">ID</th>
          <th class="px-6 py-3 text-left">User</th>
          <th class="px-6 py-3 text-left">Location</th>
          <th class="px-6 py-3 text-left">Flags</th>
          <th class="px-6 py-3"></th>
        </tr>
        </thead>
        <transition-group tag="tbody" name="fade" class="divide-y divide-gray-200">
          <tr v-for="art in artisans" :key="art.id" class="hover:bg-gray-50 transition-colors duration-200">
            <td class="px-6 py-4 font-semibold">{{ art.id }}</td>
            <td class="px-6 py-4">{{ art.user?.name || 'N/A' }}</td>
            <td class="px-6 py-4">{{ art.location || 'N/A' }}</td>
            <td class="px-6 py-4">
              <span class="inline-block bg-yellow-200 text-yellow-800 text-xs font-semibold px-2 py-1 rounded-full">
                {{ art.flag_count }}
              </span>
            </td>
            <td class="px-6 py-4 text-right">
              <button @click="openDetailModal(art)" class="text-blue-600 hover:text-blue-800 font-medium transition">
                View Details
              </button>
            </td>
          </tr>
        </transition-group>
      </table>
    </div>

    <!-- No data message -->
    <p v-if="!loading && !artisans.length && !error" class="text-center text-gray-500 py-8">
      No flagged artisans available.
    </p>

    <!-- Detail Modal -->
    <div v-if="selectedArtisan" class="fixed inset-0 z-50 flex items-center justify-center bg-black bg-opacity-50">
      <div class="bg-white rounded-lg p-6 shadow-lg w-full max-w-3xl mx-4 overflow-auto max-h-[90vh] relative">
        <button @click="closeDetailModal" class="absolute top-2 right-2 text-gray-500 hover:text-gray-700 text-2xl">✖</button>
        <div class="p-4">
          <h3 class="text-2xl font-semibold mb-4 text-gray-800">Artisan Details</h3>
          <div class="grid grid-cols-1 md:grid-cols-2 gap-4 text-gray-700">
            <div><strong>ID:</strong> {{ selectedArtisan.id }}</div>
            <div><strong>Name:</strong> {{ selectedArtisan.user?.name || 'N/A' }}</div>
            <div><strong>Location:</strong> {{ selectedArtisan.location || 'N/A' }}</div>
            <div><strong>Flags:</strong> {{ selectedArtisan.flag_count }}</div>
            <div><strong>Bio:</strong> {{ selectedArtisan.bio || '–' }}</div>
            <div><strong>Skills:</strong> {{ selectedArtisan.skills || '–' }}</div>
            <div><strong>Certifications:</strong> {{ selectedArtisan.certifications || '–' }}</div>
            <div><strong>Experience:</strong> {{ selectedArtisan.years_of_experience || 0 }} yrs</div>
            <div class="md:col-span-2"><strong>Social Links:</strong> {{ parseSocialLinks(selectedArtisan.social_links) }}</div>
            <div class="md:col-span-2"><strong>Accept Custom Orders:</strong> {{ selectedArtisan.accept_custom_orders ? 'Yes' : 'No' }}</div>
            <div><strong>Average Rating:</strong> {{ selectedArtisan.average_rating }}</div>
            <div><strong>Created:</strong> {{ formatDate(selectedArtisan.created_at) }}</div>
            <div><strong>Updated:</strong> {{ formatDate(selectedArtisan.updated_at) }}</div>
          </div>
          <div class="mt-6 flex justify-end gap-4">
            <button @click="resetFlag(selectedArtisan)" class="px-4 py-2 bg-green-600 text-white rounded-md hover:bg-green-700 transition font-semibold">
              Dismiss Flag
            </button>
            <button @click="confirmDelete(selectedArtisan)" class="px-4 py-2 bg-red-600 text-white rounded-md hover:bg-red-700 transition font-semibold">
              Delete Artisan
            </button>
          </div>
        </div>
      </div>
    </div>

    <!-- Confirm Delete Modal -->
    <div v-if="confirm.visible" class="fixed inset-0 z-50 flex items-center justify-center bg-black bg-opacity-50">
      <div class="bg-white rounded-lg p-6 shadow-lg w-full max-w-sm">
        <h3 class="text-xl font-semibold mb-4">Confirm Deletion</h3>
        <p class="mb-6">Are you sure you want to delete artisan #{{ confirm.artisan.id }}?</p>
        <div class="flex justify-end gap-4">
          <button @click="cancelDelete" class="px-4 py-2 bg-gray-200 rounded-md hover:bg-gray-300">
            Cancel
          </button>
          <button @click="deleteArtisan(confirm.artisan)" class="px-4 py-2 bg-red-600 text-white rounded-md hover:bg-red-700">
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

const apiBase = process.env.VUE_APP_API_URL + '/api/v1'
const artisans = ref([])
const loading = ref(false)
const error = ref('')
const selectedArtisan = ref(null)
const toasts = reactive([])
const confirm = reactive({ visible: false, artisan: null })

function removeToast(id) {
  const i = toasts.findIndex(t => t.id === id)
  if (i > -1) toasts.splice(i, 1)
}
function showToast(message, type = 'success') {
  const id = Date.now() + Math.random()
  toasts.push({ id, message, type })
  setTimeout(() => removeToast(id), 5000)
}

async function fetchFlaggedArtisans() {
  loading.value = true; error.value = ''
  try {
    const { data } = await axios.get(`${apiBase}/flagged-artisans`)
    artisans.value = data.data
  } catch (e) {
    error.value = e.response?.data?.message || 'Error fetching flagged artisans.'
    showToast(error.value, 'error')
  } finally { loading.value = false }
}

const refreshArtisans = () => fetchFlaggedArtisans()
const openDetailModal = a => selectedArtisan.value = a
const closeDetailModal = () => selectedArtisan.value = null

function confirmDelete(a) { confirm.artisan = a; confirm.visible = true }
function cancelDelete() { confirm.visible = false; confirm.artisan = null }

async function resetFlag(a) {
  try {
    await axios.post(`${apiBase}/reset-flag`, { type: 'artisan', id: a.id })
    artisans.value = artisans.value.filter(x => x.id !== a.id)
    showToast('Flag dismissed', 'success')
    closeDetailModal()
  } catch {
    showToast('Failed to dismiss flag', 'error')
  }
}

async function deleteArtisan(a) {
  try {
    await axios.delete(`${apiBase}/admin/artisans/${a.id}`)
    artisans.value = artisans.value.filter(x => x.id !== a.id)
    showToast('Artisan deleted', 'success')
    closeDetailModal(); cancelDelete()
  } catch {
    showToast('Failed to delete artisan', 'error')
  }
}

function parseSocialLinks(json) {
  try { return Object.entries(JSON.parse(json || '{}')).map(([k,v])=>`${k}: ${v}`).join(', ') }
  catch { return json }
}

const formatDate = d => new Date(d).toLocaleDateString()

onMounted(fetchFlaggedArtisans)
</script>

<style scoped>
.fade-enter-from { opacity: 0; transform: translateY(-5px); }
.fade-enter-active { transition: all 0.3s ease; }
.fade-enter-to { opacity: 1; transform: translateY(0); }
</style>