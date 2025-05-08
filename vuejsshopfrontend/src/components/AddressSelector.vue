<template>
  <div class="w-full">
    <!-- Toasts -->
    <Toast :toasts="toasts" @close="removeToast" />

    <div class="bg-white rounded-lg shadow-lg p-6 w-full">
      <h3 class="text-lg font-semibold mb-4">Select Address</h3>

      <!-- Loading Spinner -->
      <div v-if="isLoading" class="flex justify-center py-8">
        <div class="loader"></div>
      </div>

      <!-- Address List -->
      <div v-else class="space-y-4 max-h-64 overflow-auto">
        <div
            v-for="address in addresses"
            :key="address.id"
            class="flex items-center justify-between bg-gray-50 p-4 rounded-lg"
        >
          <label class="flex items-center space-x-3 cursor-pointer">
            <input
                type="radio"
                class="form-radio h-5 w-5 text-brown-500"
                v-model="selectedAddress"
                :value="address.id"
            />
            <div>
              <p class="font-medium">{{ address.label || '—' }}</p>
              <p class="text-sm text-gray-600">
                {{ address.street }}, {{ address.city }}
              </p>
            </div>
          </label>
          <button
              @click="promptDelete(address.id)"
              class="text-red-500 hover:text-red-700 text-xl"
              aria-label="Delete"
          >&times;</button>
        </div>
      </div>

      <button
          @click="openAddModal"
          class="mt-6 w-full bg-brown-500 hover:bg-brown-600 text-white py-2 rounded-lg transition"
      >
        + Add New Address
      </button>
    </div>

    <!-- Add Address Modal -->
    <div
        v-if="showAddModal"
        class="fixed inset-0 bg-black bg-opacity-50 flex items-center justify-center z-50"
    >
      <div class="relative bg-white rounded-lg shadow-lg p-6 max-w-md w-full mx-4">
        <button
            @click="closeAddModal"
            class="absolute top-3 right-3 text-gray-500 hover:text-gray-700 text-xl"
            aria-label="Close"
        >&times;</button>

        <h2 class="text-2xl mb-4 text-center">Add New Address</h2>

        <form @submit.prevent="submitAddress" class="space-y-4">
          <div>
            <label class="block mb-1" for="label">Label (optional)</label>
            <input
                id="label"
                type="text"
                v-model="form.label"
                placeholder="Home, Office…"
                class="input-field"
            />
          </div>
          <div>
            <label class="block mb-1" for="street">Street</label>
            <input
                id="street"
                type="text"
                v-model="form.street"
                required
                placeholder="123 Main St"
                class="input-field"
            />
          </div>
          <div>
            <label class="block mb-1" for="city">City</label>
            <input
                id="city"
                type="text"
                v-model="form.city"
                required
                placeholder="Kathmandu"
                class="input-field"
            />
          </div>
          <div>
            <label class="block mb-1" for="state">State / Province</label>
            <input
                id="state"
                type="text"
                v-model="form.state"
                placeholder="Bagmati"
                class="input-field"
            />
          </div>
          <div>
            <label class="block mb-1" for="postal_code">Postal Code</label>
            <input
                id="postal_code"
                type="text"
                v-model="form.postal_code"
                required
                placeholder="44600"
                class="input-field"
            />
          </div>
          <div>
            <label class="block mb-1" for="country">Country</label>
            <input
                id="country"
                type="text"
                v-model="form.country"
                required
                placeholder="Nepal"
                class="input-field"
            />
          </div>
          <button
              type="submit"
              :disabled="isSubmitting"
              class="w-full bg-brown-500 text-white py-2 rounded-lg hover:bg-brown-600 transition duration-300"
          >
            <span v-if="isSubmitting" class="loader w-4 h-4 inline-block mr-2"></span>
            <span v-else>Add Address</span>
          </button>
        </form>
      </div>
    </div>

    <!-- Delete Confirmation Modal -->
    <div
        v-if="showConfirm"
        class="fixed inset-0 bg-black bg-opacity-50 flex items-center justify-center z-50"
    >
      <div class="bg-white p-6 rounded-lg shadow-md max-w-sm w-full mx-4">
        <p class="mb-4">Are you sure you want to delete this address?</p>
        <div class="flex justify-end space-x-4">
          <button @click="showConfirm = false" class="px-4 py-2 bg-gray-200 rounded">
            Cancel
          </button>
          <button @click="confirmDelete" class="px-4 py-2 bg-red-500 text-white rounded">
            Delete
          </button>
        </div>
      </div>
    </div>
  </div>
