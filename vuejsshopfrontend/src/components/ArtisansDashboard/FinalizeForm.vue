<template>
  <div class="fixed inset-0 bg-black bg-opacity-60 z-50 flex justify-center items-center p-4">
    <!-- Toast stack (teleported to body) -->
    <Toast :toasts="toasts" @close="removeToast" />

    <div class="bg-white w-full max-w-md rounded-lg shadow-lg p-6 relative">
      <!-- Close Button -->
      <button
          @click="close"
          class="absolute top-2 right-3 text-gray-500 hover:text-red-500 text-xl"
      >
        &times;
      </button>

      <h2 class="text-xl font-semibold mb-4 text-center">Finalize Custom Order</h2>

      <!-- Form -->
      <form @submit.prevent="submitFinalization" class="space-y-4">
        <!-- Final Description -->
        <div>
          <label class="block text-sm font-medium text-gray-700 mb-1">Final Description</label>
          <textarea
              v-model="finalDescription"
              rows="3"
              class="w-full border border-gray-300 rounded-md px-3 py-2 text-sm focus:outline-none focus:ring focus:ring-blue-300"
              placeholder="Provide any finalized details..."
          ></textarea>
        </div>

        <!-- Final Price -->
        <div>
          <label class="block text-sm font-medium text-gray-700 mb-1">Final Price ($)</label>
          <input
              v-model="finalPrice"
              type="number"
              step="0.01"
              min="0"
              class="w-full border border-gray-300 rounded-md px-3 py-2 text-sm focus:outline-none focus:ring focus:ring-blue-300"
              required
          />
        </div>

        <!-- Final Deadline -->
        <div>
          <label class="block text-sm font-medium text-gray-700 mb-1">Final Deadline</label>
          <input
              v-model="finalDeadline"
              type="date"
              class="w-full border border-gray-300 rounded-md px-3 py-2 text-sm focus:outline-none focus:ring focus:ring-blue-300"
              required
          />
        </div>

        <!-- Attachment -->
        <div>
          <label class="block text-sm font-medium text-gray-700 mb-1">Attach Reference Image (optional)</label>
          <input
              type="file"
              accept="image/*"
              @change="handleFileChange"
              class="text-sm"
          />
          <p v-if="fileName" class="text-xs text-gray-500 mt-1">📎 {{ fileName }}</p>
        </div>

        <!-- Submit -->
        <div class="flex justify-end pt-2">
          <button
              type="submit"
              :disabled="loading"
              class="bg-blue-600 text-white px-4 py-2 rounded-md text-sm hover:bg-blue-700 disabled:opacity-50 transition"
          >
            <span v-if="loading">Submitting...</span>
            <span v-else>Finalize</span>
          </button>
        </div>
      </form>
    </div>
  </div>
</template>

<script setup>
import { ref, reactive } from 'vue'
import axios from 'axios'
import Toast from '@/components/Toast.vue'

const props = defineProps({ orderId: { type: Number, required: true } })
const emit = defineEmits(['close', 'finalized'])

// Form state
const finalDescription = ref('')
const finalPrice = ref('')
const finalDeadline = ref('')
const attachment = ref(null)
const fileName = ref('')

// Loading guard
const loading = ref(false)

const apiBaseUrl = process.env.VUE_APP_API_URL

// Toast stack
const toasts = reactive([])
function removeToast(id) {
  const idx = toasts.findIndex(t => t.id === id)
  if (idx !== -1) toasts.splice(idx, 1)
}
function showToast(message, type = 'success') {
  const id = Date.now() + Math.random()
  toasts.push({ id, message, type })
  setTimeout(() => removeToast(id), 5000)
}

function handleFileChange(event) {
  const file = event.target.files[0]
  attachment.value = file || null
  fileName.value = file?.name || ''
}

async function submitFinalization() {
  if (loading.value) return
  loading.value = true

  const token = localStorage.getItem('authToken')
  const config = { headers: { Authorization: `Bearer ${token}`, 'Content-Type': 'multipart/form-data' } }

  const formData = new FormData()
  formData.append('final_price', finalPrice.value)
  formData.append('final_deadline', finalDeadline.value)
  if (finalDescription.value) formData.append('final_description', finalDescription.value)
  if (attachment.value) formData.append('attachment', attachment.value)

  try {
    await axios.post(
        `${apiBaseUrl}/api/v1/artisan/custom-orders/${props.orderId}/finalize`,
        formData,
        config
    )

    showToast('Order finalized successfully!', 'success')
    // Clear form
    finalDescription.value = ''
    finalPrice.value = ''
    finalDeadline.value = ''
    attachment.value = null
    fileName.value = ''

    emit('finalized')
    emit('close')
  } catch (error) {
    console.error('Failed to finalize order:', error)
    showToast(
        error.response?.data?.message || 'Failed to finalize. Please try again.',
        'error'
    )
  } finally {
    loading.value = false
  }
}

function close() {
  emit('close')
}
</script>

<style scoped>
/* Optional: Smooth fade-in animation */
.fade-enter-active, .fade-leave-active { transition: opacity 0.2s ease; }
.fade-enter-from, .fade-leave-to { opacity: 0; }
</style>