</template>

<script setup>
import { ref, reactive, onMounted, watch } from 'vue'
import axios from 'axios'
import Toast from '@/components/Toast.vue'

const emit = defineEmits(['address-selected'])

const API_BASE = process.env.VUE_APP_API_URL
const authHeader = () => {
  const t = localStorage.getItem('authToken')
  return t ? { Authorization: `Bearer ${t}` } : {}
}

const addresses       = ref([])
const selectedAddress = ref(null)
const isLoading       = ref(false)

// Add‑address modal state
const showAddModal = ref(false)
const form = reactive({
  label: '',
  street: '',
  city: '',
  state: '',
  postal_code: '',
  country: '',
})
const isSubmitting = ref(false)

// Delete‑confirm modal state
const showConfirm = ref(false)
let toDeleteId = null

// Toast stack
const toasts = reactive([])
function showToast(msg, type='success') {
  const id = Date.now() + Math.random()
  toasts.push({ id, message: msg, type })
  setTimeout(()=> removeToast(id), 4000)
}
function removeToast(id) {
  const i = toasts.findIndex(t=>t.id===id)
  if(i!==-1) toasts.splice(i,1)
}

// Emit on selection
watch(selectedAddress, id => {
  if(id !== null) emit('address-selected', id)
})

async function loadAddresses() {
  isLoading.value = true
  try {
    const res = await axios.get(`${API_BASE}/api/v1/addresses`, {
      headers: authHeader()
    })
    addresses.value = res.data.data || []
    if(!selectedAddress.value && addresses.value.length) {
      selectedAddress.value = addresses.value[0].id
    }
  } catch {
    showToast('Failed to load addresses','error')
  } finally {
    isLoading.value = false
  }
}

function openAddModal() {
  showAddModal.value = true
}
function closeAddModal() {
  showAddModal.value = false
  Object.keys(form).forEach(k=> form[k]='')
}

async function submitAddress() {
  isSubmitting.value = true
  try {
    const res = await axios.post(
        `${API_BASE}/api/v1/addresses`,
        { ...form },
        { headers: authHeader() }
    )
    showToast('Address added!','success')
    await loadAddresses()
    selectedAddress.value = res.data.data.id
    closeAddModal()
  } catch (err) {
    const errs = err.response?.data?.errors
    if(errs) Object.values(errs).flat().forEach(m=>showToast(m,'error'))
    else showToast(err.response?.data?.message||'Failed to add','error')
  } finally {
    isSubmitting.value = false
  }
}

function promptDelete(id) {
  toDeleteId = id
  showConfirm.value = true
}

async function confirmDelete() {
  showConfirm.value = false
  try {
    await axios.delete(`${API_BASE}/api/v1/addresses/${toDeleteId}`, {
      headers: authHeader()
    })
    showToast('Address deleted','success')
    await loadAddresses()
  } catch {
    showToast('Failed to delete','error')
  }
}

onMounted(loadAddresses)
</script>

<style scoped>
.bg-brown-500 { background-color: #A0522D; }
.bg-brown-600 { background-color: #8B4513; }

/* Loader */
.loader {
  border: 3px solid #f3f3f3;
  border-top: 3px solid #A0522D;
  border-radius: 50%;
  width: 1em;
  height: 1em;
  animation: spin 1s linear infinite;
}
@keyframes spin { to { transform: rotate(360deg); } }

/* Radio accent */
.form-radio:checked {
  accent-color: #A0522D;
}

.input-field {
  @apply w-full p-2 border border-gray-300 rounded-lg focus:outline-none focus:ring-2 focus:ring-brown-500;
}
</style>
